# BlastoffValidatePeer Method


Validate whether the user is allowed to join the desired zone (i.e. server instance) or not.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
bool BlastoffValidatePeer(
	Guid zoneId,
	string token,
	out Guid redirect
)
```



#### Parameters
<dl><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.guid" target="_blank" rel="noopener noreferrer">Guid</a></dt><dd>The zoneId being requested by the client. This is generated either statically when the Blastoff server is initially spun up, or dynamically when the Blastoff server generates new instances.</dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a></dt><dd>The token sent from the client. This may be a JWT, HMAC, or anything else desired for authentication.</dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.guid" target="_blank" rel="noopener noreferrer">Guid</a></dt><dd>An optional output parameter to ask Blastoff connect the client to. Only used if the user is not valid and false is returned.</dd></dl>

#### Return Value
<a href="https://learn.microsoft.com/dotnet/api/system.boolean" target="_blank" rel="noopener noreferrer">Boolean</a>  
Return true to allow the user to join. Return false to reject.

## See Also


#### Reference
<a href="T_HLNC_IBlastoffServerDriver">IBlastoffServerDriver Interface</a>  
<a href="N_HLNC">HLNC Namespace</a>  
