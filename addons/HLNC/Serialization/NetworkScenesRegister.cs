using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Godot;

namespace HLNC.Serialization
{
    public enum VariantSubtype {
        None,
        Guid,
        Byte,
        Int,

    }
    internal struct CollectedNetworkProperty
    {
        public string NodePath;
        public string Name;
        public Variant.Type Type;
        public byte Index;
        public VariantSubtype Subtype;
        public long InterestMask;
    }
    public partial class NetworkScenesRegister : Node
    {
        private const int MAX_NETWORK_PROPERTIES = 64;

        /// <summary>
        /// Map of scene IDs to scene paths
        /// </summary>
        internal static Dictionary<byte, PackedScene> SCENES_MAP = [];

        /// <summary>
        /// Map of scene paths to scene IDs
        /// </summary>
        internal static Dictionary<string, byte> SCENES_PACK = [];

        // This statically tracks every node path for every networked scene
        // For example, NODE_PATHS[0] indicates the node paths for the 0th scene
        // NODE_PATHS[0][0] Indicates the node path for the 0th node in the 0th scene, in other words the "root" of that scene
        // NODE_PATHS[0][1] Indicates the node path for the 1st node in the 0th scene, in other words the first child of the root
        // And so on
        internal static Dictionary<byte, Dictionary<int, string>> NODE_PATHS_MAP = [];
        
        internal static Dictionary<byte, Dictionary<string, int>> NODE_PATHS_PACK = [];

        /// <summary>
        /// A map of every packed scene to a list of paths to its internal network nodes.
        /// </summary>
        internal static Dictionary<string, HashSet<string>> STATIC_NETWORK_NODE_PATHS = [];

        /// <summary>
        /// A Dictionary of ScenePath to NodePath to PropertyName to CollectedNetworkProperty.
        /// It includes all child Network Nodes within the Scene including itself, but not nested network scenes.
        /// </summary>
        internal static Dictionary<string, Dictionary<string, Dictionary<string, CollectedNetworkProperty>>> PROPERTIES_MAP = [];
        internal static Dictionary<string, Dictionary<byte, CollectedNetworkProperty>> PROPERTY_LOOKUP = [];
        internal delegate void LoadCompleteEventHandler();

        // public static GetPropertyById
        public override void _EnterTree()
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
                            // SCENE_PATH_TO_NAME.TryAdd(scenePath, type.Name);
                            if (SCENES_MAP.TryAdd(id, GD.Load<PackedScene>(scenePath)))
                            {
                                PROPERTIES_MAP.TryAdd(scenePath, []);
                                NODE_PATHS_MAP.TryAdd(id, []);
                                NODE_PATHS_PACK.TryAdd(id, []);
                                var node = SCENES_MAP[id].Instantiate() as NetworkNode3D;
                                node.ProcessMode = ProcessModeEnum.Disabled;
                                if (node == null)
                                {
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

                                    if (child.GetMeta("is_network_scene", false).AsBool() && child != node)
                                    { 
                                        // NetworkScenes manage their own properties, so we skip nested ones.
                                        continue;
                                    }

                                    nodes.AddRange(child.GetChildren());
                                    var nodePath = node.GetPathTo(child);
                                    NODE_PATHS_MAP[id].TryAdd(nodePathId, nodePath);
                                    NODE_PATHS_PACK[id].TryAdd(nodePath, nodePathId);
                                    nodePathId += 1;

                                    if (!child.GetMeta("is_network_node", false).AsBool())
                                    {
                                        // Don't watch properties of nodes that aren't NetworkNodes
                                        continue;
                                    }
                                    STATIC_NETWORK_NODE_PATHS.TryAdd(scenePath, []);
                                    STATIC_NETWORK_NODE_PATHS[scenePath].Add(nodePath);
                                    

                                    if (child is NetworkNode3D)
                                    {
                                        // GD.Print("Registering properties for " + child.Name, " : ", child.GetType());

                                        // Reflect on the child and collect all properties with the NetworkProperty attribute
                                        foreach (PropertyInfo property in child.GetType().GetProperties())
                                        {
                                            foreach (Attribute attr in property.GetCustomAttributes(true))
                                            {
                                                if (attr is not NetworkProperty)
                                                {
                                                    continue;
                                                }

                                                var subType = (attr as NetworkProperty).Subtype;

                                                propertyId += 1;
                                                if (propertyId >= MAX_NETWORK_PROPERTIES)
                                                {
                                                    GD.PrintErr("NetworkPropertiesSerializer: Too many network properties on " + node.Name + ". The maximum is " + MAX_NETWORK_PROPERTIES + ". Properties beyond the maximum will not be serialized.");
                                                    return;
                                                }
                                                Variant.Type propType = Variant.Type.Nil;
                                                if (property.PropertyType == typeof(long) || property.PropertyType == typeof(int) || property.PropertyType == typeof(byte))
                                                {
                                                    propType = Variant.Type.Int;
                                                    if (property.PropertyType == typeof(byte))
                                                    {
                                                        subType = VariantSubtype.Byte;
                                                    }
                                                    else if (property.PropertyType == typeof(int))
                                                    {
                                                        subType = VariantSubtype.Int;
                                                    }
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
                                                } else if (property.PropertyType == typeof(byte[]))
                                                {
                                                    propType = Variant.Type.PackedByteArray;
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
                                                    Subtype = subType,
                                                    InterestMask = (attr as NetworkProperty).InterestMask,
                                                };
                                                PROPERTIES_MAP[scenePath].TryAdd(relativeChildPath, []);
                                                PROPERTIES_MAP[scenePath][relativeChildPath].TryAdd(property.Name, collectedProperty);
                                                PROPERTY_LOOKUP.TryAdd(scenePath, []); ;
                                                PROPERTY_LOOKUP[scenePath].TryAdd((byte)propertyId, collectedProperty);
                                                // GD.Print("Registered property: " + relativeChildPath + "." + property.Name + "for scene " + scenePath);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // This is for a custom kind of NetworkNode
                                        // For HLNC, that just means GDNetworkNode3D

                                        // GD.Print("Registering properties for " + child.GetPath());
                                        var props = child.GetPropertyList();
                                        foreach (var prop in props)
                                        {
                                            if (prop.TryGetValue("name", out var name) && prop.TryGetValue("type", out var propType))
                                            {
                                                if (name.VariantType != Variant.Type.String || propType.VariantType != Variant.Type.Int)
                                                {
                                                    GD.PrintErr("NetworkPropertiesSerializer: Invalid property definition on " + node.Name + ". Only string names and int types are supported.");
                                                    return;
                                                }
                                                if (!name.AsString().StartsWith("network_"))
                                                {
                                                    continue;
                                                }
                                                if (name.AsString() == "network_id") continue;

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
                                                PROPERTIES_MAP[scenePath].TryAdd(relativeChildPath, []);
                                                PROPERTIES_MAP[scenePath][relativeChildPath].TryAdd((string)name, collectedProperty);
                                                PROPERTY_LOOKUP.TryAdd(scenePath, []); ;
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