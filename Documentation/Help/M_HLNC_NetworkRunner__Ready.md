# _Ready Method



Called when the node is "ready", i.e. when both the node and its children have entered the scene tree. If the node has children, their _Ready() callbacks get triggered first, and the parent node will receive the ready notification afterwards.

Corresponds to the NotificationReady notification in _Notification(Int32). See also the `@onready` annotation for variables.

Usually used for initialization. For even earlier initialization, #ctor() may be used. See also _EnterTree().

**Note:** This method may be called only once for each node. After removing a node from the scene tree and adding it again, _Ready() will **not** be called a second time. This can be bypassed by requesting another call with RequestReady(), which may be called anywhere before adding the node again.




## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
public override void _Ready()
```



## See Also


#### Reference
<a href="T_HLNC_NetworkRunner">NetworkRunner Class</a>  
<a href="N_HLNC">HLNC Namespace</a>  
