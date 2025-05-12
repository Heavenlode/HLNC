# TryRegisterPeerNode Method


Attempts to register a NetworkNode for a peer. This is necessary because peers track different IDs for NetworkNodes than the server does. The reason why is that the Network tracks IDs as an int64, but we don't want to send a full int64 over the network for every node.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
byte TryRegisterPeerNode(
	NetworkNodeWrapper node,
	ENetPacketPeer peer = null
)
```



#### Parameters
<dl><dt>  <a href="T_HLNC_NetworkNodeWrapper">NetworkNodeWrapper</a></dt><dd>The NetworkNode to register with the peer</dd><dt>  ENetPacketPeer  (Optional)</dt><dd>The peer in question</dd></dl>

#### Return Value
<a href="https://learn.microsoft.com/dotnet/api/system.byte" target="_blank" rel="noopener noreferrer">Byte</a>  
\[Missing &lt;returns&gt; documentation for "M:HLNC.IPeerStateController.TryRegisterPeerNode(HLNC.NetworkNodeWrapper,Godot.ENetPacketPeer)"\]

## See Also


#### Reference
<a href="T_HLNC_IPeerStateController">IPeerStateController Interface</a>  
<a href="N_HLNC">HLNC Namespace</a>  
