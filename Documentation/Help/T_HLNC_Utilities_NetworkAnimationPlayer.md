# NetworkAnimationPlayer Class


\[Missing &lt;summary&gt; documentation for "T:HLNC.Utilities.NetworkAnimationPlayer"\]



## Definition
**Namespace:** <a href="N_HLNC_Utilities">HLNC.Utilities</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
public class NetworkAnimationPlayer : NetworkNode3D
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  Node  →  Node3D  →  <a href="T_HLNC_NetworkNode3D">NetworkNode3D</a>  →  NetworkAnimationPlayer</td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_Utilities_NetworkAnimationPlayer__ctor">NetworkAnimationPlayer</a></td>
<td>Initializes a new instance of the NetworkAnimationPlayer class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_HLNC_Utilities_NetworkAnimationPlayer_active_animation">active_animation</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_Utilities_NetworkAnimationPlayer_animation_position">animation_position</a></td>
<td> </td></tr>
</table>

## Methods
<table>
<tr>
<td><a href="M_HLNC_Utilities_NetworkAnimationPlayer__NetworkProcess">_NetworkProcess</a></td>
<td><br />(Overrides <a href="M_HLNC_NetworkNode3D__NetworkProcess">NetworkNode3D._NetworkProcess(Int32)</a>)</td></tr>
<tr>
<td><a href="M_HLNC_Utilities_NetworkAnimationPlayer__Ready">_Ready</a></td>
<td><p>Called when the node is "ready", i.e. when both the node and its children have entered the scene tree. If the node has children, their _Ready() callbacks get triggered first, and the parent node will receive the ready notification afterwards.</p><p>

Corresponds to the NotificationReady notification in _Notification(Int32). See also the <code>@onready</code> annotation for variables.</p><p>

Usually used for initialization. For even earlier initialization, #ctor() may be used. See also _EnterTree().</p><p><b>

Note:</b> This method may be called only once for each node. After removing a node from the scene tree and adding it again, _Ready() will <b>not</b> be called a second time. This can be bypassed by requesting another call with RequestReady(), which may be called anywhere before adding the node again.</p><br />(Overrides Node._Ready())</td></tr>
<tr>
<td><a href="M_HLNC_Utilities_NetworkAnimationPlayer_OnNetworkChangeActiveAnimation">OnNetworkChangeActiveAnimation</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utilities_NetworkAnimationPlayer_OnNetworkChangeAnimationPosition">OnNetworkChangeAnimationPosition</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Utilities_NetworkAnimationPlayer_Play">Play</a></td>
<td> </td></tr>
</table>

## Fields
<table>
<tr>
<td><a href="F_HLNC_Utilities_NetworkAnimationPlayer_animation_player">animation_player</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC_Utilities">HLNC.Utilities Namespace</a>  
