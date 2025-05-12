using System;
using Godot;
using HLNC.Utils;
using MongoDB.Bson;

namespace HLNC.Serialization
{
    public class BsonSerialize
        {
        public static BsonValue SerializeVariant(Variant context, Variant variant, string subtype = "None")
        {
            if (variant.VariantType == Variant.Type.String)
            {
                return variant.ToString();
            }
            else if (variant.VariantType == Variant.Type.Float)
            {
                return variant.AsDouble();
            }
            else if (variant.VariantType == Variant.Type.Int)
            {
                if (subtype == "Byte")
                {
                    return variant.AsByte();
                }
                else if (subtype == "Int")
                {
                    return variant.AsInt32();
                }
                else
                {
                    return variant.AsInt64();
                }
            }
            else if (variant.VariantType == Variant.Type.Bool)
            {
                return variant.AsBool();
            }
            else if (variant.VariantType == Variant.Type.Vector2)
            {
                var vec = variant.As<Vector2>();
                return new BsonArray { vec.X, vec.Y };
            }
            else if (variant.VariantType == Variant.Type.Vector3)
            {
                var vec = variant.As<Vector3>();
                return new BsonArray { vec.X, vec.Y, vec.Z };
            }
            else if (variant.VariantType == Variant.Type.Nil)
            {
                return BsonNull.Value;
            }
            else if (variant.VariantType == Variant.Type.Object)
            {
                var obj = variant.As<GodotObject>();
                if (obj == null)
                {
                    return BsonNull.Value;
                }
                else
                {
                    if (obj is IBsonSerializableBase bsonSerializable)
                    {
                        return bsonSerializable.BsonSerialize(context);
                    }

                    Debugger.Instance.Log($"Object does not implement IBsonSerializable<T>: {obj}", Debugger.DebugLevel.ERROR);
                    return null;
                }
            }
            else if (variant.VariantType == Variant.Type.PackedByteArray)
            {
                return new BsonBinaryData(variant.AsByteArray(), BsonBinarySubType.Binary);
            }
            else if (variant.VariantType == Variant.Type.Dictionary)
            {
                var dict = variant.AsGodotDictionary();
                var bsonDict = new BsonDocument();
                foreach (var key in dict.Keys)
                {
                    bsonDict[key.ToString()] = SerializeVariant(context, dict[key]);
                }
                return bsonDict;
            }
            else
            {
                Debugger.Instance.Log($"Serializing to JSON unsupported property type: {variant.VariantType}", Debugger.DebugLevel.ERROR);
                return null;
            }
        }
    }
}