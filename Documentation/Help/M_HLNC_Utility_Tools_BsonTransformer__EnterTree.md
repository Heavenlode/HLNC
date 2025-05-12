# _EnterTree Method



Called when the node enters the SceneTree (e.g. upon instantiating, scene changing, or after calling AddChild(Node, Boolean, InternalMode) in a script). If the node has children, its _EnterTree() callback will be called first, and then that of the children.

Corresponds to the NotificationEnterTree notification in _Notification(Int32).




## Definition
**Namespace:** <a href="N_HLNC_Utility_Tools">HLNC.Utility.Tools</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+03b6c1d2e487070ae6af3c88edccb51282b75ac1

**C#**
``` C#
public override void _EnterTree()
```



## See Also


#### Reference
<a href="T_HLNC_Utility_Tools_BsonTransformer">BsonTransformer Class</a>  
<a href="N_HLNC_Utility_Tools">HLNC.Utility.Tools Namespace</a>  
