using Godot;

namespace HLNC
{
    public interface IPeerStateController
    {
        public enum PeerSyncState
        {
            INITIAL
        }
        public byte TryRegisterPeerNode(NetworkNodeWrapper node, PeerId? peer = null);
        public void DeregisterPeerNode(NetworkNodeWrapper node, PeerId? peer = null);
        public PeerId LocalPlayerId { get; }
        public Tick CurrentTick { get; }
        public void ChangeScene(NetworkNodeWrapper node);
        public byte GetPeerNodeId(PeerId peer, NetworkNodeWrapper node);
        public NetworkNodeWrapper GetNetworkNode(NetworkId networkId);
        public bool HasSpawnedForClient(NetworkId networkId, PeerId peer);
        public void SetSpawnedForClient(NetworkId networkId, PeerId peer);
    }

}