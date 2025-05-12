# IPeerStateController Methods




## Methods
<table>
<tr>
<td><a href="M_HLNC_IPeerStateController_ChangeScene">ChangeScene</a></td>
<td>Begin the process of changing the scene.</td></tr>
<tr>
<td><a href="M_HLNC_IPeerStateController_DeregisterPeerNode">DeregisterPeerNode</a></td>
<td>Remove the NetworkNode from the peer's registry.</td></tr>
<tr>
<td><a href="M_HLNC_IPeerStateController_GetNetworkNode">GetNetworkNode</a></td>
<td>Net a NetworkNode by ID.</td></tr>
<tr>
<td><a href="M_HLNC_IPeerStateController_GetPeerNodeId">GetPeerNodeId</a></td>
<td>Find the peer's ID for a network node as was registered in <a href="M_HLNC_IPeerStateController_TryRegisterPeerNode">TryRegisterPeerNode(NetworkNodeWrapper, ENetPacketPeer)</a></td></tr>
<tr>
<td><a href="M_HLNC_IPeerStateController_HasSpawnedForClient">HasSpawnedForClient</a></td>
<td>Check if a client has acknowledged a tick wherein a node was spawned.</td></tr>
<tr>
<td><a href="M_HLNC_IPeerStateController_SetSpawnedForClient">SetSpawnedForClient</a></td>
<td>Indicate that a client has acknowledged a tick wherein a node was spawned.</td></tr>
<tr>
<td><a href="M_HLNC_IPeerStateController_TryRegisterPeerNode">TryRegisterPeerNode</a></td>
<td>Attempts to register a NetworkNode for a peer. This is necessary because peers track different IDs for NetworkNodes than the server does. The reason why is that the Network tracks IDs as an int64, but we don't want to send a full int64 over the network for every node.</td></tr>
</table>

## See Also


#### Reference
<a href="T_HLNC_IPeerStateController">IPeerStateController Interface</a>  
<a href="N_HLNC">HLNC Namespace</a>  
