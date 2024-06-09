using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HLNC.StateSerializers
{
    public interface IDeserializedData { };
    public interface IStateSerailizer
    {
        public void Import(IGlobalNetworkState networkState, HLBuffer data, out NetworkNode3D nodeOut);
        public HLBuffer Export(IGlobalNetworkState networkState, PeerId peer);
        public void Acknowledge(IGlobalNetworkState networkState, PeerId peer, Tick tick);
        public void PhysicsProcess(double delta);
        public void Cleanup();
    }
    public interface IStateSerializable
    {
        public IStateSerailizer[] Serializers { get; }
    }
}