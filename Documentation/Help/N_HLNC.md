# HLNC Namespace


\[Missing &lt;summary&gt; documentation for "N:HLNC"\]



## Classes
<table>
<tr>
<td><a href="T_HLNC_NetFunction">NetFunction</a></td>
<td>Marks a method as a network function. Similar to an RPC.</td></tr>
<tr>
<td><a href="T_HLNC_NetId">NetId</a></td>
<td>A unique identifier for a networked object. The NetId for a node is different between the server and client. On the client side, a NetId is only a byte, whereas on the server side it is an int64. The server's <a href="T_HLNC_WorldRunner">WorldRunner</a> keeps a map of all NetIds to their corresponding value on each client for serialization.</td></tr>
<tr>
<td><a href="T_HLNC_NetNode">NetNode</a></td>
<td>Node, extended with HLNC networking capabilities. This is the most basic networked object. On every network tick, all NetNode nodes in the scene tree automatically have their <a href="T_HLNC_NetProperty">network properties</a> updated with the latest data from the server. Then, the special <a href="M_HLNC_NetNode__NetworkProcess">NetworkProcess</a> method is called, which indicates that a network Tick has occurred. Network properties can only update on the server side. For a client to update network properties, they must send client inputs to the server via implementing the [!:INetworkInputHandler] interface, or network function calls via <a href="T_HLNC_NetFunction">NetFunction</a> attributes. The server receives client inputs, can access them via <a href="M_HLNC_NetworkController_GetInput">GetInput()</a>, and handle them accordingly within <a href="M_HLNC_NetNode__NetworkProcess">NetworkProcess</a> to mutate state.</td></tr>
<tr>
<td><a href="T_HLNC_NetNode2D">NetNode2D</a></td>
<td>Node2D, extended with HLNC networking capabilities. This is the most basic networked 2D object. See <a href="T_HLNC_NetNode">NetNode</a> for more information.</td></tr>
<tr>
<td><a href="T_HLNC_NetNode3D">NetNode3D</a></td>
<td>Node3D, extended with HLNC networking capabilities. This is the most basic networked 3D object. See <a href="T_HLNC_NetNode">NetNode</a> for more information.</td></tr>
<tr>
<td><a href="T_HLNC_NetNodeWrapper">NetNodeWrapper</a></td>
<td>Helper class to safely interface with NetNodes across languages (e.g. C#, GDScript) or in situations where the node type is unknown.</td></tr>
<tr>
<td><a href="T_HLNC_NetProperty">NetProperty</a></td>
<td>Mark a property as being Networked. The <a href="T_HLNC_WorldRunner">WorldRunner</a> automatically processes these through the <a href="T_HLNC_Serialization_Serializers_NetPropertiesSerializer">NetPropertiesSerializer</a> to be optimally sent across the network. Only changes are networked. When the NetNode receives a change on the property, it will also attempt to call a method 

**C#**  
``` C#
NetNode.OnNetworkChange{PropertyName}(int tick, T oldValue, T newValue)
```
 on the client side if it exists.</td></tr>
<tr>
<td><a href="T_HLNC_NetRunner">NetRunner</a></td>
<td>The primary network manager for server and client. NetRunner handles the ENet stream and passing that data to the correct objects. For more information on what kind of data is sent and received on what channels, see <a href="T_HLNC_NetRunner_ENetChannelId">NetRunner.ENetChannelId</a>.</td></tr>
<tr>
<td><a href="T_HLNC_NetworkController">NetworkController</a></td>
<td>Manages the network state of a <a href="T_HLNC_NetNode">NetNode</a> (including <a href="T_HLNC_NetNode2D">NetNode2D</a> and <a href="T_HLNC_NetNode3D">NetNode3D</a>).</td></tr>
<tr>
<td><a href="T_HLNC_UUID">UUID</a></td>
<td>A UUID implementation for HLNC. Serializes into 16 bytes.</td></tr>
<tr>
<td><a href="T_HLNC_WorldRunner">WorldRunner</a></td>
<td>Manages the network state of all <a href="T_HLNC_NetNode">NetNode</a>s in the scene. Inside the <a href="T_HLNC_NetRunner">NetRunner</a> are one or more “Worlds”. Each World represents some part of the game that is isolated from other parts. For example, different maps, dungeon instances, etc. Worlds are dynamically created by calling [!:NetRunner.CreateWorld]. Worlds cannot directly interact with each other and do not share state. Players only exist in one World at a time, so it can be helpful to think of the clients as being connected to a World directly.</td></tr>
</table>

## Structures
<table>
<tr>
<td><a href="T_HLNC_WorldRunner_PeerState">WorldRunner.PeerState</a></td>
<td> </td></tr>
</table>

## Interfaces
<table>
<tr>
<td><a href="T_HLNC_INetNode">INetNode</a></td>
<td> </td></tr>
</table>

## Delegates
<table>
<tr>
<td><a href="T_HLNC_NetRunner_OnWorldCreatedEventHandler">NetRunner.OnWorldCreatedEventHandler</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetworkController_InterestChangedEventHandler">NetworkController.InterestChangedEventHandler</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetworkController_NetPropertyChangedEventHandler">NetworkController.NetPropertyChangedEventHandler</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetworkController_Yolo">NetworkController.Yolo</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_WorldRunner_OnAfterNetworkTickEventHandler">WorldRunner.OnAfterNetworkTickEventHandler</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_WorldRunner_OnPeerSyncStatusChangeEventHandler">WorldRunner.OnPeerSyncStatusChangeEventHandler</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_WorldRunner_OnPlayerJoinedEventHandler">WorldRunner.OnPlayerJoinedEventHandler</a></td>
<td> </td></tr>
</table>

## Enumerations
<table>
<tr>
<td><a href="T_HLNC_NetFunction_NetworkSources">NetFunction.NetworkSources</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetProperty_SyncFlags">NetProperty.SyncFlags</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetRunner_ENetChannelId">NetRunner.ENetChannelId</a></td>
<td>Describes the channels of communication used by the network.</td></tr>
<tr>
<td><a href="T_HLNC_NetworkController_NetworkChildrenSearchToggle">NetworkController.NetworkChildrenSearchToggle</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_WorldRunner_DebugDataType">WorldRunner.DebugDataType</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_WorldRunner_PeerSyncStatus">WorldRunner.PeerSyncStatus</a></td>
<td> </td></tr>
</table>