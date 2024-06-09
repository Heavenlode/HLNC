using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace HLNC.StateSerializers
{
    internal class NetworkPropertiesSerializer : IStateSerailizer
    {
        private struct Data
        {
            public long propertiesUpdated;
            public Dictionary<byte, Variant> properties;
        }
        private const int MAX_NETWORK_PROPERTIES = 64;
        private NetworkNode3D node;

        private Dictionary<byte, Variant> cachedPropertyChanges = [];
        public struct LerpableChangeQueue
        {
            public CollectedNetworkProperty Prop;
            public Variant From;
            public Variant To;
            public double Weight;
        }

        private Dictionary<string, LerpableChangeQueue> lerpableChangeQueue = [];

        private Dictionary<byte, bool> propertyUpdated = [];
        public NetworkPropertiesSerializer(NetworkNode3D node)
        {
            this.node = node;

            // First, determine if the Node class has the NetworkScene attribute
            // This is because only a network scene will serialize network node properties recursively
            if (!node.HasMeta("is_network_scene"))
            {
                return;
            }

            node.Ready += () =>
            {
                if (NetworkRunner.Instance.IsServer)
                {
                    node.Connect("NetworkPropertyChanged", Callable.From((string nodePath, string propertyName) =>
                    {
                        if (NetworkScenesRegister.PROPERTIES_MAP[node.SceneFilePath][nodePath].TryGetValue(propertyName, out var property))
                        {
                            propertyUpdated[property.Index] = true;
                        }
                    }));
                }
                else
                {
                    // As a client, apply all the cached changes
                    foreach (var propIndex in cachedPropertyChanges.Keys)
                    {
                        var prop = NetworkScenesRegister.PROPERTY_LOOKUP[node.SceneFilePath][propIndex];
                        ImportProperty(prop, NetworkRunner.Instance.CurrentTick, cachedPropertyChanges[propIndex]);
                    }
                }
            };

        }

        public void ImportProperty(CollectedNetworkProperty prop, Tick tick, Variant value)
        {
            var propNode = node.GetNode(prop.NodePath);
            Variant oldVal = propNode.Get(prop.Name);
            var friendlyPropName = prop.Name;
            if (friendlyPropName.StartsWith("network_"))
            {
                friendlyPropName = friendlyPropName[8..];
            }
            if (propNode.HasMethod("OnNetworkChange" + prop.Name))
            {
                propNode.Call("OnNetworkChange" + prop.Name, tick, oldVal, value);
            }
            else if (propNode.HasMethod("_on_network_change_" + friendlyPropName))
            {
                propNode.Call("_on_network_change_" + friendlyPropName, tick, oldVal, value);
            }

            lerpableChangeQueue[prop.Name] = new LerpableChangeQueue
            {
                Prop = prop,
                From = oldVal,
                To = value,
                Weight = 0.0f
            };
        }


        private Data Deserialize(HLBuffer buffer)
        {
            var data = new Data
            {
                propertiesUpdated = HLBytes.UnpackInt64(buffer),
                properties = []
            };
            for (var i = 0; i < MAX_NETWORK_PROPERTIES; i++)
            {
                if ((data.propertiesUpdated & ((long)1 << i)) == 0)
                {
                    continue;
                }
                var prop = NetworkScenesRegister.PROPERTY_LOOKUP[node.SceneFilePath][(byte)i];
                var varVal = HLBytes.UnpackVariant(buffer, prop.Type);
                data.properties[(byte)i] = varVal.Value;
            }
            return data;
        }

        public void Import(IGlobalNetworkState networkState, HLBuffer buffer, out NetworkNode3D nodeOut)
        {
            nodeOut = node;
            var data = Deserialize(buffer);
            foreach (var propIndex in data.properties.Keys)
            {
                if (node.IsNodeReady())
                {
                    var prop = NetworkScenesRegister.PROPERTY_LOOKUP[node.SceneFilePath][propIndex];
                    ImportProperty(prop, networkState.CurrentTick, data.properties[propIndex]);
                }
                else
                {
                    cachedPropertyChanges[propIndex] = data.properties[propIndex];
                }
            }

            return;
        }

        private Dictionary<PeerId, Dictionary<Tick, long>> peerBufferCache = [];
        // This should instead be a map of variable values that we can resend until acknowledgement

        public HLBuffer Export(IGlobalNetworkState networkState, PeerId peerId)
        {
            var buffer = new HLBuffer();

            if (!node.SpawnAware.ContainsKey(peerId))
            {
                // The target client is not aware of this node yet. Don't send updates.
                return buffer;
            }

            if (!peerBufferCache.ContainsKey(peerId))
            {
                peerBufferCache[peerId] = [];
            }

            long propertiesUpdated = 0;

            // First, we find any newly updated properties
            foreach (var propIndex in propertyUpdated.Keys)
            {
                propertiesUpdated |= (long)1 << propIndex;
                propertyUpdated[propIndex] = false;
            }

            // Store them in the cache to resend in the future until the client acknowledges having received the update
            if (propertiesUpdated != 0)
            {
                peerBufferCache[peerId][networkState.CurrentTick] = propertiesUpdated;
            }

            propertiesUpdated = 0;

            // Now collect every pending property update
            foreach (var tick in peerBufferCache[peerId].Keys.OrderBy(x => x))
            {
                propertiesUpdated |= peerBufferCache[peerId][tick];
            }

            if (propertiesUpdated == 0)
                return buffer;

            // Indicate which variables have been updated
            HLBytes.Pack(buffer, propertiesUpdated);

            // Finally, pack the variable values
            for (var i = 0; i < MAX_NETWORK_PROPERTIES; i++)
            {
                if ((propertiesUpdated & ((long)1 << i)) == 0)
                {
                    continue;
                }

                var prop = NetworkScenesRegister.PROPERTY_LOOKUP[node.SceneFilePath][(byte)i];
                var propNode = node.GetNode(prop.NodePath);
                var varVal = propNode.Get(prop.Name);
                HLBytes.Pack(buffer, varVal);
            }

            return buffer;
        }

        public void Cleanup()
        {
            propertyUpdated.Clear();
        }

        public void Acknowledge(IGlobalNetworkState networkState, PeerId peerId, Tick latestAck)
        {
            if (!peerBufferCache.ContainsKey(peerId))
            {
                return;
            }
            foreach (var tick in peerBufferCache[peerId].Keys.Where(x => x <= latestAck).ToList())
            {
                peerBufferCache[peerId].Remove(tick);
            }
        }

        private static Vector3 Lerp(Vector3 First, Vector3 Second, float Amount)
        {
            float retX = Mathf.Lerp(First.X, Second.X, Amount);
            float retY = Mathf.Lerp(First.Y, Second.Y, Amount);
            float retZ = Mathf.Lerp(First.Z, Second.Z, Amount);
            return new Vector3(retX, retY, retZ);
        }

        public void PhysicsProcess(double delta)
        {

            foreach (var propName in lerpableChangeQueue.Keys.ToList())
            {
                var toLerp = lerpableChangeQueue[propName];
                var lerpNode = node.GetNode(toLerp.Prop.NodePath);
                if (toLerp.Weight < 1.0)
                {
                    double result = -1;
                    if (lerpNode.HasMethod("NetworkLerp" + toLerp.Prop.Name))
                    {
                        result = (double)lerpNode.Call("NetworkLerp" + toLerp.Prop.Name, toLerp.From, toLerp.To, toLerp.Weight);
                        if (result >= 0)
                        {
                            toLerp.Weight = result;
                        }
                    }
                    if (result == -1)
                    {
                        // TODO: If this is too fast, it creates a jitter effect
                        toLerp.Weight = Math.Min(toLerp.Weight + (delta * 10), 1.0);
                        if (toLerp.Prop.Type == Variant.Type.Quaternion)
                        {
                            var next_value = ((Quaternion)toLerp.From).Normalized().Slerp(((Quaternion)toLerp.To).Normalized(), (float)toLerp.Weight);
                            lerpNode.Set(toLerp.Prop.Name, next_value);
                        }
                        else if (toLerp.Prop.Type == Variant.Type.Vector3)
                        {
                            var next_value = Lerp((Vector3)toLerp.From, (Vector3)toLerp.To, (float)toLerp.Weight);
                            lerpNode.Set(toLerp.Prop.Name, next_value);
                        }
                        else
                        {
                            lerpNode.Set(toLerp.Prop.Name, toLerp.To);
                            lerpableChangeQueue.Remove(propName);
                        }
                    }

                    if ((float)toLerp.Weight >= 1.0)
                        lerpableChangeQueue.Remove(propName);
                }
                lerpableChangeQueue[propName] = toLerp;
            }
        }
    }
}