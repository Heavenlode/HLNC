using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HLNC.Serialization;

namespace HLNC.Serialization.Serializers
{
    public interface IDeserializedData { };

    /// <summary>
    /// Defines an object which the server utilizes to serialize and send data to the client, and the client can then receive and deserialize from the server.
    /// </summary>
    public interface IStateSerailizer
    {
        /// <summary>
        /// Client-side only. Receive and deserialize binary received from the server.
        /// </summary>
        /// <param name="networkState"></param>
        /// <param name="data"></param>
        /// <param name="nodeOut"></param>
        public void Import(WorldRunner currentWorld, HLBuffer data, out NetworkNodeWrapper nodeOut);

        /// <summary>
        /// Server-side only. Serialize and send data to the client.
        /// </summary>
        /// <param name="networkState"></param>
        /// <param name="peer"></param>
        /// <returns></returns>
        public HLBuffer Export(WorldRunner currentWorld, NetPeer peer);
        public void Acknowledge(WorldRunner currentWorld, NetPeer peer, Tick tick);
        public void PhysicsProcess(double delta);
        public void Cleanup();
    }
    public interface IStateSerializable
    {
        public IStateSerailizer[] Serializers { get; }
    }
}