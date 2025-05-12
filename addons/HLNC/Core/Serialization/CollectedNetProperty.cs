using Godot;

namespace HLNC.Serialization
{
    [Tool]
    public partial class CollectedNetProperty : Resource
    {
        [Export]
        public string NodePath;
        [Export]
        public string Name;
        [Export]
        public Variant.Type VariantType;
        [Export]
        public SerialMetadata Metadata;
        [Export]
        public byte Index;
        [Export]
        public long InterestMask;
        [Export]
        public int ClassIndex = -1;
    }
}