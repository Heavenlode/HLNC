using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HLNC.Serialization;
using HLNC.Serialization.Serializers;
using Newtonsoft.Json.Linq;

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
    public partial class NetworkNode3D : Node3D, IStateSerializable, INotifyPropertyChanged
    {
        private static PackedScene NetworkNode3DBlankScene = GD.Load<PackedScene>("res://addons/HLNC/network_node_3d.tscn");
        public static NetworkNode3D Instantiate() {
            return NetworkNode3DBlankScene.Instantiate() as NetworkNode3D;
        }
        public bool IsNetworkScene => GetMeta("is_network_scene", false).AsBool();

        internal List<NetworkNodeWrapper> NetworkSceneChildren = [];

        public override void _ExitTree()
        {
            base._ExitTree();
            if (NetworkParent != null && NetworkParent.Node is NetworkNode3D _networkNodeParent)
            {
                _networkNodeParent.NetworkSceneChildren.Remove(
                    _networkNodeParent.NetworkSceneChildren.Find((NetworkNodeWrapper child) => child.Node == this)
                );
            }
        }

        public bool IsNetworkReady { get; internal set; } = false;

        private NetworkNodeWrapper _networkParent = null;
        internal NetworkNodeWrapper NetworkParent
        {
            get => _networkParent;
            set
            {
                {
                    if (IsNetworkScene && _networkParent != null && _networkParent.Node is NetworkNode3D _networkNodeParent) {
                        _networkNodeParent.NetworkSceneChildren.Remove(
                            _networkNodeParent.NetworkSceneChildren.Find((NetworkNodeWrapper child) => child.Node == this)
                        );
                    }
                }
                _networkParent = value;
                {
                    if (IsNetworkScene && value != null && value.Node is NetworkNode3D _networkNodeParent) {
                        _networkNodeParent.NetworkSceneChildren.Add(new NetworkNodeWrapper(this));
                    }
                }
            }
        }
        public bool DynamicSpawn { get; internal set; } = false;

        // Cannot have more than 8 serializers
        public IStateSerailizer[] Serializers { get; private set; }

        public JObject ToJSON(bool recurse = true)
        {
            if (!IsNetworkScene)
            {
                throw new System.Exception("Only scenes can be converted to JSON.");
            }

            var result = new JObject();
            result["data"] = new JObject();
            if (IsNetworkScene) {
                result["scenePack"] = NetworkScenesRegister.SCENES_PACK[SceneFilePath];
            }
            // We retain this for debugging purposes.
            result["nodeName"] = Name.ToString();

            if (NetworkScenesRegister.PROPERTIES_MAP.TryGetValue(SceneFilePath, out var childSceneStaticNetworkNodes))
            {
                foreach (var staticNetworkNodePathAndProps in childSceneStaticNetworkNodes)
                {
                    var nodePath = staticNetworkNodePathAndProps.Key;
                    var nodeProps = staticNetworkNodePathAndProps.Value;
                    result["data"][nodePath] = new JObject();
                    var nodeData = result["data"][nodePath] as JObject;
                    foreach (var property in nodeProps)
                    {
                        var prop = GetNode(nodePath).Get(property.Value.Name);
                        if (prop.VariantType == Variant.Type.String)
                        {
                            nodeData[property.Value.Name] = prop.ToString();
                        }
                        else if (prop.VariantType == Variant.Type.Float)
                        {
                            nodeData[property.Value.Name] = prop.AsDouble();
                        }
                        else if (prop.VariantType == Variant.Type.Int)
                        {
                            nodeData[property.Value.Name] = prop.AsInt64();
                        }
                        else if (prop.VariantType == Variant.Type.Bool)
                        {
                            nodeData[property.Value.Name] = prop.AsBool();
                        }
                        else if (prop.VariantType == Variant.Type.Vector2)
                        {
                            var vec = prop.As<Vector2>();
                            nodeData[property.Value.Name] = new JArray(vec.X, vec.Y);
                        }
                        else if (prop.VariantType == Variant.Type.Vector3)
                        {
                            var vec = prop.As<Vector3>();
                            nodeData[property.Value.Name] = new JArray(vec.X, vec.Y, vec.Z);
                        }
                    }

                    if (!nodeData.HasValues) {
                        (result["data"] as JObject).Remove(nodePath);
                    }
                }
            }

            if (recurse)
            {
                result["children"] = new JObject();
                foreach (var child in NetworkSceneChildren)
                {
                    if (child.Node is NetworkNode3D networkChild)
                    {
                        string pathTo = GetPathTo(networkChild.GetParent());
                        if (!(result["children"] as JObject).ContainsKey(pathTo))
                        {
                            result["children"][pathTo] = new JArray();
                        }
                        (result["children"][pathTo] as JArray).Add(networkChild.ToJSON());
                    }
                }
            }

            return result;
        }

        public static async Task<NetworkNode3D> FromJSON(JObject data)
        {
            NetworkNode3D node;
            if (data.ContainsKey("scenePack")) {
                node = NetworkScenesRegister.SCENES_MAP[(byte)data["scenePack"]].Instantiate<NetworkNode3D>();
            } else {
                node = Instantiate();
            }
            var tcs = new TaskCompletionSource<bool>();
            node.Ready += () => {
                foreach (var child in node.GetNetworkChildren(NetworkChildrenSearchToggle.INCLUDE_SCENES)) {
                    if (child.Node.GetMeta("is_network_scene", false).AsBool()) {
                        child.Node.QueueFree();
                    } else {
                        child.Node.SetMeta("import_from_json", true);
                    }
                }
                node.SetMeta("import_from_json", true);
                tcs.SetResult(true);
            };
            NetworkRunner.Instance.AddChild(node);
            await tcs.Task;
            NetworkRunner.Instance.RemoveChild(node);
            foreach (var networkNodePathAndProps in data["data"] as JObject) {
                var nodePath = networkNodePathAndProps.Key;
                var nodeProps = networkNodePathAndProps.Value as JObject;
                var targetNode = node.GetNodeOrNull(nodePath);
                if (targetNode == null) {
                    GD.PrintErr("Node not found for: ", nodePath);
                    continue;
                }
                foreach (var prop in nodeProps) {
                    var variantType = targetNode.Get(prop.Key).VariantType;
                    if (variantType == Variant.Type.String) {
                        targetNode.Set(prop.Key, prop.Value.ToString());
                    } else if (variantType == Variant.Type.Float) {
                        targetNode.Set(prop.Key, (float)prop.Value);
                    } else if (variantType == Variant.Type.Int) {
                        targetNode.Set(prop.Key, (int)prop.Value);
                    } else if (variantType == Variant.Type.Bool) {
                        targetNode.Set(prop.Key, (bool)prop.Value);
                    } else if (variantType == Variant.Type.Vector2) {
                        var vec = prop.Value as JArray;
                        targetNode.Set(prop.Key, new Vector2((float)vec[0], (float)vec[1]));
                    } else if (variantType == Variant.Type.Vector3) {
                        var vec = prop.Value as JArray;
                        targetNode.Set(prop.Key, new Vector3((float)vec[0], (float)vec[1], (float)vec[2]));
                    }
                }
            }
            if (data.ContainsKey("children")) {
                foreach (var child in data["children"] as JObject) { 
                    var nodePath = child.Key;
                    var children = child.Value as JArray;
                    foreach (var childData in children) {
                        var childNode = await FromJSON(childData as JObject);
                        var parent = node.GetNodeOrNull(nodePath);
                        if (parent == null) {
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

        public NetworkNode3D() {
            if (GetType().GetCustomAttributes(typeof(NetworkScenes), true).Length > 0) {
                SetMeta("is_network_scene", true);
            }
            SetMeta("is_network_node", true);
            if (Engine.IsEditorHint())
            {
                return;
            }
            Serializers = [
                new SpawnSerializer(new NetworkNodeWrapper(this)),
                new NetworkPropertiesSerializer(new NetworkNodeWrapper(this)),
            ];
        }
        public NetworkId NetworkId { get; internal set; } = -1;
        public PeerId InputAuthority { get; internal set; } = -1;

        public bool IsCurrentOwner
        {
            get { return NetworkRunner.Instance.IsServer || InputAuthority == NetworkRunner.Instance.LocalPlayerId; }
        }

        public Dictionary<long, bool> Interest = [];

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
        public IEnumerable<NetworkNodeWrapper> GetNetworkChildren(NetworkChildrenSearchToggle searchToggle = NetworkChildrenSearchToggle.EXCLUDE_SCENES)
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
                children.AddRange(child.GetChildren());
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

        public void _NetworkPrepare()
        {
            if (Engine.IsEditorHint())
            {
                return;
            }

            if (IsNetworkScene)
            {
                NetworkRunner.Instance.RegisterSpawn(new NetworkNodeWrapper(this));
                if (!NetworkRunner.Instance.IsServer)
                {
                    // Clients dequeue network scenes and prepare them later via serializers triggered by the server.
                    return;
                }
                if (!DynamicSpawn)
                {
                    // The network parent is defined on spawn for the client
                    var parentScene = GetParent();
                    while (parentScene != null)
                    {
                        if (parentScene.GetMeta("is_network_scene", false).AsBool())
                        {
                            break;
                        }
                        parentScene = parentScene.GetParent();
                    }
                    if (parentScene == null && !GetMeta("is_network_scene", false).AsBool())
                    {
                        throw new System.Exception("NetworkNode3D has no associated network scene: " + GetPath());
                    }
                    if (parentScene == null && this != NetworkRunner.Instance.CurrentScene.Node)
                    {
                        throw new System.Exception("Scene not associated with parent. Only one root scene allowed at a time: " + GetPath());
                    }
                    if (parentScene != null) {
                        NetworkParent = new NetworkNodeWrapper(parentScene);
                    }
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

            _NetworkReady();
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

            if (IsCurrentOwner && !NetworkRunner.Instance.IsServer && this is INetworkInputHandler)
            {
                INetworkInputHandler inputHandler = (INetworkInputHandler)this;
                if (inputHandler.InputBuffer.Count > 0)
                {
                    NetworkRunner.Instance.RpcId(1, "TransferInput", NetworkRunner.Instance.CurrentTick, (byte)NetworkId, inputHandler.InputBuffer);
                    inputHandler.InputBuffer.Clear();
                }
            }
        }

        public Godot.Collections.Dictionary<int, Variant> GetInput()
        {
            if (!IsCurrentOwner) return null;

            byte netId = NetworkRunner.Instance.LocalPlayerId == InputAuthority ? (byte)NetworkId : NetworkPeerManager.Instance.GetPeerNodeId(InputAuthority, new NetworkNodeWrapper(this));
            if (!NetworkRunner.Instance.InputStore.ContainsKey(InputAuthority))
                return null;
            if (!NetworkRunner.Instance.InputStore[InputAuthority].ContainsKey(netId))
                return null;

            var inputs = NetworkRunner.Instance.InputStore[InputAuthority][netId];
            NetworkRunner.Instance.InputStore[InputAuthority].Remove(netId);
            return inputs;
        }

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