# NetworkProperty Class


Mark a property as being Networked. The NetworkPeerManager automatically processes these through the NetworkPropertiesSerializer to be optimally sent across the network. Only changes are networked. When the client receives a change on the property, if a method exists 

**C#**  
``` C#
OnNetworkChange{PropertyName}(int tick, T oldValue, T newValue)
```
 it will be called on the client side.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
public class NetworkProperty : Attribute
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  <a href="https://learn.microsoft.com/dotnet/api/system.attribute" target="_blank" rel="noopener noreferrer">Attribute</a>  →  NetworkProperty</td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_NetworkProperty__ctor">NetworkProperty</a></td>
<td>Initializes a new instance of the NetworkProperty class</td></tr>
</table>

## Fields
<table>
<tr>
<td><a href="F_HLNC_NetworkProperty_flags">flags</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
