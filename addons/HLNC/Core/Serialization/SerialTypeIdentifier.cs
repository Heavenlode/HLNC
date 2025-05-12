using System;

namespace HLNC.Serialization
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class SerialTypeIdentifier : Attribute
    {
        public string Name { get; }
        public SerialTypeIdentifier(string name)
        {
            Name = name;
        }
    }
}