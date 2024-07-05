global using NetworkId = System.Int64;
global using NetPeer = Godot.ENetPacketPeer;
global using Tick = System.Int32;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using HLNC.Serialization;
using System;
using System.Diagnostics;

namespace HLNC
{
    /// <summary>
    /// The primary network manager for server and client.
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

        public string ZoneInstanceId => arguments.ContainsKey("zoneInstanceId") ? arguments["zoneInstanceId"] : "0";

        internal ENetConnection ENet;
        internal ENetPacketPeer ENetHost;
        public bool IsServer { get; private set; }
        public bool NetStarted { get; private set; }

        public NetworkNodeWrapper CurrentScene = new NetworkNodeWrapper(null);

        // public PackedScene DebugScene = (PackedScene)GD.Load("res://addons/HLNC/NetworkDebug.tscn");

        public int NetworkId_counter = 0;
        public System.Collections.Generic.Dictionary<NetworkId, NetworkNodeWrapper> NetworkScenes = [];
        public System.Collections.Generic.Dictionary<NetPeer, Array<NetworkId>> net_ids_memo = [];
        private Godot.Collections.Dictionary<NetPeer, Godot.Collections.Dictionary<byte, Godot.Collections.Dictionary<int, Variant>>> inputStore = [];
        public Godot.Collections.Dictionary<NetPeer, Godot.Collections.Dictionary<byte, Godot.Collections.Dictionary<int, Variant>>> InputStore => inputStore;

        private static NetworkRunner _instance;
        public static NetworkRunner Instance => _instance;
        public override void _EnterTree()
        {
            if (_instance != null)
            {
                QueueFree();
            }
            _instance = this;
        }
        public void DebugPrint(string msg)
        {
            GD.Print($"{(IsServer ? "Server" : "Client")}: {msg}");
        }

        System.Collections.Generic.Dictionary<string, string> arguments = [];

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
            DebugPrint("Started");
        }

        public int frame_counter = 0;
        public const int FRAMES_PER_SECOND = 60;
        public const int FRAMES_PER_TICK = 2;
        public static int TPS = FRAMES_PER_SECOND / FRAMES_PER_TICK;
        public const int MTU = 1400;

        public int CurrentTick = 0;

        public System.Collections.Generic.Dictionary<object, object> network_properties_cache = [];

        public Array<Variant> debug_data_sizes = [];
        public System.Collections.Generic.Dictionary<object, object> debug_player_ping = [];

        [Signal]
        public delegate void OnAfterNetworkTickEventHandler(Tick tick);

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
                // DebugPrint(BitConverter.ToString(buffer.bytes));

                peer.Send(1, buffer.bytes, (int)ENetPacketPeer.FlagUnsequenced);
            }
        }

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
                        if (IsServer) {
                            if (channel == 1) {
                                // Peer is acknowledging a Tick
                                var tick = HLBytes.UnpackInt32(data);
                                NetworkPeerManager.Instance.PeerAcknowledge(packetPeer, tick);
                            }
                        } else {
                            if (channel == 1) {
                                if (data.bytes.Length == 0)
                                {
                                    // DebugPrint("Empty data received");
                                    break;
                                }
                                var tick = HLBytes.UnpackInt32(data);
                                var bytes = HLBytes.UnpackByteArray(data);
                                NetworkPeerManager.Instance.ClientHandleTick(tick, bytes);
                            }
                        }
                        break;
                }
                enetEvent = ENet.Service();
            }

            if (IsServer)
            {
                frame_counter += 1;
                if (frame_counter < FRAMES_PER_TICK)
                    return;
                frame_counter = 0;
                CurrentTick += 1;
                ServerProcessTick();
                EmitSignal("OnAfterNetworkTick", CurrentTick);
            }
        }

        [Signal]
        public delegate void PlayerConnectedEventHandler();

        public void _OnPeerConnected(NetPeer peer)
        {
            DebugPrint($"Peer {peer} joined");

            foreach (var node in GetTree().GetNodesInGroup("global_interest"))
            {
                if (node is NetworkNode3D networkNode)
                    networkNode.Interest[peer] = true;
            }
            NetworkPeerManager.Instance.RegisterPlayer(peer);

            if (IsServer)
            {
                EmitSignal("PlayerConnected", peer);
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