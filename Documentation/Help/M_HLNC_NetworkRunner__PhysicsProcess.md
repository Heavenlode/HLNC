# _PhysicsProcess Method



Called during the physics processing step of the main loop. Physics processing means that the frame rate is synced to the physics, i.e. the *delta* variable should be constant. *delta* is in seconds.

It is only called if physics processing is enabled, which is done automatically if this method is overridden, and can be toggled with SetPhysicsProcess(Boolean).

Corresponds to the NotificationPhysicsProcess notification in _Notification(Int32).

**Note:** This method is only called if the node is present in the scene tree (i.e. if it's not an orphan).




## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
public override void _PhysicsProcess(
	double delta
)
```



#### Parameters
<dl><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.double" target="_blank" rel="noopener noreferrer">Double</a></dt><dd>\[Missing &lt;param name="delta"/&gt; documentation for "M:HLNC.NetworkRunner._PhysicsProcess(System.Double)"\]</dd></dl>

## See Also


#### Reference
<a href="T_HLNC_NetworkRunner">NetworkRunner Class</a>  
<a href="N_HLNC">HLNC Namespace</a>  
