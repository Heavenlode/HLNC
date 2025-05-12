# SerialMetadata Class


This resource is used to extend Godot's Variant type, particularly for the types sent across the network for <a href="T_HLNC_Serialization_ProtocolNetProperty">ProtocolNetProperty</a> and <a href="T_HLNC_Serialization_ProtocolNetFunction">ProtocolNetFunction</a>. The purpose is to have additional information at runtime about how the data should be encoded and decoded without using reflection. We can't depend on Godot's variant alone, as the provided types in Variant.Type are not detailed enough. The value used for the TypeIdentifier comes from the [!:ProtocolRegistry.SerialTypeIdentifiers] dictionary, which itself is populated at compile time by the <a href="T_HLNC_Serialization_SerialTypeIdentifier">SerialTypeIdentifier</a> attribute.



## Definition
**Namespace:** <a href="N_HLNC_Serialization">HLNC.Serialization</a>  
**Assembly:** HLNC (in HLNC.dll) Version: 1.0.0+53875a383067c2ec9e9ab8259c59e7345e0d5bf9

**C#**
``` C#
public class SerialMetadata : Resource
```

<table><tr><td><strong>Inheritance</strong></td><td><a href="https://learn.microsoft.com/dotnet/api/system.object" target="_blank" rel="noopener noreferrer">Object</a>  →  GodotObject  →  RefCounted  →  Resource  →  SerialMetadata</td></tr>
</table>



## Example
Some examples of how/why this is used: <ul><li>Godot's variant includes "Variant.Type.Int" but does not differentiate between 8 bit, 16 bit, 32 bit, or 64 bit integers. By appending a TypeIdentifier to a CollectedNetProperty, the serializer knows how many bytes to read from the stream.</li><li>Godot's variant includes "Variant.Type.Object", but doesn't tell us what type of object. Implicitly, we know that only [!:INetSerializable] Objects are sent across the network, but this allows us to know what the implementing type is.</li></ul>



## Constructors
<table>
<tr>
<td><a href="M_HLNC_Serialization_SerialMetadata__ctor">SerialMetadata</a></td>
<td>Initializes a new instance of the SerialMetadata class</td></tr>
</table>

## Fields
<table>
<tr>
<td><a href="F_HLNC_Serialization_SerialMetadata_TypeIdentifier">TypeIdentifier</a></td>
<td> </td></tr>
</table>

## See Also


#### Reference
<a href="N_HLNC_Serialization">HLNC.Serialization Namespace</a>  
