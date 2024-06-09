using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using HLNC.Serialization;
using HLNC.Serialization.Serializers;

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
        <example>
        <code language="cs">
        public partial class Player : NetworkNode3D, INetworkInputHandler
        {
            public Dictionary<int, Variant> InputBuffer { get; private set; } = [];

            public enum InputType {
                MOVE = 0,
            }

            public override void _Process(double delta)
            {
                base._Process(delta);
                if (!IsCurrentOwner || NetworkRunner.Instance.IsServer) return;

                // Using the regular _Process method, we fill the InputBuffer with data which will automagically be transferred
                // to the server on the next tick

                var moveDirection = new Vector2();

                if (Input.IsActionPressed("player_up")) {
                    moveDirection.Y -= 1;
                }
                if (Input.IsActionPressed("player_down")) {
                    moveDirection.Y += 1;
                }
                if (Input.IsActionPressed("player_left")) {
                    moveDirection.X -= 1;
                }
                if (Input.IsActionPressed("player_right")) {
                    moveDirection.X += 1;
                }

                inputBuffer[(int)InputType.MOVE] = moveDirection;
            }

            public override void _NetworkProcess(int _tick)
            {
                base._NetworkProcess(_tick);
            
                if (!NetworkRunner.Instance.IsServer) return;

                var input = GetInput();
                if (input == null) return;

                // Eventually the server receives the client input, at which point it can decide how it should mutate the game state.
                // This is how we ensure clients aren't able to cheat/hack the system and arbitrarily change values.

                if (input.ContainsKey((int)InputType.MOVE)) {
                    var moveDirection = ((Vector2)input[(int)InputType.MOVE]).Normalized();
                    Position = new Vector3(
                        Mathf.Clamp(Position.X + moveDirection.X, -5, 5),
                        Position.Y,
                        Mathf.Clamp(Position.Z + moveDirection.Y, -5, 5)
                    );
                }
            }
        }
        </code>
        </example>
    */
    public partial class NetworkNode3D : Node3D, IStateSerializable, INotifyPropertyChanged
    {
        /// <summary>
        /// Server-side only. A map of PeerIds that should receive network updates for this node.
        /// </summary>
        public Dictionary<long, bool> Interest { get; } = [];

        /// <summary>
        /// The ID of the node tracked across the network.
        /// This is different on the server side than on the client side.
        /// </summary>
        /// <remarks>
        ///  The server tracks a global NetworkId for each node, as well as a local NetworkId for each peer. The reason for this is that it would cost too much bandwidth to send the full node's NetworkId to each peer.
        ///  To minimize bandwidth, each client can store up to 64 NetworkIds at a time. The server is then able to talk about all 64 nodes to the client at once by using a single 64 bit integer.
        ///  In the future, this may be expanded with an additional 64 bit integer to allow up to 4096 network nodes at once.
        /// </remarks>
        public NetworkId NetworkId { get; internal set; } = -1;

        /// <summary>
        /// The current peer who can send inputs via this node. On the client side, this is either -1, or their own PeerId. Clients cannot see who is the input authority of other objects.
        /// </summary>
        public PeerId InputAuthority { get; internal set; } = -1;

        /// <summary>
        /// A list of serializer objects which manage the server-side exporting of data to the client, and the client-side importing of data received from the server.
        /// </summary>
        public IStateSerailizer[] Serializers { get; }

        /// <summary>
        /// A helper function which indicates if the current caller is the input authority. Always true for the server.
        /// </summary>
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

        /// <summary>
        /// Get a network node by the NetworkId.
        /// </summary>
        /// <param name="network_id">The NetworkId in question.</param>
        /// <returns><see cref="NetworkNode3D"/></returns>
        public static NetworkNode3D GetFromNetworkId(NetworkId network_id)
        {
            if (network_id == -1)
                return null;
            if (!NetworkRunner.Instance.NetworkNodes.ContainsKey(network_id))
                return null;
            return NetworkRunner.Instance.NetworkNodes[network_id];
        }

        /// <summary>
        /// Finds the nearest <see cref="NetworkNode3D"/> parent. If the input node is a <see cref="NetworkNode3D"/>, then this function returns that input parameter.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get all <see cref="NetworkNode3D"/> children recursively. Does not include <see cref="NetworkScenes"/>.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// De-register the object from the network and dequeue it.
        /// </summary>
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