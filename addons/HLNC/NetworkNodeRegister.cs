using Godot;

namespace HLNC
{
    public partial class NetworkNodeRegister : Resource
    {
        [Export]
        public string network_class_name;

        [Export]
        public PackedScene network_scene;
    }
}