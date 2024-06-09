using Godot;
using Godot.Collections;

namespace HLNC
{
    public interface INetworkInputHandler
    {
        public Godot.Collections.Dictionary<int, Variant> InputBuffer { get; }
    }
}