using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Godot;
using HLNC.Addons.Lazy;
using HLNC.Serialization;
using HLNC.Utils;
using HLNC.Utils.Bson;

namespace HLNC
{
	[Tool]
	public partial class NetworkController : RefCounted
	{
		public NetworkNodeWrapper Owner { get; internal set; }
		public bool IsNetworkScene => NetworkScenesRegister.IsNetworkScene(Owner.Node.SceneFilePath);

		internal HashSet<NetworkNodeWrapper> NetworkSceneChildren = [];
		internal List<Tuple<string, string>> InitialSetNetworkProperties = [];
		public WorldRunner CurrentWorld { get; internal set; }
		internal Godot.Collections.Dictionary<byte, Variant> InputBuffer = [];
		internal Godot.Collections.Dictionary<byte, Variant> PreviousInputBuffer = [];
		public Godot.Collections.Dictionary<string, long> InterestLayers = [];

		[Signal]
		public delegate void InterestChangedEventHandler(string peerId, long interestLayers);
		public void SetPeerInterest(string peerId, long interestLayers, bool recurse = true)
		{
			InterestLayers[peerId] = interestLayers;
			EmitSignal("InterestChanged", peerId, interestLayers);
			if (recurse)
			{
				foreach (var child in GetNetworkChildren(NetworkChildrenSearchToggle.INCLUDE_SCENES))
				{
					child.SetPeerInterest(peerId, interestLayers, recurse);
				}
			}
		}

		public override void _Notification(int what)
		{
			if (what == NotificationPredelete)
			{
				if (!IsWorldReady) return;
				if (NetworkParent != null && NetworkParent.Node is INetworkNode _networkNodeParent)
				{
					_networkNodeParent.Network.NetworkSceneChildren.Remove(Owner);
				}
			}
		}

		public void SetNetworkInput(byte input, Variant value)
		{
			if (IsNetworkScene)
			{
				InputBuffer[input] = value;
			}
			else
			{
				NetworkParent.SetNetworkInput(input, value);
			}
		}

		public Variant GetNetworkInput(byte input, Variant defaultValue)
		{
			if (IsNetworkScene)
			{
				return InputBuffer.GetValueOrDefault(input, defaultValue);
			}
			else
			{
				return NetworkParent.GetNetworkInput(input, defaultValue);
			}
		}

		public bool IsWorldReady { get; internal set; } = false;

		private NetworkId _networkParentId;
		public NetworkId NetworkParentId
		{
			get
			{
				return _networkParentId;
			}
			set
			{
				{
					if (IsNetworkScene && NetworkParent != null && NetworkParent.Node is INetworkNode _networkNodeParent)
					{
						_networkNodeParent.Network.NetworkSceneChildren.Remove(Owner);
					}
				}
				_networkParentId = value;
				{
					if (IsNetworkScene && value != 0 && CurrentWorld.GetNodeFromNetworkId(value).Node is INetworkNode _networkNodeParent)
					{
						_networkNodeParent.Network.NetworkSceneChildren.Add(Owner);
					}
				}
			}
		}
		public NetworkNodeWrapper NetworkParent
		{
			get
			{
				if (CurrentWorld == null) return null;
				return CurrentWorld.GetNodeFromNetworkId(NetworkParentId);
			}
			internal set
			{
				NetworkParentId = value?.NetworkId ?? 0;
			}
		}
		public bool IsClientSpawn { get; internal set; } = false;
		public NetworkController(Node owner)
		{
			if (owner is not INetworkNode)
			{
				Debugger.Log($"Node {owner.GetPath()} does not implement INetworkNode", Debugger.DebugLevel.ERROR);
				return;
			}
			Owner = new NetworkNodeWrapper(owner);
		}

		[Signal]
		public delegate void NetworkPropertyChangedEventHandler(string nodePath, StringName propertyName);

		[NetworkProperty(InterestMask = 0, Subtype = VariantSubtype.NetworkId)]
		public NetworkId NetworkId { get; internal set; } = -1;

		public NetPeer InputAuthority { get; internal set; } = null;

		public bool IsCurrentOwner
		{
			get { return NetworkRunner.Instance.IsServer || (NetworkRunner.Instance.IsClient && InputAuthority == NetworkRunner.Instance.ENetHost); }
		}

