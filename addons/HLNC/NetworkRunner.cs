global using NetworkId = System.Int64;
global using NetPeer = Godot.ENetPacketPeer;
global using Tick = System.Int32;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HLNC.Serialization;
using System;

namespace HLNC
{
    /// <summary>
    /// The primary network manager for server and client. NetworkRunner handles the ENet stream and passing that data to the correct objects. For more information on what kind of data is sent and received on what channels, see <see cref="ENetChannelId"/>.
    /// </summary>
    public partial class NetworkRunner : Node
    {
        /// <summary>
        /// A fully qualified domain (www.example.com) or IP address (192.168.1.1) of the host. Used for client connections.
        /// </summary>
        [Export] public string ServerAddress = "127.0.0.1";

        /// <summary>
        /// The port for the server to listen on, and the client to connect to.
        /// </summary>
        [Export] public int Port = 8888;

        /// <summary>
        /// The maximum number of allowed connections before the server starts rejecting clients.
        /// </summary>
        [Export] public int MaxPeers = 5;

        /// <summary>
        /// The current Zone ID. This is mainly used for Blastoff.
        /// </summary>
        public string ZoneInstanceId => arguments.ContainsKey("zoneInstanceId") ? arguments["zoneInstanceId"] : "0";

        internal ENetConnection ENet;
        internal ENetPacketPeer ENetHost;

        /// <summary>
        /// This is set after <see cref="StartClient"/> or <see cref="StartServer"/> is called, i.e. when <see cref="NetStarted"/> == true. Before that, this value is unreliable.
        /// </summary>
        public bool IsServer { get; private set; }

        /// <summary>
        /// This is set to true once <see cref="StartClient"/> or <see cref="StartServer"/> have succeeded.
        /// </summary>
        public bool NetStarted { get; private set; }
        
        internal IBlastoffServerDriver BlastoffServer { get; private set; }
        internal IBlastoffClientDriver BlastoffClient { get; private set; }

        /// <summary>
        /// These are commands which the server may send to Blastoff, which informs Blastoff how to act upon the client connection.
        /// </summary>
        public enum BlastoffCommands {

            /// <summary>
            /// Requests Blastoff to create a new server instance, i.e. of the game.
            /// </summary>
            NewInstance = 0,

            /// <summary>
            /// Informs Blastoff that the client is valid and communication may be bridged.
            /// </summary>
            ValidateClient = 1,

            /// <summary>
            /// Requests Blastoff to redirect the user to another zone Id.
            /// </summary>
            RedirectClient = 2,

            /// <summary>
            /// Requests Blastoff to disconnect the client.
            /// </summary>
            InvalidClient = 3,
        }
        internal HashSet<NetPeer> BlastoffPendingValidation = new HashSet<NetPeer>();
        internal Guid ZoneId = Guid.Empty;
        
        /// <summary>
        /// Describes the channels of communication used by the network.
        /// </summary>
        public enum ENetChannelId {

            /// <summary>
            /// Tick data sent by the server to the client, and from the client indicating the most recent tick it has received.
            /// </summary>
            Tick = 1,

            /// <summary>
            /// Input data sent from the client.
            /// </summary>
            Input = 2,

            /// <summary>
            /// Client data sent to the server to authenticate themselves and connect to a zone.
            /// </summary>
            ClientAuth = 3,

            /// <summary>
            /// Server communication with Blastoff. Data sent to this channel from a client will be ignored by Blastoff.
            /// </summary>
            BlastoffAdmin = 254,
        }

        /// <summary>
        /// The currently active root network scene. This should only be set via <see cref="ChangeSceneInstance(NetworkNodeWrapper)"/> or <see cref="ChangeScenePacked(PackedScene)"/>.
        /// </summary>
        public NetworkNodeWrapper CurrentScene = new NetworkNodeWrapper(null);

        internal int NetworkId_counter = 0;
        internal System.Collections.Generic.Dictionary<NetworkId, NetworkNodeWrapper> NetworkScenes = [];
        private Godot.Collections.Dictionary<NetPeer, Godot.Collections.Dictionary<byte, Godot.Collections.Dictionary<int, Variant>>> inputStore = [];
        public Godot.Collections.Dictionary<NetPeer, Godot.Collections.Dictionary<byte, Godot.Collections.Dictionary<int, Variant>>> InputStore => inputStore;

