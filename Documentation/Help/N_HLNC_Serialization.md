# HLNC.Serialization Namespace


\[Missing &lt;summary&gt; documentation for "N:HLNC.Serialization"\]



## Classes
<table>
<tr>
<td><a href="T_HLNC_Serialization_BsonTransformer">BsonTransformer</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_Serialization_HLBuffer">HLBuffer</a></td>
<td>Standard object used to package data that will be transferred across the network. Used extensively by <a href="T_HLNC_Serialization_HLBytes">HLBytes</a>.</td></tr>
<tr>
<td><a href="T_HLNC_Serialization_HLBytes">HLBytes</a></td>
<td>Converts variables and Godot variants into binary and vice-versa. <a href="T_HLNC_Serialization_HLBuffer">HLBuffer</a> is the medium of storage.</td></tr>
<tr>
<td><a href="T_HLNC_Serialization_NetFunctionArgument">NetFunctionArgument</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_Serialization_ProtocolNetFunction">ProtocolNetFunction</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_Serialization_ProtocolNetProperty">ProtocolNetProperty</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_Serialization_ProtocolRegistry">ProtocolRegistry</a></td>
<td>The singleton instance of the ProtocolRegistry. This is used to serialize and deserialize scenes, network properties, and network functions sent across the network. The point is that we can't send an entire scene string across the network or some other lengthy identifier because it would take up too much bandwidth. So we use this to classify scenes as bytes which can be sent across the network, using minimal bandwidth. The same goes for network properties and functions. See [!:ProtocolRegistry.Build] for more information.</td></tr>
<tr>
<td><a href="T_HLNC_Serialization_ProtocolRegistryBuilder">ProtocolRegistryBuilder</a></td>
<td>This extension of ProtocolRegistry is used for generating the <a href="T_HLNC_Serialization_ProtocolResource">ProtocolResource</a>.</td></tr>
<tr>
<td><a href="T_HLNC_Serialization_ProtocolResource">ProtocolResource</a></td>
<td>A resource that contains the compiled data of a <a href="T_HLNC_Serialization_ProtocolRegistry">ProtocolRegistry</a>. This resource defines the bytes used to encode and decode network scenes, nodes, properties, and functions, as well as the lookup tables used to quickly find the corresponding data. With this compiled resource, the program is able to understand how to send and receive game state.</td></tr>
<tr>
<td><a href="T_HLNC_Serialization_SerialMetadata">SerialMetadata</a></td>
<td>This resource is used to extend Godot's Variant type, particularly for the types sent across the network for <a href="T_HLNC_Serialization_ProtocolNetProperty">ProtocolNetProperty</a> and <a href="T_HLNC_Serialization_ProtocolNetFunction">ProtocolNetFunction</a>. The purpose is to have additional information at runtime about how the data should be encoded and decoded without using reflection. We can't depend on Godot's variant alone, as the provided types in Variant.Type are not detailed enough. The value used for the TypeIdentifier comes from the [!:ProtocolRegistry.SerialTypeIdentifiers] dictionary, which itself is populated at compile time by the <a href="T_HLNC_Serialization_SerialTypeIdentifier">SerialTypeIdentifier</a> attribute.</td></tr>
<tr>
<td><a href="T_HLNC_Serialization_SerialTypeIdentifier">SerialTypeIdentifier</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_Serialization_StaticMethodResource">StaticMethodResource</a></td>
<td> </td></tr>
</table>

## Interfaces
<table>
<tr>
<td><a href="T_HLNC_Serialization_IBsonSerializable_1">IBsonSerializable(T)</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_Serialization_IBsonSerializableBase">IBsonSerializableBase</a></td>
<td> </td></tr>
<tr>
<td><a href="T_HLNC_Serialization_INetSerializable_1">INetSerializable(T)</a></td>
<td> </td></tr>
</table>

## Enumerations
<table>
<tr>
<td><a href="T_HLNC_Serialization_StaticMethodType">StaticMethodType</a></td>
<td> </td></tr>
</table>