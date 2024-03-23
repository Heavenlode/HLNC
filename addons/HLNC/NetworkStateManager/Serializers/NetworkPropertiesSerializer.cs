using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;

namespace HLNC.StateSerializers
{
	public class NetworkPropertiesSerializer : IStateSerailizer
	{
		private const int MAX_NETWORK_PROPERTIES = 64;
		private NetworkNode3D node;

		private struct CollectedNetworkProperty
		{
			public NetworkNode3D Node;
			public string Name;
			public Variant.Type Type;
		}

		private struct LerpableChangeQueue
		{
			public CollectedNetworkProperty Prop;
			public Variant From;
			public Variant To;
			public double Weight;
		}

		private Dictionary<string, LerpableChangeQueue> lerpableChangeQueue = new Dictionary<string, LerpableChangeQueue>();

		private List<CollectedNetworkProperty> networkProperties = new List<CollectedNetworkProperty>();
		private Dictionary<string, byte> propertyIndices = new Dictionary<string, byte>();
		private Dictionary<byte, bool> propertyUpdated = new Dictionary<byte, bool>();
		public NetworkPropertiesSerializer(NetworkNode3D node)
		{
			this.node = node;

			// First, determine if the Node class has the NetworkScene attribute
			if (!node.NetworkScene)
			{
				return;
			}

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
							networkProperties.Add(new CollectedNetworkProperty
							{
								Node = (NetworkNode3D)child,
								Name = property.Name,
								Type = type
							});
							propertyIndices[property.Name] = (byte)propertyId;
						}
					}

					if (NetworkRunner.Instance.IsServer)
					{
						((INotifyPropertyChanged)child).PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
						{
							if (propertyIndices.TryGetValue(e.PropertyName, out byte propIndex))
							{
								propertyUpdated[propIndex] = true;
							}
						};
					}
				}
			};
		}

		public void Import(IGlobalNetworkState networkState, HLBuffer buffer, out NetworkNode3D nodeOut)
		{
			nodeOut = node;

			var propertiesUpdated = HLBytes.UnpackInt64(buffer);
			for (var i = 0; i < MAX_NETWORK_PROPERTIES; i++)
			{
				if ((propertiesUpdated & ((long)1 << i)) == 0)
				{
					continue;
				}
				var prop = networkProperties[i];
				var varVal = HLBytes.UnpackVariant(buffer, prop.Type);
				Variant oldVal = prop.Node.Get(prop.Name);
				if (prop.Node.HasMethod("OnNetworkChange" + prop.Name))
				{
					prop.Node.Call("OnNetworkChange" + prop.Name, networkState.CurrentTick, oldVal, varVal.Value);
				}

				lerpableChangeQueue[prop.Name] = new LerpableChangeQueue
				{
					Prop = prop,
					From = oldVal,
					To = varVal.Value,
					Weight = 0.0f
				};
			}

			return;
		}

		private Dictionary<PeerId, Dictionary<Tick, long>> peerBufferCache = new Dictionary<PeerId, Dictionary<Tick, long>>();
		// This should instead be a map of variable values that we can resend until acknowledgement

		public HLBuffer Export(IGlobalNetworkState networkState, PeerId peerId)
		{
			var buffer = new HLBuffer();

			if (!peerBufferCache.ContainsKey(peerId))
			{
				peerBufferCache[peerId] = new Dictionary<Tick, long>();
			}

			long propertiesUpdated = 0;

			// First, we find any newly updated properties
			foreach (var propIndex in propertyUpdated.Keys)
			{
				var prop = networkProperties[propIndex];
				var varVal = prop.Node.Get(prop.Name);
				propertiesUpdated |= (long)1 << propIndex;
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

				var prop = networkProperties[i];
				var varVal = prop.Node.Get(prop.Name);
				HLBytes.Pack(buffer, varVal);
			}

			return buffer;
		}

		public void Acknowledge(IGlobalNetworkState networkState, PeerId peerId, Tick latestAck)
		{
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

			foreach (var propName in lerpableChangeQueue.Keys)
			{
				var toLerp = lerpableChangeQueue[propName];
				if (toLerp.Weight < 1.0)
				{
					toLerp.Weight = Math.Min(toLerp.Weight + (delta * 10), 1.0);
					if (toLerp.Prop.Type == Variant.Type.Quaternion)
					{
						var next_value = ((Quaternion)toLerp.From).Normalized().Slerp(((Quaternion)toLerp.To).Normalized(), (float)toLerp.Weight);
						toLerp.Prop.Node.Set(toLerp.Prop.Name, next_value);
					} else if (toLerp.Prop.Type == Variant.Type.Vector3)
					{
						var next_value = Lerp((Vector3)toLerp.From, (Vector3)toLerp.To, (float)toLerp.Weight);
						toLerp.Prop.Node.Set(toLerp.Prop.Name, next_value);
					} else {
						toLerp.Prop.Node.Set(toLerp.Prop.Name, toLerp.To);
						lerpableChangeQueue.Remove(propName);
					}

					if ((float)toLerp.Weight >= 1.0)
						lerpableChangeQueue.Remove(propName);
				}
			}
		}
	}
}