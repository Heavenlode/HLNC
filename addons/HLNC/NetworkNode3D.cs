using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using HLNC.Serialization;
using HLNC.Serialization.Serializers;

namespace HLNC
{
    public partial class NetworkNode3D : Node3D, IStateSerializable, INotifyPropertyChanged
    {

        public Dictionary<long, bool> Interest { get; } = [];
        public NetworkId NetworkId { get; internal set; } = -1;
        public PeerId InputAuthority { get; internal set; } = -1;
        public IStateSerailizer[] Serializers { get; }
        public bool IsCurrentAuthority => NetworkRunner.Instance.IsServer || InputAuthority == NetworkRunner.Instance.LocalPlayerId;

        [Signal]
        public delegate void NetworkPropertyChangedEventHandler(string nodePath, StringName propertyName);

        internal Dictionary<PeerId, bool> SpawnAware = [];
        internal List<Node> NetworkChildren = [];
        internal NetworkNode3D NetworkParent = null;
        internal bool DynamicSpawn = false;
        internal byte NetworkSceneId => NetworkScenesRegister.SCENES_PACK[SceneFilePath];

        public NetworkNode3D()
        {
            // First, determine if the Node class has the NetworkScene attribute.
            if (GetType().GetCustomAttributes(typeof(NetworkScenes), true).Length > 0)
            {
                SetMeta("is_network_scene", true);
            }
            SetMeta("is_network_node", true);
            if (Engine.IsEditorHint())
            {
                return;
            }


            Serializers = [
                new SpawnSerializer(this),
                new NetworkPropertiesSerializer(this),
            ];
        }

        public static NetworkNode3D GetFromNetworkId(NetworkId network_id)
        {
            if (network_id == -1)
                return null;
            if (!NetworkRunner.Instance.NetworkNodes.ContainsKey(network_id))
                return null;
            return NetworkRunner.Instance.NetworkNodes[network_id];
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

        public IEnumerable<Node> GetNetworkChildren()
        {
            var children = GetChildren();
            while (children.Count > 0)
            {
                var child = children[0];
                children.RemoveAt(0);
                if (child.HasMeta("is_network_scene"))
                {
                    continue;
                }
                children.AddRange(child.GetChildren());
                if (child.HasMeta("is_network_node"))
                {
                    yield return child;
                }
            }
        }

        public void Despawn()
        {
            if (!NetworkRunner.Instance.IsServer)
                return;
            // NetworkRunner.Instance.Despawn(this);
        }

        public override void _Ready()
        {
            base._Ready();
            if (Engine.IsEditorHint())
            {
                return;
            }

            if (!HasMeta("is_network_scene"))
            {
                return;
            }

            NetworkRunner.Instance.RegisterSpawn(this);

            if (!DynamicSpawn)
            {
                if (!NetworkRunner.Instance.IsServer)
                {
                    // Clients do not setup static scenes, only the server does
                    // The server will spawn this on the client later
                    return;
                }

                // The network parent is defined on spawn for the client
                var parentScene = GetParent();
                while (parentScene != null)
                {
                    if (parentScene.HasMeta("is_network_scene"))
                    {
                        break;
                    }
                    parentScene = parentScene.GetParent();
                }
                NetworkParent = (NetworkNode3D)parentScene;
                if (parentScene == null && !HasMeta("is_network_scene"))
                {
                    GD.PrintErr("NetworkNode3D has no associated network scene: " + GetPath());
                }

                if (parentScene == null && this != NetworkRunner.Instance.CurrentScene)
                {
                    GD.PrintErr("Scene not associated with parent. Only one root scene allowed at a time: " + GetPath());
                }
            }

            foreach (var child in GetNetworkChildren())
            {
                NetworkChildren.Add(child);
                if (NetworkRunner.Instance.IsServer && child is NetworkNode3D)
                {
                    var networkChild = (NetworkNode3D)child;
                    networkChild.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
                    {
                        EmitSignal("NetworkPropertyChanged", GetPathTo(networkChild), e.PropertyName);
                    };
                }
            }
        }

        public virtual void _NetworkProcess(int _tick)
        {
            if (Engine.IsEditorHint())
            {
                return;
            }
            if (NetworkRunner.Instance.IsServer)
                return;

            if (IsCurrentAuthority && !NetworkRunner.Instance.IsServer && this is INetworkInputHandler)
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
            if (!IsCurrentAuthority) return null;

            byte netId = NetworkRunner.Instance.LocalPlayerId == InputAuthority ? (byte)NetworkId : NetworkPeerManager.Instance.GetPeerNodeId(InputAuthority, this);
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
            if (HasMeta("is_network_scene"))
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