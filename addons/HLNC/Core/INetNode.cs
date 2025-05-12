using HLNC.Serialization.Serializers;

namespace HLNC {
    public interface INetNode {
        public NetworkController Network { get; }
        public IStateSerializer[] Serializers { get; }
        public void SetupSerializers();
    }
}