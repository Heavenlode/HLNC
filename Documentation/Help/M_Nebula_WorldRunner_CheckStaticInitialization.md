# CheckStaticInitialization Method


This is called for nodes that are initialized in a scene by default. Clients automatically dequeue all network nodes on initialization. All network nodes on the client side must come from the server by gaining Interest in the node.



## Definition
**Namespace:** <a href="N_Nebula">Nebula</a>  
**Assembly:** Nebula (in Nebula.dll) Version: 1.0.0+a74e1f454fb572dfd95c249b7895aa6542c85b05

**C#**
``` C#
public bool CheckStaticInitialization(
	NetNodeWrapper wrapper
)
```



#### Parameters
<dl><dt>  <a href="T_Nebula_NetNodeWrapper">NetNodeWrapper</a></dt><dd /></dl>

#### Return Value
<a href="https://learn.microsoft.com/dotnet/api/system.boolean" target="_blank" rel="noopener noreferrer">Boolean</a>  


## See Also


#### Reference
<a href="T_Nebula_WorldRunner">WorldRunner Class</a>  
<a href="N_Nebula">Nebula Namespace</a>  
