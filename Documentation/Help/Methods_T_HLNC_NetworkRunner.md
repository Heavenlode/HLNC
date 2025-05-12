# NetworkRunner Methods




## Methods
<table>
<tr>
<td><a href="M_HLNC_NetworkRunner__EnterTree">_EnterTree</a></td>
<td><p>Called when the node enters the SceneTree (e.g. upon instantiating, scene changing, or after calling AddChild(Node, Boolean, InternalMode) in a script). If the node has children, its _EnterTree() callback will be called first, and then that of the children.</p><p>

Corresponds to the NotificationEnterTree notification in _Notification(Int32).</p><br />(Overrides Node._EnterTree())</td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner__OnPeerConnected">_OnPeerConnected</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner__OnPeerDisconnected">_OnPeerDisconnected</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner__PhysicsProcess">_PhysicsProcess</a></td>
<td><p>Called during the physics processing step of the main loop. Physics processing means that the frame rate is synced to the physics, i.e. the <em>delta</em> variable should be constant. <em>delta</em> is in seconds.</p><p>

It is only called if physics processing is enabled, which is done automatically if this method is overridden, and can be toggled with SetPhysicsProcess(Boolean).</p><p>

Corresponds to the NotificationPhysicsProcess notification in _Notification(Int32).</p><p><b>

Note:</b> This method is only called if the node is present in the scene tree (i.e. if it's not an orphan).</p><br />(Overrides Node._PhysicsProcess(Double))</td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner__Ready">_Ready</a></td>
<td><p>Called when the node is "ready", i.e. when both the node and its children have entered the scene tree. If the node has children, their _Ready() callbacks get triggered first, and the parent node will receive the ready notification afterwards.</p><p>

Corresponds to the NotificationReady notification in _Notification(Int32). See also the <code>@onready</code> annotation for variables.</p><p>

Usually used for initialization. For even earlier initialization, #ctor() may be used. See also _EnterTree().</p><p><b>

Note:</b> This method may be called only once for each node. After removing a node from the scene tree and adding it again, _Ready() will <b>not</b> be called a second time. This can be bypassed by requesting another call with RequestReady(), which may be called anywhere before adding the node again.</p><br />(Overrides Node._Ready())</td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_ChangeSceneInstance">ChangeSceneInstance</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_ChangeScenePacked">ChangeScenePacked</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_ChangeZone">ChangeZone</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_GetAllNetworkNodes">GetAllNetworkNodes</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_GetFromNetworkId">GetFromNetworkId</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_InstallBlastoffClientDriver">InstallBlastoffClientDriver</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_InstallBlastoffServerDriver">InstallBlastoffServerDriver</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_RegisterSpawn">RegisterSpawn</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_ServerProcessTick">ServerProcessTick</a></td>
<td>This method is executed every tick on the Server side, and kicks off all logic which processes and sends data to every client.</td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_Spawn">Spawn</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_StartClient">StartClient</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_StartServer">StartServer</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetworkRunner_TransferInput">TransferInput</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="T_HLNC_NetworkRunner">NetworkRunner Class</a>  
<a href="N_HLNC">HLNC Namespace</a>  
