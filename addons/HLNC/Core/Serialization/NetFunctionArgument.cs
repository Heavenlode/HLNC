using Godot;

namespace HLNC.Serialization
{
    [Tool]
    public partial class NetFunctionArgument : Resource
    {
        [Export]
        public Variant.Type VariantType;
        [Export]
        public SerialMetadata Metadata;
    }

}