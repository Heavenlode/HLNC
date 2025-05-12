# _ExitTree Method



Called when the node is about to leave the SceneTree (e.g. upon freeing, scene changing, or after calling RemoveChild(Node) in a script). If the node has children, its _ExitTree() callback will be called last, after all its children have left the tree.

Corresponds to the NotificationExitTree notification in _Notification(Int32) and signal TreeExiting. To get notified when the node has already left the active tree, connect to the TreeExited.




## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
public override void _ExitTree()
```



## See Also


#### Reference
<a href="T_HLNC_NetworkNode3D">NetworkNode3D Class</a>  
<a href="N_HLNC">HLNC Namespace</a>  