        private static NetworkRunner _instance;

        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static NetworkRunner Instance => _instance;
        
        /// <inheritdoc/>
        public override void _EnterTree()
        {
            if (_instance != null)
            {
                QueueFree();
            }
            _instance = this;
        }
        internal void DebugPrint(string msg)
        {
            GD.Print($"{(IsServer ? "Server" : "Client")}: {msg}");
        }

        System.Collections.Generic.Dictionary<string, string> arguments = [];

        /// <inheritdoc/>
        public override void _Ready()
        {
            foreach (var argument in OS.GetCmdlineArgs())
            {
                if (argument.Contains('='))
                {
                    var keyValuePair = argument.Split("=");
                    arguments[keyValuePair[0].TrimStart('-')] = keyValuePair[1];
                }
                else
                {
                    // Options without an argument will be present in the dictionary,
                    // with the value set to an empty string.
                    arguments[argument.TrimStart('-')] = "";
                }
            }
        }

        public void RegisterSpawn(NetworkNodeWrapper wrapper)
        {
            if (IsServer)
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

        public void InstallBlastoffServerDriver(IBlastoffServerDriver blastoff)
        {
            if (!OS.HasFeature("dedicated_server"))
            {
                DebugPrint("Incorrectly installing Blastoff server driver on client.");
                return;
            }
            BlastoffServer = blastoff;
            GD.Print("Server: Blastoff Installed");
        }

        public void InstallBlastoffClientDriver(IBlastoffClientDriver blastoff)
        {
            if (OS.HasFeature("dedicated_server"))
            {
                DebugPrint("Incorrectly installing Blastoff client driver on server.");
                return;
            }
            BlastoffClient = blastoff;
            GD.Print("Client: Blastoff Installed");
        }

        public void StartServer()
        {
            IsServer = true;
            DebugPrint("Starting Server");
            GetTree().MultiplayerPoll = false;
            var custom_port = Port;
            if (arguments.ContainsKey("port"))
            {
                GD.Print("PORT OVERRIDE: {0}", arguments["port"]);
                custom_port = int.Parse(arguments["port"]);
            }

            if (arguments.ContainsKey("zoneId")) {
                ZoneId = new Guid(arguments["zoneId"]);
                DebugPrint($"Zone ID: {ZoneId}");
            }

            ENet = new ENetConnection();
            var err = ENet.CreateHostBound(ServerAddress, custom_port, MaxPeers);
            ENet.Compress(ENetConnection.CompressionMode.RangeCoder);
            if (err != Error.Ok)
            {
                DebugPrint($"Error starting: {err}");
                return;
            }
            NetStarted = true;
            DebugPrint("Started");
        }

        public void StartClient()
        {
            ENet = new ENetConnection();
            ENet.CreateHost();
            ENetHost = ENet.ConnectToHost(ServerAddress, 8888);
            ENet.Compress(ENetConnection.CompressionMode.RangeCoder);
            // ENetHost = ENet.ConnectToHost(ServerAddress, 20406);
            if (ENetHost == null)
            {
                DebugPrint($"Error connecting.");
                return;
            }
            NetStarted = true;
            if (BlastoffClient != null) {
                var zoneBytes = BlastoffClient.BlastoffGetZoneId().ToByteArray();
                var tokenBytes = System.Text.Encoding.UTF8.GetBytes(BlastoffClient.BlastoffGetToken());
                var combinedBytes = new byte[zoneBytes.Length + tokenBytes.Length];
                zoneBytes.CopyTo(combinedBytes, 0);
                tokenBytes.CopyTo(combinedBytes, zoneBytes.Length);
                ENetHost.Send((int)ENetChannelId.ClientAuth, combinedBytes, (int)ENetPacketPeer.FlagReliable);
            }
            DebugPrint("Started");
        }

        /// <summary>
        /// This determines how fast the network sends data. When physics runs at 60 ticks per second, then at 2 PhysicsTicksPerNetworkTick, the network runs at 30hz.
        /// </summary>
        public const int PhysicsTicksPerNetworkTick = 2;

        /// <summary>
        /// Ticks Per Second. The number of Ticks which are expected to elapse every second.
        /// </summary>
        public static int TPS = Engine.PhysicsTicksPerSecond / PhysicsTicksPerNetworkTick;

        /// <summary>
        /// Maximum Transferrable Unit. The maximum number of bytes that should be sent in a single ENet UDP Packet (i.e. a single tick)
        /// Not a hard limit.
        /// </summary>
        public const int MTU = 1400;

        /// <summary>
        /// The current network tick. On the client side, this does not represent the server's current tick, which will always be slightly ahead.
        /// </summary>
        public int CurrentTick { get; internal set; } = 0;

        [Signal]
        public delegate void OnAfterNetworkTickEventHandler(Tick tick);


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
                foreach (var networkChild in networkNode.StaticNetworkChildren)
                {
                    networkChild._NetworkProcess(CurrentTick);
                }
            }

