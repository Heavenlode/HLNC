# NetId Class


A unique identifier for a networked object. The NetId for a node is different between the server and client. On the client side, a NetId is only a byte, whereas on the server side it is an int64. The server's <a href="T_HLNC_WorldRunner">WorldRunner</a> keeps a map of all NetIds to their corresponding value on each client for serialization.



## Definition
**Namespace:** <a href="N_HLNC">HLNC</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+53875a383067c2ec9e9ab8259c59e7345e0d5bf9

**C#**
``` C#
public class NetId : RefCounted, INetSerializable<NetId>, 
	IBsonSerializable<NetId>, IBsonSerializableBase
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  RefCounted  →  NetId</td></tr>
<tr><td><strong>Implements</strong></td><td><a href="T_HLNC_Serialization_IBsonSerializable_1">IBsonSerializable</a>(NetId), <a href="T_HLNC_Serialization_IBsonSerializableBase">IBsonSerializableBase</a>, <a href="T_HLNC_Serialization_INetSerializable_1">INetSerializable</a>(NetId)</td></tr>
</table>



## Properties
<table>
<tr>
<td><a href="P_HLNC_NetId_Node">Node</a></td>
<td> </td></tr>
<tr>
<td><a href="P_HLNC_NetId_Value">Value</a></td>
<td> </td></tr>
</table>

## Methods
<table>
<tr>
<td><a href="M_HLNC_NetId_BsonDeserialize">BsonDeserialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetId_BsonSerialize">BsonSerialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetId_NetworkDeserialize">NetworkDeserialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_HLNC_NetId_NetworkSerialize">NetworkSerialize</a></td>
<td> </td></tr>
</table>

## Fields
<table>
<tr>
<td><a href="F_HLNC_NetId_NONE">NONE</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC">HLNC Namespace</a>  
