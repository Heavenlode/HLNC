# Spawn&lt;T&gt; Method




## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+53875a383067c2ec9e9ab8259c59e7345e0d5bf9

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
<dl><dt>  T</dt><dd> </dd><dt>  <a href="T_HLNC_NetNodeWrapper">NetNodeWrapper</a>  (Optional)</dt><dd> </dd><dt>  ENetPacketPeer  (Optional)</dt><dd> </dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a>  (Optional)</dt><dd> </dd></dl>

#### Type Parameters
<dl><dt /><dd /></dl>

#### Return Value
T

## See Also


#### Reference
<a href="T_HLNC_WorldRunner">WorldRunner Class</a>  
<a href="N_HLNC">HLNC Namespace</a>  
