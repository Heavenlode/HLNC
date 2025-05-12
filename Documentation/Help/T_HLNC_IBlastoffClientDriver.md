# IBlastoffClientDriver Interface


Provides client logic for Blastoff communication. <a href="T_HLNC_NetworkRunner">NetworkRunner</a> calls these functions to handle Blastoff communications. For more info on Blastoff, visit the repo:



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
public interface IBlastoffClientDriver
```



## Methods
<table>
<tr>
<td><a href="M_HLNC_IBlastoffClientDriver_BlastoffGetToken">BlastoffGetToken</a></td>
<td>NetworkRunner uses this to request the user's authentication token to send along to the server.</td></tr>
<tr>
<td><a href="M_HLNC_IBlastoffClientDriver_BlastoffGetZoneId">BlastoffGetZoneId</a></td>
<td>NetworkRunner uses this to tell the server which zone the client is trying to connect to.</td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
