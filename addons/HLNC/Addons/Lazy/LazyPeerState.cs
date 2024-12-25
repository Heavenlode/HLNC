using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using HLNC.Serialization;
using MongoDB.Bson;

namespace HLNC.Addons.Lazy
{

    internal partial class LazyPeerStateContext : RefCounted {
        public Variant ContextId;
    }

    public partial class LazyPeerState : RefCounted, INetworkSerializable<LazyPeerState>, IBsonSerializable
    {
        private Godot.Collections.Dictionary<Variant, Variant> _values = new Godot.Collections.Dictionary<Variant, Variant>();
        private Variant _peerValue;
        private Variant _defaultValue;
        public event PropertyChangedEventHandler PropertyChanged;
        public Variant GetValue(Variant peerId)
        {
            if (NetworkRunner.Instance.IsClient)
            {
                throw new InvalidOperationException("GetValue has no peer argument when called on the client.");
            }

            if (_values.ContainsKey(peerId))
            {
                return _values[peerId];
            }
            else
            {
                return _defaultValue;
            }
        }
        public Variant GetValue()
        {
            if (NetworkRunner.Instance.IsServer)
            {
                throw new InvalidOperationException("GetValue must be provided with a peer when called on the server.");
            }
            return _peerValue;
        }
        public bool SetValue(Variant peerId, Variant value)
        {
            if (value.VariantType != _defaultValue.VariantType)
            {
                throw new InvalidOperationException("Value must be of the same type as the default value.");
            }
            var isChanged = !_values.GetValueOrDefault(peerId, _defaultValue).Equals(value);
            if (isChanged)
            {
                _values[peerId] = value;
                PropertyChanged.Invoke(this, null);
            }
            return isChanged;
        }

        public LazyPeerState(Variant defaultValue)
        {
            _defaultValue = defaultValue;
            _peerValue = defaultValue;
        }

        public static HLBuffer NetworkSerialize(WorldRunner currentWorld, NetPeer peer, LazyPeerState obj)
        {
            var buffer = new HLBuffer();
            HLBytes.PackVariant(buffer, obj._values.GetValueOrDefault(currentWorld.GetPeerWorldState(peer).Value.Id, obj._defaultValue));
            return buffer;
        }
        public static LazyPeerState NetworkDeserialize(WorldRunner currentWorld, HLBuffer buffer, LazyPeerState initialObject)
        {
            var result = new LazyPeerState(initialObject._defaultValue);
            var unpacked = HLBytes.UnpackVariant(buffer, knownType: initialObject._defaultValue.VariantType);
            if (!unpacked.HasValue) {
                throw new InvalidOperationException("Failed to unpack Lazy.");
            }
            result._peerValue = unpacked.Value;
            return result;
        }

        public BsonValue BsonSerialize(Variant context)
        {
            var ContextId = context.As<LazyPeerStateContext>().ContextId;
            if (_values.ContainsKey(ContextId)) {
                return Serialization.BsonSerialize.SerializeVariant(context, _values[ContextId]);
            }
            return Serialization.BsonSerialize.SerializeVariant(context, _defaultValue);
        }

        public static GodotObject BsonDeserialize(Variant context, BsonValue data, GodotObject instance)
        {
            var result = instance == null ? new LazyPeerState(0) : (LazyPeerState)instance;
            var ContextId = context.As<LazyPeerStateContext>().ContextId;
            // if (data.IsBsonNull) {
            //     result._values.Remove(ContextId);
            //     return result;
            // }
            result.SetValue(ContextId, data.AsInt64);
            if (instance == null) {
                return result;
            } else {
                return null;
            }
        }
    }
}