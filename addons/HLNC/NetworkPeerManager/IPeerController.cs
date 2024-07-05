using Godot;

namespace HLNC
{
    public interface IPeerStateController
    {
        public enum PeerSyncState
        {
            INITIAL
        }
        public byte TryRegisterPeerNode(NetworkNodeWrapper node, NetPeer? peer = null);
        public void DeregisterPeerNode(NetworkNodeWrapper node, NetPeer? peer = null);
        public Tick CurrentTick { get; }
        public void ChangeScene(NetworkNodeWrapper node);
        public byte GetPeerNodeId(NetPeer peer, NetworkNodeWrapper node);
        public NetworkNodeWrapper GetNetworkNode(NetworkId networkId);
        public bool HasSpawnedForClient(NetworkId networkId, NetPeer peer);
        public void SetSpawnedForClient(NetworkId networkId, NetPeer peer);
    }

}