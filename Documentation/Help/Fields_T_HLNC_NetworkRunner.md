# NetworkRunner Fields




## Fields
<table>
<tr>
<td><a href="F_HLNC_NetworkRunner_CurrentScene">CurrentScene</a></td>
<td>The currently active root network scene. This should only be set via <a href="M_HLNC_NetworkRunner_ChangeSceneInstance">ChangeSceneInstance(NetworkNodeWrapper)</a> or <a href="M_HLNC_NetworkRunner_ChangeScenePacked">ChangeScenePacked(PackedScene)</a>.</td></tr>
<tr>
<td><a href="F_HLNC_NetworkRunner_MaxPeers">MaxPeers</a></td>
<td>The maximum number of allowed connections before the server starts rejecting clients.</td></tr>
<tr>
<td><a href="F_HLNC_NetworkRunner_MTU">MTU</a></td>
<td>Maximum Transferrable Unit. The maximum number of bytes that should be sent in a single ENet UDP Packet (i.e. a single tick) Not a hard limit.</td></tr>
<tr>
<td><a href="F_HLNC_NetworkRunner_PhysicsTicksPerNetworkTick">PhysicsTicksPerNetworkTick</a></td>
<td>This determines how fast the network sends data. When physics runs at 60 ticks per second, then at 2 PhysicsTicksPerNetworkTick, the network runs at 30hz.</td></tr>
<tr>
<td><a href="F_HLNC_NetworkRunner_Port">Port</a></td>
<td>The port for the server to listen on, and the client to connect to.</td></tr>
<tr>
<td><a href="F_HLNC_NetworkRunner_ServerAddress">ServerAddress</a></td>
<td>A fully qualified domain (www.example.com) or IP address (192.168.1.1) of the host. Used for client connections.</td></tr>
<tr>
<td><a href="F_HLNC_NetworkRunner_TPS">TPS</a></td>
<td>Ticks Per Second. The number of Ticks which are expected to elapse every second.</td></tr>
</table>

## See Also


#### Reference
<a href="T_HLNC_NetworkRunner">NetworkRunner Class</a>  
<a href="N_HLNC">HLNC Namespace</a>  