		public static INetworkNode FindFromChild(Node node)
		{
			while (node != null)
			{
				if (node is INetworkNode networkNode)
					return networkNode;
				node = node.GetParent();
			}
			return null;
		}

		public enum NetworkChildrenSearchToggle { INCLUDE_SCENES, EXCLUDE_SCENES, ONLY_SCENES }
		public IEnumerable<NetworkNodeWrapper> GetNetworkChildren(NetworkChildrenSearchToggle searchToggle = NetworkChildrenSearchToggle.EXCLUDE_SCENES, bool nestedSceneChildren = true)
		{
			var children = Owner.Node.GetChildren();
			while (children.Count > 0)
			{
				var child = children[0];
				children.RemoveAt(0);
				var isNetworkScene = NetworkScenesRegister.IsNetworkScene(child.SceneFilePath);
				if (isNetworkScene && searchToggle == NetworkChildrenSearchToggle.EXCLUDE_SCENES)
				{
					continue;
				}
				if (nestedSceneChildren || (!nestedSceneChildren && !isNetworkScene))
				{
					children.AddRange(child.GetChildren());
				}
				if (child is not INetworkNode)
				{
					continue;
				}
				if (!isNetworkScene && searchToggle == NetworkChildrenSearchToggle.ONLY_SCENES)
				{
					continue;
				}
				if (searchToggle == NetworkChildrenSearchToggle.INCLUDE_SCENES || isNetworkScene)
				{
					yield return new NetworkNodeWrapper(child);
				}
			}
		}

		public void Despawn()
		{
			if (NetworkRunner.Instance.IsClient)
				return;
			// NetworkRunner.Instance.Despawn(this);
		}

		internal void _NetworkPrepare(WorldRunner world)
		{
			if (Engine.IsEditorHint())
			{
				return;
			}

			CurrentWorld = world;
			if (IsNetworkScene)
			{
				if (!world.RegisterSpawn(Owner))
				{
					return;
				}
				var networkChildren = GetNetworkChildren(NetworkChildrenSearchToggle.INCLUDE_SCENES, false).ToList();
				networkChildren.Reverse();
				networkChildren.ForEach(child =>
				{
					// TODO: This is a little messed up.
					child.CurrentWorld = world;
					child.NetworkParentId = NetworkId;
					child._NetworkPrepare(world);
				});
				if (NetworkRunner.Instance.IsClient)
				{
					return;
				}
				foreach (var tuple in NetworkScenesRegister.ListProperties(Owner.Node.SceneFilePath))
				{
					var nodePath = tuple.Item1;
					var nodeProps = tuple.Item2;

					// Ensure every networked "INetworkNode" property is correctly linked to the WorldRunner.
					foreach (var property in nodeProps)
					{
						if (property.Type == Variant.Type.Object && property.Subtype == VariantSubtype.NetworkNode)
						{
							var node = Owner.Node.GetNode(nodePath);
							var prop = node.Get(property.Name);
							var tempNetworkNode = prop.As<RefCounted>();
							if (tempNetworkNode == null)
							{
								continue;
							}
							if (tempNetworkNode is INetworkNode networkNode)
							{
								var referencedNodeInWorld = CurrentWorld.GetNodeFromNetworkId(networkNode.Network._prepareNetworkId).Node as INetworkNode;
								if (referencedNodeInWorld.Network.IsNetworkScene && !string.IsNullOrEmpty(networkNode.Network._prepareStaticChildPath))
								{
									referencedNodeInWorld = referencedNodeInWorld.Network.Owner.Node.GetNodeOrNull(networkNode.Network._prepareStaticChildPath) as INetworkNode;
								}
								node.Set(property.Name, referencedNodeInWorld.Network.Owner);
							}
						}
					}

					// Ensure all property changes are linked up to the signal
					var networkChild = Owner.Node.GetNodeOrNull<INetworkNode>(nodePath);
					if (networkChild == null)
					{
						continue;
					}
					if (networkChild.Network.Owner is INotifyPropertyChanged propertyChangeNode)
					{
						propertyChangeNode.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
						{
							if (!NetworkScenesRegister.LookupProperty(Owner.Node.SceneFilePath, nodePath, e.PropertyName, out _))
							{
								return;
							}
							EmitSignal("NetworkPropertyChanged", nodePath, e.PropertyName);
						};
					}
					else
					{
						Debugger.Log($"NetworkChild {nodePath} is not INotifyPropertyChanged. Ensure your custom NetworkNode implements INotifyPropertyChanged.", Debugger.DebugLevel.ERROR);
					}
				}

				if (IsNetworkScene) {
					(Owner.Node as INetworkNode).SetupSerializers();
				}
				foreach (var initialSetProp in InitialSetNetworkProperties)
				{
					EmitSignal("NetworkPropertyChanged", initialSetProp.Item1, initialSetProp.Item2);
				}
			}
		}

