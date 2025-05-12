# UnpackFunction Method


Get a NetFunction by its scene and index (typically received from the network).



## Definition
**Namespace:** <a href="N_HLNC_Serialization">HLNC.Serialization</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+03b6c1d2e487070ae6af3c88edccb51282b75ac1

**C#**
``` C#
public CollectedNetFunction UnpackFunction(
	string scene,
	byte functionId
)
```



#### Parameters
<dl><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.string" target="_blank" rel="noopener noreferrer">String</a></dt><dd>The scene path.</dd><dt>  <a href="https://learn.microsoft.com/dotnet/api/system.byte" target="_blank" rel="noopener noreferrer">Byte</a></dt><dd>The function index.</dd></dl>

#### Return Value
<a href="T_HLNC_Serialization_CollectedNetFunction">CollectedNetFunction</a>  
The function.

## See Also


#### Reference
<a href="T_HLNC_Serialization_ProtocolRegistry">ProtocolRegistry Class</a>  
<a href="N_HLNC_Serialization">HLNC.Serialization Namespace</a>  
