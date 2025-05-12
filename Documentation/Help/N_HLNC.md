# HLNC Namespace


\[Missing &lt;summary&gt; documentation for "N:HLNC"\]



## Classes
<table>
<tr>
<td><a href="T_HLNC_NetworkNode3D">NetworkNode3D</a></td>
<td>Node3D, extended with HLNC networking capabilities. This is the most basic networked 3D object. On every network tick, all NetworkNode3D nodes in the scene tree automatically have their <a href="T_HLNC_NetworkProperty">network properties</a> updated with the latest data from the server. Then, the special <a href="M_HLNC_NetworkNode3D__NetworkProcess">NetworkProcess</a> method is called, which indicates that a network Tick is occurring. Network properties can only update on the server side. For a client to update network properties, they must send client inputs to the server via implementing the <a href="T_HLNC_INetworkInputHandler">INetworkInputHandler</a> interface. The server receives client inputs, can access them via <a href="M_HLNC_NetworkNode3D_GetInput">GetInput()</a>, and handle them accordingly within <a href="M_HLNC_NetworkNode3D__NetworkProcess">NetworkProcess</a> to mutate state.</td></tr>
<tr>
<td><a href="T_HLNC_NetworkNodeWrapper">NetworkNodeWrapper</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetworkProperty">NetworkProperty</a></td>
<td>Mark a property as being Networked. The NetworkPeerManager automatically processes these through the NetworkPropertiesSerializer to be optimally sent across the network. Only changes are networked. When the client receives a change on the property, if a method exists 

**C#**  
``` C#
OnNetworkChange{PropertyName}(int tick, T oldValue, T newValue)
```
 it will be called on the client side.</td></tr>
<tr>
<td><a href="T_HLNC_NetworkRunner">NetworkRunner</a></td>
<td>The primary network manager for server and client. NetworkRunner handles the ENet stream and passing that data to the correct objects. For more information on what kind of data is sent and received on what channels, see <a href="T_HLNC_NetworkRunner_ENetChannelId">NetworkRunner.ENetChannelId</a>.</td></tr>
<tr>
<td><a href="T_HLNC_NetworkScenes">NetworkScenes</a></td>
<td> </td></tr>
</table>

## Interfaces
<table>
<tr>
<td><a href="T_HLNC_IBlastoffClientDriver">IBlastoffClientDriver</a></td>
<td>Provides client logic for Blastoff communication. <a href="T_HLNC_NetworkRunner">NetworkRunner</a> calls these functions to handle Blastoff communications. For more info on Blastoff, visit the repo:</td></tr>
<tr>
<td><a href="T_HLNC_IBlastoffServerDriver">IBlastoffServerDriver</a></td>
<td>Provides server logic for Blastoff communication. <a href="T_HLNC_NetworkRunner">NetworkRunner</a> calls these functions to handle Blastoff communications. For more info on Blastoff, visit the repo: https://github.com/Heavenlode/Blastoff</td></tr>
<tr>
<td><a href="T_HLNC_INetworkInputHandler">INetworkInputHandler</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_IPeerStateController">IPeerStateController</a></td>
<td>Manages the state of a peer in the network, from the perspective of the server.</td></tr>
</table>

## Delegates
<table>
<tr>
<td><a href="T_HLNC_NetworkNode3D_NetworkPropertyChangedEventHandler">NetworkNode3D.NetworkPropertyChangedEventHandler</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetworkRunner_OnAfterNetworkTickEventHandler">NetworkRunner.OnAfterNetworkTickEventHandler</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetworkRunner_PlayerConnectedEventHandler">NetworkRunner.PlayerConnectedEventHandler</a></td>
<td> </td></tr>
</table>

## Enumerations
<table>
<tr>
<td><a href="T_HLNC_IPeerStateController_PeerSyncState">IPeerStateController.PeerSyncState</a></td>
<td>Unused</td></tr>
<tr>
<td><a href="T_HLNC_NetworkNode3D_NetworkChildrenSearchToggle">NetworkNode3D.NetworkChildrenSearchToggle</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetworkProperty_Flags">NetworkProperty.Flags</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetworkRunner_BlastoffCommands">NetworkRunner.BlastoffCommands</a></td>
<td>These are commands which the server may send to Blastoff, which informs Blastoff how to act upon the client connection.</td></tr>
<tr>
<td><a href="T_HLNC_NetworkRunner_ENetChannelId">NetworkRunner.ENetChannelId</a></td>
<td>Describes the channels of communication used by the network.</td></tr>
</table>