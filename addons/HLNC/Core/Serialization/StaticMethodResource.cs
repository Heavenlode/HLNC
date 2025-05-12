using Godot;

namespace HLNC.Serialization
{
    public enum StaticMethodType {
        NetworkSerialize = 1 << 0,
        NetworkDeserialize = 1 << 1,
        BsonDeserialize = 1 << 2,
    }

    [Tool]
    public partial class StaticMethodResource : Resource
    {
        [Export]
        public StaticMethodType StaticMethodType;
        
        [Export]
        public string ReflectionPath;
    }
}
