using Godot;

namespace HLNC
{
    /// <summary>
    /// Manages the state of a peer in the network, from the perspective of the server.
    /// </summary>
    public interface IPeerStateController
    {

        /// <summary>
        /// Unused
        /// </summary>
        public enum PeerSyncState {
            /// <summary>
            /// Unused
            /// </summary>
            INITIAL
        }

        /// <summary>
        /// Attempts to register a NetworkNode for a peer. This is necessary because peers track different IDs for NetworkNodes than the server does.
        /// The reason why is that the Network tracks IDs as an int64, but we don't want to send a full int64 over the network for every node.
        /// </summary>
        /// <param name="node">The NetworkNode to register with the peer</param>
        /// <param name="peer">The peer in question</param>
        /// <returns></returns>
        public byte TryRegisterPeerNode(NetworkNodeWrapper node, NetPeer peer = null);

        /// <summary>
        /// Remove the NetworkNode from the peer's registry.
        /// </summary>
        /// <param name="node">The NetworkNode to remove</param>
        /// <param name="peer">The peer in question</param>
        public void DeregisterPeerNode(NetworkNodeWrapper node, NetPeer peer = null);

        /// <summary>
        /// The most recently acknowledged Tick from the peer. This is not necessarily the current peer's actual Tick (almost certainly slightly behind)
        /// </summary>
        public Tick CurrentTick { get; }

        /// <summary>
        /// Begin the process of changing the scene.
        /// </summary>
        /// <param name="node">The Networked Scene node to change to</param>
        public void ChangeScene(NetworkNodeWrapper node);

        /// <summary>
        /// Find the peer's ID for a network node as was registered in <see cref="TryRegisterPeerNode(NetworkNodeWrapper, ENetPacketPeer)"/>
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public byte GetPeerNodeId(NetPeer peer, NetworkNodeWrapper node);

        /// <summary>
        /// Net a NetworkNode by ID.
        /// </summary>
        /// <param name="networkId">The ID of the node. This will be different between the server and the client.</param>
        /// <returns></returns>
        public NetworkNodeWrapper GetNetworkNode(NetworkId networkId);

        /// <summary>
        /// Check if a client has acknowledged a tick wherein a node was spawned.
        /// </summary>
        /// <param name="networkId">The server's NetworkId for that node.</param>
        /// <param name="peer">The peer in question</param>
        /// <returns></returns>
        public bool HasSpawnedForClient(NetworkId networkId, NetPeer peer);

        /// <summary>
        /// Indicate that a client has acknowledged a tick wherein a node was spawned.
        /// </summary>
        /// <param name="networkId">The server's NetworkId for that node.</param>
        /// <param name="peer">The peer in question</param>
        /// <returns></returns>
        public void SetSpawnedForClient(NetworkId networkId, NetPeer peer);
    }

}