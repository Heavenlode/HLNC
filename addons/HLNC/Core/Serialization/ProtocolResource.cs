using Godot;
using Godot.Collections;

namespace HLNC.Serialization
{
    /// <summary>
    /// A resource that contains the compiled data of a <see cref="ProtocolRegistry"/>.
    /// This resource defines the bytes used to encode and decode network scenes, nodes, properties, and functions,
    /// as well as the lookup tables used to quickly find the corresponding data.
    /// With this compiled resource, the program is able to understand how to send and receive game state.
    /// </summary>
    [Tool]
    public partial class ProtocolResource : Resource
    {
        [Export]
        public Dictionary<int, StaticMethodResource> STATIC_METHODS = [];

        [Export]
        public Dictionary<byte, string> SCENES_MAP = [];

        [Export]
        public Dictionary<string, byte> SCENES_PACK = [];
        [Export]
        public Dictionary<string, Dictionary<byte, string>> STATIC_NETWORK_NODE_PATHS_MAP = [];

        [Export]
        public Dictionary<string, Dictionary<string, byte>> STATIC_NETWORK_NODE_PATHS_PACK = [];

        [Export]
        public Dictionary<string, Dictionary<string, Dictionary<string, ProtocolNetProperty>>> PROPERTIES_MAP = [];

        [Export]
        public Dictionary<string, Dictionary<string, Dictionary<string, ProtocolNetFunction>>> FUNCTIONS_MAP = [];

        [Export]
        public Dictionary<string, Dictionary<int, ProtocolNetFunction>> FUNCTIONS_LOOKUP = [];

        [Export]
        public Dictionary<string, Dictionary<int, ProtocolNetProperty>> PROPERTIES_LOOKUP = [];

        [Export]
        public Dictionary<string, int> SERIAL_TYPE_PACK = [];
    }
}