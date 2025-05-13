# Spawn&lt;T&gt; Method




## Definition
**Namespace:** <a href="N_Nebula">Nebula</a>  
**Assembly:** Nebula (in Nebula.dll) Version: 1.0.0+a74e1f454fb572dfd95c249b7895aa6542c85b05

**C#**
``` C#
public T Spawn<T>(
	T node,
	NetNodeWrapper parent = null,
	ENetPacketPeer inputAuthority = null,
	string nodePath = "."
)
where T : Node, INetNode

```



#### Parameters
<dl><dt>  T</dt><dd> </dd><dt>  <a href="T_Nebula_NetNodeWrapper">NetNodeWrapper</a>  (Optional)</dt><dd> </dd><dt>  ENetPacketPeer  (Optional)</dt><dd> </dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a>  (Optional)</dt><dd> </dd></dl>

#### Type Parameters
<dl><dt /><dd /></dl>

#### Return Value
T

## See Also


#### Reference
<a href="T_Nebula_WorldRunner">WorldRunner Class</a>  
<a href="N_Nebula">Nebula Namespace</a>  
