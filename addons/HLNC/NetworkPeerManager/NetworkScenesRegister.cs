using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

		// This statically tracks every node path for every networked scene
		// For example, NODE_PATHS[0] indicates the node paths for the 0th scene
		// NODE_PATHS[0][0] Indicates the node path for the 0th node in the 0th scene, in other words the "root" of that scene
		// NODE_PATHS[0][1] Indicates the node path for the 1st node in the 0th scene, in other words the first child of the root
		// And so on
		public static Dictionary<byte, Dictionary<int, string>> NODE_PATHS_MAP = new Dictionary<byte, Dictionary<int, string>>() { };
		public static Dictionary<byte, Dictionary<string, int>> NODE_PATHS_PACK = new Dictionary<byte, Dictionary<string, int>>() { };

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
								PROPERTIES_MAP.TryAdd(scenePath, new Dictionary<string, Dictionary<string, CollectedNetworkProperty>>());
								NODE_PATHS_MAP.TryAdd(id, new Dictionary<int, string>());
								NODE_PATHS_PACK.TryAdd(id, new Dictionary<string, int>());
								var node = SCENES_MAP[id].Instantiate() as NetworkNode3D;
								node.ProcessMode = ProcessModeEnum.Disabled;
								if (node == null) {
									GD.Print("NetworkScene failed to register: " + scenePath);
									GD.Print("Did you attach the correct NetworkNode3D script to the scene root node?");
									continue;
								}
	 
								var propertyId = -1;
								var nodePathId = 0;
								var nodes = new List<Node>() { node };
								while (nodes.Count > 0)
								{
									var child = nodes[0];
									nodes.RemoveAt(0);

									if (child.HasMeta("is_network_scene") && child != node)
									{
										// NetworkScenes manage their own properties, so we skip nested ones.
										continue;
									}

									nodes.AddRange(child.GetChildren());
									var nodePath = node.GetPathTo(child);
									NODE_PATHS_MAP[id].TryAdd(nodePathId, nodePath);
									NODE_PATHS_PACK[id].TryAdd(nodePath, nodePathId);
									nodePathId += 1;

									if (!child.HasMeta("is_network_node")) {
										// Don't watch properties of nodes that aren't NetworkNodes
										continue;
									} 

									if (child is NetworkNode3D)
									{
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
												Variant.Type propType = Variant.Type.Nil;
												if (property.PropertyType == typeof(int))
												{
													propType = Variant.Type.Int;
												}
												else if (property.PropertyType == typeof(float))
												{
													propType = Variant.Type.Float;
												}
												else if (property.PropertyType == typeof(string))
												{
													propType = Variant.Type.String;
												}
												else if (property.PropertyType == typeof(Vector3))
												{
													propType = Variant.Type.Vector3;
												}
												else if (property.PropertyType == typeof(Quaternion))
												{
													propType = Variant.Type.Quaternion;
												}
												else if (property.PropertyType == typeof(bool))
												{
													propType = Variant.Type.Bool;
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
													Type = propType,
													Index = (byte)propertyId,
												};
												PROPERTIES_MAP[scenePath].TryAdd(relativeChildPath, new Dictionary<string, CollectedNetworkProperty>());
												PROPERTIES_MAP[scenePath][relativeChildPath].TryAdd(property.Name, collectedProperty);
												PROPERTY_LOOKUP.TryAdd(scenePath, new Dictionary<byte, CollectedNetworkProperty>());;
												PROPERTY_LOOKUP[scenePath].TryAdd((byte)propertyId, collectedProperty);
												// GD.Print("Registered property: " + relativeChildPath + "." + property.Name + "for scene " + scenePath);
											}
										}
									} else {
										// This is for a custom kind of NetworkNode
										// For HLNC, that just means GDNetworkNode3D

										// GD.Print("Registering properties for " + child.GetPath());
										var props = child.GetPropertyList();
										foreach (var prop in props) {
											if (prop.TryGetValue("name", out var name) && prop.TryGetValue("type", out var propType)) {
												if (name.VariantType != Variant.Type.String || propType.VariantType != Variant.Type.Int)
												{
													GD.PrintErr("NetworkPropertiesSerializer: Invalid property definition on " + node.Name + ". Only string names and int types are supported.");
													return;
												} 
												if (!((string)name).StartsWith("network_"))
												{
													continue;
												}
												propertyId += 1;
												if (propertyId >= MAX_NETWORK_PROPERTIES)
												{
													GD.PrintErr("NetworkPropertiesSerializer: Too many network properties on " + node.Name + ". The maximum is " + MAX_NETWORK_PROPERTIES + ". Properties beyond the maximum will not be serialized.");
													return;
												}
												var relativeChildPath = node.GetPathTo(child);
												var collectedProperty = new CollectedNetworkProperty
												{
													NodePath = relativeChildPath,
													Name = (string)name,
													Type = (Variant.Type)(int)propType,
													Index = (byte)propertyId,
												};
												// GD.Print("Registered property: " + relativeChildPath + "." + name + " of type " + type + " for scene " + scenePath);
												PROPERTIES_MAP[scenePath].TryAdd(relativeChildPath, new Dictionary<string, CollectedNetworkProperty>());
												PROPERTIES_MAP[scenePath][relativeChildPath].TryAdd((string)name, collectedProperty);
												PROPERTY_LOOKUP.TryAdd(scenePath, new Dictionary<byte, CollectedNetworkProperty>());;
												PROPERTY_LOOKUP[scenePath].TryAdd((byte)propertyId, collectedProperty);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}