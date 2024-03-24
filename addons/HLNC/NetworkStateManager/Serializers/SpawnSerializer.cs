using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace HLNC.StateSerializers
{
    public class SpawnSerializer : IStateSerailizer {
		private Dictionary<PeerId, bool> spawnAware = new Dictionary<PeerId, bool>();

		private NetworkNode3D node;
		private Dictionary<PeerId, Tick> setupTicks = new Dictionary<PeerId, Tick>();
		public SpawnSerializer(NetworkNode3D node) {
			this.node = node;
		}
		public void Import(IGlobalNetworkState networkState, HLBuffer buffer, out NetworkNode3D nodeOut) {
			nodeOut = node;

			// If the node is already registered, then we don't need to spawn it again
			var result = networkState.TryRegisterPeerNode(nodeOut);
			if (result == 0) {
				// Skip classId
				HLBytes.UnpackInt8(buffer);

				// Skip hasInputAuthority
				HLBytes.UnpackInt8(buffer);

				return;
			}

			var classId = HLBytes.UnpackInt8(buffer);

			// Deregister and delete the node, because it is simply a "Placeholder" that doesn't really exist
			networkState.DeregisterPeerNode(nodeOut);
			node.QueueFree();
			
			// Replace the node with the desired scene
			var newNode = NetworkScenesRegister.SCENES_MAP[classId].Instantiate();
			nodeOut = (NetworkNode3D)newNode;

			// Contextually we already know the NetworkId
			nodeOut.NetworkId = node.NetworkId;

			networkState.TryRegisterPeerNode(nodeOut);
			nodeOut.DynamicSpawn = true;
			var hasInputAuthority = HLBytes.UnpackInt8(buffer);
			if (hasInputAuthority == 1) {
				nodeOut.InputAuthority = networkState.LocalPlayerId;
			}

			// Iterate through all child nodes of nodeOut
			// If the child node is a NetworkNode3D, then we set it to dynamic spawn
			// We don't register it as a node in the NetworkRunner because only the parent needs registration
			var children = nodeOut.GetChildren().ToList();
			while (children.Count > 0) {
				var child = children[0];
				children.RemoveAt(0);
				if (child is NetworkNode3D) {
					var childNode = (NetworkNode3D)child;
					childNode.DynamicSpawn = true;
					childNode.InputAuthority = nodeOut.InputAuthority;
					children.AddRange(childNode.GetChildren());
				}
			}

			networkState.CurrentScene.AddChild(nodeOut);
			return;
		}

        public HLBuffer Export(IGlobalNetworkState networkState, PeerId peerId) {
            var buffer = new HLBuffer();
			if (spawnAware.ContainsKey(peerId)) {
				// The target client is already aware of this node.
				return buffer;
			}
			var id = networkState.TryRegisterPeerNode(node, peerId);
			if (id == 0) {
				// Unable to spawn this node. The client is already tracking the max amount.
				return buffer;
			}

			HLBytes.Pack(buffer, NetworkScenesRegister.SCENES_PACK[node.SceneFilePath]);

			// Other data such as position and rotation are not packed as part of the Spawn data
			// Instead, it is handled by NetworkProperiesSerializer via NetworkTransform, etc.
			if (node.InputAuthority == peerId) {
				HLBytes.Pack(buffer, (byte)1);
			} else {
				HLBytes.Pack(buffer, (byte)0);
			}

			setupTicks[peerId] = networkState.CurrentTick;

			return buffer;
        }

		public void Acknowledge(IGlobalNetworkState networkState, PeerId peer, Tick tick) {
			var peerTick = setupTicks.TryGetValue(peer, out var setupTick) ? setupTick : 0;
			if (setupTick == 0) {
				return;
			}

			if (tick >= peerTick) {
				spawnAware[peer] = true;
			}
		}

		public void PhysicsProcess(double delta) {}

		public void Cleanup() { }
	}
}