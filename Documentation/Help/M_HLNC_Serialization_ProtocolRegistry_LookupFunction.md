# LookupFunction Method


Lookup a NetFunction by its scene, node, and name.



## Definition
**Namespace:** <a href="N_HLNC_Serialization">HLNC.Serialization</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+7c8369b309950da5e6f9dfc534f2804635131157

**C#**
``` C#
public bool LookupFunction(
	string scene,
	string node,
	string function,
	out ProtocolNetFunction func
)
```



#### Parameters
<dl><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a></dt><dd>The scene path.</dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a></dt><dd>The node path.</dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a></dt><dd>The function name.</dd><dt>  <a href="T_HLNC_Serialization_ProtocolNetFunction">ProtocolNetFunction</a></dt><dd>The function, if found.</dd></dl>

#### Return Value
<a href="https://learn.microsoft.com/dotnet/api/system.boolean" target="_blank" rel="noopener noreferrer">Boolean</a>  
True if the function was found, false otherwise.

## See Also


#### Reference
<a href="T_HLNC_Serialization_ProtocolRegistry">ProtocolRegistry Class</a>  
<a href="N_HLNC_Serialization">HLNC.Serialization Namespace</a>  
