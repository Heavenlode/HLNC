global using VariableId = System.Int32;
global using NetworkId = System.Int64;
global using PeerId = System.Int64;
global using Tick = System.Int32;

using Godot;
using Godot.Collections;
using System.Linq;
using System;
using System.Diagnostics;

namespace HLNC
{
	public partial class NetworkRunner : Node
	{
		[Export] public string ServerAddress = "127.0.0.1";
		[Export] public int Port = 8888;
		[Export] public int MaxPeers = 5;

		public ENetMultiplayerPeer NetPeer = new ENetMultiplayerPeer();
		public MultiplayerApi MultiplayerInstance;

		private bool _isServer = false;
		public bool IsServer => _isServer;

		private bool _netStarted = false;
		public bool NetStarted => _netStarted;

		// public PackedScene DebugScene = (PackedScene)GD.Load("res://addons/HLNC/NetworkDebug.tscn");

		public int NetworkId_counter = 0;
		public System.Collections.Generic.Dictionary<NetworkId, NetworkNode3D> NetworkNodes = new System.Collections.Generic.Dictionary<NetworkId, NetworkNode3D>();
		public System.Collections.Generic.Dictionary<PeerId, Array<NetworkId>> net_ids_memo = new System.Collections.Generic.Dictionary<PeerId, Array<NetworkId>>();
		public long LocalPlayerId
		{
			get { return MultiplayerInstance.GetUniqueId(); }
		}

		public readonly Godot.Collections.Dictionary<PeerId, Godot.Collections.Dictionary<int, Variant>> InputStore = new Godot.Collections.Dictionary<PeerId, Godot.Collections.Dictionary<int, Variant>>();

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


		public NetworkDebug network_debug;

		Dictionary<string, string> arguments = new Dictionary<string, string>();

		public void _ready()
		{
			foreach (var argument in OS.GetCmdlineArgs())
			{
				if (argument.Contains("="))
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

		public void RegisterStaticSpawn(NetworkNode3D node)
		{
			if (IsServer) {
				NetworkId_counter += 1;
				while (NetworkNodes.ContainsKey(NetworkId_counter))
				{
					NetworkId_counter += 1;
				}
				NetworkNodes[NetworkId_counter] = node;
				node.NetworkId = NetworkId_counter;
				return;
			}

			node.QueueFree();
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
		public const int TPS = FRAMES_PER_SECOND / FRAMES_PER_TICK;
		public const int MTU = 1400;

		public int CurrentTick = 0;

		public System.Collections.Generic.Dictionary<object, object> network_properties_cache = new System.Collections.Generic.Dictionary<object, object>();

		public Array<Variant> debug_data_sizes = new Array<Variant>();
		public System.Collections.Generic.Dictionary<object, object> debug_player_ping = new System.Collections.Generic.Dictionary<object, object>();

		public void ProcessDebugData()
		{
			// if (network_debug == null)
			// 	return;
			// if (debug_data_sizes.Count >= TPS)
			// {
			// 	var bytes_per_second = debug_data_sizes.Sum();
			// 	var largest_tick_value = debug_data_sizes.Max();
			// 	network_debug.log(new Array<object> { NetworkDebug.Message.BYTES_PER_SECOND, bytes_per_second, largest_tick_value });
			// 	debug_data_sizes.Clear();
			// }
			// foreach (var peerId in debug_player_ping.Keys)
			// {
			// 	var ping = debug_player_ping[peerId];
			// 	if (ping.Count >= TPS)
			// 	{
			// 		var avg_ping = ping.Sum() / TPS;
			// 		network_debug.log(new Array<object> { NetworkDebug.Message.PING, peerId, avg_ping });
			// 		debug_player_ping[peerId].Clear();
			// 	}
			// }
		}

		public void ServerProcessTick()
		{
			var peers = NetworkRunner.Instance.MultiplayerInstance.GetPeers();
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
					networkChild._NetworkProcess(CurrentTick);
				}
			}

			var exportedState = NetworkStateManager.Instance.ExportState(peers, CurrentTick);
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
					NetworkStateManager.Instance.RpcId(peerId, "Tick", CurrentTick, compressed_payload);
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
				ProcessDebugData();
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
				if (node is NetworkNode3D)
					((NetworkNode3D)node).interest[peerId] = true;
			}
			NetworkStateManager.Instance.RegisterPlayer(peerId);

			if (IsServer) {
				EmitSignal("PlayerConnected", peerId);
			}
		}

		public void _OnPeerDisconnected(long peerId)
		{
			DebugPrint($"Peer disconnected peerId: {peerId}");
		}

		public Array<NetworkNode3D> GetAllNetworkNodes(Node node)
		{
			Array<NetworkNode3D> nodes = new Array<NetworkNode3D>();
			if (node is NetworkNode3D)
				nodes.Add((NetworkNode3D)node);
			foreach (Node N in node.GetChildren())
			{
				Array<NetworkNode3D> childNodes = GetAllNetworkNodes(N);
				foreach (NetworkNode3D childNode in childNodes)
				{
					if (childNode is NetworkNode3D)
						nodes.Add(childNode);
				}
			}
			return nodes;
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, TransferChannel = 1)]
		public void TransferInput(int tick, Godot.Collections.Dictionary<int, Variant> incoming_input)
		{
			var sender = MultiplayerInstance.GetRemoteSenderId();

			if (!InputStore.ContainsKey(sender))
				InputStore[sender] = new Godot.Collections.Dictionary<int, Variant>() { };

			foreach (var key in incoming_input.Keys)
			{
				var incoming = incoming_input[key];

				if (incoming.VariantType == Variant.Type.Vector2 || incoming.VariantType == Variant.Type.Vector3)
				{
					incoming = ((Vector3)incoming).Normalized();
				}

				InputStore[sender][key] = incoming;
			}
		}
	}
}