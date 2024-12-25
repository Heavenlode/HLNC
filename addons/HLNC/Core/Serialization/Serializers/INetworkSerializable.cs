using Godot;
using HLNC.Serialization;

namespace HLNC {

    // TODO: Document this interface, when it's necessary, how it's used, etc.
    public interface INetworkSerializable<T> where T : GodotObject {
        public static abstract HLBuffer NetworkSerialize(WorldRunner currentWorld, NetPeer peer, T obj);
        public static abstract T NetworkDeserialize(WorldRunner currentWorld, HLBuffer buffer, T initialObject);
    }
}