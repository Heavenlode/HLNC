using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using Heavenlode;
using HLNC.Serialization;

namespace HLNC
{
    public partial class WorldRunner : Node
    {
        public enum PeerSyncStatus {
            INITIAL,
            IN_WORLD
        }

        public struct PeerState
        {
            public NetPeer Peer;
            public Tick Tick;
            public PeerSyncStatus Status;
            public string Token;
            public Dictionary<NetworkId, byte> WorldToPeerNodeMap;
            public Dictionary<byte, NetworkId> PeerToWorldNodeMap;
            public Dictionary<NetworkId, bool> SpawnAware;
            public long AvailableNodes;
        }

        public Guid WorldId { get; internal set; }

        // A bit list of all nodes in use by each peer
        // For example, 0 0 0 0 (... etc ...) 0 1 0 1 would mean that the first and third nodes are in use
        public long ClientAvailableNodes = 0;
        readonly static byte MAX_NETWORK_NODES = 64;
        private Dictionary<NetPeer, PeerState> PeerStates = [];

        [Signal]
        public delegate void OnPeerSyncStatusChangeEventHandler(string peerId, int status);


        /// <summary>
        /// Only applicable on the client side.
        /// </summary>
        public static WorldRunner CurrentWorld { get; internal set; }

        /// <summary>
        /// Only used by the client to determine the current root scene.
        /// </summary>
        public NetworkNodeWrapper RootScene;

        internal int NetworkId_counter = 0;
        internal System.Collections.Generic.Dictionary<NetworkId, NetworkNodeWrapper> NetworkScenes = [];
        private Godot.Collections.Dictionary<NetPeer, Godot.Collections.Dictionary<byte, Godot.Collections.Dictionary<int, Variant>>> inputStore = [];
        public Godot.Collections.Dictionary<NetPeer, Godot.Collections.Dictionary<byte, Godot.Collections.Dictionary<int, Variant>>> InputStore => inputStore;

        public override void _Ready()
        {
            base._Ready();
            Name = "WorldRunner";
        }

        internal void DebugPrint(string msg)
        {
            GD.Print($"{(OS.HasFeature("dedicated_server") ? "Server" : "Client")} (world {WorldId}): {msg}");
        }
        
        /// <summary>
        /// The current network tick. On the client side, this does not represent the server's current tick, which will always be slightly ahead.
        /// </summary>
        public int CurrentTick { get; internal set; } = 0;

        public NetworkNodeWrapper GetNodeFromNetworkId(NetworkId network_id)
        {
            if (network_id == -1)
                return new NetworkNodeWrapper(null);
            if (!NetworkScenes.ContainsKey(network_id))
                return new NetworkNodeWrapper(null);
            return NetworkScenes[network_id];
        }

        [Signal]
        public delegate void OnAfterNetworkTickEventHandler(Tick tick);

        [Signal]
        public delegate void OnPlayerJoinedEventHandler(string peerId);

