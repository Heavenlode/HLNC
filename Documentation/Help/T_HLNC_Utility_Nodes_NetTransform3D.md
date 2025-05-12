# NetTransform3D Class


\[Missing &lt;summary&gt; documentation for "T:HLNC.Utility.Nodes.NetTransform3D"\]



## Definition
**Namespace:** <a href="N_HLNC_Utility_Nodes">HLNC.Utility.Nodes</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+7c8369b309950da5e6f9dfc534f2804635131157

**C#**
``` C#
public class NetTransform3D : NetNode3D
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  Node  →  Node3D  →  <a href="T_HLNC_NetNode3D">NetNode3D</a>  →  NetTransform3D</td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform3D__ctor">NetTransform3D</a></td>
<td>Initializes a new instance of the NetTransform3D class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_HLNC_Utility_Nodes_NetTransform3D_IsTeleporting">IsTeleporting</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utility_Nodes_NetTransform3D_NetPosition">NetPosition</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utility_Nodes_NetTransform3D_NetRotation">NetRotation</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utility_Nodes_NetTransform3D_SourceNode">SourceNode</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utility_Nodes_NetTransform3D_TargetNode">TargetNode</a></td>
<td> </td></tr>
</table>

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
<a href="N_HLNC_Utility_Nodes">HLNC.Utility.Nodes Namespace</a>  
