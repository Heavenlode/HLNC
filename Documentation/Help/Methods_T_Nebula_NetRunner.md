# NetRunner Methods




## Methods
<table>
<tr>
<td><a href="M_Nebula_NetRunner__EnterTree">_EnterTree</a></td>
<td><p>Called when the node enters the SceneTree (e.g. upon instantiating, scene changing, or after calling AddChild(Node, Boolean, InternalMode) in a script). If the node has children, its _EnterTree() callback will be called first, and then that of the children.</p><p>

Corresponds to the NotificationEnterTree notification in _Notification(Int32).</p><br />(Overrides Node._EnterTree())</td></tr>
<tr>
<td><a href="M_Nebula_NetRunner__OnPeerDisconnected">_OnPeerDisconnected</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner__PhysicsProcess">_PhysicsProcess</a></td>
<td><p>Called during the physics processing step of the main loop. Physics processing means that the frame rate is synced to the physics, i.e. the <em>delta</em> parameter will <em>generally</em> be constant (see exceptions below). <em>delta</em> is in seconds.</p><p>

It is only called if physics processing is enabled, which is done automatically if this method is overridden, and can be toggled with SetPhysicsProcess(Boolean).</p><p>

Processing happens in order of ProcessPhysicsPriority, lower priority values are called first. Nodes with the same priority are processed in tree order, or top to bottom as seen in the editor (also known as pre-order traversal).</p><p>

Corresponds to the NotificationPhysicsProcess notification in _Notification(Int32).</p><p><b>

Note:</b> This method is only called if the node is present in the scene tree (i.e. if it's not an orphan).</p><p><b>

Note:</b><em>delta</em> will be larger than expected if running at a framerate lower than PhysicsTicksPerSecond / MaxPhysicsStepsPerFrame FPS. This is done to avoid "spiral of death" scenarios where performance would plummet due to an ever-increasing number of physics steps per frame. This behavior affects both _Process(Double) and _PhysicsProcess(Double). As a result, avoid using <em>delta</em> for time measurements in real-world seconds. Use the Time singleton's methods for this purpose instead, such as GetTicksUsec().</p><br />(Overrides Node._PhysicsProcess(Double))</td></tr>
<tr>
<td><a href="M_Nebula_NetRunner__Ready">_Ready</a></td>
<td><br />(Overrides Node._Ready())</td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_CreateWorldPacked">CreateWorldPacked</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_EmitSignalOnWorldCreated">EmitSignalOnWorldCreated</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_GetPeer">GetPeer</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_GetPeerId">GetPeerId</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_SetupWorldInstance">SetupWorldInstance</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_StartClient">StartClient</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_NetRunner_StartServer">StartServer</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="T_Nebula_NetRunner">NetRunner Class</a>  
<a href="N_Nebula">Nebula Namespace</a>  
