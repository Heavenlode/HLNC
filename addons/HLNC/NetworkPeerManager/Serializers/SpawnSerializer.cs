using System.Collections.Generic;
using System.Linq;
using Godot;

namespace HLNC.StateSerializers
{
    internal class SpawnSerializer(NetworkNode3D node) : IStateSerailizer
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

        private NetworkNode3D node = node;
        private Dictionary<PeerId, Tick> setupTicks = [];

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

        public void Import(IGlobalNetworkState networkState, HLBuffer buffer, out NetworkNode3D nodeOut)
        {
            nodeOut = node;
            var data = Deserialize(buffer);

            // If the node is already registered, then we don't need to spawn it again
            var result = networkState.TryRegisterPeerNode(nodeOut);
            if (result == 0)
            {
                return;
            }

            var networkId = node.NetworkId;

            // Deregister and delete the node, because it is simply a "Placeholder" that doesn't really exist
            networkState.DeregisterPeerNode(nodeOut);
            node.QueueFree();

            var networkParent = networkState.GetNetworkNode(data.parentId);
            if (data.parentId != 0 && networkParent == null)
            {
                // The parent node is not registered, so we can't spawn this node
                GD.PrintErr("Parent node not found for: ", NetworkScenesRegister.SCENES_MAP[data.classId].ResourcePath, " - Parent ID: ", data.parentId);
                return;
            }

            var newNode = NetworkScenesRegister.SCENES_MAP[data.classId].Instantiate();
            nodeOut = (NetworkNode3D)newNode;
            nodeOut.NetworkParent = networkParent;
            nodeOut.NetworkId = networkId;
            networkState.TryRegisterPeerNode(nodeOut);
            nodeOut.DynamicSpawn = true;

            // Iterate through all child nodes of nodeOut
            // If the child node is a NetworkNode3D, then we set it to dynamic spawn
            // We don't register it as a node in the NetworkRunner because only the parent needs registration
            var children = nodeOut.GetChildren().ToList();
            while (children.Count > 0)
            {
                var child = children[0];
                children.RemoveAt(0);
                if (child is NetworkNode3D networkNode)
                {
                    // Nested network scenes are spawned separately
                    if (child.HasMeta("is_network_scene"))
                    {
                        nodeOut.RemoveChild(child);
                        continue;
                    }
                    networkNode.DynamicSpawn = true;
                    networkNode.InputAuthority = nodeOut.InputAuthority;
                    children.AddRange(networkNode.GetChildren());
                }
            }

            if (data.parentId == 0)
            {
                networkState.ChangeScene(nodeOut);
                return;
            }

            if (data.hasInputAuthority == 1)
            {
                nodeOut.InputAuthority = networkState.LocalPlayerId;
            }

            networkParent.GetNode(NetworkScenesRegister.NODE_PATHS_MAP[networkParent.NetworkSceneId][data.nodePathId]).AddChild(nodeOut);
            nodeOut.Position = data.position;
            nodeOut.Rotation = data.rotation;

            return;
        }

        public HLBuffer Export(IGlobalNetworkState networkState, PeerId peerId)
        {
            var buffer = new HLBuffer();
            if (node.SpawnAware.ContainsKey(peerId))
            {
                // The target client is already aware of this node.
                return buffer;
            }

            if (node.NetworkParent != null && !node.NetworkParent.SpawnAware.ContainsKey(peerId))
            {
                // The parent node is not registered with the client yet, so we can't spawn this node
                return buffer;
            }

            var id = networkState.TryRegisterPeerNode(node, peerId);
            if (id == 0)
            {
                // Unable to spawn this node. The client is already tracking the max amount.
                return buffer;
            }

            setupTicks[peerId] = networkState.CurrentTick;
            HLBytes.Pack(buffer, node.NetworkSceneId);

            // Pack the node path
            if (node.NetworkParent == null)
            {
                // If this scene has no parent, then it is the root scene
                // In other words, this indicates a scene change
                HLBytes.Pack(buffer, (byte)0);

                // We exit early because no other property is valid on a root scene
                return buffer;
            }

            // Pack the parent network ID and the node path
            var parentId = networkState.GetPeerNodeId(peerId, node.NetworkParent);
            HLBytes.Pack(buffer, parentId);
            HLBytes.Pack(buffer, NetworkScenesRegister.NODE_PATHS_PACK[node.NetworkParent.NetworkSceneId][node.NetworkParent.GetPathTo(node.GetParent())]);

            HLBytes.Pack(buffer, node.Position);
            HLBytes.Pack(buffer, node.Rotation);

            if (node.InputAuthority == peerId)
            {
                HLBytes.Pack(buffer, (byte)1);
            }
            else
            {
                HLBytes.Pack(buffer, (byte)0);
            }

            return buffer;
        }

        public void Acknowledge(IGlobalNetworkState networkState, PeerId peer, Tick tick)
        {
            var peerTick = setupTicks.TryGetValue(peer, out var setupTick) ? setupTick : 0;
            if (setupTick == 0)
            {
                return;
            }

            if (tick >= peerTick)
            {
                node.SpawnAware[peer] = true;
            }
        }

        public void PhysicsProcess(double delta) { }

        public void Cleanup() { }
    }
}