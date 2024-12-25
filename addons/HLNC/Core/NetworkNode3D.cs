using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using HLNC.Addons.Lazy;
using HLNC.Serialization;
using HLNC.Serialization.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace HLNC
{
	/**
		<summary>
		<see cref="Godot.Node3D">Node3D</see>, extended with HLNC networking capabilities. This is the most basic networked 3D object.
		On every network tick, all NetworkNode3D nodes in the scene tree automatically have their <see cref="HLNC.NetworkProperty">network properties</see> updated with the latest data from the server.
		Then, the special <see cref="_NetworkProcess(int)">NetworkProcess</see> method is called, which indicates that a network Tick is occurring.
		Network properties can only update on the server side.
		For a client to update network properties, they must send client inputs to the server via implementing the <see cref="HLNC.INetworkInputHandler"/> interface.
		The server receives client inputs, can access them via <see cref="GetInput"/>, and handle them accordingly within <see cref="_NetworkProcess(int)">NetworkProcess</see> to mutate state.
		</summary>
	*/
	[Tool]
	public partial class NetworkNode3D : Node3D, IStateSerializable, INotifyPropertyChanged, INetworkSerializable<NetworkNode3D>, IBsonSerializable
	{
		public bool IsNetworkScene => NetworkScenesRegister.IsNetworkScene(SceneFilePath);

		internal List<NetworkNodeWrapper> NetworkSceneChildren = [];
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

		/// <inheritdoc/>
		public override void _ExitTree()
		{
			if (Engine.IsEditorHint())
			{
				return;
			}
			base._ExitTree();
			if (!IsWorldReady) return;

			if (NetworkParent != null && NetworkParent.Node is NetworkNode3D _networkNodeParent)
			{
				_networkNodeParent.NetworkSceneChildren.Remove(
					_networkNodeParent.NetworkSceneChildren.Find((NetworkNodeWrapper child) => child.Node == this)
				);
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
					if (IsNetworkScene && NetworkParent != null && NetworkParent.Node is NetworkNode3D _networkNodeParent)
					{
						_networkNodeParent.NetworkSceneChildren.Remove(
							_networkNodeParent.NetworkSceneChildren.Find((NetworkNodeWrapper child) => child.Node == this)
						);
					}
				}
				_networkParentId = value;
				{
					if (IsNetworkScene && value != 0 && CurrentWorld.GetNodeFromNetworkId(value).Node is NetworkNode3D _networkNodeParent)
					{
						_networkNodeParent.NetworkSceneChildren.Add(new NetworkNodeWrapper(this));
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

		// Cannot have more than 8 serializers
		public IStateSerializer[] Serializers { get; private set; } = [];

		public BsonDocument ToBSONDocument(Variant context = new Variant(), bool recurse = true, HashSet<Type> skipNodeTypes = null, HashSet<Tuple<Variant.Type, VariantSubtype>> propTypes = null, HashSet<Tuple<Variant.Type, VariantSubtype>> skipPropTypes = null)
		{
			if (!IsNetworkScene)
			{
				throw new System.Exception("Only scenes can be converted to BSON: " + GetPath());
			}
			BsonDocument result = new BsonDocument();
			result["data"] = new BsonDocument();
			if (IsNetworkScene)
			{
				result["scene"] = SceneFilePath;
			}
			// We retain this for debugging purposes.
			result["nodeName"] = Name.ToString();

			foreach (var node in NetworkScenesRegister.ListProperties(SceneFilePath))
			{
				var nodePath = node.Item1;
				var nodeProps = node.Item2;
				result["data"][nodePath] = new BsonDocument();
				var nodeData = result["data"][nodePath] as BsonDocument;
				var hasValues = false;
				foreach (var property in nodeProps)
				{
					if (propTypes != null && !propTypes.Contains(new Tuple<Variant.Type, VariantSubtype>(property.Type, property.Subtype)))
					{
						continue;
					}
					if (skipPropTypes != null && skipPropTypes.Contains(new Tuple<Variant.Type, VariantSubtype>(property.Type, property.Subtype)))
					{
						continue;
					}
					var prop = GetNode(nodePath).Get(property.Name);
					var val = HLNC.Serialization.BsonSerialize.SerializeVariant(context, prop, property.Subtype);
					// GD.Print("Serializing: ", nodePath, ".", property.Name, " with value: ", val);
					if (val == null) continue;
					nodeData[property.Name] = val;
					hasValues = true;
				}

				if (!hasValues)
				{
					// Delete empty objects from JSON, i.e. network nodes with no network properties.
					(result["data"] as BsonDocument).Remove(nodePath);
				}
			}

			if (recurse)
			{
				result["children"] = new BsonDocument();
				foreach (var child in NetworkSceneChildren)
				{
					if (child.Node is NetworkNode3D networkChild && (skipNodeTypes == null || !skipNodeTypes.Contains(networkChild.GetType())))
					{
						string pathTo = GetPathTo(networkChild.GetParent());
						if (!(result["children"] as BsonDocument).Contains(pathTo))
						{
							result["children"][pathTo] = new BsonArray();
						}
						(result["children"][pathTo] as BsonArray).Add(networkChild.ToBSONDocument(context, recurse, skipNodeTypes, propTypes, skipPropTypes));
					}
				}
			}

			return result;
		}

		public async void FromBSON(Variant context, byte[] data)
		{
			await NetworkNode3D.FromBSON<NetworkNode3D>(context, data, this);
		}

		public static async Task<T> FromBSON<T>(Variant context, byte[] data, T fillNode = null) where T : NetworkNode3D
		{
			return await FromBSON<T>(context, BsonSerializer.Deserialize<BsonDocument>(data), fillNode);
		}

		public static async Task<T> FromBSON<T>(Variant context, BsonDocument data, T fillNode = null) where T : NetworkNode3D
		{
			T node = fillNode;
			if (fillNode == null)
			{
				if (data.Contains("scene"))
				{
					node = GD.Load<PackedScene>(data["scene"].AsString).Instantiate<T>();
				}
				else
				{
					throw new System.Exception("No scene path found in BSON data");
				}
			}

			// Mark imported nodes accordingly
			if (!node.GetMeta("import_from_external", false).AsBool())
			{
				var tcs = new TaskCompletionSource<bool>();
				node.Ready += () =>
				{
					foreach (var child in node.GetNetworkChildren(NetworkChildrenSearchToggle.INCLUDE_SCENES))
					{
						if (child.IsNetworkScene)
						{
							child.Node.QueueFree();
						}
						else
						{
							child.Node.SetMeta("import_from_external", true);
						}
					}
					node.SetMeta("import_from_external", true);
					tcs.SetResult(true);
				};
				NetworkRunner.Instance.AddChild(node);
				await tcs.Task;
				NetworkRunner.Instance.RemoveChild(node);
			}

			foreach (var networkNodePathAndProps in data["data"] as BsonDocument)
			{
				var nodePath = networkNodePathAndProps.Name;
				var nodeProps = networkNodePathAndProps.Value as BsonDocument;
				var targetNode = node.GetNodeOrNull(nodePath);
				if (targetNode == null)
				{
					GD.PrintErr("Node not found for: ", nodePath);
					continue;
				}
				foreach (var prop in nodeProps)
				{
					node.InitialSetNetworkProperties.Add(new Tuple<string, string>(nodePath, prop.Name));
					CollectedNetworkProperty propData;
					if (!NetworkScenesRegister.LookupProperty(node.SceneFilePath, nodePath, prop.Name, out propData)) {
						throw new Exception($"Failed to pack property: {nodePath}.{prop.Name}");
					}
					var variantType = propData.Type;
					try
					{
						if (variantType == Variant.Type.String)
						{
							targetNode.Set(prop.Name, prop.Value.ToString());
						}
						else if (variantType == Variant.Type.Float)
						{
							targetNode.Set(prop.Name, prop.Value.AsDouble);
						}
						else if (variantType == Variant.Type.Int)
						{
							switch (propData.Subtype)
							{
								case VariantSubtype.NetworkId:
									if (prop.Value.AsInt64 != -1)
									{
										targetNode.Set(prop.Name, prop.Value.AsInt64);
									}
									break;
								case VariantSubtype.Int:
									targetNode.Set(prop.Name, prop.Value.AsInt32);
									break;
								case VariantSubtype.Byte:
									// Convert MongoDB Binary value to Byte
									targetNode.Set(prop.Name, (byte)prop.Value.AsInt32);
									break;
								default:
									targetNode.Set(prop.Name, prop.Value.AsInt64);
									break;
							}
						}
						else if (variantType == Variant.Type.Bool)
						{
							targetNode.Set(prop.Name, (bool)prop.Value);
						}
						else if (variantType == Variant.Type.Vector2)
						{
							var vec = prop.Value as BsonArray;
							targetNode.Set(prop.Name, new Vector2((float)vec[0].AsDouble, (float)vec[1].AsDouble));
						}
						else if (variantType == Variant.Type.PackedByteArray)
						{
							if (propData.Subtype == VariantSubtype.Guid)
							{
								targetNode.Set(prop.Name, new BsonBinaryData(prop.Value.AsGuid, GuidRepresentation.CSharpLegacy).AsByteArray);
							}
							else
							{
								targetNode.Set(prop.Name, prop.Value.AsByteArray);
							}
						}
						else if (variantType == Variant.Type.Vector3)
						{
							var vec = prop.Value as BsonArray;
							targetNode.Set(prop.Name, new Vector3((float)vec[0].AsDouble, (float)vec[1].AsDouble, (float)vec[2].AsDouble));
						}
						else if (variantType == Variant.Type.Object)
						{
							var value = propData.BsonDeserialize(context, prop.Value, targetNode.Get(prop.Name).AsGodotObject());
							if (value != null) {
								targetNode.Set(prop.Name, value);
							}
						}
					}
					catch (InvalidCastException)
					{
						GD.PrintErr("Failed to set property: ", prop.Name, " on ", nodePath, " with value: ", prop.Value, " and type: ", variantType);
					}
				}
			}
			if (data.Contains("children"))
			{
				foreach (var child in data["children"] as BsonDocument)
				{
					var nodePath = child.Name;
					var children = child.Value as BsonArray;
					foreach (var childData in children)
					{
						var childNode = await FromBSON<T>(context, childData as BsonDocument);
						var parent = node.GetNodeOrNull(nodePath);
						if (parent == null)
						{
							GD.PrintErr("Parent node not found for: ", nodePath);
							continue;
						}
						parent.AddChild(childNode);
					}
				}
			}
			return node;
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

		public static NetworkNode3D FindFromChild(Node node)
		{
			while (node != null)
			{
				if (node is NetworkNode3D networkNode)
					return networkNode;
				node = node.GetParent();
			}
			return null;
		}

		public enum NetworkChildrenSearchToggle { INCLUDE_SCENES, EXCLUDE_SCENES, ONLY_SCENES }
		public IEnumerable<NetworkNodeWrapper> GetNetworkChildren(NetworkChildrenSearchToggle searchToggle = NetworkChildrenSearchToggle.EXCLUDE_SCENES, bool nestedSceneChildren = true)
		{
			var children = GetChildren();
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
				if (child is not NetworkNode3D)
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
				if (!world.RegisterSpawn(new NetworkNodeWrapper(this)))
				{
					return;
				}
				var networkChildren = GetNetworkChildren(NetworkNode3D.NetworkChildrenSearchToggle.INCLUDE_SCENES, false).ToList();
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
				foreach (var tuple in NetworkScenesRegister.ListProperties(SceneFilePath))
				{
					var nodePath = tuple.Item1;
					var nodeProps = tuple.Item2;

					// Ensure every networked "NetworkNode3D" property is correctly linked to the WorldRunner.
					foreach (var property in nodeProps)
					{
						if (property.Type == Variant.Type.Object && property.Subtype == VariantSubtype.NetworkNode)
						{
							var node = GetNode(nodePath);
							var prop = node.Get(property.Name);
							var tempNetworkNode = prop.As<NetworkNode3D>();
							if (tempNetworkNode == null)
							{
								continue;
							}
							var networkNode = CurrentWorld.GetNodeFromNetworkId(tempNetworkNode._prepareNetworkId).Node as NetworkNode3D;
							if (networkNode.IsNetworkScene && !string.IsNullOrEmpty(tempNetworkNode._prepareStaticChildPath))
							{
								networkNode = networkNode.GetNodeOrNull(tempNetworkNode._prepareStaticChildPath) as NetworkNode3D;
							}
							node.Set(property.Name, networkNode);
						}
					}

					// Ensure all property changes are linked up to the signal
					var networkChild = GetNodeOrNull<NetworkNode3D>(nodePath);
					if (networkChild == null)
					{
						continue;
					}
					networkChild.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
					{
						if (!NetworkScenesRegister.LookupProperty(SceneFilePath, nodePath, e.PropertyName, out _))
						{
							return;
						}
						EmitSignal("NetworkPropertyChanged", nodePath, e.PropertyName);
					};
				}

				SetupSerializers(true);
				foreach (var initialSetProp in InitialSetNetworkProperties)
				{
					EmitSignal("NetworkPropertyChanged", initialSetProp.Item1, initialSetProp.Item2);
				}
			}
		}

		internal Godot.Collections.Dictionary<NetPeer, bool> spawnReady = new Godot.Collections.Dictionary<NetPeer, bool>();
		internal Godot.Collections.Dictionary<NetPeer, bool> preparingSpawn = new Godot.Collections.Dictionary<NetPeer, bool>();

		public async void LazySpawnReady(byte[] data, NetPeer peer)
		{
			spawnReady[peer] = true;
			preparingSpawn.Remove(peer);
			if (data == null || data.Length == 0)
			{
				return;
			}
			await FromBSON(context: new LazyPeerStateContext{ ContextId = CurrentWorld.GetPeerWorldState(peer).Value.Id }, data, this);
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
			GD.Print("Loading peer values for: ", Name);
			// TODO: Will this cause an exception if peer is altered?
			var peerLoader = this as ILazyPeerStatesLoader;
			var result = await peerLoader.LoadPeerValues(peer);
			CallDeferred("LazySpawnReady", result, peer);
		}

		internal void SetupSerializers(bool checkIfNetworkScene = false)
		{
			if (!checkIfNetworkScene || IsNetworkScene)
			{
				var spawnSerializer = new SpawnSerializer();
				AddChild(spawnSerializer);
				var propertySerializer = new NetworkPropertiesSerializer();
				AddChild(propertySerializer);
				Serializers = [spawnSerializer, propertySerializer];
			}
		}



		internal long _prepareNetworkId;
		internal string _prepareStaticChildPath;
		public virtual void _WorldReady()
		{
			if (IsNetworkScene)
			{
				var networkChildren = GetNetworkChildren(NetworkNode3D.NetworkChildrenSearchToggle.INCLUDE_SCENES, false).ToList();
				networkChildren.Reverse();
				networkChildren.ForEach(child =>
				{
					child._WorldReady();
				});
			}
			IsWorldReady = true;
		}

		public virtual void _NetworkProcess(int _tick)
		{
			if (Engine.IsEditorHint())
			{
				return;
			}
			if (NetworkRunner.Instance.IsServer)
				return;
		}

		public Godot.Collections.Dictionary<int, Variant> GetInput()
		{
			if (!IsCurrentOwner) return null;
			if (!CurrentWorld.InputStore.ContainsKey(InputAuthority))
				return null;

			byte netId;
			if (NetworkRunner.Instance.IsServer)
			{
				netId = CurrentWorld.GetPeerNodeId(InputAuthority, new NetworkNodeWrapper(this));
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

		/// <inheritdoc/>
		public override void _PhysicsProcess(double delta)
		{
			if (Engine.IsEditorHint())
			{
				return;
			}
			if (IsQueuedForDeletion())
				return;

			if (!IsWorldReady) return;
		}
		public static HLBuffer NetworkSerialize(WorldRunner currentWorld, NetPeer peer, NetworkNode3D obj)
		{
			var buffer = new HLBuffer();
			if (obj == null)
			{
				HLBytes.Pack(buffer, (byte)0);
				return buffer;
			}
			NetworkId targetNetId;
			byte staticChildId = 0;
			if (obj.IsNetworkScene)
			{
				targetNetId = obj.NetworkId;
			}
			else
			{
				if (NetworkScenesRegister.PackNode(obj.NetworkParent.Node.SceneFilePath, obj.NetworkParent.Node.GetPathTo(obj), out staticChildId))
				{
					targetNetId = obj.NetworkParent.NetworkId;
				}
				else
				{
					throw new Exception($"Failed to pack node: {obj.GetPath()}");
				}
			}
			var peerNodeId = currentWorld.GetPeerWorldState(peer).Value.WorldToPeerNodeMap[targetNetId];
			HLBytes.Pack(buffer, peerNodeId);
			HLBytes.Pack(buffer, staticChildId);
			return buffer;
		}
		public static NetworkNode3D NetworkDeserialize(WorldRunner currentWorld, HLBuffer buffer, NetworkNode3D initialObject)
		{
			var networkID = HLBytes.UnpackByte(buffer);
			if (networkID == 0)
			{
				return null;
			}
			var staticChildId = HLBytes.UnpackByte(buffer);
			var node = currentWorld.GetNodeFromNetworkId(networkID).Node as NetworkNode3D;
			if (staticChildId > 0)
			{
				node = node.GetNodeOrNull(NetworkScenesRegister.UnpackNode(node.SceneFilePath, staticChildId)) as NetworkNode3D;
			}
			return node;
		}

		public BsonValue BsonSerialize(Variant context)
		{
			var doc = new BsonDocument();
			if (IsNetworkScene)
			{
				doc["NetworkId"] = NetworkId;
			}
			else
			{
				doc["NetworkId"] = NetworkParent.NetworkId;
				doc["StaticChildPath"] = NetworkParent.Node.GetPathTo(this).ToString();
			}
			return doc;
		}

		public static GodotObject BsonDeserialize(Variant context, BsonValue data, GodotObject instance)
		{
			if (data.IsBsonNull) return null;
			var doc = data.AsBsonDocument;
			var node = new NetworkNode3D
			{
				_prepareNetworkId = doc["NetworkId"].AsInt64
			};
			if (doc.Contains("StaticChildPath"))
			{
				node._prepareStaticChildPath = doc["StaticChildPath"].AsString;

			}
			return node;
		}


		/// <summary>
		/// Used by NetworkFunction to determine whether the call should be send over the network, or if it is coming from the network.
		/// </summary>
		internal bool IsRemoteCall { get; set; } = false;
		public string NodePathFromNetworkScene()
		{
			if (IsNetworkScene)
			{
				return GetPathTo(this);
			}

			return NetworkParent.Node.GetPathTo(this);
		}
	}
}