		internal Godot.Collections.Dictionary<NetPeer, bool> spawnReady = new Godot.Collections.Dictionary<NetPeer, bool>();
		internal Godot.Collections.Dictionary<NetPeer, bool> preparingSpawn = new Godot.Collections.Dictionary<NetPeer, bool>();

		public async void LazySpawnReady<T>(byte[] data, NetPeer peer) where T : Node, INetworkNode
		{
			spawnReady[peer] = true;
			preparingSpawn.Remove(peer);
			if (data == null || data.Length == 0)
			{
				return;
			}
			await DataTransformer.FromBSON(context: new LazyPeerStateContext { ContextId = CurrentWorld.GetPeerWorldState(peer).Value.Id }, data, Owner.Node as T);
		}

		public void PrepareSpawn(NetPeer peer)
		{
			if (this is not ILazyPeerStatesLoader)
			{
				spawnReady[peer] = true;
				return;
			}
			if (preparingSpawn.ContainsKey(peer) || spawnReady.ContainsKey(peer))
			{
				return;
			}
			preparingSpawn[peer] = true;
			Thread myThread = new Thread(() => _prepareSpawn(peer));
			myThread.Start();
		}
		public async void _prepareSpawn(NetPeer peer)
		{
			Debugger.Log($"Loading peer values for: {Owner.Node.Name}", Debugger.DebugLevel.VERBOSE);
			// TODO: Will this cause an exception if peer is altered?
			var peerLoader = this as ILazyPeerStatesLoader;
			var result = await peerLoader.LoadPeerValues(peer);
			CallDeferred("LazySpawnReady", result, peer);
		}

		internal long _prepareNetworkId;
		internal string _prepareStaticChildPath;
		public virtual void _WorldReady()
		{
			if (IsNetworkScene)
			{
				var networkChildren = GetNetworkChildren(NetworkChildrenSearchToggle.INCLUDE_SCENES, false).ToList();
				networkChildren.Reverse();
				networkChildren.ForEach(child =>
				{
					child._WorldReady();
				});
			}
			IsWorldReady = true;
		}

		public virtual void _NetworkProcess(Tick tick)
		{
			Owner.Node.Call("_NetworkProcess", tick);
		}

		public Godot.Collections.Dictionary<int, Variant> GetInput()
		{
			if (!IsCurrentOwner) return null;
			if (!CurrentWorld.InputStore.ContainsKey(InputAuthority))
				return null;

			byte netId;
			if (NetworkRunner.Instance.IsServer)
			{
				netId = CurrentWorld.GetPeerNodeId(InputAuthority, Owner);
			}
			else
			{
				netId = (byte)NetworkId;
			}

			if (!CurrentWorld.InputStore[InputAuthority].ContainsKey(netId))
				return null;

			var inputs = CurrentWorld.InputStore[InputAuthority][netId];
			CurrentWorld.InputStore[InputAuthority].Remove(netId);
			return inputs;
		}

		/// <summary>
		/// Used by NetworkFunction to determine whether the call should be send over the network, or if it is coming from the network.
		/// </summary>
		internal bool IsRemoteCall { get; set; } = false;
		public string NodePathFromNetworkScene()
		{
			if (IsNetworkScene)
			{
				return Owner.Node.GetPathTo(Owner.Node);
			}

			return NetworkParent.Node.GetPathTo(Owner.Node);
		}
	}
}

