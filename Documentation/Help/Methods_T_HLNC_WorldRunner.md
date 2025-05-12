# WorldRunner Methods




## Methods
<table>
<tr>
<td><a href="M_HLNC_WorldRunner__ExitTree">_ExitTree</a></td>
<td><br />(Overrides Node._ExitTree())</td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner__PhysicsProcess">_PhysicsProcess</a></td>
<td><br />(Overrides Node._PhysicsProcess(Double))</td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner__Ready">_Ready</a></td>
<td><br />(Overrides Node._Ready())</td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_AllocateNetId">AllocateNetId()</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_AllocateNetId_1">AllocateNetId(Byte)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_ChangeScene">ChangeScene</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_CheckStaticInitialization">CheckStaticInitialization</a></td>
<td>This is called for nodes that are initialized in a scene by default. Clients automatically dequeue all network nodes on initialization. All network nodes on the client side must come from the server by gaining Interest in the node.</td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_ClientHandleTick">ClientHandleTick</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_EmitSignalOnAfterNetworkTick">EmitSignalOnAfterNetworkTick</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_EmitSignalOnPeerSyncStatusChange">EmitSignalOnPeerSyncStatusChange</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_EmitSignalOnPlayerJoined">EmitSignalOnPlayerJoined</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_GetNetId">GetNetId</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_GetNetIdFromPeerId">GetNetIdFromPeerId</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_GetNodeFromNetId_1">GetNodeFromNetId(Int64)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_GetNodeFromNetId">GetNodeFromNetId(NetId)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_GetPeerNode">GetPeerNode</a></td>
<td>Get the network node from a peer and a network ID relative to that peer.</td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_GetPeerNodeId">GetPeerNodeId</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_GetPeerWorldState">GetPeerWorldState(ENetPacketPeer)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_GetPeerWorldState_1">GetPeerWorldState(UUID)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_HasSpawnedForClient">HasSpawnedForClient</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_Log">Log</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_PeerAcknowledge">PeerAcknowledge</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_QueuePeerState">QueuePeerState</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_ServerProcessTick">ServerProcessTick</a></td>
<td>This method is executed every tick on the Server side, and kicks off all logic which processes and sends data to every client.</td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_SetPeerState">SetPeerState(ENetPacketPeer, WorldRunner.PeerState)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_SetPeerState_1">SetPeerState(UUID, WorldRunner.PeerState)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_SetSpawnedForClient">SetSpawnedForClient</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_WorldRunner_Spawn__1">Spawn(T)</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="T_HLNC_WorldRunner">WorldRunner Class</a>  
<a href="N_HLNC">HLNC Namespace</a>  
