# NetTransform3D Methods




## Methods
<table>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform3D__NetworkProcess">_NetworkProcess</a></td>
<td><br />(Overrides <a href="M_HLNC_NetNode3D__NetworkProcess">NetNode3D._NetworkProcess(Int32)</a>)</td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform3D__PhysicsProcess">_PhysicsProcess</a></td>
<td><p>Called during the physics processing step of the main loop. Physics processing means that the frame rate is synced to the physics, i.e. the <em>delta</em> parameter will <em>generally</em> be constant (see exceptions below). <em>delta</em> is in seconds.</p><p>

It is only called if physics processing is enabled, which is done automatically if this method is overridden, and can be toggled with SetPhysicsProcess(Boolean).</p><p>

Processing happens in order of ProcessPhysicsPriority, lower priority values are called first. Nodes with the same priority are processed in tree order, or top to bottom as seen in the editor (also known as pre-order traversal).</p><p>

Corresponds to the NotificationPhysicsProcess notification in _Notification(Int32).</p><p><b>

Note:</b> This method is only called if the node is present in the scene tree (i.e. if it's not an orphan).</p><p><b>

Note:</b><em>delta</em> will be larger than expected if running at a framerate lower than PhysicsTicksPerSecond / MaxPhysicsStepsPerFrame FPS. This is done to avoid "spiral of death" scenarios where performance would plummet due to an ever-increasing number of physics steps per frame. This behavior affects both _Process(Double) and _PhysicsProcess(Double). As a result, avoid using <em>delta</em> for time measurements in real-world seconds. Use the Time singleton's methods for this purpose instead, such as GetTicksUsec().</p><br />(Overrides <a href="M_HLNC_NetNode3D__PhysicsProcess">

NetNode3D._PhysicsProcess(Double)</a>)</td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform3D__WorldReady">_WorldReady</a></td>
<td><br />(Overrides <a href="M_HLNC_NetNode3D__WorldReady">NetNode3D._WorldReady()</a>)</td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform3D_Face">Face</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform3D_GetParent3D">GetParent3D</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform3D_NetworkLerpNetPosition">NetworkLerpNetPosition</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform3D_NetworkLerpNetRotation">NetworkLerpNetRotation</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform3D_OnNetworkChangeIsTeleporting">OnNetworkChangeIsTeleporting</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform3D_Teleport">Teleport</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="T_HLNC_Utility_Nodes_NetTransform3D">NetTransform3D Class</a>  
<a href="N_HLNC_Utility_Nodes">HLNC.Utility.Nodes Namespace</a>  
