using Godot;
using Godot.Collections;

namespace HLNC.Serialization
{
    [Tool]
    internal partial class SceneBytecode : RefCounted
    {
        public Dictionary<string, Dictionary<string, CollectedNetProperty>> Properties;
        public Dictionary<string, Dictionary<string, CollectedNetFunction>> Functions;
        public Array<Dictionary> StaticNetNodes;
        public bool IsNetScene;
    }
}