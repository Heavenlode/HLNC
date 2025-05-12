using Godot;

namespace HLNC.Serialization
{
    [Tool]
    public partial class ProtocolNetFunction : Resource
    {
        [Export]
        public string NodePath;
        [Export]
        public string Name;
        [Export]
        public byte Index;
        [Export]
        public NetFunctionArgument[] Arguments;
        [Export]
        public bool WithPeer;
        [Export]
        public NetFunction.NetworkSources Sources;
    }

}