# NetworkNode3D Class


Node3D, extended with HLNC networking capabilities. This is the most basic networked 3D object. On every network tick, all NetworkNode3D nodes in the scene tree automatically have their <a href="T_HLNC_NetworkProperty">network properties</a> updated with the latest data from the server. Then, the special <a href="M_HLNC_NetworkNode3D__NetworkProcess">NetworkProcess</a> method is called, which indicates that a network Tick is occurring. Network properties can only update on the server side. For a client to update network properties, they must send client inputs to the server via implementing the <a href="T_HLNC_INetworkInputHandler">INetworkInputHandler</a> interface. The server receives client inputs, can access them via <a href="M_HLNC_NetworkNode3D_GetInput">GetInput()</a>, and handle them accordingly within <a href="M_HLNC_NetworkNode3D__NetworkProcess">NetworkProcess</a> to mutate state.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
public class NetworkNode3D : Node3D, 
	IStateSerializable, INotifyPropertyChanged
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  Node  →  Node3D  →  NetworkNode3D</td></tr>
<tr><td><strong>Derived</strong></td><td><a href="T_HLNC_Utilities_NetworkAnimationPlayer">HLNC.Utilities.NetworkAnimationPlayer</a><br /><a href="T_HLNC_Utilities_NetworkTransform">HLNC.Utilities.NetworkTransform</a></td></tr>
<tr><td><strong>Implements</strong></td><td><a href="T_HLNC_Serialization_Serializers_IStateSerializable">IStateSerializable</a>, <a href="https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged" target="_blank" rel="noopener noreferrer">INotifyPropertyChanged</a></td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_NetworkNode3D__ctor">NetworkNode3D</a></td>
<td>Initializes a new instance of the NetworkNode3D class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_HLNC_NetworkNode3D_DynamicSpawn">DynamicSpawn</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkNode3D_InputAuthority">InputAuthority</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkNode3D_IsCurrentOwner">IsCurrentOwner</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkNode3D_IsNetworkReady">IsNetworkReady</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkNode3D_IsNetworkScene">IsNetworkScene</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkNode3D_NetworkId">NetworkId</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkNode3D_NetworkParent">NetworkParent</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkNode3D_NetworkParentId">NetworkParentId</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetworkNode3D_Serializers">Serializers</a></td>
<td> </td></tr>
</table>

## Methods
<table>
<tr>
<td><a href="M_HLNC_NetworkNode3D__ExitTree">_ExitTree</a></td>
<td><p>Called when the node is about to leave the SceneTree (e.g. upon freeing, scene changing, or after calling RemoveChild(Node) in a script). If the node has children, its _ExitTree() callback will be called last, after all its children have left the tree.</p><p>

Corresponds to the NotificationExitTree notification in _Notification(Int32) and signal TreeExiting. To get notified when the node has already left the active tree, connect to the TreeExited.</p><br />(Overrides Node._ExitTree())</td></tr>
<tr>
<td><a href="M_HLNC_NetworkNode3D__NetworkPrepare">_NetworkPrepare</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkNode3D__NetworkProcess">_NetworkProcess</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkNode3D__NetworkReady">_NetworkReady</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkNode3D__PhysicsProcess">_PhysicsProcess</a></td>
<td><p>Called during the physics processing step of the main loop. Physics processing means that the frame rate is synced to the physics, i.e. the <em>delta</em> variable should be constant. <em>delta</em> is in seconds.</p><p>

It is only called if physics processing is enabled, which is done automatically if this method is overridden, and can be toggled with SetPhysicsProcess(Boolean).</p><p>

Corresponds to the NotificationPhysicsProcess notification in _Notification(Int32).</p><p><b>

Note:</b> This method is only called if the node is present in the scene tree (i.e. if it's not an orphan).</p><br />(Overrides Node._PhysicsProcess(Double))</td></tr>
<tr>
<td><a href="M_HLNC_NetworkNode3D_Despawn">Despawn</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkNode3D_FindFromChild">FindFromChild</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkNode3D_FromJSON">FromJSON</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkNode3D_GetInput">GetInput</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkNode3D_GetNetworkChildren">GetNetworkChildren</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkNode3D_OnPropertyChanged">OnPropertyChanged(PropertyChangedEventArgs)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkNode3D_OnPropertyChanged_1">OnPropertyChanged(String)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkNode3D_ToJSON">ToJSON</a></td>
<td> </td></tr>
</table>

## Events
<table>
<tr>
<td><a href="E_HLNC_NetworkNode3D_NetworkPropertyChanged">NetworkPropertyChanged</a></td>
<td> </td></tr>
<tr>
<td><a href="E_HLNC_NetworkNode3D_PropertyChanged">PropertyChanged</a></td>
<td> </td></tr>
</table>

## Fields
<table>
<tr>
<td><a href="F_HLNC_NetworkNode3D_Interest">Interest</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
