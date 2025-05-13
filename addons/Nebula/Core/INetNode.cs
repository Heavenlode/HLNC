using Nebula.Serialization.Serializers;

namespace Nebula {
    public interface INetNode {
        public NetworkController Network { get; }
        public IStateSerializer[] Serializers { get; }
        public void SetupSerializers();
    }
}