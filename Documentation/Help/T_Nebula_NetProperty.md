# NetProperty Class


Mark a property as being Networked. The <a href="T_Nebula_WorldRunner">WorldRunner</a> automatically processes these through the <a href="T_Nebula_Serialization_Serializers_NetPropertiesSerializer">NetPropertiesSerializer</a> to be optimally sent across the network. Only changes are networked. When the NetNode receives a change on the property, it will also attempt to call a method 

**C#**  
``` C#
NetNode.OnNetworkChange{PropertyName}(int tick, T oldValue, T newValue)
```
 on the client side if it exists.



## Definition
**Namespace:** <a href="N_Nebula">Nebula</a>  
**Assembly:** Nebula (in Nebula.dll) Version: 1.0.0+a74e1f454fb572dfd95c249b7895aa6542c85b05

**C#**
``` C#
public class NetProperty : Attribute
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  <a href="https://learn.microsoft.com/dotnet/api/system.attribute" target="_blank" rel="noopener noreferrer">Attribute</a>  →  NetProperty</td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_Nebula_NetProperty__ctor">NetProperty</a></td>
<td>Initializes a new instance of the NetProperty class</td></tr>
</table>

## Fields
<table>
<tr>
<td><a href="F_Nebula_NetProperty_Flags">Flags</a></td>
<td> </td></tr>
<tr>
<td><a href="F_Nebula_NetProperty_InterestMask">InterestMask</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_Nebula">Nebula Namespace</a>  
