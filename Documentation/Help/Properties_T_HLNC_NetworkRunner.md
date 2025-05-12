# NetworkRunner Properties




## Properties
<table>
<tr>
<td><a href="P_HLNC_NetworkRunner_CurrentTick">CurrentTick</a></td>
<td>The current network tick. On the client side, this does not represent the server's current tick, which will always be slightly ahead.</td></tr>
<tr>
<td><a href="P_HLNC_NetworkRunner_InputStore">InputStore</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkRunner_Instance">Instance</a></td>
<td>The singleton instance.</td></tr>
<tr>
<td><a href="P_HLNC_NetworkRunner_IsServer">IsServer</a></td>
<td>This is set after <a href="M_HLNC_NetworkRunner_StartClient">StartClient()</a> or <a href="M_HLNC_NetworkRunner_StartServer">StartServer()</a> is called, i.e. when <a href="P_HLNC_NetworkRunner_NetStarted">NetStarted</a> == true. Before that, this value is unreliable.</td></tr>
<tr>
<td><a href="P_HLNC_NetworkRunner_NetStarted">NetStarted</a></td>
<td>This is set to true once <a href="M_HLNC_NetworkRunner_StartClient">StartClient()</a> or <a href="M_HLNC_NetworkRunner_StartServer">StartServer()</a> have succeeded.</td></tr>
<tr>
<td><a href="P_HLNC_NetworkRunner_ZoneInstanceId">ZoneInstanceId</a></td>
<td>The current Zone ID. This is mainly used for Blastoff.</td></tr>
</table>

## See Also


#### Reference
<a href="T_HLNC_NetworkRunner">NetworkRunner Class</a>  
<a href="N_HLNC">HLNC Namespace</a>  
