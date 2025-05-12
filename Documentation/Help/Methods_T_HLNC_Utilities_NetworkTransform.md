# NetworkTransform Methods




## Methods
<table>
<tr>
<td><a href="M_HLNC_Utilities_NetworkTransform__NetworkProcess">_NetworkProcess</a></td>
<td><br />(Overrides <a href="M_HLNC_NetworkNode3D__NetworkProcess">NetworkNode3D._NetworkProcess(Int32)</a>)</td></tr>
<tr>
<td><a href="M_HLNC_Utilities_NetworkTransform__NetworkReady">_NetworkReady</a></td>
<td><br />(Overrides <a href="M_HLNC_NetworkNode3D__NetworkReady">NetworkNode3D._NetworkReady()</a>)</td></tr>
<tr>
<td><a href="M_HLNC_Utilities_NetworkTransform__PhysicsProcess">_PhysicsProcess</a></td>
<td><p>Called during the physics processing step of the main loop. Physics processing means that the frame rate is synced to the physics, i.e. the <em>delta</em> variable should be constant. <em>delta</em> is in seconds.</p><p>

It is only called if physics processing is enabled, which is done automatically if this method is overridden, and can be toggled with SetPhysicsProcess(Boolean).</p><p>

Corresponds to the NotificationPhysicsProcess notification in _Notification(Int32).</p><p><b>

Note:</b> This method is only called if the node is present in the scene tree (i.e. if it's not an orphan).</p><br />(Overrides <a href="M_HLNC_NetworkNode3D__PhysicsProcess">

NetworkNode3D._PhysicsProcess(Double)</a>)</td></tr>
<tr>
<td><a href="M_HLNC_Utilities_NetworkTransform_Face">Face</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utilities_NetworkTransform_GetParent3D">GetParent3D</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utilities_NetworkTransform_NetworkLerpNetPosition">NetworkLerpNetPosition</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utilities_NetworkTransform_NetworkLerpNetRotation">NetworkLerpNetRotation</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utilities_NetworkTransform_OnNetworkChangeIsTeleporting">OnNetworkChangeIsTeleporting</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utilities_NetworkTransform_Teleport">Teleport</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="T_HLNC_Utilities_NetworkTransform">NetworkTransform Class</a>  
<a href="N_HLNC_Utilities">HLNC.Utilities Namespace</a>  
