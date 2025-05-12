# _PhysicsProcess Method



Called during the physics processing step of the main loop. Physics processing means that the frame rate is synced to the physics, i.e. the *delta* parameter will *generally* be constant (see exceptions below). *delta* is in seconds.

It is only called if physics processing is enabled, which is done automatically if this method is overridden, and can be toggled with SetPhysicsProcess(Boolean).

Processing happens in order of ProcessPhysicsPriority, lower priority values are called first. Nodes with the same priority are processed in tree order, or top to bottom as seen in the editor (also known as pre-order traversal).

Corresponds to the NotificationPhysicsProcess notification in _Notification(Int32).

**Note:** This method is only called if the node is present in the scene tree (i.e. if it's not an orphan).

**Note:***delta* will be larger than expected if running at a framerate lower than PhysicsTicksPerSecond / MaxPhysicsStepsPerFrame FPS. This is done to avoid "spiral of death" scenarios where performance would plummet due to an ever-increasing number of physics steps per frame. This behavior affects both _Process(Double) and _PhysicsProcess(Double). As a result, avoid using *delta* for time measurements in real-world seconds. Use the Time singleton's methods for this purpose instead, such as GetTicksUsec().




## Definition
**Namespace:** <a href="N_HLNC_Utility_Nodes">HLNC.Utility.Nodes</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+03b6c1d2e487070ae6af3c88edccb51282b75ac1

**C#**
``` C#
public override void _PhysicsProcess(
	double delta
)
```



#### Parameters
<dl><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.double" target="_blank" rel="noopener noreferrer">Double</a></dt><dd>\[Missing &lt;param name="delta"/&gt; documentation for "M:HLNC.Utility.Nodes.NetTransform2D._PhysicsProcess(System.Double)"\]</dd></dl>

## See Also


#### Reference
<a href="T_HLNC_Utility_Nodes_NetTransform2D">NetTransform2D Class</a>  
<a href="N_HLNC_Utility_Nodes">HLNC.Utility.Nodes Namespace</a>  
