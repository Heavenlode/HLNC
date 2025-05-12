# IStateSerailizer Interface


Defines an object which the server utilizes to serialize and send data to the client, and the client can then receive and deserialize from the server.



## Definition
**Namespace:** <a href="N_HLNC_Serialization_Serializers">HLNC.Serialization.Serializers</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+67fc7a7b454bc0ade857a4ae4930fb238e351d35

**C#**
``` C#
public interface IStateSerailizer
```



## Methods
<table>
<tr>
<td><a href="M_HLNC_Serialization_Serializers_IStateSerailizer_Acknowledge">Acknowledge</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Serialization_Serializers_IStateSerailizer_Cleanup">Cleanup</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Serialization_Serializers_IStateSerailizer_Export">Export</a></td>
<td>Server-side only. Serialize and send data to the client.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_Serializers_IStateSerailizer_Import">Import</a></td>
<td>Client-side only. Receive and deserialize binary received from the server.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_Serializers_IStateSerailizer_PhysicsProcess">PhysicsProcess</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC_Serialization_Serializers">HLNC.Serialization.Serializers Namespace</a>  
