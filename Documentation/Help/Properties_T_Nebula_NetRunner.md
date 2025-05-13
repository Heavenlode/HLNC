# NetRunner Properties




## Properties
<table>
<tr>
<td><a href="P_Nebula_NetRunner_Instance">Instance</a></td>
<td>The singleton instance.</td></tr>
<tr>
<td><a href="P_Nebula_NetRunner_IsClient">IsClient</a></td>
<td> </td></tr>
<tr>
<td><a href="P_Nebula_NetRunner_IsServer">IsServer</a></td>
<td>This is set after <a href="M_Nebula_NetRunner_StartClient">StartClient()</a> or <a href="M_Nebula_NetRunner_StartServer">StartServer()</a> is called, i.e. when <a href="P_Nebula_NetRunner_NetStarted">NetStarted</a> == true. Before that, this value is unreliable.</td></tr>
<tr>
<td><a href="P_Nebula_NetRunner_MTU">MTU</a></td>
<td>Maximum Transferrable Unit. The maximum number of bytes that should be sent in a single ENet UDP Packet (i.e. a single tick) Not a hard limit.</td></tr>
<tr>
<td><a href="P_Nebula_NetRunner_NetStarted">NetStarted</a></td>
<td>This is set to true once <a href="M_Nebula_NetRunner_StartClient">StartClient()</a> or <a href="M_Nebula_NetRunner_StartServer">StartServer()</a> have succeeded.</td></tr>
<tr>
<td><a href="P_Nebula_NetRunner_Worlds">Worlds</a></td>
<td>The current World ID. This is mainly used for Blastoff.</td></tr>
</table>

## See Also


#### Reference
<a href="T_Nebula_NetRunner">NetRunner Class</a>  
<a href="N_Nebula">Nebula Namespace</a>  
