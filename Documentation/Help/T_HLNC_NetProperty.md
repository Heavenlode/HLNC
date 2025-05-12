# NetProperty Class


Mark a property as being Networked. The <a href="T_HLNC_WorldRunner">WorldRunner</a> automatically processes these through the <a href="T_HLNC_Serialization_Serializers_NetPropertiesSerializer">NetPropertiesSerializer</a> to be optimally sent across the network. Only changes are networked. When the NetNode receives a change on the property, it will also attempt to call a method 

**C#**  
``` C#
NetNode.OnNetworkChange{PropertyName}(int tick, T oldValue, T newValue)
```
 on the client side if it exists.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+f8729a03e7629d74435a0f1f1b469444b44e5bbc

**C#**
``` C#
public class NetProperty : Attribute
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  <a href="https://learn.microsoft.com/dotnet/api/system.attribute" target="_blank" rel="noopener noreferrer">Attribute</a>  →  NetProperty</td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_NetProperty__ctor">NetProperty</a></td>
<td>Initializes a new instance of the NetProperty class</td></tr>
</table>

## Fields
<table>
<tr>
<td><a href="F_HLNC_NetProperty_Flags">Flags</a></td>
<td> </td></tr>
<tr>
<td><a href="F_HLNC_NetProperty_InterestMask">InterestMask</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
