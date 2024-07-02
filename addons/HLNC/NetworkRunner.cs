global using NetworkId = System.Int64;
global using PeerId = System.Int64;
global using Tick = System.Int32;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using HLNC.Serialization;

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

        public ENetMultiplayerPeer NetPeer = new();
        public MultiplayerApi MultiplayerInstance;

        private bool _isServer = false;
        public bool IsServer => _isServer;

        private bool _netStarted = false;

        /// <summary>
        /// Indicates whether the network connection has been established. Network is started via <see cref="StartServer()"/> or <see cref="StartClient()"/>. Once started, <see cref="NetworkNode3D._NetworkProcess(Tick)"/> will begin running per tick.
        /// </summary>
        public bool NetStarted => _netStarted;

        public NetworkNodeWrapper CurrentScene = new NetworkNodeWrapper(null);

        // public PackedScene DebugScene = (PackedScene)GD.Load("res://addons/HLNC/NetworkDebug.tscn");

        public int NetworkId_counter = 0;
        public System.Collections.Generic.Dictionary<NetworkId, NetworkNodeWrapper> NetworkScenes = [];
        public System.Collections.Generic.Dictionary<PeerId, Array<NetworkId>> net_ids_memo = [];
        public long LocalPlayerId
        {
            get { return MultiplayerInstance.GetUniqueId(); }
        }
        private Godot.Collections.Dictionary<PeerId, Godot.Collections.Dictionary<byte, Godot.Collections.Dictionary<int, Variant>>> inputStore = [];
        public Godot.Collections.Dictionary<PeerId, Godot.Collections.Dictionary<byte, Godot.Collections.Dictionary<int, Variant>>> InputStore => inputStore;

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
            _isServer = true;
            DebugPrint("Starting Server");
            GetTree().MultiplayerPoll = false;
            var custom_port = Port;
            if (arguments.ContainsKey("port"))
            {
                GD.Print("PORT OVERRIDE: {0}", arguments["port"]);
                custom_port = int.Parse(arguments["port"]);
            }
            var err = NetPeer.CreateServer(custom_port, MaxPeers);
            if (err != Error.Ok)
            {
                DebugPrint($"Error starting: {err}");
                return;
            }
            MultiplayerInstance = MultiplayerApi.CreateDefaultInterface();
            MultiplayerInstance.PeerConnected += _OnPeerConnected;
            MultiplayerInstance.PeerDisconnected += _OnPeerDisconnected;
            GetTree().SetMultiplayer(MultiplayerInstance, "/root");
            MultiplayerInstance.MultiplayerPeer = NetPeer;
            _netStarted = true;
            DebugPrint("Started");
        }

        public void StartClient()
        {
            GetTree().MultiplayerPoll = false;
            var err = NetPeer.CreateClient(ServerAddress, Port);
            if (err != Error.Ok)
            {
                DebugPrint($"Error connecting: {err}");
                return;
            }
            MultiplayerInstance = MultiplayerApi.CreateDefaultInterface();
            MultiplayerInstance.PeerConnected += _OnPeerConnected;
            MultiplayerInstance.PeerDisconnected += _OnPeerDisconnected;
            GetTree().SetMultiplayer(MultiplayerInstance, "/root");
            MultiplayerInstance.MultiplayerPeer = NetPeer;
            _netStarted = true;
            DebugPrint("Started");
        }

        public int frame_counter = 0;
        public const int FRAMES_PER_SECOND = 60;
        public const int FRAMES_PER_TICK = 2;

        /// <summary>
        /// Ticks per second. The number of server-side network ticks occur within a second.
        /// </summary>
        public static int TPS = FRAMES_PER_SECOND / FRAMES_PER_TICK;

        /// <summary>
        /// Maximum transfer unit, in bits. The number of bits the server can send in a single Tick to a client before it truncates serialized data.
        /// 1400 is generally considered a safe maximum number for all network devices, including mobile.
        /// </summary>
        public const int MTU = 1400;

        public int CurrentTick = 0;

        public System.Collections.Generic.Dictionary<object, object> network_properties_cache = [];

        public Array<Variant> debug_data_sizes = [];
        public System.Collections.Generic.Dictionary<object, object> debug_player_ping = [];

        [Signal]
        public delegate void OnAfterNetworkTickEventHandler(Tick tick);

        public void ServerProcessTick()
        {
            var peers = Instance.MultiplayerInstance.GetPeers();
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

            var exportedState = NetworkPeerManager.Instance.ExportState(peers);
            foreach (var peerId in MultiplayerInstance.GetPeers())
            {
                if (peerId == 1)
                    continue;
                if (!exportedState.ContainsKey(peerId))
                {
                    GD.PrintErr($"No state to send to peer {peerId}");
                }
                else
                {
                    // GD.Print("SENT STATE for peer " + peerId + " : " + BitConverter.ToString(exportedState[peerId].bytes));
                    var compressed_payload = HLBytes.Compress(exportedState[peerId].bytes);
                    var size = exportedState[peerId].bytes.Length;
                    // if (network_debug != null)
                    // {
                    // 	debug_data_sizes.Add(compressed_payload.Length);
                    // }
                    if (size > MTU)
                    {
                        DebugPrint($"Warning: Data size {size} exceeds MTU {MTU}");
                    }
                    NetworkPeerManager.Instance.RpcId(peerId, "Tick", CurrentTick, compressed_payload);
                }
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!NetStarted)
                return;

            if (MultiplayerInstance.HasMultiplayerPeer())
                MultiplayerInstance.Poll();

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

        public void _OnPeerConnected(long peerId)
        {
            if (!IsServer)
            {
                if (peerId == 1)
                    DebugPrint("Connected to server");
                else
                    DebugPrint("Peer connected to server");
                return;
            }

            DebugPrint($"Peer {peerId} joined");

            foreach (var node in GetTree().GetNodesInGroup("global_interest"))
            {
                if (node is NetworkNode3D networkNode)
                    networkNode.Interest[peerId] = true;
            }
            NetworkPeerManager.Instance.RegisterPlayer(peerId);

            if (IsServer)
            {
                EmitSignal("PlayerConnected", peerId);
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

        public void _OnPeerDisconnected(long peerId)
        {
            DebugPrint($"Peer disconnected peerId: {peerId}");
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

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 1)]
        public void TransferInput(int tick, byte networkId, Godot.Collections.Dictionary<int, Variant> incomingInput)
        {
            var sender = MultiplayerInstance.GetRemoteSenderId();
            var node = NetworkPeerManager.Instance.GetPeerNode(sender, networkId);

            if (node == null)
            {
                return;
            }

            if (sender != node.InputAuthority)
            {
                return;
            }

            if (!inputStore.ContainsKey(sender))
            {
                inputStore[sender] = [];
            }

            if (!inputStore[sender].ContainsKey(networkId))
            {
                inputStore[sender][networkId] = [];
            }

            inputStore[sender][networkId] = incomingInput;
        }
    }
}