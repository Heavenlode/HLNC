# NetworkNode3D Methods




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

## See Also


#### Reference
<a href="T_HLNC_NetworkNode3D">NetworkNode3D Class</a>  
<a href="N_HLNC">HLNC Namespace</a>  
