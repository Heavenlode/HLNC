# NetTransform2D Class




## Definition
**Namespace:** <a href="N_HLNC_Utility_Nodes">HLNC.Utility.Nodes</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+53875a383067c2ec9e9ab8259c59e7345e0d5bf9

**C#**
``` C#
public class NetTransform2D : NetNode2D
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  Node  →  CanvasItem  →  Node2D  →  <a href="T_HLNC_NetNode2D">NetNode2D</a>  →  NetTransform2D</td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform2D__ctor">NetTransform2D</a></td>
<td>Initializes a new instance of the NetTransform2D class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_HLNC_Utility_Nodes_NetTransform2D_IsTeleporting">IsTeleporting</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utility_Nodes_NetTransform2D_NetPosition">NetPosition</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utility_Nodes_NetTransform2D_NetRotation">NetRotation</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utility_Nodes_NetTransform2D_SourceNode">SourceNode</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utility_Nodes_NetTransform2D_TargetNode">TargetNode</a></td>
<td> </td></tr>
</table>

## Methods
<table>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform2D__NetworkProcess">_NetworkProcess</a></td>
<td><br />(Overrides <a href="M_HLNC_NetNode2D__NetworkProcess">NetNode2D._NetworkProcess(Int32)</a>)</td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform2D__PhysicsProcess">_PhysicsProcess</a></td>
<td><p>Called during the physics processing step of the main loop. Physics processing means that the frame rate is synced to the physics, i.e. the <em>delta</em> parameter will <em>generally</em> be constant (see exceptions below). <em>delta</em> is in seconds.</p><p>

It is only called if physics processing is enabled, which is done automatically if this method is overridden, and can be toggled with SetPhysicsProcess(Boolean).</p><p>

Processing happens in order of ProcessPhysicsPriority, lower priority values are called first. Nodes with the same priority are processed in tree order, or top to bottom as seen in the editor (also known as pre-order traversal).</p><p>

Corresponds to the NotificationPhysicsProcess notification in _Notification(Int32).</p><p><b>

Note:</b> This method is only called if the node is present in the scene tree (i.e. if it's not an orphan).</p><p><b>

Note:</b><em>delta</em> will be larger than expected if running at a framerate lower than PhysicsTicksPerSecond / MaxPhysicsStepsPerFrame FPS. This is done to avoid "spiral of death" scenarios where performance would plummet due to an ever-increasing number of physics steps per frame. This behavior affects both _Process(Double) and _PhysicsProcess(Double). As a result, avoid using <em>delta</em> for time measurements in real-world seconds. Use the Time singleton's methods for this purpose instead, such as GetTicksUsec().</p><br />(Overrides <a href="M_HLNC_NetNode2D__PhysicsProcess">

NetNode2D._PhysicsProcess(Double)</a>)</td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform2D__WorldReady">_WorldReady</a></td>
<td><br />(Overrides <a href="M_HLNC_NetNode2D__WorldReady">NetNode2D._WorldReady()</a>)</td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform2D_GetParent2D">GetParent2D</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform2D_NetworkLerpNetPosition">NetworkLerpNetPosition</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform2D_NetworkLerpNetRotation">NetworkLerpNetRotation</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform2D_OnNetworkChangeIsTeleporting">OnNetworkChangeIsTeleporting</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utility_Nodes_NetTransform2D_Teleport">Teleport</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC_Utility_Nodes">HLNC.Utility.Nodes Namespace</a>  
