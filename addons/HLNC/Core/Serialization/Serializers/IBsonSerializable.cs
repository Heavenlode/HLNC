using Godot;
using MongoDB.Bson;

namespace HLNC {

    // Non-generic base interface
    public interface IBsonSerializableBase {
        BsonValue BsonSerialize(Variant context);
    }

    // Generic interface inherits from base
    public interface IBsonSerializable<T> : IBsonSerializableBase where T : GodotObject {
        // BsonSerialize is inherited from IBsonSerializableBase
        static abstract T BsonDeserialize(Variant context, byte[] bson, T initialObject);
    }
}