# NetworkRunner.ENetChannelId Enumeration


Describes the channels of communication used by the network.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
public enum ENetChannelId
```



## Members
<table>
<tr>
<td>Tick</td>
<td>1</td>
<td>Tick data sent by the server to the client, and from the client indicating the most recent tick it has received.</td></tr>
<tr>
<td>Input</td>
<td>2</td>
<td>Input data sent from the client.</td></tr>
<tr>
<td>ClientAuth</td>
<td>3</td>
<td>Client data sent to the server to authenticate themselves and connect to a zone.</td></tr>
<tr>
<td>BlastoffAdmin</td>
<td>254</td>
<td>Server communication with Blastoff. Data sent to this channel from a client will be ignored by Blastoff.</td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
