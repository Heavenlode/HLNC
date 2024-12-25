global using NetworkId = System.Int64;
global using NetPeer = Godot.ENetPacketPeer;
global using Tick = System.Int32;
using System.Collections.Generic;
using Godot;
using HLNC.Serialization;
using System;
using HLNC.Addons.Blastoff;

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
        /// The port for the server to listen on, and the client to connect to. If BlastoffClient is installed, this will be overridden to 20406, the Blastoff port.
        /// </summary>
        [Export] public int Port = 8888;

        /// <summary>
        /// The maximum number of allowed connections before the server starts rejecting clients.
        /// </summary>
        [Export] public int MaxPeers = 100;

        /// <summary>
        /// The current World ID. This is mainly used for Blastoff.
        /// </summary>
        public Dictionary<Guid, WorldRunner> Worlds { get; private set; } = [];
        internal ENetConnection ENet;
        internal ENetPacketPeer ENetHost;

        internal Dictionary<string, NetPeer> Peers = [];
        internal Dictionary<NetPeer, string> PeerIds = [];
        internal Dictionary<Guid, List<NetPeer>> WorldPeerMap = [];
        internal Dictionary<NetPeer, WorldRunner> PeerWorldMap = [];

        public NetPeer GetPeer(string id)
        {
            if (Peers.ContainsKey(id))
            {
                return Peers[id];
            }
            return null;
        }

        public string GetPeerId(NetPeer peer)
        {
            if (PeerIds.ContainsKey(peer))
            {
                return PeerIds[peer];
            }
            return null;
        }

        /// <summary>
        /// This is set after <see cref="StartClient"/> or <see cref="StartServer"/> is called, i.e. when <see cref="NetStarted"/> == true. Before that, this value is unreliable.
        /// </summary>
        public bool IsServer { get; private set; }

        public bool IsClient => !IsServer;

        /// <summary>
        /// This is set to true once <see cref="StartClient"/> or <see cref="StartServer"/> have succeeded.
        /// </summary>
        public bool NetStarted { get; private set; }

        internal IBlastoffServerDriver BlastoffServer { get; private set; }
        internal IBlastoffClientDriver BlastoffClient { get; private set; }

        /// <summary>
        /// These are commands which the server may send to Blastoff, which informs Blastoff how to act upon the client connection.
        /// </summary>
        public enum BlastoffCommands
        {

            /// <summary>
            /// Requests Blastoff to create a new server instance, i.e. of the game.
            /// </summary>
            NewInstance = 0,

            /// <summary>
            /// Informs Blastoff that the client is valid and communication may be bridged.
            /// </summary>
            ValidateClient = 1,

            /// <summary>
            /// Requests Blastoff to redirect the user to another world Id.
            /// </summary>
            RedirectClient = 2,

            /// <summary>
            /// Requests Blastoff to disconnect the client.
            /// </summary>
            InvalidClient = 3,
        }
        internal HashSet<NetPeer> BlastoffPendingValidation = new HashSet<NetPeer>();

        /// <summary>
        /// Describes the channels of communication used by the network.
        /// </summary>
        public enum ENetChannelId
        {

            /// <summary>
            /// Tick data sent by the server to the client, and from the client indicating the most recent tick it has received.
            /// </summary>
            Tick = 1,

            /// <summary>
            /// Input data sent from the client.
            /// </summary>
            Input = 2,

            /// <summary>
            /// NetworkFunction call.
            /// </summary>
            Function = 3,

            /// <summary>
            /// Server communication with Blastoff. Data sent to this channel from a client will be ignored by Blastoff.
            /// </summary>
            BlastoffAdmin = 249,
        }

        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static NetworkRunner Instance { get; internal set; }

        /// <inheritdoc/>
        public override void _EnterTree()
        {
            if (Instance != null)
            {
                QueueFree();
            }
            Instance = this;
        }
        internal static void DebugPrint(string msg)
        {
            GD.Print($"{(OS.HasFeature("dedicated_server") ? "Server" : "Client")}: {msg}");
        }

        public void InstallBlastoffServerDriver(IBlastoffServerDriver blastoff)
        {
            if (!OS.HasFeature("dedicated_server"))
            {
                DebugPrint("Incorrectly installing Blastoff server driver on client.");
                return;
            }
            BlastoffServer = blastoff;
            DebugPrint("Blastoff Installed");
        }

        public void InstallBlastoffClientDriver(IBlastoffClientDriver blastoff)
        {
            if (OS.HasFeature("dedicated_server"))
            {
                DebugPrint("Incorrectly installing Blastoff client driver on server.");
                return;
            }
            BlastoffClient = blastoff;
            DebugPrint("Blastoff Installed");
        }

        public void StartServer()
        {
            IsServer = true;
            DebugPrint("Starting Server");
            GetTree().MultiplayerPoll = false;

            ENet = new ENetConnection();
            var err = ENet.CreateHostBound(ServerAddress, Port, MaxPeers);
            ENet.Compress(ENetConnection.CompressionMode.RangeCoder);
            if (err != Error.Ok)
            {
                DebugPrint($"Error starting: {err}");
                return;
            }
            NetStarted = true;
            DebugPrint($"Started on {ServerAddress}:{Port}");
        }

        public void StartClient()
        {
            ENet = new ENetConnection();
            ENet.CreateHost();
            ENetHost = ENet.ConnectToHost(ServerAddress, BlastoffClient != null ? 20406 : Port);
            ENet.Compress(ENetConnection.CompressionMode.RangeCoder);
            if (ENetHost == null)
            {
                DebugPrint($"Error connecting.");
                return;
            }
            NetStarted = true;
            var worldRunner = new WorldRunner();
            WorldRunner.CurrentWorld = worldRunner;
            GetTree().CurrentScene.AddChild(worldRunner);
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


        /// <inheritdoc/>
        public override void _PhysicsProcess(double delta)
        {
            if (!NetStarted)
                return;

            while (true)
            {
                var enetEvent = ENet.Service();
                var eventType = enetEvent[0].As<ENetConnection.EventType>();
                if (eventType == ENetConnection.EventType.None)
                {
                    break;
                }
                var packetPeer = enetEvent[1].As<ENetPacketPeer>();
                switch (eventType)
                {
                    case ENetConnection.EventType.Connect:
                        if (packetPeer == ENetHost)
                        {
                            _OnConnectedToServer();
                        }
                        else
                        {
                            _OnPeerConnected(packetPeer);
                        }
                        break;
                    case ENetConnection.EventType.Disconnect:
                        _OnPeerDisconnected(packetPeer);
                        break;
                    case ENetConnection.EventType.Receive:
                        var data = new HLBuffer(packetPeer.GetPacket());
                        var channel = enetEvent[3].As<int>();
                        switch ((ENetChannelId)channel)
                        {
                            case ENetChannelId.Tick:
                                if (IsServer)
                                {
                                    var tick = HLBytes.UnpackInt32(data);
                                    PeerWorldMap[packetPeer].PeerAcknowledge(packetPeer, tick);
                                }
                                else
                                {
                                    if (data.bytes.Length == 0)
                                    {
                                        break;
                                    }
                                    var tick = HLBytes.UnpackInt32(data);
                                    var bytes = HLBytes.UnpackByteArray(data);
                                    // GD.Print(BitConverter.ToString(bytes));
                                    WorldRunner.CurrentWorld.ClientHandleTick(tick, bytes);
                                }
                                break;
                            case ENetChannelId.Input:
                                if (IsServer)
                                {
                                    PeerWorldMap[packetPeer].ReceiveInput(packetPeer, data);
                                }
                                else
                                {
                                    // Clients should never receive messages on the Input channel
                                    break;
                                }
                                break;
                            case ENetChannelId.Function:
                                if (IsServer) {
                                    PeerWorldMap[packetPeer].ReceiveNetworkFunction(packetPeer, data);
                                } else {
                                    WorldRunner.CurrentWorld.ReceiveNetworkFunction(ENetHost, data);
                                }
                                break;
                            case ENetChannelId.BlastoffAdmin:
                                if (IsServer)
                                {
                                    if (BlastoffServer == null)
                                    {
                                        // This channel is only used for Blastoff which must be enabled.
                                        break;
                                    }
                                    if (BlastoffPendingValidation.Contains(packetPeer))
                                    {
                                        // We're in the process of validating a peer for Blastoff.
                                        var token = System.Text.Encoding.UTF8.GetString(data.bytes);
                                        if (BlastoffServer.BlastoffValidatePeer(token, out var worldId))
                                        {
                                            _validatePeerConnected(packetPeer, worldId, token);
                                            packetPeer.Send((int)ENetChannelId.BlastoffAdmin, [(byte)BlastoffCommands.ValidateClient], (int)ENetPacketPeer.FlagReliable);
                                        }
                                        else
                                        {
                                            packetPeer.Send((int)ENetChannelId.BlastoffAdmin, [(byte)BlastoffCommands.InvalidClient], (int)ENetPacketPeer.FlagReliable);
                                        }
                                    }
                                }
                                else
                                {
                                    // Clients should never receive messages on the Blastoff channel
                                    break;
                                }
                                break;

                        }
                        break;
                }
            }
        }

        internal void _validatePeerConnected(NetPeer peer, Guid worldId, string token = "")
        {
            var peerId = Guid.NewGuid().ToString();
            Peers[peerId] = peer;
            PeerIds[peer] = peerId;

            foreach (var node in GetTree().GetNodesInGroup("global_interest"))
            {
                var wrapper = new NetworkNodeWrapper(node);
                if (wrapper == null) continue;
                wrapper.SetPeerInterest(peerId, Int64.MaxValue, true);
            }
            Worlds[worldId].JoinPeer(peer, token);
            BlastoffPendingValidation.Remove(peer);
        }

        private void StartBlastoffNegotiation()
        {
            // We build the UUID as a string because of endian issues... or something
            // This is related to UUID Representation in C#
            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(BlastoffClient.BlastoffGetToken());
            var err = ENetHost.Send((int)ENetChannelId.BlastoffAdmin, tokenBytes, (int)ENetPacketPeer.FlagReliable);
            if (err != Error.Ok)
            {
                DebugPrint($"Error sending Blastoff data: {err}");
            }
        }

        private void _OnConnectedToServer()
        {
            DebugPrint("Connected to server");
            if (BlastoffClient != null)
            {
                StartBlastoffNegotiation();
            }
        }

        private void _OnPeerConnected(NetPeer peer)
        {
            DebugPrint($"Peer {peer} joined");
            if (BlastoffServer != null)
            {
                BlastoffPendingValidation.Add(peer);
            }
            else
            {
                // TODO: Don't use GUID Empty
                _validatePeerConnected(peer, Guid.Empty);
            }
        }

        [Signal]
        public delegate void OnWorldCreatedEventHandler(WorldRunner world);

        public WorldRunner CreateWorldPacked(Guid worldId, PackedScene scene)
        {
            if (!IsServer) return null;
            var node = new NetworkNodeWrapper((NetworkNode3D)scene.Instantiate());
            return SetupWorldInstance(worldId, node);
        }

        public WorldRunner SetupWorldInstance(Guid worldId, NetworkNodeWrapper node, NetworkId initialNetworkId = 0)
        {
            if (!IsServer) return null;
            var godotPhysicsWorld = new SubViewport
            {

                OwnWorld3D = true,
                World3D = new World3D(),
                Name = worldId.ToString()
            };
            var worldRunner = new WorldRunner
            {
                WorldId = worldId,
                RootScene = node,
                NetworkId_counter = initialNetworkId
            };
            Worlds[worldId] = worldRunner;
            WorldPeerMap[worldId] = [];
            // godotPhysicsWorld.ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;

            godotPhysicsWorld.AddChild(worldRunner);
            godotPhysicsWorld.AddChild(node.Node);
            GetTree().CurrentScene.AddChild(godotPhysicsWorld);
            node._NetworkPrepare(worldRunner);
            node._WorldReady();
            EmitSignal("OnWorldCreated", worldRunner);
            return worldRunner;
        }

        public void _OnPeerDisconnected(ENetPacketPeer peer)
        {
            DebugPrint($"Peer disconnected peerId: {peer}");
        }

        public IEnumerable<NetworkNode3D> GetAllNetworkNodes(Node node, bool onlyScenes = false)
        {
            if (node is NetworkNode3D networkNode)
            {
                if (!onlyScenes || NetworkScenesRegister.IsNetworkScene(networkNode.SceneFilePath))
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
    }
}