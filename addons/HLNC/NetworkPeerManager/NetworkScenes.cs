using System;

namespace HLNC {
    
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class NetworkScenes : Attribute
	{
		public string[] scenePaths;
		public NetworkScenes(params string[] path)
		{
			scenePaths = path;
		}	
	}
}