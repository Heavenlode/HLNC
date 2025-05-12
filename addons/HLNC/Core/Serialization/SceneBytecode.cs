using Godot;
using Godot.Collections;

namespace HLNC.Serialization
{
    [Tool]
    internal partial class SceneBytecode : RefCounted
    {
        public Dictionary<string, Dictionary<string, ProtocolNetProperty>> Properties;
        public Dictionary<string, Dictionary<string, ProtocolNetFunction>> Functions;
        public Array<Dictionary> StaticNetNodes;
        public bool IsNetScene;
    }
}