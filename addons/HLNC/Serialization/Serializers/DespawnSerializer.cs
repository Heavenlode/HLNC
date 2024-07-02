using System.Collections.Generic;
using System.Threading.Tasks;
using HLNC.Serialization;

namespace HLNC.Serialization.Serializers
{
    internal class DespawnSerializer(NetworkNodeWrapper node) : IStateSerailizer
    {
        private struct Data
        {

        }

        private NetworkNodeWrapper node = node;

        private Data Deserialize(HLBuffer data)
        {
            return new Data();
        }

        public void Import(IPeerStateController peerStateController, HLBuffer buffer, out NetworkNodeWrapper nodeOut)
        {
            nodeOut = node;
            var data = Deserialize(buffer);
            return;
        }

        public void Cleanup() { }

        public HLBuffer Export(IPeerStateController peerStateController, PeerId peerId)
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

        public void Acknowledge(IPeerStateController peerStateController, PeerId peer, Tick tick)
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