            var exportedState = NetworkPeerManager.Instance.ExportState(ENet.GetPeers());
            foreach (var peer in ENet.GetPeers())
            {
                var size = exportedState[peer].bytes.Length;
                // if (network_debug != null)
                // {
                // 	debug_data_sizes.Add(compressed_payload.Length);
                // }
                if (size > MTU)
                {
                    DebugPrint($"Warning: Data size {size} exceeds MTU {MTU}");
                }

                var buffer = new HLBuffer();
                HLBytes.Pack(buffer, CurrentTick);
                HLBytes.Pack(buffer, exportedState[peer].bytes, true);

                peer.Send(1, buffer.bytes, (int)ENetPacketPeer.FlagUnsequenced);
            }
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(double delta)
        {
            if (!NetStarted)
                return;

            var enetEvent = ENet.Service();

            while (true)
            {
                var eventType = enetEvent[0].As<ENetConnection.EventType>();
                if (eventType == ENetConnection.EventType.None)
                {
                    break;
                }
                var packetPeer = enetEvent[1].As<ENetPacketPeer>();
                switch (eventType)
                {
                    case ENetConnection.EventType.Connect:
                        _OnPeerConnected(packetPeer);
                        break;
                    case ENetConnection.EventType.Disconnect:
                        _OnPeerDisconnected(packetPeer);
                        break;
                    case ENetConnection.EventType.Receive:
                        var data = new HLBuffer(packetPeer.GetPacket());
                        var channel = enetEvent[3].As<int>();
                        switch ((ENetChannelId)channel) {
                            case ENetChannelId.Tick:
                                if (IsServer) {
                                    var tick = HLBytes.UnpackInt32(data);
                                    NetworkPeerManager.Instance.PeerAcknowledge(packetPeer, tick);
                                } else {
                                    if (data.bytes.Length == 0)
                                    {
                                        break;
                                    }
                                    var tick = HLBytes.UnpackInt32(data);
                                    var bytes = HLBytes.UnpackByteArray(data);
                                    NetworkPeerManager.Instance.ClientHandleTick(tick, bytes);
                                }
                            break;
                            case ENetChannelId.BlastoffAdmin:
                                if (IsServer) {
                                    if (BlastoffServer == null) {
                                        // This channel is only used for Blastoff which must be enabled.
                                        break;
                                    }
                                    if (BlastoffPendingValidation.Contains(packetPeer)) {
                                        // We're in the process of validating a peer for Blastoff.
                                        // The packet is two parts: a zone UUID, and a token
                                        var zoneId = new Guid(data.bytes[0..32]);
                                        var token = System.Text.Encoding.UTF8.GetString(data.bytes[32..]);
                                        if (BlastoffServer.BlastoffValidatePeer(zoneId, token, out var redirect)) {
                                            _validatePeerConnected(packetPeer);
                                            packetPeer.Send((int)ENetChannelId.BlastoffAdmin, [(byte)BlastoffCommands.ValidateClient], (int)ENetPacketPeer.FlagReliable);
                                        } else {
                                            // TODO: If redirect is not Guid.Empty or null, we should redirect the client to that zone
                                            packetPeer.Send((int)ENetChannelId.BlastoffAdmin, [(byte)BlastoffCommands.InvalidClient], (int)ENetPacketPeer.FlagReliable);
                                        }
                                    }
                                } else {
                                    // Clients should never receive messages on the Blastoff channel
                                    break;
                                }
                            break;
                            
                        }
                        break;
                }
                enetEvent = ENet.Service();
            }

            if (IsServer)
            {
                _frameCounter += 1;
                if (_frameCounter < PhysicsTicksPerNetworkTick)
                    return;
                _frameCounter = 0;
                CurrentTick += 1;
                ServerProcessTick();
                EmitSignal("OnAfterNetworkTick", CurrentTick);
            }
        }

