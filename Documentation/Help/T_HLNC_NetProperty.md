# NetProperty Class


Mark a property as being Networked. The <a href="T_HLNC_WorldRunner">WorldRunner</a> automatically processes these through the NetPropertiesSerializer to be optimally sent across the network. Only changes are networked. When the client receives a change on the property, if a method exists 

**C#**  
``` C#
OnNetworkChange{PropertyName}(int tick, T oldValue, T newValue)
```
 it will be called on the client side.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+03b6c1d2e487070ae6af3c88edccb51282b75ac1

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
