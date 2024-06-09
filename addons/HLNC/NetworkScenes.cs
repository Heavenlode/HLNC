using System;

namespace HLNC
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NetworkScenes(params string[] path) : Attribute
    {
        public string[] scenePaths = path;
    }
}