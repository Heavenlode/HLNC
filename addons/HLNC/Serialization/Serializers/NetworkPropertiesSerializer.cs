using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace HLNC.Serialization.Serializers
{
    internal partial class NetworkPropertiesSerializer : Node, IStateSerailizer
    {
        private struct Data
        {
            public long propertiesUpdated;
            public Dictionary<byte, Variant> properties;
        }
        private const int MAX_NETWORK_PROPERTIES = 64;
        private NetworkNodeWrapper wrapper;

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

        private Dictionary<NetPeer, long> peerInitialPropSync = [];
        public override void _EnterTree()
        {
            wrapper = new NetworkNodeWrapper(GetParent());
            Name = "NetworkPropertiesSerializer";

            // First, determine if the Node class has the NetworkScene attribute
            // This is because only a network scene will serialize network node properties recursively
            if (!wrapper.Node.GetMeta("is_network_scene", false).AsBool())
            {
                return;
            }

            if (NetworkRunner.Instance.IsServer)
            {
                wrapper.Node.Connect("NetworkPropertyChanged", Callable.From((string nodePath, string propertyName) =>
                {
                    if (!NetworkScenesRegister.PROPERTIES_MAP.TryGetValue(wrapper.Node.SceneFilePath, out var _prop))
                    {
                        GD.Print("Failed to load ", wrapper.Node.SceneFilePath);
                        return;
                    }
                    if (!NetworkScenesRegister.PROPERTIES_MAP[wrapper.Node.SceneFilePath].TryGetValue(nodePath, out var _prop2))
                    {
                        GD.Print("Failed to process ", wrapper.Node.SceneFilePath, "/", nodePath, "/", propertyName);
                        return;
                    }
                    if (NetworkScenesRegister.PROPERTIES_MAP[wrapper.Node.SceneFilePath][nodePath].TryGetValue(propertyName, out var property))
                    {
                        propertyUpdated[property.Index] = true;
                        nonDefaultProperties.Add(property.Index);
                    }
                }));

                wrapper.Node.Connect("InterestChanged", Callable.From((string peerId, long interest) =>
                {
                    var peer = NetworkRunner.Instance.GetPeer(peerId);
                    if (!peerInitialPropSync.ContainsKey(peer))
                    {
                        peerInitialPropSync.Remove(peer);
                    }
                }));
            }
            else
            {
                // As a client, apply all the cached changes
                foreach (var propIndex in cachedPropertyChanges.Keys)
                {
                    var prop = NetworkScenesRegister.PROPERTY_LOOKUP[wrapper.Node.SceneFilePath][propIndex];
                    ImportProperty(prop, wrapper.CurrentWorld.CurrentTick, cachedPropertyChanges[propIndex]);
                }
            }

        }

        public void ImportProperty(CollectedNetworkProperty prop, Tick tick, Variant value)
        {
            var propNode = wrapper.Node.GetNode(prop.NodePath);
            Variant oldVal = propNode.Get(prop.Name);
            if (oldVal.Equals(value))
            {
                return;
            }
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

#if DEBUG
            var netProps = GetMeta("NETWORK_PROPS", new Godot.Collections.Dictionary()).AsGodotDictionary();
            netProps[prop.NodePath + ":" + prop.Name] = value;
            SetMeta("NETWORK_PROPS", netProps);
#endif

            lerpableChangeQueue[prop.NodePath + ":" + prop.Name] = new LerpableChangeQueue
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
                if ((data.propertiesUpdated & (long)1 << i) == 0)
                {
                    continue;
                }
                var prop = NetworkScenesRegister.PROPERTY_LOOKUP[wrapper.Node.SceneFilePath][(byte)i];
                var varVal = HLBytes.UnpackVariant(buffer, knownType: prop.Type);
                data.properties[(byte)i] = varVal.Value;
            }
            return data;
        }

        public void Import(WorldRunner currentWorld, HLBuffer buffer, out NetworkNodeWrapper nodeOut)
        {
            nodeOut = wrapper;

            var data = Deserialize(buffer);
            foreach (var propIndex in data.properties.Keys)
            {
                var prop = NetworkScenesRegister.PROPERTY_LOOKUP[wrapper.Node.SceneFilePath][propIndex];
                if (wrapper.Node.IsNodeReady())
                {
                    ImportProperty(prop, currentWorld.CurrentTick, data.properties[propIndex]);
                }
                else
                {
                    cachedPropertyChanges[propIndex] = data.properties[propIndex];
                }
            }

            return;
        }

        private HashSet<byte> nonDefaultProperties = [];

        private Dictionary<NetPeer, Dictionary<Tick, long>> peerBufferCache = [];
        // This should instead be a map of variable values that we can resend until acknowledgement

        private long FilterPropsAgainstInterest(NetPeer peer, long prop)
        {
            var result = prop;
            var peerId = NetworkRunner.Instance.GetPeerId(peer);
            if (!wrapper.InterestLayers.ContainsKey(peerId) || wrapper.InterestLayers[peerId] == 0)
            {
                GD.Print("No interest layers found for " + peerId + " on " + wrapper.Node.Name);
                return 0;
            }
            for (var i = 0; i < MAX_NETWORK_PROPERTIES; i++)
            {
                if ((prop & (long)1 << i) == 0)
                {
                    continue;
                }

                var propNode = NetworkScenesRegister.PROPERTY_LOOKUP[wrapper.Node.SceneFilePath][(byte)i];
                if ((propNode.InterestMask & (long)wrapper.InterestLayers[peerId]) == 0)
                {
                    result &= ~(long)1 << i;
                    GD.Print("Filtering ", propNode.Name, " from ", wrapper.Node.Name, " for ", peerId);
                }
            }
            return result;
        }

        public HLBuffer Export(WorldRunner currentWorld, NetPeer peerId)
        {
            var buffer = new HLBuffer();
            if (!peerInitialPropSync.ContainsKey(peerId))
            {
                peerInitialPropSync[peerId] = 0;
            }
            if (!currentWorld.HasSpawnedForClient(wrapper.NetworkId, peerId))
            {
                // GD.Print("Client ", peerId, " has not spawned ", wrapper.Node.Name);
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
            }

            foreach (var propIndex in nonDefaultProperties)
            {
                var propSlot = (long)1 << propIndex;
                if ((peerInitialPropSync[peerId] & propSlot) == 0)
                {

                    propertiesUpdated |= propSlot;
                    peerInitialPropSync[peerId] |= propSlot;
                }
            }

            // Store them in the cache to resend in the future until the client acknowledges having received the update
            if (propertiesUpdated != 0)
            {
                peerBufferCache[peerId][currentWorld.CurrentTick] = propertiesUpdated;
            }

            propertiesUpdated = 0;

            // Now collect every pending property update
            foreach (var tick in peerBufferCache[peerId].Keys.OrderBy(x => x))
            {
                var filteredProps = FilterPropsAgainstInterest(peerId, peerBufferCache[peerId][tick]);
                propertiesUpdated |= filteredProps;

            }

            if (propertiesUpdated == 0)
            {
                return buffer;
            }

            // Indicate which variables have been updated
            HLBytes.Pack(buffer, propertiesUpdated);

            // Finally, pack the variable values
            for (var i = 0; i < MAX_NETWORK_PROPERTIES; i++)
            {
                if ((propertiesUpdated & (long)1 << i) == 0)
                {
                    continue;
                }

                var prop = NetworkScenesRegister.PROPERTY_LOOKUP[wrapper.Node.SceneFilePath][(byte)i];
                var propNode = wrapper.Node.GetNode(prop.NodePath);
                var varVal = propNode.Get(prop.Name);
                HLBytes.PackVariant(buffer, varVal, true);
            }

            return buffer;
        }

        public void Cleanup()
        {
            propertyUpdated.Clear();
        }

        public void Acknowledge(WorldRunner currentWorld, NetPeer peerId, Tick latestAck)
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
            if (NetworkRunner.Instance.IsServer)
            {
                return;
            }

            foreach (var queueKey in lerpableChangeQueue.Keys.ToList())
            {
                var toLerp = lerpableChangeQueue[queueKey];
                var lerpNode = wrapper.Node.GetNode(toLerp.Prop.NodePath);
                if (toLerp.Weight < 1.0)
                {
                    toLerp.Weight = Math.Min(toLerp.Weight + delta * 10, 1.0);
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
                            lerpableChangeQueue.Remove(queueKey);
                        }
                    }

                    if ((float)toLerp.Weight >= 1.0)
                        lerpableChangeQueue.Remove(queueKey);
                }
                lerpableChangeQueue[queueKey] = toLerp;
            }
        }
    }
}