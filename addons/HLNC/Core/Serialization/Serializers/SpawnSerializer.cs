using System.Collections.Generic;
using System.Linq;
using Godot;
using HLNC.Utils;

namespace HLNC.Serialization.Serializers
{
    internal partial class SpawnSerializer : Node, IStateSerializer
    {
        private struct Data
        {
            public byte classId;
            public byte parentId;
            public byte nodePathId;
            public Vector3 position;
            public Vector3 rotation;
            public byte hasInputAuthority;
        }

        private NetworkNodeWrapper wrapper;

        public override void _EnterTree()
        {
            base._EnterTree();
            Name = "SpawnSerializer";
            wrapper = new NetworkNodeWrapper(GetParent());
        }
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
            spawnData.nodePathId = HLBytes.UnpackByte(data);
            spawnData.position = HLBytes.UnpackVector3(data);
            spawnData.rotation = HLBytes.UnpackVector3(data);
            spawnData.hasInputAuthority = HLBytes.UnpackByte(data);
            return spawnData;
        }

        public void Begin() {}

        public void Import(WorldRunner currentWorld, HLBuffer buffer, out NetworkNodeWrapper nodeOut)
        {
            nodeOut = wrapper;
            var data = Deserialize(buffer);

            // If the node is already registered, then we don't need to spawn it again
            var result = currentWorld.TryRegisterPeerNode(nodeOut);
            if (result == 0)
            {
                return;
            }

            var networkId = wrapper.NetworkId;

            // Deregister and delete the node, because it is simply a "Placeholder" that doesn't really exist
            currentWorld.DeregisterPeerNode(nodeOut);
            wrapper.Node.QueueFree();

            var networkParent = currentWorld.GetNetworkNode(data.parentId);
            if (data.parentId != 0 && networkParent == null)
            {
                // The parent node is not registered, so we can't spawn this node
                Debugger.Log($"Parent node not found for: {NetworkScenesRegister.UnpackScene(data.classId).ResourcePath} - Parent ID: {data.parentId}", Debugger.DebugLevel.ERROR);
                return;
            }

            NetworkRunner.Instance.RemoveChild(nodeOut.Node);
            var newNode = NetworkScenesRegister.UnpackScene(data.classId).Instantiate<NetworkNode3D>();
            newNode.IsClientSpawn = true;
            newNode.NetworkId = networkId;
            newNode.CurrentWorld = currentWorld;
            newNode.SetupSerializers();
            NetworkRunner.Instance.AddChild(newNode);
            nodeOut = new NetworkNodeWrapper(newNode);
            if (networkParent != null)
            {
                nodeOut.NetworkParentId = networkParent.NetworkId;
            }
            currentWorld.TryRegisterPeerNode(nodeOut);

            // Iterate through all child nodes of nodeOut
            // If the child node is a NetworkNodeWrapper, then we set it to dynamic spawn
            // We don't register it as a node in the NetworkRunner because only the parent needs registration
            var children = nodeOut.Node.GetChildren().ToList();
            List<NetworkNodeWrapper> networkChildren = new List<NetworkNodeWrapper>();
            while (children.Count > 0)
            {
                var child = children[0];
                var networkChild = new NetworkNodeWrapper(child);
                children.RemoveAt(0);
                if (networkChild != null && networkChild.IsNetworkScene)
                {
                    // Nested network scenes are spawned separately
                    networkChild.Node.GetParent().RemoveChild(networkChild.Node);
                    networkChild.Node.QueueFree();
                    continue;
                }
                children.AddRange(child.GetChildren());
                if (networkChild == null) {
                    continue;
                }
                networkChild.IsClientSpawn = true;
                networkChild.InputAuthority = nodeOut.InputAuthority;
                networkChildren.Add(networkChild);
            }
            networkChildren.Reverse();
            NetworkRunner.Instance.RemoveChild(nodeOut.Node);

            if (data.parentId == 0)
            {
                currentWorld.ChangeScene(nodeOut);
                return;
            }

            if (data.hasInputAuthority == 1)
            {
                nodeOut.InputAuthority = NetworkRunner.Instance.ENetHost;
            }

            networkParent.Node.GetNode(NetworkScenesRegister.UnpackNode(networkParent.Node.SceneFilePath, data.nodePathId)).AddChild(nodeOut.Node);
            if (nodeOut.Node is Node3D node) {
                // node.Position = data.position;
                // node.Rotation = data.rotation;
            }

            nodeOut._NetworkPrepare(currentWorld);
            nodeOut._WorldReady();

            return;
        }

        public HLBuffer Export(WorldRunner currentWorld, NetPeer peer)
        {
            var buffer = new HLBuffer();
            if (currentWorld.HasSpawnedForClient(wrapper.NetworkId, peer))
            {
                // The target client is already aware of this node.
                return buffer;
            }

            if (wrapper.NetworkParent != null && !currentWorld.HasSpawnedForClient(wrapper.NetworkParent.NetworkId, peer))
            {
                // The parent node is not registered with the client yet, so we can't spawn this node
                return buffer;
            }

            if (wrapper.Node is NetworkNode3D networkNode) {
                // TODO: Maybe this should exist in the node wrapper?
                if (!networkNode.spawnReady.GetValueOrDefault(peer, false))
                {
                    networkNode.PrepareSpawn(peer);
                    // The node is not ready to be spawned yet
                    return buffer;
                }
            }

            var id = currentWorld.TryRegisterPeerNode(wrapper, peer);
            if (id == 0)
            {
                // Unable to spawn this node. The client is already tracking the max amount.
                return buffer;
            }

            setupTicks[peer] = currentWorld.CurrentTick;
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
            var parentId = currentWorld.GetPeerNodeId(peer, wrapper.NetworkParent);
            HLBytes.Pack(buffer, parentId);
            if (NetworkScenesRegister.PackNode(wrapper.NetworkParent.Node.SceneFilePath, wrapper.NetworkParent.Node.GetPathTo(wrapper.Node.GetParent()), out var nodePathId))
            {
                HLBytes.Pack(buffer, nodePathId);
            } else {
                throw new System.Exception($"FAILED TO PACK FOR SPAWN: Node path not found for {wrapper.Node.GetPath()}");
            }

            if (wrapper.Node is Node3D node)
            {
                // TODO: Support Node2D
                HLBytes.Pack(buffer, node.Position);
                HLBytes.Pack(buffer, node.Rotation);
            }

            if (wrapper.InputAuthority == peer)
            {
                HLBytes.Pack(buffer, (byte)1);
            }
            else
            {
                HLBytes.Pack(buffer, (byte)0);
            }

            return buffer;
        }

        public void Acknowledge(WorldRunner currentWorld, NetPeer peer, Tick tick)
        {
            var peerTick = setupTicks.TryGetValue(peer, out var setupTick) ? setupTick : 0;
            if (setupTick == 0)
            {
                return;
            }

            if (tick >= peerTick)
            {
                currentWorld.SetSpawnedForClient(wrapper.NetworkId, peer);
            }
        }

        public void PhysicsProcess(double delta) { }

        public void Cleanup() { }
    }
}