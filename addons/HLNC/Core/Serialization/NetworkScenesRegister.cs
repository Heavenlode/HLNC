using System;
using Godot;
using System.Linq;
using MongoDB.Bson;
using System.Collections.Generic;
using HLNC.Addons.Lazy;

namespace HLNC.Serialization
{
    public enum VariantSubtype
    {
        None,
        Guid,
        Byte,
        Int,
        NetworkId,
        NetworkNode,
        Lazy

    }
    public struct VariantType
    {
        public Variant.Type Type;
        public VariantSubtype Subtype;
    }
    public struct CollectedNetworkProperty
    {
        public string NodePath;
        public string Name;
        public Variant.Type Type;
        public byte Index;
        public VariantSubtype Subtype;
        public long InterestMask;
        public Callable NetworkSerialize;
        public Callable NetworkDeserialize;
        public Func<Variant, BsonValue, GodotObject, GodotObject> BsonDeserialize;
    }

    public struct CollectedNetworkFunction
    {
        public string NodePath;
        public string Name;
        public byte Index;
        public VariantType[] Arguments;
        public bool WithPeer;
    }

    public static partial class NetworkScenesRegister
    {
        public static VariantType GetVariantType(Type t)
        {
            Variant.Type propType = Variant.Type.Nil;
            VariantSubtype subType = VariantSubtype.None;

            if (t == typeof(long) || t == typeof(int) || t == typeof(byte))
            {
                propType = Variant.Type.Int;
                if (t == typeof(byte))
                {
                    subType = VariantSubtype.Byte;
                }
                else if (t == typeof(int))
                {
                    subType = VariantSubtype.Int;
                }
            }
            else if (t == typeof(float))
            {
                propType = Variant.Type.Float;
            }
            else if (t == typeof(string))
            {
                propType = Variant.Type.String;
            }
            else if (t == typeof(Vector3))
            {
                propType = Variant.Type.Vector3;
            }
            else if (t == typeof(Quaternion))
            {
                propType = Variant.Type.Quaternion;
            }
            else if (t == typeof(bool))
            {
                propType = Variant.Type.Bool;
            }
            else if (t == typeof(byte[]))
            {
                propType = Variant.Type.PackedByteArray;
            }
            else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Godot.Collections.Dictionary<,>))
            {
                propType = Variant.Type.Dictionary;
            }
            // Check if the property is an enum
            else if (t.IsEnum)
            {
                propType = Variant.Type.Int;
                // var T = t.GetEnumUnderlyingType();
            }
            // TODO: Does this have performance issues?
            else if (t.GetInterfaces()
                        .Where(i => i.IsGenericType)
                        .Any(i => i.GetGenericTypeDefinition() == typeof(INetworkSerializable<>))
                    )
            {
                propType = Variant.Type.Object;
                if (t == typeof(LazyPeerState))
                {
                    subType = VariantSubtype.Lazy;
                }
                else if (t == typeof(INetworkNode))
                {
                    subType = VariantSubtype.NetworkNode;
                }
            }
            else
            {
                return new VariantType
                {
                    Type = Variant.Type.Nil,
                    Subtype = VariantSubtype.None
                };
            }

            return new VariantType
            {
                Type = propType,
                Subtype = subType
            };
        }

        private static Dictionary<byte, PackedScene> SCENES_CACHE = [];

        public static byte PackScene(string scene) {
            return SCENES_PACK[scene];
        }

        public static PackedScene UnpackScene(byte sceneId) {
            if (!SCENES_CACHE.ContainsKey(sceneId)) {
                SCENES_CACHE[sceneId] = GD.Load<PackedScene>(SCENES_MAP[sceneId]);
            } 
            return SCENES_CACHE[sceneId];
        }

        public static bool LookupProperty(string scene, string node, string property, out CollectedNetworkProperty prop) {
            if (!PROPERTIES_MAP.ContainsKey(scene) || !PROPERTIES_MAP[scene].ContainsKey(node) || !PROPERTIES_MAP[scene][node].ContainsKey(property)) {
                prop = new CollectedNetworkProperty();
                return false;
            }

            prop = PROPERTIES_MAP[scene][node][property];
            return true;
        }

        public static List<Tuple<string, List<CollectedNetworkProperty>>> ListProperties(string scene) {
            if (!PROPERTIES_MAP.ContainsKey(scene)) {
                return new List<Tuple<string, List<CollectedNetworkProperty>>>();
            }

            var result = new List<Tuple<string, List<CollectedNetworkProperty>>>();
            foreach (var node in PROPERTIES_MAP[scene]) {
                result.Add(new Tuple<string, List<CollectedNetworkProperty>>(node.Key, new List<CollectedNetworkProperty>(node.Value.Values)));
            }
            return result;
        }

        public static int GetPropertyCount(string scene) {
            if (!PROPERTIES_LOOKUP.ContainsKey(scene)) {
                return 0;
            }
            return PROPERTIES_LOOKUP[scene].Count;
        }

        public static CollectedNetworkProperty UnpackProperty(string scene, int propertyId) {
            return PROPERTIES_LOOKUP[scene][propertyId];
        }

        public static string UnpackNode(string scene, byte nodeId) {
            return STATIC_NETWORK_NODE_PATHS_MAP[scene][nodeId];
        }

        public static bool PackNode(string scene, string node, out byte nodeId) {
            if (!STATIC_NETWORK_NODE_PATHS_PACK.ContainsKey(scene) || !STATIC_NETWORK_NODE_PATHS_PACK[scene].ContainsKey(node)) {
                nodeId = 0;
                return false;
            }

            nodeId = STATIC_NETWORK_NODE_PATHS_PACK[scene][node];
            return true;
        }

        public static List<NetworkNodeWrapper> ListNetworkChildren(Node node) { 
            return NetworkScenesRegister.STATIC_NETWORK_NODE_PATHS_MAP[node.SceneFilePath]
                // The first element is the root node, so we skip
                .Skip(1)
                .Aggregate(new List<NetworkNodeWrapper>(), (acc, path) => {
                var child = new NetworkNodeWrapper(node.GetNodeOrNull(path.Value));
                if (child != null) {
                    acc.Add(child);
                }
                return acc;
            });
        }

        public static bool LookupFunction(string scene, string node, string function, out CollectedNetworkFunction func) {
            if (!FUNCTIONS_MAP.ContainsKey(scene) || !FUNCTIONS_MAP[scene].ContainsKey(node) || !FUNCTIONS_MAP[scene][node].ContainsKey(function)) {
                func = new CollectedNetworkFunction();
                return false;
            }

            func = FUNCTIONS_MAP[scene][node][function];
            return true;
        }

        public static CollectedNetworkFunction UnpackFunction(string scene, byte functionId) {
            return FUNCTIONS_LOOKUP[scene][functionId];
        }

        public static bool IsNetworkScene(string scenePath)
        {
            return SCENES_PACK.ContainsKey(scenePath);
        }
    }
}