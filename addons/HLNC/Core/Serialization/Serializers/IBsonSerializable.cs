using Godot;
using HLNC.Serialization;
using MongoDB.Bson;

namespace HLNC {

    // TODO: Document this interface, when it's necessary, how it's used, etc.
    public interface IBsonSerializable {
        public BsonValue BsonSerialize(Variant context);
        public static abstract GodotObject BsonDeserialize(Variant context, BsonValue bson, GodotObject initialObject);
    }
}