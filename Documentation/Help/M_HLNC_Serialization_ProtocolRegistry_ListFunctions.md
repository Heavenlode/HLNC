# ListFunctions Method


List all NetFunctions for a given NetNode within the scene.



## Definition
**Namespace:** <a href="N_HLNC_Serialization">HLNC.Serialization</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+f8729a03e7629d74435a0f1f1b469444b44e5bbc

**C#**
``` C#
public Array<ProtocolNetFunction> ListFunctions(
	string scene,
	string node
)
```



#### Parameters
<dl><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a></dt><dd>The scene path.</dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a></dt><dd>The node path.</dd></dl>

#### Return Value
Array(<a href="T_HLNC_Serialization_ProtocolNetFunction">ProtocolNetFunction</a>)  
An array of NetFunctions.

## See Also


#### Reference
<a href="T_HLNC_Serialization_ProtocolRegistry">ProtocolRegistry Class</a>  
<a href="N_HLNC_Serialization">HLNC.Serialization Namespace</a>  
