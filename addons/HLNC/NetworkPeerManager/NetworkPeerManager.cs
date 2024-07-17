using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using HLNC.Serialization;

namespace HLNC
{
    internal partial class NetworkPeerManager : Node, IPeerStateController
    {
        readonly static byte MAX_NETWORK_NODES = 64;

        // A bit list of all nodes in use by each peer
        // For example, 0 0 0 0 (... etc ...) 0 1 0 1 would mean that the first and third nodes are in use
        readonly private Dictionary<NetPeer, long> availablePeerNodes = [];
        readonly private Dictionary<NetPeer, Dictionary<NetworkId, byte>> globalNodeToLocalNodeMap = [];
        readonly private Dictionary<NetPeer, Dictionary<byte, NetworkId>> localNodeToGlobalNodeMap = [];
        private Dictionary<NetPeer, Dictionary<NetworkId, bool>> spawnAware = [];
        public Dictionary<NetPeer, IPeerStateController.PeerSyncState> PeerSyncState = [];
        public Tick CurrentTick => NetworkRunner.Instance.CurrentTick;

        public bool HasSpawnedForClient(NetworkId networkId, NetPeer peer)
        {
            if (!spawnAware.ContainsKey(peer))
            {
                return false;
            }
            if (!spawnAware[peer].ContainsKey(networkId))
            {
                return false;
            }
            return spawnAware[peer][networkId];
        }

        public void SetSpawnedForClient(NetworkId networkId, NetPeer peer)
        {
            if (!spawnAware.ContainsKey(peer))
            {
                spawnAware[peer] = [];
            }
            spawnAware[peer][networkId] = true;
        }

        public void ChangeScene(NetworkNodeWrapper node)
        {
            if (NetworkRunner.Instance.IsServer) return;

            if (NetworkRunner.Instance.CurrentScene != null) {
                NetworkRunner.Instance.CurrentScene.Node.QueueFree();
            }
            GD.Print("Changing scene to " + node.Node.Name);
            // TODO: Support this more generally
            GetTree().CurrentScene.AddChild(node.Node);
            NetworkRunner.Instance.CurrentScene = node;
            var networkChildren = (node.Node as NetworkNode3D).GetNetworkChildren(NetworkNode3D.NetworkChildrenSearchToggle.INCLUDE_SCENES).ToList();
            networkChildren.Reverse();
            networkChildren.ForEach(child => child._NetworkPrepare());
            node._NetworkPrepare();
        }

        public IPeerStateController.PeerSyncState GetPeerSyncState(NetPeer peer)
        {
            if (!PeerSyncState.ContainsKey(peer))
            {
                return IPeerStateController.PeerSyncState.INITIAL;
            }
            return PeerSyncState[peer];
        }

        private struct PendingSyncState
        {
            public Tick tick;
            public IPeerStateController.PeerSyncState state;
        }

        readonly private Dictionary<NetPeer, PendingSyncState> pendingSyncStates = [];
        public void SetPeerSyncState(NetPeer peer, IPeerStateController.PeerSyncState state)
        {
            PeerSyncState[peer] = state;
        }

        public void QueuePeerSyncState(NetPeer peer, IPeerStateController.PeerSyncState state)
        {
            pendingSyncStates[peer] = new PendingSyncState
            {
                tick = CurrentTick,
                state = state
            };
        }

        public NetworkNodeWrapper GetNetworkNode(NetworkId networkId)
        {
            if (NetworkRunner.Instance.NetworkScenes.ContainsKey(networkId))
            {
                return NetworkRunner.Instance.NetworkScenes[networkId];
            }
            return null;
        }
        public byte GetPeerNodeId(NetPeer peer, NetworkNodeWrapper node)
        {
            if (node == null) return 0;
            if (!globalNodeToLocalNodeMap.ContainsKey(peer))
            {
                return 0;
            }
            if (!globalNodeToLocalNodeMap[peer].ContainsKey(node.NetworkId))
            {
                return 0;
            }
            return globalNodeToLocalNodeMap[peer][node.NetworkId];
        }