        [Signal]
        public delegate void PlayerConnectedEventHandler();

        internal void _validatePeerConnected(NetPeer peer) {
            foreach (var node in GetTree().GetNodesInGroup("global_interest"))
            {
                if (node is NetworkNode3D networkNode)
                    networkNode.Interest[peer] = true;
            }
            NetworkPeerManager.Instance.RegisterPlayer(peer);

            EmitSignal("PlayerConnected", peer);
        }

        public void _OnPeerConnected(NetPeer peer)
        {
            DebugPrint($"Peer {peer} joined");

            if (!IsServer)
            {
                return;
            }

            if (BlastoffServer != null) {
                BlastoffPendingValidation.Add(peer);
            } else {
                _validatePeerConnected(peer);
            }
        }

        public void ChangeScenePacked(PackedScene scene)
        {
            // This allows us to change scenes without using Godot's built-in scene changer
            // We do this because Godot's scene changer doesn't work well with networked scenes
            if (!IsServer) return;
            var node = new NetworkNodeWrapper((NetworkNode3D)scene.Instantiate());
            ChangeSceneInstance(node);
        }

        public void ChangeSceneInstance(NetworkNodeWrapper node)
        {
            if (!IsServer) return;
            if (CurrentScene.Node != null) {
                CurrentScene.Node.QueueFree();
            }
            node.DynamicSpawn = true;
            // TODO: Support this more generally
            GetTree().CurrentScene.AddChild(node.Node);
            CurrentScene = node;
            var networkChildren = (node.Node as NetworkNode3D).GetNetworkChildren(NetworkNode3D.NetworkChildrenSearchToggle.INCLUDE_SCENES).ToList();
            networkChildren.Reverse();
            networkChildren.ForEach(child => child._NetworkPrepare());
            node._NetworkPrepare();

        }

        public void ChangeZone() {
            if (IsServer) return;
            // var node = new NetworkNodeWrapper(new NetworkNode3D());
            // ChangeSceneInstance(node);
        }

        public void Spawn(NetworkNode3D node, NetworkNode3D parent = null, string nodePath = ".")
        {
            if (!IsServer) return;

            node.DynamicSpawn = true;
            node.NetworkParent = new NetworkNodeWrapper(null);
            if (parent == null)
            {
                CurrentScene.Node.GetNode(nodePath).AddChild(node);
            }
            else
            {
                parent.GetNode(nodePath).AddChild(node);
            }
        }

        public NetworkNodeWrapper GetFromNetworkId(NetworkId network_id)
        {
            if (network_id == -1)
                return new NetworkNodeWrapper(null);
            if (!NetworkScenes.ContainsKey(network_id))
                return new NetworkNodeWrapper(null);
            return NetworkScenes[network_id];
        }

        public void _OnPeerDisconnected(ENetPacketPeer peer)
        {
            DebugPrint($"Peer disconnected peerId: {peer}");
        }

        public IEnumerable<NetworkNode3D> GetAllNetworkNodes(Node node, bool onlyScenes = false)
        {
            if (node is NetworkNode3D networkNode)
            {
                if (!onlyScenes || networkNode.GetMeta("is_network_scene", false).AsBool())
                {
                    yield return networkNode;
                }
            }
            foreach (Node childNode in node.GetChildren())
            {
                foreach (var nestedNode in GetAllNetworkNodes(childNode, onlyScenes))
                {
                    yield return nestedNode;
                }
            }
        }
        public void TransferInput(int tick, byte networkId, Godot.Collections.Dictionary<int, Variant> incomingInput)
        {
            // var sender = MultiplayerInstance.GetRemoteSenderId();
            // var node = NetworkPeerManager.Instance.GetPeerNode(sender, networkId);

            // if (node == null)
            // {
            //     return;
            // }

            // if (sender != node.InputAuthority)
            // {
            //     return;
            // }

            // if (!inputStore.ContainsKey(sender))
            // {
            //     inputStore[sender] = [];
            // }

            // if (!inputStore[sender].ContainsKey(networkId))
            // {
            //     inputStore[sender][networkId] = [];
            // }

            // inputStore[sender][networkId] = incomingInput;
        }
    }
}