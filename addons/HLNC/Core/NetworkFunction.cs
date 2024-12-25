using System;
using System.Linq;
using Godot;
using HLNC.Serialization;
using MethodBoundaryAspect.Fody.Attributes;

namespace HLNC
{
    public sealed class NetworkFunction : OnMethodBoundaryAspect
    {
        public bool WithPeer { get; set; } = false;
        // TODO: I'm worried that this somehow may produce memory leaks
        public override void OnEntry(MethodExecutionArgs args)
        {
            if (args.Instance is NetworkNode3D netNode)
            {
                if (netNode.IsRemoteCall)
                {
                    // We only send a remote call if the incoming call isn't already from remote.
                    return;
                }
                var networkScene = "";
                if (netNode.IsNetworkScene)
                {
                    networkScene = netNode.SceneFilePath;
                }
                else
                {
                    networkScene = netNode.NetworkParent.Node.SceneFilePath;
                }

                NetworkId netId;
                if (netNode.IsNetworkScene)
                {
                    netId = netNode.NetworkId;
                }
                else
                {
                    netId = netNode.NetworkParent.NetworkId;
                }

                CollectedNetworkFunction functionInfo;
                if (!NetworkScenesRegister
                        .LookupFunction(networkScene, netNode.NodePathFromNetworkScene(), args.Method.Name, out functionInfo))
                {
                    throw new Exception($"Function {args.Method.Name} not found in network scene {networkScene}");
                }

                var arguments = args.Arguments;
                if (functionInfo.WithPeer)
                {
                    arguments = arguments.Skip(1).ToArray();
                }

                netNode.CurrentWorld
                    .SendNetworkFunction(netId, functionInfo.Index, arguments.ToList().Select((x, index) =>
                    {
                        switch (functionInfo.Arguments[index].Type)
                        {
                            case Variant.Type.Int:
                                if (functionInfo.Arguments[index].Subtype == VariantSubtype.Int)
                                    return Variant.From((int)x);
                                else if (functionInfo.Arguments[index].Subtype == VariantSubtype.Byte)
                                    return Variant.From((byte)x);
                                else
                                    return Variant.From((long)x);
                            case Variant.Type.Float:
                                return Variant.From((float)x);
                            case Variant.Type.String:
                                return Variant.From((string)x);
                            case Variant.Type.Vector2:
                                return Variant.From((Vector2)x);
                            case Variant.Type.Vector3:
                                return Variant.From((Vector3)x);
                            case Variant.Type.Quaternion:
                                return Variant.From((Quaternion)x);
                            case Variant.Type.Color:
                                return Variant.From((Color)x);
                            default:
                                throw new Exception($"Unsupported argument type {functionInfo.Arguments[index].Type}");
                        }
                    }).ToArray()
                    );
            }
            else
            {
                throw new Exception("NetworkFunction attribute can only be used on classes that implement INetworkFunctionHandler");
            }
        }
    }
}