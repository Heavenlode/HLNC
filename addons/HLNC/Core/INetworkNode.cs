using HLNC.Serialization.Serializers;

namespace HLNC {
    public interface INetworkNode {
        public NetworkController Network { get; }
        public IStateSerializer[] Serializers { get; }
        public void SetupSerializers();
    }
}