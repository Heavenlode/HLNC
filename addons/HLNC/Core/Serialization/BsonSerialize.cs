using System;
using Godot;
using MongoDB.Bson;

namespace HLNC.Serialization
{
    public class BsonSerialize
    {
        public static BsonValue SerializeVariant(Variant context, Variant variant, VariantSubtype subtype = 0)
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
                switch (subtype)
                {
                    case VariantSubtype.Byte:
                        return variant.AsByte();
                    case VariantSubtype.Int:
                        return variant.AsInt32();
                    case VariantSubtype.NetworkId:
                        if (variant.AsInt64() != -1)
                        {
                            // Only record network ids that are not -1.
                            return variant.AsInt64();
                        }
                        else
                        {
                            return null;
                        }
                    default:
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
                    // Ensure obj implements IBsonSerializable.
                    if (!(obj is IBsonSerializable))
                    {
                        GD.PrintErr("Object does not implement IBsonSerializable: ", obj);
                        return null;
                    }
                    return (obj as IBsonSerializable).BsonSerialize(context);
                }
            }
            else if (variant.VariantType == Variant.Type.PackedByteArray)
            {
                if (subtype == VariantSubtype.Guid)
                {
                    return new BsonBinaryData(new Guid(variant.AsByteArray()), GuidRepresentation.Standard);
                }
                else
                {
                    return new BsonBinaryData(variant.AsByteArray(), BsonBinarySubType.Binary);
                }
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
                GD.PrintErr("Serializing to JSON unsupported property type: ", variant.VariantType);
                return null;
            }
        }
    }
}