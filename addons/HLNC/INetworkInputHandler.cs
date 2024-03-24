using Godot;
using Godot.Collections;

namespace HLNC {
    public interface INetworkInputHandler {
        public Dictionary<int, Variant> InputBuffer { get; }
    }
}