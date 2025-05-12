# IStateSerializer Interface


Defines an object which the server utilizes to serialize and send data to the client, and the client can then receive and deserialize from the server.



## Definition
**Namespace:** <a href="N_HLNC_Serialization_Serializers">HLNC.Serialization.Serializers</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+7c8369b309950da5e6f9dfc534f2804635131157

**C#**
``` C#
public interface IStateSerializer
```



## Methods
<table>
<tr>
<td><a href="M_HLNC_Serialization_Serializers_IStateSerializer_Acknowledge">Acknowledge</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Serialization_Serializers_IStateSerializer_Begin">Begin</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Serialization_Serializers_IStateSerializer_Cleanup">Cleanup</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_Serialization_Serializers_IStateSerializer_Export">Export</a></td>
<td>Server-side only. Serialize and send data to the client.</td></tr>
<tr>
<td><a href="M_HLNC_Serialization_Serializers_IStateSerializer_Import">Import</a></td>
<td>Client-side only. Receive and deserialize binary received from the server.</td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC_Serialization_Serializers">HLNC.Serialization.Serializers Namespace</a>  
