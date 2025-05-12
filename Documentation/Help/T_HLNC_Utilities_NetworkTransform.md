# NetworkTransform Class


\[Missing &lt;summary&gt; documentation for "T:HLNC.Utilities.NetworkTransform"\]



## Definition
**Namespace:** <a href="N_HLNC_Utilities">HLNC.Utilities</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
public class NetworkTransform : NetworkNode3D
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  Node  →  Node3D  →  <a href="T_HLNC_NetworkNode3D">NetworkNode3D</a>  →  NetworkTransform</td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_Utilities_NetworkTransform__ctor">NetworkTransform</a></td>
<td>Initializes a new instance of the NetworkTransform class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_HLNC_Utilities_NetworkTransform_IsTeleporting">IsTeleporting</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utilities_NetworkTransform_NetPosition">NetPosition</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utilities_NetworkTransform_NetRotation">NetRotation</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utilities_NetworkTransform_SourceNode">SourceNode</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utilities_NetworkTransform_TargetNode">TargetNode</a></td>
<td> </td></tr>
</table>

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
<a href="N_HLNC_Utilities">HLNC.Utilities Namespace</a>  
