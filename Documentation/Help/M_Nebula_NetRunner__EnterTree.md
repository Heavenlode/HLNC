# _EnterTree Method



Called when the node enters the SceneTree (e.g. upon instantiating, scene changing, or after calling AddChild(Node, Boolean, InternalMode) in a script). If the node has children, its _EnterTree() callback will be called first, and then that of the children.

Corresponds to the NotificationEnterTree notification in _Notification(Int32).




## Definition
**Namespace:** <a href="N_Nebula">Nebula</a>  
**Assembly:** Nebula (in Nebula.dll) Version: 1.0.0+a74e1f454fb572dfd95c249b7895aa6542c85b05

**C#**
``` C#
public override void _EnterTree()
```



## See Also


#### Reference
<a href="T_Nebula_NetRunner">NetRunner Class</a>  
<a href="N_Nebula">Nebula Namespace</a>  
