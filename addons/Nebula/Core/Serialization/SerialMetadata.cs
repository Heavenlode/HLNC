using Godot;

namespace Nebula.Serialization {
    /// <summary>
    /// This resource is used to extend Godot's Variant type,
    /// particularly for the types sent across the network for <see cref="ProtocolNetProperty"/> and <see cref="ProtocolNetFunction"/>.
    /// The purpose is to have additional information at runtime about how the data should be encoded and decoded without using reflection.
    /// We can't depend on Godot's variant alone, as the provided types in Variant.Type are not detailed enough.
    /// 
    /// The value used for the TypeIdentifier comes from the <see cref="ProtocolRegistry.SerialTypeIdentifiers"/> dictionary,
    /// which itself is populated at compile time by the <see cref="SerialTypeIdentifier"/> attribute.
    /// </summary>
    /// <example>
    /// Some examples of how/why this is used:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Godot's variant includes "Variant.Type.Int" but does not differentiate between 8 bit, 16 bit, 32 bit, or 64 bit integers.
    /// By appending a TypeIdentifier to a CollectedNetProperty, the serializer knows how many bytes to read from the stream.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Godot's variant includes "Variant.Type.Object", but doesn't tell us what type of object.
    /// Implicitly, we know that only <see cref="INetSerializable"/> Objects are sent across the network,
    /// but this allows us to know what the implementing type is.
    /// </description>
    /// </item>
    /// </list>
    /// </example>
    [Tool]
    public partial class SerialMetadata : Resource
    {
        [Export]
        public string TypeIdentifier;
    }
}