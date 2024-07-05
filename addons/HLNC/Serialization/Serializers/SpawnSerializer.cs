using System.Collections.Generic;
using System.Linq;
using Godot;

namespace HLNC.Serialization.Serializers
{
    internal class SpawnSerializer(NetworkNodeWrapper wrapper) : IStateSerailizer
    {
        private struct Data
        {
            public byte classId;
            public byte parentId;
            public int nodePathId;
            public Vector3 position;
            public Vector3 rotation;
            public byte hasInputAuthority;
        }

        private NetworkNodeWrapper wrapper = wrapper;
        private Dictionary<NetPeer, Tick> setupTicks = [];

        private Data Deserialize(HLBuffer data)
        {
            var spawnData = new Data
            {
                classId = HLBytes.UnpackByte(data),
                parentId = HLBytes.UnpackByte(data),
            };
            if (spawnData.parentId == 0)
            {
                return spawnData;
            }
            spawnData.nodePathId = HLBytes.UnpackInt32(data);
            spawnData.position = HLBytes.UnpackVector3(data);
            spawnData.rotation = HLBytes.UnpackVector3(data);
            spawnData.hasInputAuthority = HLBytes.UnpackByte(data);
            return spawnData;
        }

        public void Import(IPeerStateController peerStateController, HLBuffer buffer, out NetworkNodeWrapper nodeOut)
        {
            nodeOut = wrapper;
            var data = Deserialize(buffer);

            // If the node is already registered, then we don't need to spawn it again
            var result = peerStateController.TryRegisterPeerNode(nodeOut);
            if (result == 0)
            {
                return;
            }

            var networkId = wrapper.NetworkId;

            // Deregister and delete the node, because it is simply a "Placeholder" that doesn't really exist
            peerStateController.DeregisterPeerNode(nodeOut);
            wrapper.Node.QueueFree();

            var networkParent = peerStateController.GetNetworkNode(data.parentId);
            if (data.parentId != 0 && networkParent == null)
            {
                // The parent node is not registered, so we can't spawn this node
                GD.PrintErr("Parent node not found for: ", NetworkScenesRegister.SCENES_MAP[data.classId].ResourcePath, " - Parent ID: ", data.parentId);
                return;
            }

            var newNode = NetworkScenesRegister.SCENES_MAP[data.classId].Instantiate();
            nodeOut = new NetworkNodeWrapper(newNode)
            {
                NetworkId = networkId,
                DynamicSpawn = true
            };
            if (networkParent != null)
            {
                nodeOut.NetworkParentId = networkParent.NetworkId;
            }
            peerStateController.TryRegisterPeerNode(nodeOut);

            // Iterate through all child nodes of nodeOut
            // If the child node is a NetworkNodeWrapper, then we set it to dynamic spawn
            // We don't register it as a node in the NetworkRunner because only the parent needs registration
            NetworkRunner.Instance.AddChild(nodeOut.Node);
            var children = nodeOut.Node.GetChildren().ToList();
            List<NetworkNodeWrapper> networkChildren = new List<NetworkNodeWrapper>();
            while (children.Count > 0)
            {
                var child = children[0];
                var networkChild = new NetworkNodeWrapper(child);
                children.RemoveAt(0);
                if (networkChild != null && networkChild.Node.GetMeta("is_network_scene", false).AsBool())
                {
                    networkChild.Node.GetParent().RemoveChild(networkChild.Node);
                    networkChild.Node.QueueFree();
                    continue;
                }
                children.AddRange(child.GetChildren());
                if (networkChild == null) {
                    continue;
                }
                // Nested network scenes are spawned separately
                networkChild.DynamicSpawn = true;
                networkChild.InputAuthority = nodeOut.InputAuthority;
                networkChildren.Add(networkChild);
            }
            networkChildren.Reverse();
            NetworkRunner.Instance.RemoveChild(nodeOut.Node);

            if (data.parentId == 0)
            {
                peerStateController.ChangeScene(nodeOut);
                return;
            }

            if (data.hasInputAuthority == 1)
            {
                nodeOut.InputAuthority = NetworkRunner.Instance.ENetHost;
            }

            var networkSceneId = NetworkScenesRegister.SCENES_PACK[networkParent.Node.SceneFilePath];
            networkParent.Node.GetNode(NetworkScenesRegister.NODE_PATHS_MAP[networkSceneId][data.nodePathId]).AddChild(nodeOut.Node);
            if (nodeOut.Node is Node3D node) {
                node.Position = data.position;
                node.Rotation = data.rotation;
            }

            GD.Print("Spawned", nodeOut.Node.GetPath());
            foreach (var child in networkChildren)
            {
                child._NetworkPrepare();
            }
            nodeOut._NetworkPrepare();

            return;
        }

        public HLBuffer Export(IPeerStateController peerStateController, NetPeer peerId)
        {
            var buffer = new HLBuffer();
            if (peerStateController.HasSpawnedForClient(wrapper.NetworkId, peerId))
            {
                // The target client is already aware of this node.
                return buffer;
            }

            if (wrapper.NetworkParent != null && !peerStateController.HasSpawnedForClient(wrapper.NetworkParent.NetworkId, peerId))
            {
                // The parent node is not registered with the client yet, so we can't spawn this node
                return buffer;
            }

            var id = peerStateController.TryRegisterPeerNode(wrapper, peerId);
            if (id == 0)
            {
                // Unable to spawn this node. The client is already tracking the max amount.
                return buffer;
            }

            setupTicks[peerId] = peerStateController.CurrentTick;
            HLBytes.Pack(buffer, wrapper.NetworkSceneId);

            // Pack the node path
            if (wrapper.NetworkParent == null)
            {
                // If this scene has no parent, then it is the root scene
                // In other words, this indicates a scene change
                HLBytes.Pack(buffer, (byte)0);

                // We exit early because no other property is valid on a root scene
                return buffer;
            }

            // Pack the parent network ID and the node path
            var parentId = peerStateController.GetPeerNodeId(peerId, wrapper.NetworkParent);
            HLBytes.Pack(buffer, parentId);
            var networkSceneId = NetworkScenesRegister.SCENES_PACK[wrapper.NetworkParent.Node.SceneFilePath];
            HLBytes.Pack(buffer, NetworkScenesRegister.NODE_PATHS_PACK[networkSceneId][wrapper.NetworkParent.Node.GetPathTo(wrapper.Node.GetParent())]);

            if (wrapper.Node is Node3D node)
            {
                // TODO: Support Node2D
                HLBytes.Pack(buffer, node.Position);
                HLBytes.Pack(buffer, node.Rotation);
            }

            if (wrapper.InputAuthority == peerId)
            {
                HLBytes.Pack(buffer, (byte)1);
            }
            else
            {
                HLBytes.Pack(buffer, (byte)0);
            }

            return buffer;
        }

        public void Acknowledge(IPeerStateController peerStateController, NetPeer peer, Tick tick)
        {
            var peerTick = setupTicks.TryGetValue(peer, out var setupTick) ? setupTick : 0;
            if (setupTick == 0)
            {
                return;
            }

            if (tick >= peerTick)
            {
                peerStateController.SetSpawnedForClient(wrapper.NetworkId, peer);
            }
        }

        public void PhysicsProcess(double delta) { }

        public void Cleanup() { }
    }
}