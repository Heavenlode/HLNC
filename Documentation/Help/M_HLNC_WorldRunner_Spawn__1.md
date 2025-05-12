# Spawn&lt;T&gt; Method




## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+f8729a03e7629d74435a0f1f1b469444b44e5bbc

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