        public NetworkNodeWrapper GetPeerNode(NetPeer peer, byte networkId)
        {
            if (!localNodeToGlobalNodeMap.ContainsKey(peer))
            {
                return null;
            }
            if (!localNodeToGlobalNodeMap[peer].ContainsKey(networkId))
            {
                return null;
            }
            return NetworkRunner.Instance.NetworkScenes[localNodeToGlobalNodeMap[peer][networkId]];
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
                if (globalNodeToLocalNodeMap[peer].ContainsKey(node.NetworkId))
                {
                    availablePeerNodes[peer] &= ~(1 << globalNodeToLocalNodeMap[peer][node.NetworkId]);
                    globalNodeToLocalNodeMap[peer].Remove(node.NetworkId);
                }
            }
            else
            {
                NetworkRunner.Instance.NetworkScenes.Remove(node.NetworkId);
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
                if (globalNodeToLocalNodeMap[peer].ContainsKey(node.NetworkId))
                {
                    return globalNodeToLocalNodeMap[peer][node.NetworkId];
                }
                for (byte i = 0; i < MAX_NETWORK_NODES; i++)
                {
                    byte localNodeId = (byte)(i + 1);
                    if ((availablePeerNodes[peer] & ((long)1 << localNodeId)) == 0)
                    {
                        globalNodeToLocalNodeMap[peer][node.NetworkId] = localNodeId;
                        localNodeToGlobalNodeMap[peer][localNodeId] = node.NetworkId;
                        availablePeerNodes[peer] |= (long)1 << localNodeId;
                        return localNodeId;
                    }
                }

                GD.PrintErr("Peer " + peer + " has reached the maximum amount of nodes.");
                return 0;
            }

            if (NetworkRunner.Instance.NetworkScenes.ContainsKey(node.NetworkId))
            {
                return 0;
            }

            NetworkRunner.Instance.NetworkScenes[node.NetworkId] = node;
            return 1;
        }

        public bool is_loading = true;

        private static NetworkPeerManager _instance;
        public static NetworkPeerManager Instance => _instance;

        /// <inheritdoc/>
        public override void _EnterTree()
        {
            if (_instance != null)
            {
                QueueFree();
            }
            _instance = this;
        }

        [Signal]
        public delegate void PlayerJoinedEventHandler(NetPeer peer);

        public void RegisterPlayer(NetPeer peer)
        {
            PeerSyncState[peer] = IPeerStateController.PeerSyncState.INITIAL;
            globalNodeToLocalNodeMap[peer] = [];
            localNodeToGlobalNodeMap[peer] = [];
            availablePeerNodes[peer] = 0;
        }

        public Dictionary<ENetPacketPeer, HLBuffer> ExportState(Godot.Collections.Array<ENetPacketPeer> peers)
        {
            Dictionary<NetPeer, HLBuffer> peerBuffers = [];
            foreach (ENetPacketPeer peer in peers)
            {
                long updatedNodes = 0;
                peerBuffers[peer] = new HLBuffer();
                var peerNodesBuffers = new Dictionary<long, HLBuffer>();
                var peerNodesSerializersList = new Dictionary<long, byte>();
                foreach (var node in NetworkRunner.Instance.NetworkScenes.Values)
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
                    byte localNodeId = globalNodeToLocalNodeMap[peer][node.NetworkId];
                    updatedNodes |= 1 << localNodeId;
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

            foreach (var node in NetworkRunner.Instance.NetworkScenes.Values)
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
                NetworkRunner.Instance.NetworkScenes.TryGetValue(localNodeId, out NetworkNodeWrapper node);
                if (node == null) {
                    var blankScene = new NetworkNode3D
                    {
                        NetworkId = localNodeId
                    };
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
            if (pendingSyncStates.TryGetValue(peer, out PendingSyncState pendingSyncState))
            {
                if (pendingSyncState.tick <= tick)
                {
                    PeerSyncState[peer] = pendingSyncState.state;
                    pendingSyncStates.Remove(peer);
                }
            }
            foreach (var node in NetworkRunner.Instance.NetworkScenes.Values)
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
            if (incomingTick <= NetworkRunner.Instance.CurrentTick)
            {
                return;
            }
            // GD.Print("INCOMING DATA: " + BitConverter.ToString(HLBytes.Decompress(stateBytes)));
            NetworkRunner.Instance.CurrentTick = incomingTick;
            ImportState(new HLBuffer(stateBytes));
            foreach (var net_id in NetworkRunner.Instance.NetworkScenes.Keys)
            {
                var node = NetworkRunner.Instance.NetworkScenes[net_id];
                if (node == null)
                    continue;
                if (node.Node.IsQueuedForDeletion())
                {
                    NetworkRunner.Instance.NetworkScenes.Remove(net_id);
                    continue;
                }
                node._NetworkProcess(CurrentTick);

                foreach (var wrapper in node.StaticNetworkChildren)
                {
                    if (wrapper == null || wrapper.Node.IsQueuedForDeletion())
                    {
                        continue;
                    }
                    wrapper._NetworkProcess(CurrentTick);
                }
            }
            HLBuffer buffer = new HLBuffer();
            HLBytes.Pack(buffer, incomingTick);
            NetworkRunner.Instance.ENetHost.Send(1, buffer.bytes, (int)ENetPacketPeer.FlagUnsequenced);
        }
    }
}