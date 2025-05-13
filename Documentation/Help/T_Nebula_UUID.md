# UUID Class


A UUID implementation for Nebula. Serializes into 16 bytes.



## Definition
**Namespace:** <a href="N_Nebula">Nebula</a>  
**Assembly:** Nebula (in Nebula.dll) Version: 1.0.0+a74e1f454fb572dfd95c249b7895aa6542c85b05

**C#**
``` C#
public class UUID : RefCounted, INetSerializable<UUID>, 
	IBsonSerializable<UUID>, IBsonSerializableBase
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  RefCounted  →  UUID</td></tr>
<tr><td><strong>Implements</strong></td><td><a href="T_Nebula_Serialization_IBsonSerializable_1">IBsonSerializable</a>(UUID), <a href="T_Nebula_Serialization_IBsonSerializableBase">IBsonSerializableBase</a>, <a href="T_Nebula_Serialization_INetSerializable_1">INetSerializable</a>(UUID)</td></tr>
</table>



## Constructors
<table>
<tr>
<td><a href="M_Nebula_UUID__ctor">UUID()</a></td>
<td>Initializes a new instance of the UUID class</td></tr>
<tr>
<td><a href="M_Nebula_UUID__ctor_1">UUID(Byte[])</a></td>
<td>Initializes a new instance of the UUID class</td></tr>
<tr>
<td><a href="M_Nebula_UUID__ctor_2">UUID(String)</a></td>
<td>Initializes a new instance of the UUID class</td></tr>
</table>

## Properties
<table>
<tr>
<td><a href="P_Nebula_UUID_Empty">Empty</a></td>
<td> </td></tr>
<tr>
<td><a href="P_Nebula_UUID_Guid">Guid</a></td>
<td> </td></tr>
</table>

## Methods
<table>
<tr>
<td><a href="M_Nebula_UUID_BsonDeserialize">BsonDeserialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_UUID_BsonSerialize">BsonSerialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_UUID_NetworkDeserialize">NetworkDeserialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_UUID_NetworkSerialize">NetworkSerialize</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_UUID_ToByteArray">ToByteArray</a></td>
<td> </td></tr>
<tr>
<td><a href="M_Nebula_UUID_ToString">ToString</a></td>
<td><br />(Overrides GodotObject.ToString())</td></tr>
</table>

## See Also


#### Reference
<a href="N_Nebula">Nebula Namespace</a>  
