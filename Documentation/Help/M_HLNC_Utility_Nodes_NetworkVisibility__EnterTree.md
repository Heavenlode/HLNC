# _EnterTree Method



Called when the node enters the SceneTree (e.g. upon instantiating, scene changing, or after calling AddChild(Node, Boolean, InternalMode) in a script). If the node has children, its _EnterTree() callback will be called first, and then that of the children.

Corresponds to the NotificationEnterTree notification in _Notification(Int32).




## Definition
**Namespace:** <a href="N_HLNC_Utility_Nodes">HLNC.Utility.Nodes</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+1d526f6d6059a0ffb6384edc7f75446241490e0f

**C#**
``` C#
public override void _EnterTree()
```



## See Also


#### Reference
<a href="T_HLNC_Utility_Nodes_NetworkVisibility">NetworkVisibility Class</a>  
<a href="N_HLNC_Utility_Nodes">HLNC.Utility.Nodes Namespace</a>  
