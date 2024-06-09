using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HLNC.Serialization;

namespace HLNC.Serialization.Serializers
{
    public interface IDeserializedData { };
    public interface IStateSerailizer
    {
        public void Import(IPeerController networkState, HLBuffer data, out NetworkNode3D nodeOut);
        public HLBuffer Export(IPeerController networkState, PeerId peer);
        public void Acknowledge(IPeerController networkState, PeerId peer, Tick tick);
        public void PhysicsProcess(double delta);
        public void Cleanup();
    }
    public interface IStateSerializable
    {
        public IStateSerailizer[] Serializers { get; }
    }
}