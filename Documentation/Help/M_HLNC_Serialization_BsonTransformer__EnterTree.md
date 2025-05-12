# _EnterTree Method



Called when the node enters the SceneTree (e.g. upon instantiating, scene changing, or after calling AddChild(Node, Boolean, InternalMode) in a script). If the node has children, its _EnterTree() callback will be called first, and then that of the children.

Corresponds to the NotificationEnterTree notification in _Notification(Int32).




## Definition
**Namespace:** <a href="N_HLNC_Serialization">HLNC.Serialization</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+53875a383067c2ec9e9ab8259c59e7345e0d5bf9

**C#**
``` C#
public override void _EnterTree()
```



## See Also


#### Reference
<a href="T_HLNC_Serialization_BsonTransformer">BsonTransformer Class</a>  
<a href="N_HLNC_Serialization">HLNC.Serialization Namespace</a>  