        private int _frameCounter = 0;
        /// <summary>
        /// This method is executed every tick on the Server side, and kicks off all logic which processes and sends data to every client.
        /// </summary>
        public void ServerProcessTick()
        {

            foreach (var net_id in NetworkScenes.Keys)
            {
                var networkNode = NetworkScenes[net_id];
                if (networkNode == null)
                    continue;

                if (!IsInstanceValid(networkNode.Node) || networkNode.Node.IsQueuedForDeletion())
                {
                    NetworkScenes.Remove(net_id);
                    continue;
                }
                networkNode._NetworkProcess(CurrentTick);
                networkNode.InputBuffer = [];
                foreach (var networkChild in networkNode.StaticNetworkChildren)
                {
                    networkChild._NetworkProcess(CurrentTick);
                    networkChild.InputBuffer = [];
                }
            }

            var peers = PeerStates.Keys.ToList();
            var exportedState = ExportState(peers);
            foreach (var peer in peers)
            {
                var size = exportedState[peer].bytes.Length;
                if (size > NetworkRunner.MTU)
                {
                    NetworkRunner.DebugPrint($"Warning: Data size {size} exceeds MTU {NetworkRunner.MTU}");
                }

                var buffer = new HLBuffer();
                HLBytes.Pack(buffer, CurrentTick);
                HLBytes.Pack(buffer, exportedState[peer].bytes, true);

                peer.Send(1, buffer.bytes, (int)ENetPacketPeer.FlagUnsequenced);
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            if (NetworkRunner.Instance.IsServer)
            {
                _frameCounter += 1;
                if (_frameCounter < NetworkRunner.PhysicsTicksPerNetworkTick)
                    return;
                _frameCounter = 0;
                CurrentTick += 1;
                ServerProcessTick();
                EmitSignal("OnAfterNetworkTick", CurrentTick);
            }
        }

        public bool HasSpawnedForClient(NetworkId networkId, NetPeer peer)
        {
            if (!PeerStates.ContainsKey(peer))
            {
                return false;
            }
            if (!PeerStates[peer].SpawnAware.ContainsKey(networkId))
            {
                return false;
            }
            return PeerStates[peer].SpawnAware[networkId];
        }

        public void SetSpawnedForClient(NetworkId networkId, NetPeer peer)
        {
            PeerStates[peer].SpawnAware[networkId] = true;
        }

        public void ChangeScene(NetworkNodeWrapper node)
        {
            if (NetworkRunner.Instance.IsServer) return;

            if (RootScene != null) {
                RootScene.Node.QueueFree();
            }
            NetworkRunner.DebugPrint("Changing scene to " + node.Node.Name);
            // TODO: Support this more generally
            GetTree().CurrentScene.AddChild(node.Node);
            RootScene = node;
            node._NetworkPrepare(this);
        }

        public PeerState? GetPeerWorldState(string peerId)
        {
            var peer = NetworkRunner.Instance.GetPeer(peerId);
            if (!PeerStates.ContainsKey(peer))
            {
                return null;
            }
            return PeerStates[peer];
        }

        public PeerState? GetPeerWorldState(NetPeer peer)
        {
            if (!PeerStates.ContainsKey(peer))
            {
                return null;
            }
            return PeerStates[peer];
        }

        readonly private Dictionary<NetPeer, PeerState> pendingSyncStates = [];
        public void SetPeerState(NetPeer peer, PeerState state)
        {
            if (PeerStates[peer].Status != state.Status) {
                EmitSignal("OnPeerSyncStatusChange", NetworkRunner.Instance.GetPeerId(peer), (int)state.Status);
            }
            if (state.Status == PeerSyncStatus.IN_WORLD)
            {
                EmitSignal("OnPlayerJoined", NetworkRunner.Instance.GetPeerId(peer));
            }
            PeerStates[peer] = state;
        }

        public void QueuePeerState(NetPeer peer, PeerSyncStatus status)
        {
            var newState = PeerStates[peer];
            newState.Status = status;
            newState.Tick = CurrentTick;
            pendingSyncStates[peer] = newState;
        }

        public NetworkNodeWrapper GetNetworkNode(NetworkId networkId)
        {
            if (NetworkScenes.ContainsKey(networkId))
            {
                return NetworkScenes[networkId];
            }
            return null;
        }
        public byte GetPeerNodeId(NetPeer peer, NetworkNodeWrapper node)
        {
            if (node == null) return 0;
            if (!PeerStates.ContainsKey(peer))
            {
                return 0;
            }
            if (!PeerStates[peer].WorldToPeerNodeMap.ContainsKey(node.NetworkId))
            {
                return 0;
            }
            return PeerStates[peer].WorldToPeerNodeMap[node.NetworkId];
        }

        public NetworkNodeWrapper GetPeerNode(NetPeer peer, byte networkId)
        {
            if (!PeerStates.ContainsKey(peer))
            {
                return null;
            }
            if (!PeerStates[peer].PeerToWorldNodeMap.ContainsKey(networkId))
            {
                return null;
            }
            return NetworkScenes[PeerStates[peer].PeerToWorldNodeMap[networkId]];
        }

        public void DeregisterPeerNode(NetworkNodeWrapper node, NetPeer peer = null)
        {
            if (NetworkRunner.Instance.IsServer)
            {
                if (peer == null)
                {
                    GD.PrintErr("Server must specify a peer when deregistering a node.");
                    return;
                }
                if (PeerStates[peer].WorldToPeerNodeMap.ContainsKey(node.NetworkId))
                {
                    var peerState = PeerStates[peer];
                    peerState.AvailableNodes &= ~(1 << PeerStates[peer].WorldToPeerNodeMap[node.NetworkId]);
                    PeerStates[peer] = peerState;
                    PeerStates[peer].WorldToPeerNodeMap.Remove(node.NetworkId);
                }
            }
            else
            {
                NetworkScenes.Remove(node.NetworkId);
            }
        }

        // A local peer node ID is assigned to each node that a peer owns
        // This allows us to sync nodes across the network without sending long integers
        // 0 indicates that the node is not registered. Node ID starts at 1
        // Up to 64 nodes can be networked per peer at a time.
        // TODO: Consider supporting more
        // TODO: Handle de-registration of nodes (e.g. despawn, and object interest)
        public byte TryRegisterPeerNode(NetworkNodeWrapper node, NetPeer peer = null)
        {
            if (NetworkRunner.Instance.IsServer)
            {
                if (peer == null)
                {
                    GD.PrintErr("Server must specify a peer when registering a node.");
                    return 0;
                }
                if (PeerStates[peer].WorldToPeerNodeMap.ContainsKey(node.NetworkId))
                {
                    return PeerStates[peer].WorldToPeerNodeMap[node.NetworkId];
                }
                for (byte i = 0; i < MAX_NETWORK_NODES; i++)
                {
                    byte localNodeId = (byte)(i + 1);
                    if ((PeerStates[peer].AvailableNodes & ((long)1 << localNodeId)) == 0)
                    {
                        PeerStates[peer].WorldToPeerNodeMap[node.NetworkId] = localNodeId;
                        PeerStates[peer].PeerToWorldNodeMap[localNodeId] = node.NetworkId;
                        var peerState = PeerStates[peer];
                        peerState.AvailableNodes |= (long)1 << localNodeId;
                        PeerStates[peer] = peerState;
                        return localNodeId;
                    }
                }

                GD.PrintErr("Peer " + peer + " has reached the maximum amount of nodes.");
                return 0;
            }

            if (NetworkScenes.ContainsKey(node.NetworkId))
            {
                return 0;
            }

            NetworkScenes[node.NetworkId] = node;
            return 1;
        }

        public NetworkNode3D Spawn(NetworkNode3D node, NetworkNode3D parent = null, NetPeer inputAuthority = null, string nodePath = ".")
        {
            if (!NetworkRunner.Instance.IsServer) return null;

            node.DynamicSpawn = true;
            node.CurrentWorld = this;
            node.InputAuthority = inputAuthority;
            if (parent == null)
            {
                node.NetworkParent = RootScene;
                node.NetworkParent.Node.GetNode(nodePath).AddChild(node);
            }
            else
            {
                node.NetworkParent = new NetworkNodeWrapper(parent);
                parent.GetNode(nodePath).AddChild(node);
            }
            node._NetworkPrepare(this);
            return node;
        }

        public void JoinPeer(NetPeer peer, string token)
        {
            NetworkRunner.Instance.PeerWorldMap[peer] = this;
            PeerStates[peer] = new PeerState
            {
                Peer = peer,
                Tick = 0,
                Status = PeerSyncStatus.INITIAL,
                Token = token,
                WorldToPeerNodeMap = [],
                PeerToWorldNodeMap = [],
                SpawnAware = []
            };
        }

        public Dictionary<ENetPacketPeer, HLBuffer> ExportState(List<ENetPacketPeer> peers)
        {
            Dictionary<NetPeer, HLBuffer> peerBuffers = [];
            foreach (ENetPacketPeer peer in peers)
            {
                long updatedNodes = 0;
                peerBuffers[peer] = new HLBuffer();
                var peerNodesBuffers = new Dictionary<long, HLBuffer>();
                var peerNodesSerializersList = new Dictionary<long, byte>();
                foreach (var node in NetworkScenes.Values)
                {
                    var serializersBuffer = new HLBuffer();
                    byte serializersRun = 0;
                    for (var serializerIdx = 0; serializerIdx < node.Serializers.Length; serializerIdx++)
                    {
                        var serializer = node.Serializers[serializerIdx];
                        var serializerResult = serializer.Export(this, peer);
                        if (serializerResult.bytes.Length == 0)
                        {
                            continue;
                        }
                        serializersRun |= (byte)(1 << serializerIdx);
                        HLBytes.Pack(serializersBuffer, serializerResult.bytes);
                    }
                    if (serializersRun == 0)
                    {
                        continue;
                    }
                    byte localNodeId = PeerStates[peer].WorldToPeerNodeMap[node.NetworkId];
                    updatedNodes |= (long)1 << localNodeId;
                    peerNodesSerializersList[localNodeId] = serializersRun;
                    peerNodesBuffers[localNodeId] = new HLBuffer();
                    HLBytes.Pack(peerNodesBuffers[localNodeId], serializersBuffer.bytes);
                }

                // 1. Pack a bit list of all nodes which have serialized data
                HLBytes.Pack(peerBuffers[peer], updatedNodes);

                // 2. Pack what serializers are run for every node 
                var orderedNodeKeys = peerNodesBuffers.OrderBy(x => x.Key).Select(x => x.Key).ToList();
                foreach (var nodeKey in orderedNodeKeys)
                {
                    HLBytes.Pack(peerBuffers[peer], peerNodesSerializersList[nodeKey]);
                }

                // 3. Pack the serialized data for every node
                foreach (var nodeKey in orderedNodeKeys)
                {
                    HLBytes.Pack(peerBuffers[peer], peerNodesBuffers[nodeKey].bytes);
                }
            }

            foreach (var node in NetworkScenes.Values)
            {
                // Finally, cleanup serializers
                foreach (var serializer in node.Serializers)
                {
                    serializer.Cleanup();
                }
            }

            return peerBuffers;
        }

        public void ImportState(HLBuffer stateBytes)
        {
            var affectedNodes = HLBytes.UnpackInt64(stateBytes);
            var nodeIdToSerializerList = new Dictionary<byte, byte>();
            for (byte i = 0; i < MAX_NETWORK_NODES; i++)
            {
                if ((affectedNodes & ((long)1 << i)) == 0)
                {
                    continue;
                }
                var serializersRun = HLBytes.UnpackInt8(stateBytes);
                nodeIdToSerializerList[i] = serializersRun;
            }

            foreach (var nodeIdSerializerList in nodeIdToSerializerList)
            {
                var localNodeId = nodeIdSerializerList.Key;
                NetworkScenes.TryGetValue(localNodeId, out NetworkNodeWrapper node);
                if (node == null) {
                    var blankScene = new NetworkNode3D
                    {
                        NetworkId = localNodeId
                    };
                    blankScene.SetupSerializers();
                    NetworkRunner.Instance.AddChild(blankScene);
                    node = new NetworkNodeWrapper(blankScene);
                }
                for (var serializerIdx = 0; serializerIdx < node.Serializers.Length; serializerIdx++)
                {
                    if ((nodeIdSerializerList.Value & ((long)1 << serializerIdx)) == 0)
                    {
                        continue;
                    }
                    var serializerInstance = node.Serializers[serializerIdx];
                    serializerInstance.Import(this, stateBytes, out NetworkNodeWrapper nodeOut);
                    if (node != nodeOut)
                    {
                        node = nodeOut;
                        serializerIdx = 0;
                    }
                }
            }
        }

        public void PeerAcknowledge(NetPeer peer, Tick tick)
        {
            if (PeerStates[peer].Tick >= tick)
            {
                return;
            }
            if (PeerStates[peer].Status == PeerSyncStatus.INITIAL)
            {
                var newPeerState = PeerStates[peer];
                newPeerState.Tick = tick;
                newPeerState.Status = PeerSyncStatus.IN_WORLD;
                // The first time a peer acknowledges a tick, we know they are in the zone
                SetPeerState(peer, newPeerState);
            }
            // if (pendingSyncStates.TryGetValue(peer, out PendingSyncState pendingSyncState))
            // {
            //     if (pendingSyncState.tick <= tick)
            //     {
            //         PeerState[peer] = pendingSyncState.state;
            //         pendingSyncStates.Remove(peer);
            //     }
            // }
            foreach (var node in NetworkScenes.Values)
            {
                for (var serializerIdx = 0; serializerIdx < node.Serializers.Length; serializerIdx++)
                {
                    var serializer = node.Serializers[serializerIdx];
                    serializer.Acknowledge(this, peer, tick);
                }
            }
        }
        public void ClientHandleTick(int incomingTick, byte[] stateBytes)
        {
            if (incomingTick <= CurrentTick)
            {
                return;
            }
            // GD.Print("INCOMING DATA: " + BitConverter.ToString(HLBytes.Decompress(stateBytes)));
            CurrentTick = incomingTick;
            ImportState(new HLBuffer(stateBytes));
            foreach (var net_id in NetworkScenes.Keys)
            {
                var node = NetworkScenes[net_id];
                if (node == null)
                    continue;
                if (node.Node.IsQueuedForDeletion())
                {
                    NetworkScenes.Remove(net_id);
                    continue;
                }
                node._NetworkProcess(CurrentTick);
                SendInput(node);

                foreach (var staticChild in node.StaticNetworkChildren)
                {
                    if (staticChild == null || staticChild.Node.IsQueuedForDeletion())
                    {
                        continue;
                    }
                    staticChild._NetworkProcess(CurrentTick);
                    SendInput(staticChild);
                }
            }
            HLBuffer buffer = new HLBuffer();
            HLBytes.Pack(buffer, incomingTick);
            NetworkRunner.Instance.ENetHost.Send(1, buffer.bytes, (int)ENetPacketPeer.FlagUnsequenced);
        }

        public void RegisterSpawn(NetworkNodeWrapper wrapper)
        {
            if (NetworkRunner.Instance.IsServer)
            {
                NetworkId_counter += 1;
                while (NetworkScenes.ContainsKey(NetworkId_counter))
                {
                    NetworkId_counter += 1;
                }
                NetworkScenes[NetworkId_counter] = wrapper;
                wrapper.NetworkId = NetworkId_counter;
                return;
            }

            if (!wrapper.DynamicSpawn)
            {
                wrapper.Node.QueueFree();
            }
        }

        public void SendInput(NetworkNodeWrapper networkNode) {
            if (NetworkRunner.Instance.IsServer) return;
            var setInputs = networkNode.InputBuffer.Keys.Aggregate((long)0, (acc, key) => {
                acc |= (long)1 << key;
                return acc;
            });
            if (setInputs == 0)
            {
                return;
            }

            var inputBuffer = new HLBuffer();
            HLBytes.Pack(inputBuffer, (byte)networkNode.NetworkId);
            HLBytes.Pack(inputBuffer, setInputs);
            foreach (var key in networkNode.InputBuffer.Keys)
            {
                HLBytes.Pack(inputBuffer, key);
                HLBytes.PackVariant(inputBuffer, networkNode.InputBuffer[key], true, true);
            }

            NetworkRunner.Instance.ENetHost.Send((int)NetworkRunner.ENetChannelId.Input, inputBuffer.bytes, (int)ENetPacketPeer.FlagReliable);
            networkNode.InputBuffer = [];
        }

        public void ReceiveInput(NetPeer peer, HLBuffer buffer)
        {
            if (!NetworkRunner.Instance.IsServer) return;
            var networkId = HLBytes.UnpackByte(buffer);
            var worldNetworkId = PeerStates[peer].PeerToWorldNodeMap.GetValueOrDefault(networkId, -1);
            var node = GetNodeFromNetworkId(worldNetworkId);
            if (node == null)
            {
                GD.PrintErr("Received input for unknown node " + worldNetworkId);
                return;
            }

            if (node.InputAuthority != peer)
            {
                GD.PrintErr("Received input for node " + worldNetworkId + " from unauthorized peer " + peer);
                return;
            }

            var setInputs = HLBytes.UnpackInt64(buffer);
            while (setInputs > 0)
            {
                var key = HLBytes.UnpackInt8(buffer);
                var value = HLBytes.UnpackVariant(buffer);
                if (value.HasValue) {
                    node.InputBuffer[key] = value.Value;
                }
                setInputs &= ~((long)1 << key);
            }
        }
    }
}