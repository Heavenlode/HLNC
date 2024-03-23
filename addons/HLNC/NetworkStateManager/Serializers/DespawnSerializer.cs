using System.Collections.Generic;
using System.Threading.Tasks;

namespace HLNC.StateSerializers
{
	public class DespawnSerializer : IStateSerailizer
	{

		private Dictionary<PeerId, bool> interestCache = new Dictionary<PeerId, bool>();
		private NetworkNode3D node;

		public DespawnSerializer(NetworkNode3D node)
		{
			this.node = node;
		}

		public void Import(IGlobalNetworkState networkState, HLBuffer buffer, out NetworkNode3D nodeOut)
		{
			nodeOut = node;
			return;
		}

		public HLBuffer Export(IGlobalNetworkState networkState, PeerId peerId)
		{
			var buffer = new HLBuffer();
			// Dictionary<PeerId, HLBuffer> despawnsBuffer = new Dictionary<PeerId, HLBuffer>();
			// foreach (int peer_id in NetworkRunner.Instance.MultiplayerInstance.GetPeers())
			// {
			// 	despawnsBuffer[peer_id] = new HLBuffer();
			// }
			// foreach (int peer_id in DespawnBuffers.Keys)
			// {
			// 	if (!despawnsBuffer.ContainsKey(peer_id))
			// 		continue;

			// 	List<int> despawn_ids = new List<int>();
			// 	foreach (int tick_number in DespawnBuffers[peer_id].Keys)
			// 	{
			// 		foreach (int network_id in DespawnBuffers[peer_id][tick_number])
			// 		{
			// 			despawn_ids.Add(network_id);
			// 		}
			// 	}
			// 	HLBytes.Pack(despawnsBuffer[peer_id], despawn_ids.ToArray());
			// }

			// return despawnsBuffer;
			return buffer;
		}

		public void Acknowledge(IGlobalNetworkState networkState, PeerId peer, Tick tick)
		{
			// if (!DespawnBuffers.ContainsKey(peer))
			// 	return;
			// if (!DespawnBuffers[peer].ContainsKey(tick))
			// 	return;
			// foreach (int network_id in DespawnBuffers[peer][tick])
			// {
			// 	if (NetworkRunner.Instance.NetworkNodes.ContainsKey(network_id))
			// 	{
			// 		NetworkRunner.Instance.NetworkNodes[network_id].QueueFree();
			// 	}
			// }
			// DespawnBuffers[peer].Remove(tick);
		}
		public void PhysicsProcess(double delta)
		{
		}
	}

}