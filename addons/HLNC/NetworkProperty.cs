using System;

namespace HLNC
{
    /// <summary>
    /// Mark a property as being Networked.
    /// The <see cref="NetworkPeerManager"/> automatically processes these through the <see cref="Serialization.Serializers.NetworkPropertiesSerializer"/> to be optimally sent across the network.
    /// Only changes are networked.
    /// When the client receives a change on the property, if a method exists <code>OnNetworkChange{PropertyName}(int tick, T oldValue, T newValue)</code> it will be called on the client side.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NetworkProperty : Attribute
    {
        public Flags flags;
        public enum Flags
        {
            LinearInterpolation = 1 << 0,
            LossyConsistency = 1 << 1,
            SyncOnInterest = 1 << 2,
        }
        public NetworkProperty(Flags flags = 0)
        {
            this.flags = flags;
        }
    }
}