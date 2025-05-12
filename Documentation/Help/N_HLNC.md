# HLNC Namespace


\[Missing &lt;summary&gt; documentation for "N:HLNC"\]



## Classes
<table>
<tr>
<td><a href="T_HLNC_NetFunction">NetFunction</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetId">NetId</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetNode">NetNode</a></td>
<td>Node, extended with HLNC networking capabilities. This is the most basic networked 3D object. On every network tick, all NetNode nodes in the scene tree automatically have their <a href="T_HLNC_NetProperty">network properties</a> updated with the latest data from the server. Then, the special <a href="M_HLNC_NetNode__NetworkProcess">NetworkProcess</a> method is called, which indicates that a network Tick has occurred. Network properties can only update on the server side. For a client to update network properties, they must send client inputs to the server via implementing the [!:INetworkInputHandler] interface, or network function calls via <a href="T_HLNC_NetFunction">NetFunction</a> attributes. The server receives client inputs, can access them via [!:GetInput], and handle them accordingly within <a href="M_HLNC_NetNode__NetworkProcess">NetworkProcess</a> to mutate state.</td></tr>
<tr>
<td><a href="T_HLNC_NetNode2D">NetNode2D</a></td>
<td>Node2D, extended with HLNC networking capabilities. This is the most basic networked 3D object. On every network tick, all NetNode2D nodes in the scene tree automatically have their <a href="T_HLNC_NetProperty">network properties</a> updated with the latest data from the server. Then, the special <a href="M_HLNC_NetNode2D__NetworkProcess">NetworkProcess</a> method is called, which indicates that a network Tick has occurred. Network properties can only update on the server side. For a client to update network properties, they must send client inputs to the server via implementing the [!:INetworkInputHandler] interface, or network function calls via <a href="T_HLNC_NetFunction">NetFunction</a> attributes. The server receives client inputs, can access them via [!:GetInput], and handle them accordingly within <a href="M_HLNC_NetNode2D__NetworkProcess">NetworkProcess</a> to mutate state.</td></tr>
<tr>
<td><a href="T_HLNC_NetNode3D">NetNode3D</a></td>
<td>Node3D, extended with HLNC networking capabilities. This is the most basic networked 3D object. On every network tick, all NetNode3D nodes in the scene tree automatically have their network properties updated with the latest data from the server. Then, the special NetworkProcess method is called, which indicates that a network Tick has occurred. Network properties can only update on the server side. For a client to update network properties, they must send client inputs to the server via implementing the INetworkInputHandler interface, or network function calls via NetFunction attributes. The server receives client inputs, can access them via GetInput, and handle them accordingly within NetworkProcess to mutate state.</td></tr>
<tr>
<td><a href="T_HLNC_NetNodeWrapper">NetNodeWrapper</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_NetProperty">NetProperty</a></td>
<td>Mark a property as being Networked. The <a href="T_HLNC_WorldRunner">WorldRunner</a> automatically processes these through the NetPropertiesSerializer to be optimally sent across the network. Only changes are networked. When the client receives a change on the property, if a method exists 

**C#**  
``` C#
OnNetworkChange{PropertyName}(int tick, T oldValue, T newValue)
```
 it will be called on the client side.</td></tr>
<tr>
<td><a href="T_HLNC_NetRunner">NetRunner</a></td>
<td>The primary network manager for server and client. NetRunner handles the ENet stream and passing that data to the correct objects. For more information on what kind of data is sent and received on what channels, see <a href="T_HLNC_NetRunner_ENetChannelId">NetRunner.ENetChannelId</a>.</td></tr>
<tr>
<td><a href="T_HLNC_NetworkController">NetworkController</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_UUID">UUID</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_WorldRunner">WorldRunner</a></td>
<td> </td></tr>
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
<td><a href="T_HLNC_IBsonSerializable_1">IBsonSerializable(T)</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_IBsonSerializableBase">IBsonSerializableBase</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_INetNode">INetNode</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_INetSerializable_1">INetSerializable(T)</a></td>
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