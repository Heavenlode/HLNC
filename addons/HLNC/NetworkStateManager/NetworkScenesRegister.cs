using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace HLNC
{

	public struct CollectedNetworkProperty
	{
		public string NodePath;
		public string Name;
		public Variant.Type Type;
		public byte Index;
	}
	public partial class NetworkScenesRegister : Node
	{
		private const int MAX_NETWORK_PROPERTIES = 64;

		public static Dictionary<byte, PackedScene> SCENES_MAP = new Dictionary<byte, PackedScene>() { };
		public static Dictionary<string, byte> SCENES_PACK = new Dictionary<string, byte>() { };

		public static Dictionary<string, Dictionary<string, Dictionary<string, CollectedNetworkProperty>>> PROPERTIES_MAP = new Dictionary<string,Dictionary<string, Dictionary<string, CollectedNetworkProperty>>>() { };
		public static Dictionary<string, Dictionary<byte, CollectedNetworkProperty>> PROPERTY_LOOKUP = new Dictionary<string, Dictionary<byte, CollectedNetworkProperty>>() { };
		
		public delegate void LoadCompleteEventHandler();

		// public static GetPropertyById
		public NetworkScenesRegister()
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					var attribs = type.GetCustomAttributes(typeof(NetworkScenes), false);
					if (attribs != null && attribs.Length > 0)
					{
						var attrib = (NetworkScenes)attribs[0];
						for (int i = 0; i < attrib.scenePaths.Length; i++)
						{
							var scenePath = attrib.scenePaths[i];
							var id = (byte)SCENES_MAP.Count;

							// Register the scenes to be instantiated across the network
							SCENES_PACK.TryAdd(scenePath, id);
							if (SCENES_MAP.TryAdd(id, GD.Load<PackedScene>(scenePath)))
							{
								var node = SCENES_MAP[id].Instantiate() as NetworkNode3D;
								// GD.Print("Registering scene: " + scenePath + " with id " + id);

								// Statically cache the locations of network properties of the child nodes within the scene
								PROPERTIES_MAP.TryAdd(scenePath, new Dictionary<string, Dictionary<string, CollectedNetworkProperty>>());
								node.Ready += () =>
								{
									var propertyId = -1;
									var nodes = new List<Node>() { node };
									while (nodes.Count > 0)
									{
										var child = nodes[0];
										nodes.RemoveAt(0);
										nodes.AddRange(child.GetChildren());
										if (child is not NetworkNode3D)
										{
											continue;
										}
										// GD.Print("Registering properties for " + child.GetPath());

										// Reflect on the child and collect all properties with the NetworkProperty attribute
										foreach (PropertyInfo property in child.GetType().GetProperties())
										{
											foreach (Attribute attr in property.GetCustomAttributes(true))
											{
												if (attr is not NetworkProperty)
												{
													continue;
												}

												propertyId += 1;
												if (propertyId >= MAX_NETWORK_PROPERTIES)
												{
													GD.PrintErr("NetworkPropertiesSerializer: Too many network properties on " + node.Name + ". The maximum is " + MAX_NETWORK_PROPERTIES + ". Properties beyond the maximum will not be serialized.");
													return;
												}
												Variant.Type type = Variant.Type.Nil;
												if (property.PropertyType == typeof(int))
												{
													type = Variant.Type.Int;
												}
												else if (property.PropertyType == typeof(float))
												{
													type = Variant.Type.Float;
												}
												else if (property.PropertyType == typeof(string))
												{
													type = Variant.Type.String;
												}
												else if (property.PropertyType == typeof(Vector3))
												{
													type = Variant.Type.Vector3;
												}
												else if (property.PropertyType == typeof(Quaternion))
												{
													type = Variant.Type.Quaternion;
												}
												else if (property.PropertyType == typeof(bool))
												{
													type = Variant.Type.Bool;
												}
												else
												{
													GD.PrintErr("NetworkPropertiesSerializer: Unsupported property type " + property.PropertyType + " on " + node.Name + "." + property.Name + ". Only int, float, string, Vector3, Quat, Color, and bool are supported.");
													return;
												}
												var relativeChildPath = node.GetPathTo(child);
												var collectedProperty = new CollectedNetworkProperty
												{
													NodePath = relativeChildPath,
													Name = property.Name,
													Type = type,
													Index = (byte)propertyId,
												};
												PROPERTIES_MAP[scenePath].TryAdd(relativeChildPath, new Dictionary<string, CollectedNetworkProperty>());
												PROPERTIES_MAP[scenePath][relativeChildPath].TryAdd(property.Name, collectedProperty);
												PROPERTY_LOOKUP.TryAdd(scenePath, new Dictionary<byte, CollectedNetworkProperty>());;
												PROPERTY_LOOKUP[scenePath].TryAdd((byte)propertyId, collectedProperty);
												// GD.Print("Registered property: " + relativeChildPath + "." + property.Name + "for scene " + scenePath);
											}
										}
									}
								};
								AddChild(node);
							}
						}
					}
				}
			}
		}
	}
}