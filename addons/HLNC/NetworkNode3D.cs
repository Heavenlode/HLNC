using Godot;
using HLNC.StateSerializers;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace HLNC
{

    public partial class NetworkNode3D : Node3D, IStateSerializable, INotifyPropertyChanged
    {
        private List<NetworkNode3D> networkChildren = new List<NetworkNode3D>();
        private bool networkScene = false;
        public bool NetworkScene => networkScene;
        public bool DynamicSpawn = false;

        // Cannot have more than 8 serializers
        public List<IStateSerailizer> Serializers { get; }
        public NetworkNode3D()
        {
            // First, determine if the Node class has the NetworkScene attribute.
			networkScene = GetType().GetCustomAttributes(typeof(NetworkScenes), true).Length > 0;
            Serializers = new List<IStateSerailizer> {
                new SpawnSerializer(this),
                new NetworkPropertiesSerializer(this),
            };
            
        }
        public NetworkId NetworkId = -1;
        public PeerId InputAuthority = -1;

        public bool IsCurrentOwner
        {
            get { return NetworkRunner.Instance.IsServer || InputAuthority == NetworkRunner.Instance.LocalPlayerId; }
        }
        public Dictionary<long, bool> interest = new Dictionary<long, bool>();

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
                if (node is NetworkNode3D)
                    return (NetworkNode3D)node;
                node = node.GetParent();
            }
            return null;
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

            if (DynamicSpawn)
                return;

            if (NetworkScene) {
                var children = GetChildren();
                while (children.Count > 0) {
                    var child = children[0];
                    children.RemoveAt(0);
                    children.AddRange(child.GetChildren());
                    if (child is NetworkNode3D) {
                        networkChildren.Add((NetworkNode3D)child);
                    }
                }
                NetworkRunner.Instance.RegisterStaticSpawn(this);
            }
        }

        public virtual void _NetworkProcess(int _tick)
        {
            if (IsQueuedForDeletion())
                return;
            foreach (var child in networkChildren) {
                child._NetworkProcess(_tick);
            }
        }

        public object GetInput()
        {
            if (!NetworkRunner.Instance.InputStore.ContainsKey(InputAuthority) || !IsCurrentOwner)
                return null;
            return NetworkRunner.Instance.InputStore[InputAuthority];
        }

        public override void _PhysicsProcess(double delta)
        {
            if (IsQueuedForDeletion())
                return;
            if (NetworkRunner.Instance.IsServer)
                return;

            if (NetworkScene) {
                for (var i = 0; i < Serializers.Count; i++)
                {
                    Serializers[i].PhysicsProcess(delta);
                }
            }
        }
    }
}