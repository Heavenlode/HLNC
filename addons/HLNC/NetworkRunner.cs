global using NetworkId = System.Int64;
global using PeerId = System.Int64;
global using Tick = System.Int32;

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

        /// <summary>
        /// Singleton access for C#.
        /// 
        /// This allows the class to be added as a Godot Autoload and utilized as a C# singleton.
        /// In GDScript, access NetworkRunner as a normal singleton without using this field.
        /// </summary>
        public static NetworkRunner Instance { get; private set; }

        public const int FRAMES_PER_SECOND = 60;
        public const int FRAMES_PER_TICK = 2;

        /// <summary>
        /// Ticks per second. The number of server-side network ticks occur within a second.
        /// </summary>
        public const int TPS = FRAMES_PER_SECOND / FRAMES_PER_TICK;

        /// <summary>
        /// Maximum transfer unit, in bits. The number of bits the server can send in a single Tick to a client before it truncates serialized data.
        /// 1400 is generally considered a safe maximum number for all network devices, including mobile.
        /// </summary>
        public const int MTU = 1400;

        /// <summary>
        /// The current tick for the server and client.
        /// </summary>
        public int CurrentTick { get; internal set; }

        /// <summary>
        /// Indicates whether this NetworkRunner is a server. If false, it is a client, or the network runner hasn't yet started via <see cref="StartServer()"/> / <see cref="StartClient()"/>.
        /// </summary>
        public bool IsServer { get; private set; }

        /// <summary>
        /// Indicates whether the network connection has been established. Network is started via <see cref="StartServer()"/> or <see cref="StartClient()"/>. Once started, <see cref="NetworkNode3D._NetworkProcess(Tick)"/> will begin running per tick.
        /// </summary>
        public bool NetStarted { get; private set; }

        /// <summary>
        /// The nodes registered with the network. For the server, this is every NetworkNode within the entire root scene tree. For the client, this is every NetworkNode that they are aware of.
        /// </summary>
        public System.Collections.Generic.Dictionary<NetworkId, NetworkNode3D> NetworkNodes = [];

        /// <summary>
        /// The current root Network Scene. The server automatically keeps clients synchronized with this.
        /// </summary>
        public NetworkNode3D CurrentScene { get; internal set; }

        /// <summary>
        /// The current client ID.
        /// </summary>
        public long LocalPlayerId => MultiplayerInstance.GetUniqueId();

        private int networkIdCounter = 0;

        internal ENetMultiplayerPeer NetPeer = new();
        internal MultiplayerApi MultiplayerInstance;
        internal Godot.Collections.Dictionary<PeerId, Godot.Collections.Dictionary<byte, Godot.Collections.Dictionary<int, Variant>>> InputStore { get; private set; }
        
        public override void _EnterTree()
        {
            if (Instance != null)
            {
                QueueFree();
            }
            Instance = this;
        }
        internal void DebugPrint(string msg)
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

        internal void RegisterSpawn(NetworkNode3D node)
        {
            if (IsServer)
            {
                networkIdCounter += 1;
                while (NetworkNodes.ContainsKey(networkIdCounter))
                {
                    networkIdCounter += 1;
                }
                NetworkNodes[networkIdCounter] = node;
                node.NetworkId = networkIdCounter;
                return;
            }

            if (!node.DynamicSpawn)
            {
                node.QueueFree();
            }

        }

        /// <summary>
        /// Start the server and begin listening for client connections. Upon success, <see cref="NetStarted"/> is set to true and <see cref="NetworkNode3D._NetworkProcess(Tick)"/> begins firing for all network nodes per tick.
        /// </summary>
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
            NetStarted = true;
            DebugPrint("Started");
        }

        /// <summary>
        /// Start the client and attempt to connect to the server. Upon success, <see cref="NetStarted"/> is set to true and <see cref="NetworkNode3D._NetworkProcess(Tick)"/> begins firing for all network nodes per tick received from the server.
        /// </summary>
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
            NetStarted = true;
            DebugPrint("Started");
        }

        private int frame_counter = 0;
        internal void ServerProcessTick()
        {
            var peers = Instance.MultiplayerInstance.GetPeers();
            foreach (var net_id in NetworkNodes.Keys)
            {
                var node = NetworkNodes[net_id];
                if (node == null)
                    continue;
                if (node.IsQueuedForDeletion())
                {
                    NetworkNodes.Remove(net_id);
                    continue;
                }
                node._NetworkProcess(CurrentTick);
                foreach (var networkChild in node.NetworkChildren)
                {
                    if (networkChild.HasMethod("_NetworkProcess"))
                    {
                        networkChild.Call("_NetworkProcess", CurrentTick);
                    }
                    else if (networkChild.HasMethod("_network_process"))
                    {
                        networkChild.Call("_network_process", CurrentTick);
                    }
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

        public void _OnPeerDisconnected(long peerId)
        {
            DebugPrint($"Peer disconnected peerId: {peerId}");
        }

        public void ChangeScene(PackedScene scene)
        {
            if (!IsServer) return;
            var node = (NetworkNode3D)scene.Instantiate();
            CurrentScene?.QueueFree();
            node.DynamicSpawn = true;
            GetTree().CurrentScene.AddChild(node);
            CurrentScene = node;
        }

        public void Spawn(NetworkNode3D node, NetworkNode3D parent = null, string nodePath = ".")
        {
            if (!IsServer) return;

            node.DynamicSpawn = true;
            node.NetworkParent = parent;
            if (parent == null)
            {
                CurrentScene.GetNode(nodePath).AddChild(node);
            }
            else
            {
                parent.GetNode(nodePath).AddChild(node);
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

            if (!InputStore.ContainsKey(sender))
            {
                InputStore[sender] = [];
            }

            if (!InputStore[sender].ContainsKey(networkId))
            {
                InputStore[sender][networkId] = [];
            }

            InputStore[sender][networkId] = incomingInput;
        }
    }
}