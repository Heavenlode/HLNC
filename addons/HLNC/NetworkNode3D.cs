using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Grpc.Core;
using HLNC.Serialization;
using HLNC.Serialization.Serializers;
using Microsoft.VisualBasic;
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
    public partial class NetworkNode3D : Node3D, IStateSerializable, INotifyPropertyChanged
    {
        public bool IsNetworkScene => GetMeta("is_network_scene", false).AsBool();

        internal List<NetworkNodeWrapper> NetworkSceneChildren = [];
        internal List<Tuple<string, string>> InitialSetNetworkProperties = [];
        public WorldRunner CurrentWorld { get; internal set; }
        public Godot.Collections.Dictionary<byte, Variant> InputBuffer = [];
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
            if (NetworkParent != null && NetworkParent.Node is NetworkNode3D _networkNodeParent)
            {
                _networkNodeParent.NetworkSceneChildren.Remove(
                    _networkNodeParent.NetworkSceneChildren.Find((NetworkNodeWrapper child) => child.Node == this)
                );
            }
        }

        public bool IsNetworkReady { get; internal set; } = false;

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
        public bool DynamicSpawn { get; internal set; } = false;

        // Cannot have more than 8 serializers
        public IStateSerailizer[] Serializers { get; private set; } = [];

        public BsonDocument ToBSONDocument(bool recurse = true, HashSet<Type> skipTypes = null)
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

            if (NetworkScenesRegister.PROPERTIES_MAP.TryGetValue(SceneFilePath, out var childSceneStaticNetworkNodes))
            {
                foreach (var staticNetworkNodePathAndProps in childSceneStaticNetworkNodes)
                {
                    var nodePath = staticNetworkNodePathAndProps.Key;
                    var nodeProps = staticNetworkNodePathAndProps.Value;
                    result["data"][nodePath] = new BsonDocument();
                    var nodeData = result["data"][nodePath] as BsonDocument;
                    var hasValues = false;
                    foreach (var property in nodeProps)
                    {
                        var prop = GetNode(nodePath).Get(property.Value.Name);
                        if (property.Value.Type == Variant.Type.String)
                        {
                            nodeData[property.Value.Name] = prop.ToString();
                        }
                        else if (property.Value.Type == Variant.Type.Float)
                        {
                            nodeData[property.Value.Name] = prop.AsDouble();
                        }
                        else if (property.Value.Type == Variant.Type.Int)
                        {
                            nodeData[property.Value.Name] = prop.AsInt64();
                        }
                        else if (property.Value.Type == Variant.Type.Bool)
                        {
                            nodeData[property.Value.Name] = prop.AsBool();
                        }
                        else if (property.Value.Type == Variant.Type.Vector2)
                        {
                            var vec = prop.As<Vector2>();
                            nodeData[property.Value.Name] = new BsonArray { vec.X, vec.Y };
                        }
                        else if (property.Value.Type == Variant.Type.Vector3)
                        {
                            var vec = prop.As<Vector3>();
                            nodeData[property.Value.Name] = new BsonArray { vec.X, vec.Y, vec.Z };
                        }
                        else if (property.Value.Type == Variant.Type.Nil)
                        {
                            nodeData[property.Value.Name] = null;
                        }
                        else if (property.Value.Type == Variant.Type.PackedByteArray)
                        {
                            if (property.Value.Subtype == VariantSubtype.Guid)
                            {
                                nodeData[property.Value.Name] = new BsonBinaryData(new Guid(prop.AsByteArray()), GuidRepresentation.Standard);
                            }
                            else
                            {
                                nodeData[property.Value.Name] = new BsonBinaryData(prop.AsByteArray(), BsonBinarySubType.Binary);
                            }
                        }
                        else
                        {
                            GD.PrintErr("Serializing to JSON unsupported property type: ", prop.VariantType);
                            continue;
                        }
                        hasValues = true;
                    }

                    if (!hasValues)
                    {
                        // Delete empty objects from JSON, i.e. network nodes with no network properties.
                        (result["data"] as BsonDocument).Remove(nodePath);
                    }
                }
            }

            if (recurse)
            {
                result["children"] = new BsonDocument();
                foreach (var child in NetworkSceneChildren)
                {
                    if (child.Node is NetworkNode3D networkChild && (skipTypes == null || !skipTypes.Contains(networkChild.GetType())))
                    {
                        string pathTo = GetPathTo(networkChild.GetParent());
                        if (!(result["children"] as BsonDocument).Contains(pathTo))
                        {
                            result["children"][pathTo] = new BsonArray();
                        }
                        (result["children"][pathTo] as BsonArray).Add(networkChild.ToBSONDocument());
                    }
                }
            }

            return result;
        }

        public async void FromBSON(byte[] data)
        {
            await NetworkNode3D.FromBSON<NetworkNode3D>(data, this);
        }

        public static async Task<T> FromBSON<T>(byte[] data, T fillNode = null) where T : NetworkNode3D
        {
            return await FromBSON<T>(BsonSerializer.Deserialize<BsonDocument>(data), fillNode);
        }

        public static async Task<T> FromBSON<T>(BsonDocument data, T fillNode = null) where T : NetworkNode3D
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
            var tcs = new TaskCompletionSource<bool>();
            node.Ready += () =>
            {
                foreach (var child in node.GetNetworkChildren(NetworkChildrenSearchToggle.INCLUDE_SCENES))
                {
                    if (child.Node.GetMeta("is_network_scene", false).AsBool())
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
                    var variantType = targetNode.Get(prop.Name).VariantType;
                    var propData = NetworkScenesRegister.PROPERTIES_MAP[node.SceneFilePath][nodePath][prop.Name];
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
                        targetNode.Set(prop.Name, prop.Value.AsInt64);
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
                        var childNode = await FromBSON<T>(childData as BsonDocument);
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
            NetworkRunner.Instance.RemoveChild(node);
            return node;
        }

        [Signal]
        public delegate void NetworkPropertyChangedEventHandler(string nodePath, StringName propertyName);

        public override void _EnterTree()
        {
            if (GetType().GetCustomAttributes(typeof(NetworkScenes), true).Length > 0)
            {
                SetMeta("is_network_scene", true);
            }
            SetMeta("is_network_node", true);
            if (Engine.IsEditorHint())
            {
                return;
            }
        }

        public NetworkId NetworkId { get; internal set; } = -1;
        public NetPeer InputAuthority { get; internal set; } = null;

        public bool IsCurrentOwner
        {
            get { return NetworkRunner.Instance.IsServer || (!NetworkRunner.Instance.IsServer && InputAuthority == NetworkRunner.Instance.ENetHost); }
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
                var isNetworkScene = child.GetMeta("is_network_scene", false).AsBool();
                if (isNetworkScene && searchToggle == NetworkChildrenSearchToggle.EXCLUDE_SCENES)
                {
                    continue;
                }
                if (nestedSceneChildren || (!nestedSceneChildren && !isNetworkScene))
                {
                    children.AddRange(child.GetChildren());
                }
                if (!child.GetMeta("is_network_node", false).AsBool())
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
            if (!NetworkRunner.Instance.IsServer)
                return;
            // NetworkRunner.Instance.Despawn(this);
        }

        public void _NetworkPrepare(WorldRunner world)
        {
            if (Engine.IsEditorHint())
            {
                return;
            }

            CurrentWorld = world;

            if (IsNetworkScene)
            {
                var networkChildren = GetNetworkChildren(NetworkNode3D.NetworkChildrenSearchToggle.INCLUDE_SCENES).ToList();
                networkChildren.Reverse();
                networkChildren.ForEach(child => child._NetworkPrepare(world));
                world.RegisterSpawn(new NetworkNodeWrapper(this));
                if (!NetworkRunner.Instance.IsServer)
                {
                    // Clients dequeue network scenes and prepare them later via serializers triggered by the server.
                    return;
                }
                foreach (var child in GetNetworkChildren(NetworkChildrenSearchToggle.INCLUDE_SCENES, false))
                {
                    child.NetworkParentId = NetworkId;
                }

                if (NetworkRunner.Instance.IsServer)
                {
                    if (NetworkScenesRegister.PROPERTIES_MAP.TryGetValue(SceneFilePath, out var childDict))
                    {
                        foreach (var childPath in childDict.Keys)
                        {
                            var networkChild = GetNodeOrNull<NetworkNode3D>(childPath);
                            if (networkChild == null)
                            {
                                continue;
                            }
                            networkChild.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
                            {
                                if (!childDict[childPath].ContainsKey(e.PropertyName))
                                {
                                    return;
                                }
                                EmitSignal("NetworkPropertyChanged", childPath, e.PropertyName);
                            };
                        }
                    }
                }
            }

            SetupSerializers(true);
            foreach (var initialSetProp in InitialSetNetworkProperties)
            {
                EmitSignal("NetworkPropertyChanged", initialSetProp.Item1, initialSetProp.Item2);
            }
            _NetworkReady();
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

        public virtual void _NetworkReady()
        {
            IsNetworkReady = true;
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
            if (IsNetworkScene)
            {
                for (var i = 0; i < Serializers.Length; i++)
                {
                    Serializers[i].PhysicsProcess(delta);
                }
            }
            if (NetworkRunner.Instance.IsServer)
                return;
        }
    }
}