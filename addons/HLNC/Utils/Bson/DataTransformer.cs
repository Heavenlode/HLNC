using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HLNC.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace HLNC.Utils.Bson
{
    public static class DataTransformer
    {
        public static BsonDocument ToBSONDocument(INetworkNode networkNode, Variant context = new Variant(), bool recurse = true, HashSet<Type> skipNodeTypes = null, HashSet<Tuple<Variant.Type, VariantSubtype>> propTypes = null, HashSet<Tuple<Variant.Type, VariantSubtype>> skipPropTypes = null)
        {
            var network = networkNode.Network;
            if (!network.IsNetworkScene)
            {
                throw new System.Exception("Only scenes can be converted to BSON: " + network.Owner.Node.GetPath());
            }
            BsonDocument result = new BsonDocument();
            result["data"] = new BsonDocument();
            if (network.IsNetworkScene)
            {
                result["scene"] = network.Owner.Node.SceneFilePath;
            }
            // We retain this for debugging purposes.
            result["nodeName"] = network.Owner.Node.Name.ToString();

            foreach (var node in NetworkScenesRegister.ListProperties(network.Owner.Node.SceneFilePath))
            {
                var nodePath = node.Item1;
                var nodeProps = node.Item2;
                result["data"][nodePath] = new BsonDocument();
                var nodeData = result["data"][nodePath] as BsonDocument;
                var hasValues = false;
                foreach (var property in nodeProps)
                {
                    if (propTypes != null && !propTypes.Contains(new Tuple<Variant.Type, VariantSubtype>(property.Type, property.Subtype)))
                    {
                        continue;
                    }
                    if (skipPropTypes != null && skipPropTypes.Contains(new Tuple<Variant.Type, VariantSubtype>(property.Type, property.Subtype)))
                    {
                        continue;
                    }
                    var prop = network.Owner.Node.GetNode(nodePath).Get(property.Name);
                    var val = Serialization.BsonSerialize.SerializeVariant(context, prop, property.Subtype);
                    Debugger.Log($"Serializing: {nodePath}.{property.Name} with value: {val}", Debugger.DebugLevel.VERBOSE);
                    if (val == null) continue;
                    nodeData[property.Name] = val;
                    hasValues = true;
                }

                if (!hasValues)
                {
                    // Delete empty objects from JSON, i.e. network nodes with no network properties.
                    (result["data"] as BsonDocument).Remove(nodePath);
                }
            }

            if (recurse)
            {
                result["children"] = new BsonDocument();
                foreach (var child in network.NetworkSceneChildren)
                {
                    if (child.Node is INetworkNode networkChild && (skipNodeTypes == null || !skipNodeTypes.Contains(networkChild.GetType())))
                    {
                        string pathTo = network.Owner.Node.GetPathTo(networkChild.Network.Owner.Node);
                        if (!(result["children"] as BsonDocument).Contains(pathTo))
                        {
                            result["children"][pathTo] = new BsonArray();
                        }
                        (result["children"][pathTo] as BsonArray).Add(ToBSONDocument(networkChild, context, recurse, skipNodeTypes, propTypes, skipPropTypes));
                    }
                }
            }

            return result;
        }

        public static async Task<T> FromBSON<T>(Variant context, byte[] data, T fillNode = null) where T : Node, INetworkNode
        {
            return await FromBSON(context, BsonSerializer.Deserialize<BsonDocument>(data), fillNode);
        }

        public static async Task<T> FromBSON<T>(Variant context, BsonDocument data, T fillNode = null) where T : Node, INetworkNode
        {
            T node = fillNode;
            if (fillNode == null)
            {
                if (data.Contains("scene"))
                {
                    node = GD.Load<PackedScene>(data["scene"].AsString).Instantiate<T>();
                }
                else
                {
                    throw new System.Exception("No scene path found in BSON data");
                }
            }

            // Mark imported nodes accordingly
            if (!node.GetMeta("import_from_external", false).AsBool())
            {
                var tcs = new TaskCompletionSource<bool>();
                node.Ready += () =>
                {
                    foreach (var child in node.Network.GetNetworkChildren(NetworkController.NetworkChildrenSearchToggle.INCLUDE_SCENES))
                    {
                        if (child.IsNetworkScene)
                        {
                            child.Node.QueueFree();
                        }
                        else
                        {
                            child.Node.SetMeta("import_from_external", true);
                        }
                    }
                    node.SetMeta("import_from_external", true);
                    tcs.SetResult(true);
                };
                NetworkRunner.Instance.AddChild(node);
                await tcs.Task;
                NetworkRunner.Instance.RemoveChild(node);
            }

            foreach (var networkNodePathAndProps in data["data"] as BsonDocument)
            {
                var nodePath = networkNodePathAndProps.Name;
                var nodeProps = networkNodePathAndProps.Value as BsonDocument;
                var targetNode = node.GetNodeOrNull(nodePath);
                if (targetNode == null)
                {
                    Debugger.Log($"Node not found for: ${nodePath}", Debugger.DebugLevel.ERROR);
                    continue;
                }
                foreach (var prop in nodeProps)
                {
                    node.Network.InitialSetNetworkProperties.Add(new Tuple<string, string>(nodePath, prop.Name));
                    CollectedNetworkProperty propData;
                    if (!NetworkScenesRegister.LookupProperty(node.SceneFilePath, nodePath, prop.Name, out propData))
                    {
                        throw new Exception($"Failed to pack property: {nodePath}.{prop.Name}");
                    }
                    var variantType = propData.Type;
                    try
                    {
                        if (variantType == Variant.Type.String)
                        {
                            targetNode.Set(prop.Name, prop.Value.ToString());
                        }
                        else if (variantType == Variant.Type.Float)
                        {
                            targetNode.Set(prop.Name, prop.Value.AsDouble);
                        }
                        else if (variantType == Variant.Type.Int)
                        {
                            switch (propData.Subtype)
                            {
                                case VariantSubtype.NetworkId:
                                    if (prop.Value.AsInt64 != -1)
                                    {
                                        targetNode.Set(prop.Name, prop.Value.AsInt64);
                                    }
                                    break;
                                case VariantSubtype.Int:
                                    targetNode.Set(prop.Name, prop.Value.AsInt32);
                                    break;
                                case VariantSubtype.Byte:
                                    // Convert MongoDB Binary value to Byte
                                    targetNode.Set(prop.Name, (byte)prop.Value.AsInt32);
                                    break;
                                default:
                                    targetNode.Set(prop.Name, prop.Value.AsInt64);
                                    break;
                            }
                        }
                        else if (variantType == Variant.Type.Bool)
                        {
                            targetNode.Set(prop.Name, (bool)prop.Value);
                        }
                        else if (variantType == Variant.Type.Vector2)
                        {
                            var vec = prop.Value as BsonArray;
                            targetNode.Set(prop.Name, new Vector2((float)vec[0].AsDouble, (float)vec[1].AsDouble));
                        }
                        else if (variantType == Variant.Type.PackedByteArray)
                        {
                            if (propData.Subtype == VariantSubtype.Guid)
                            {
                                targetNode.Set(prop.Name, new BsonBinaryData(prop.Value.AsGuid, GuidRepresentation.CSharpLegacy).AsByteArray);
                            }
                            else
                            {
                                targetNode.Set(prop.Name, prop.Value.AsByteArray);
                            }
                        }
                        else if (variantType == Variant.Type.Vector3)
                        {
                            var vec = prop.Value as BsonArray;
                            targetNode.Set(prop.Name, new Vector3((float)vec[0].AsDouble, (float)vec[1].AsDouble, (float)vec[2].AsDouble));
                        }
                        else if (variantType == Variant.Type.Object)
                        {
                            var value = propData.BsonDeserialize(context, prop.Value, targetNode.Get(prop.Name).AsGodotObject());
                            if (value != null)
                            {
                                targetNode.Set(prop.Name, value);
                            }
                        }
                    }
                    catch (InvalidCastException)
                    {
                        Debugger.Log($"Failed to set property: {prop.Name} on {nodePath} with value: {prop.Value} and type: {variantType}", Debugger.DebugLevel.ERROR);
                    }
                }
            }
            if (data.Contains("children"))
            {
                foreach (var child in data["children"] as BsonDocument)
                {
                    var nodePath = child.Name;
                    var children = child.Value as BsonArray;
                    foreach (var childData in children)
                    {
                        var childNode = await FromBSON<T>(context, childData as BsonDocument);
                        var parent = node.GetNodeOrNull(nodePath);
                        if (parent == null)
                        {
                            Debugger.Log($"Parent node not found for: {nodePath}", Debugger.DebugLevel.ERROR);
                            continue;
                        }
                        parent.AddChild(childNode);
                    }
                }
            }
            return node;
        }
    }
}