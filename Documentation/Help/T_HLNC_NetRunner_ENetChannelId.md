# NetRunner.ENetChannelId Enumeration


Describes the channels of communication used by the network.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+7c8369b309950da5e6f9dfc534f2804635131157

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
<td>Function</td>
<td>3</td>
<td>NetFunction call.</td></tr>
<tr>
<td>BlastoffAdmin</td>
<td>249</td>
<td>Server communication with Blastoff. Data sent to this channel from a client will be ignored by Blastoff.</td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
