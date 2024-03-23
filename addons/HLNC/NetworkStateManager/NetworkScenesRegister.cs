using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace HLNC
{
	public partial class NetworkScenesRegister : Node
	{
		public static Dictionary<byte, PackedScene> SCENES_MAP = new Dictionary<byte, PackedScene>() { };
		public static Dictionary<string, byte> SCENES_PACK = new Dictionary<string, byte>() { };

		public NetworkScenesRegister() {
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach (Type type in assembly.GetTypes()) {
					var attribs = type.GetCustomAttributes(typeof(NetworkScenes), false);
					if (attribs != null && attribs.Length > 0) {
						var attrib = (NetworkScenes)attribs[0];
						for (int i = 0; i < attrib.scenePaths.Length; i++) {
							var scenePath = attrib.scenePaths[i];
							var id = (byte)SCENES_MAP.Count;
							SCENES_PACK.TryAdd(scenePath, id);
							SCENES_MAP.TryAdd(id, GD.Load<PackedScene>(scenePath));
						}
					}
				}
			}
		}
	}
}