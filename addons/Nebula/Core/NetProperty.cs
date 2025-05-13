using System;

namespace Nebula
{
    /// <summary>
    /// Mark a property as being Networked.
    /// The <see cref="WorldRunner"/> automatically processes these through the <see cref="Serialization.Serializers.NetPropertiesSerializer"/> to be optimally sent across the network.
    /// Only changes are networked.
    /// When the NetNode receives a change on the property, it will also attempt to call a method <code>NetNode.OnNetworkChange{PropertyName}(int tick, T oldValue, T newValue)</code> on the client side if it exists.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NetProperty : Attribute
    {
        public enum SyncFlags
        {
            LinearInterpolation = 1 << 0,
            LossyConsistency = 1 << 1,
        }

        public SyncFlags Flags;
        public long InterestMask = long.MaxValue;
    }
}