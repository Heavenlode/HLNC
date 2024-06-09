using System;
using Godot;

namespace HLNC
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NetworkProperty : Attribute
    {
        public Flags flags;
        public enum Flags
        {
            LinearInterpolation = 1 << 0,
            LossyConsistency = 1 << 1,
            SyncOnInterest = 1 << 2
        }
        public NetworkProperty(Flags flags = 0)
        {
            this.flags = flags;
        }
    }
}