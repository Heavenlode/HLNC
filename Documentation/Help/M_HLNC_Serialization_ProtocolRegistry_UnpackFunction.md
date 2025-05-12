# UnpackFunction Method


Get a NetFunction by its scene and index (typically received from the network).



## Definition
**Namespace:** <a href="N_HLNC_Serialization">HLNC.Serialization</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+1d526f6d6059a0ffb6384edc7f75446241490e0f

**C#**
``` C#
public ProtocolNetFunction UnpackFunction(
	string scene,
	byte functionId
)
```



#### Parameters
<dl><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a></dt><dd>The scene path.</dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.byte" target="_blank" rel="noopener noreferrer">Byte</a></dt><dd>The function index.</dd></dl>

#### Return Value
<a href="T_HLNC_Serialization_ProtocolNetFunction">ProtocolNetFunction</a>  
The function.

## See Also


#### Reference
<a href="T_HLNC_Serialization_ProtocolRegistry">ProtocolRegistry Class</a>  
<a href="N_HLNC_Serialization">HLNC.Serialization Namespace</a>  
