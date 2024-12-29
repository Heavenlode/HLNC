using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using HLNC.Addons.Lazy;
using HLNC.Serialization;
using HLNC.Serialization.Serializers;
using HLNC.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace HLNC
{
	/**
		<summary>
		<see cref="Node3D">Node3D</see>, extended with HLNC networking capabilities. This is the most basic networked 3D object.
		On every network tick, all NetworkNode3D nodes in the scene tree automatically have their <see cref="NetworkProperty">network properties</see> updated with the latest data from the server.
		Then, the special <see cref="_NetworkProcess(int)">NetworkProcess</see> method is called, which indicates that a network Tick has occurred.
		Network properties can only update on the server side.
		For a client to update network properties, they must send client inputs to the server via implementing the <see cref="INetworkInputHandler"/> interface, or network function calls via <see cref="NetworkFunction"/> attributes.
		The server receives client inputs, can access them via <see cref="GetInput"/>, and handle them accordingly within <see cref="_NetworkProcess(int)">NetworkProcess</see> to mutate state.
		</summary>
	*/
	public partial class NetworkNode3D : Node3D, INetworkNode, INotifyPropertyChanged, INetworkSerializable<NetworkNode3D>, IBsonSerializable
	{
		public NetworkController Network { get; internal set; }
		public NetworkNode3D() {
			Network = new NetworkController(this);
		}
		// Cannot have more than 8 serializers
		public IStateSerializer[] Serializers { get; private set; } = [];

		public void SetupSerializers()
		{
			var spawnSerializer = new SpawnSerializer();
			AddChild(spawnSerializer);
			var propertySerializer = new NetworkPropertiesSerializer();
			AddChild(propertySerializer);
			Serializers = [spawnSerializer, propertySerializer];
		}

		public virtual void _WorldReady() {}
		public virtual void _NetworkProcess(int _tick) {}

		/// <inheritdoc/>
		public override void _PhysicsProcess(double delta) {}
		public static HLBuffer NetworkSerialize(WorldRunner currentWorld, NetPeer peer, NetworkNode3D obj)
		{
			var buffer = new HLBuffer();
			if (obj == null)
			{
				HLBytes.Pack(buffer, (byte)0);
				return buffer;
			}
			NetworkId targetNetId;
			byte staticChildId = 0;
			if (obj.Network.IsNetworkScene)
			{
				targetNetId = obj.Network.NetworkId;
			}
			else
			{
				if (NetworkScenesRegister.PackNode(obj.Network.NetworkParent.Node.SceneFilePath, obj.Network.NetworkParent.Node.GetPathTo(obj), out staticChildId))
				{
					targetNetId = obj.Network.NetworkParent.NetworkId;
				}
				else
				{
					throw new Exception($"Failed to pack node: {obj.GetPath()}");
				}
			}
			var peerNodeId = currentWorld.GetPeerWorldState(peer).Value.WorldToPeerNodeMap[targetNetId];
			HLBytes.Pack(buffer, peerNodeId);
			HLBytes.Pack(buffer, staticChildId);
			return buffer;
		}
		public static NetworkNode3D NetworkDeserialize(WorldRunner currentWorld, HLBuffer buffer, NetworkNode3D initialObject)
		{
			var networkID = HLBytes.UnpackByte(buffer);
			if (networkID == 0)
			{
				return null;
			}
			var staticChildId = HLBytes.UnpackByte(buffer);
			var node = currentWorld.GetNodeFromNetworkId(networkID).Node as NetworkNode3D;
			if (staticChildId > 0)
			{
				node = node.GetNodeOrNull(NetworkScenesRegister.UnpackNode(node.SceneFilePath, staticChildId)) as NetworkNode3D;
			}
			return node;
		}

		public BsonValue BsonSerialize(Variant context)
		{
			var doc = new BsonDocument();
			if (Network.IsNetworkScene)
			{
				doc["NetworkId"] = Network.NetworkId;
			}
			else
			{
				doc["NetworkId"] = Network.NetworkParent.NetworkId;
				doc["StaticChildPath"] = Network.NetworkParent.Node.GetPathTo(this).ToString();
			}
			return doc;
		}

		public static GodotObject BsonDeserialize(Variant context, BsonValue data, GodotObject instance)
		{
			if (data.IsBsonNull) return null;
			var doc = data.AsBsonDocument;
			var node = new NetworkNode3D();
			node.Network._prepareNetworkId = doc["NetworkId"].AsInt64;
			if (doc.Contains("StaticChildPath"))
			{
				node.Network._prepareStaticChildPath = doc["StaticChildPath"].AsString;
			}
			return node;
		}

		public string NodePathFromNetworkScene()
		{
			if (Network.IsNetworkScene)
			{
				return GetPathTo(this);
			}

			return Network.NetworkParent.Node.GetPathTo(this);
		}
	}
}
