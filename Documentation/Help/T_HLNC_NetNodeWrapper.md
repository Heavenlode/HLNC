# NetNodeWrapper Class


Helper class to safely interface with NetNodes across languages (e.g. C#, GDScript) or in situations where the node type is unknown.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+f8729a03e7629d74435a0f1f1b469444b44e5bbc

**C#**
``` C#
public class NetNodeWrapper : RefCounted
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  RefCounted  →  NetNodeWrapper</td></tr>
</table>



## Remarks
This class automatically handles NetNode validation. For example: 

**C#**  
``` C#
var maybeNetNode = new NetNodeWrapper(GetNodeOrNull("MyAmbiguousNode"));
// If MyAmbiguousNode is not a NetNode, maybeNetNode == null
```


## Constructors
<table>
<tr>
<td><a href="M_HLNC_NetNodeWrapper__ctor">NetNodeWrapper</a></td>
<td>Initializes a new instance of the NetNodeWrapper class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_HLNC_NetNodeWrapper_CurrentWorld">CurrentWorld</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetNodeWrapper_InputAuthority">InputAuthority</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetNodeWrapper_InterestLayers">InterestLayers</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetNodeWrapper_IsClientSpawn">IsClientSpawn</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetNodeWrapper_NetId">NetId</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetNodeWrapper_NetParent">NetParent</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetNodeWrapper_NetParentId">NetParentId</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetNodeWrapper_Network">Network</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetNodeWrapper_Node">Node</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetNodeWrapper_Serializers">Serializers</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetNodeWrapper_StaticNetworkChildren">StaticNetworkChildren</a></td>
<td> </td></tr>
</table>

## Methods
<table>
<tr>
<td><a href="M_HLNC_NetNodeWrapper__NetworkProcess">_NetworkProcess</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNodeWrapper_GetNetworkInput">GetNetworkInput</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNodeWrapper_IsNetScene">IsNetScene</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNodeWrapper_SetNetworkInput">SetNetworkInput</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNodeWrapper_SetPeerInterest">SetPeerInterest</a></td>
<td> </td></tr>
</table>

## Operators
<table>
<tr>
<td><a href="M_HLNC_NetNodeWrapper_op_Equality">Equality(NetNodeWrapper, NetNodeWrapper)</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetNodeWrapper_op_Inequality">Inequality(NetNodeWrapper, NetNodeWrapper)</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
