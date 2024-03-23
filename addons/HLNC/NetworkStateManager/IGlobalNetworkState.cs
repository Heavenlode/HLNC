using Godot;

namespace HLNC
{

	public interface IGlobalNetworkState
	{
		public byte TryRegisterPeerNode(NetworkNode3D node, PeerId? peer = null);
		public void DeregisterPeerNode(NetworkNode3D node, PeerId? peer = null);
		public PeerId LocalPlayerId { get; }
		public Tick CurrentTick { get; }
		public Node CurrentScene { get; }
		public byte GetPeerNodeId(PeerId peer, NetworkNode3D node);
		public NetworkNode3D GetNetworkNode(NetworkId networkId);
	}

}