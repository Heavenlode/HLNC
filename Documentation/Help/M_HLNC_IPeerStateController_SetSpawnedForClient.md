# SetSpawnedForClient Method


Indicate that a client has acknowledged a tick wherein a node was spawned.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
void SetSpawnedForClient(
	long networkId,
	ENetPacketPeer peer
)
```



#### Parameters
<dl><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.int64" target="_blank" rel="noopener noreferrer">Int64</a></dt><dd>The server's NetworkId for that node.</dd><dt>  ENetPacketPeer</dt><dd>The peer in question</dd></dl>

#### Return Value


## See Also


#### Reference
<a href="T_HLNC_IPeerStateController">IPeerStateController Interface</a>  
<a href="N_HLNC">HLNC Namespace</a>  
