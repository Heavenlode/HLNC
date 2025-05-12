# BlastoffGetToken Method


NetworkRunner uses this to request the user's authentication token to send along to the server.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
string BlastoffGetToken()
```



#### Return Value
<a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a>  
The user's authentication token which is validated in <a href="M_HLNC_IBlastoffServerDriver_BlastoffValidatePeer">BlastoffValidatePeer(Guid, String, Guid)</a>

## See Also


#### Reference
<a href="T_HLNC_IBlastoffClientDriver">IBlastoffClientDriver Interface</a>  
<a href="N_HLNC">HLNC Namespace</a>  
