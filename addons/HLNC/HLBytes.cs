using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace HLNC
{
	public class HLBytes
	{
		public static byte[] Compress(byte[] data)
		{
			using (var compressedStream = new MemoryStream())
			{
				using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
				{
					zipStream.Write(data, 0, data.Length);
				}
				return compressedStream.ToArray();
			}
		}

		public static byte[] Decompress(byte[] data)
		{
			using (var compressedStream = new MemoryStream(data))
			using (var uncompressedStream = new MemoryStream())
			{
				using (var gZipDecompressor = new GZipStream(compressedStream, CompressionMode.Decompress))
				{
					gZipDecompressor.CopyTo(uncompressedStream);
				}
				return uncompressedStream.ToArray();
			}
		}

		public static void AddRange(ref byte[] array, byte[] toAdd)
		{
			Array.Resize(ref array, array.Length + toAdd.Length);
			Array.Copy(toAdd, 0, array, array.Length - toAdd.Length, toAdd.Length);
		}
		public static void Pack(HLBuffer buffer, Variant varVal, bool pack_type = false)
		{
			if (varVal.VariantType == Variant.Type.Vector3)
			{
				Pack(buffer, (Vector3)varVal, pack_type);
			}
			else if (varVal.VariantType == Variant.Type.Vector2)
			{
				Pack(buffer, (Vector2)varVal, pack_type);
			}
			else if (varVal.VariantType == Variant.Type.Quaternion)
			{
				Pack(buffer, (Quaternion)varVal, pack_type);
			}
			else if (varVal.VariantType == Variant.Type.Float)
			{
				Pack(buffer, (float)varVal, pack_type);
			}
			else if (varVal.VariantType == Variant.Type.Int)
			{
				Pack(buffer, (long)varVal, pack_type);
			}
			else if (varVal.VariantType == Variant.Type.Bool)
			{
				Pack(buffer, (bool)varVal, pack_type);
			}
			else if (varVal.VariantType == Variant.Type.Array)
			{
				Pack(buffer, (Godot.Collections.Array)varVal, pack_type);
			}
			else
			{
				GD.Print("HLBytes.Pack: Unhandled type: " + varVal.VariantType);
			}
		}

		public static void Pack(HLBuffer buffer, List<ConsistencyBufferItem> consistencyBuffer)
		{
			if (consistencyBuffer.Count > HLBuffer.CONSISTENCY_BUFFER_SIZE_LIMIT)
			{
				// if (!buffer.warned)
				// {
				// 	GD.Print("HLBytes.Pack: Consistency buffer size limit reached, discarding oldest values");
				// 	buffer.warned = true;
				// }
				consistencyBuffer.RemoveRange(0, consistencyBuffer.Count - HLBuffer.CONSISTENCY_BUFFER_SIZE_LIMIT);
			}
			Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
			buffer.bytes[buffer.pointer] = (byte)consistencyBuffer.Count;
			buffer.pointer += 1;
			foreach (var item in consistencyBuffer)
			{
				Array.Resize(ref buffer.bytes, buffer.bytes.Length + 4);
				byte[] tickBytes = BitConverter.GetBytes(item.tick);
				Array.Copy(tickBytes, 0, buffer.bytes, buffer.pointer, 4);
				buffer.pointer += 4;
				Pack(buffer, item.value);
			}
		}

		public static void Pack(HLBuffer buffer, Vector3 varVal, bool pack_type = false)
		{
			if (pack_type)
			{
				Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
				buffer.bytes[buffer.pointer] = (byte)Variant.Type.Vector3;
				buffer.pointer += 1;
			}

			Array.Resize(ref buffer.bytes, buffer.bytes.Length + 6);
			byte[] floatBytes = BitConverter.GetBytes((Half)varVal.X);
			Array.Copy(floatBytes, 0, buffer.bytes, buffer.pointer, 2);
			buffer.pointer += 2;
			floatBytes = BitConverter.GetBytes((Half)varVal.Y);
			Array.Copy(floatBytes, 0, buffer.bytes, buffer.pointer, 2);
			buffer.pointer += 2;
			floatBytes = BitConverter.GetBytes((Half)varVal.Z);
			Array.Copy(floatBytes, 0, buffer.bytes, buffer.pointer, 2);
			buffer.pointer += 2;
		}

		public static void Pack(HLBuffer buffer, Vector2 varVal, bool pack_type = false)
		{
			if (pack_type)
			{
				Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
				buffer.bytes[buffer.pointer] = (byte)Variant.Type.Vector2;
				buffer.pointer += 1;
			}

			Array.Resize(ref buffer.bytes, buffer.bytes.Length + 4);
			byte[] floatBytes = BitConverter.GetBytes((Half)varVal.X);
			Array.Copy(floatBytes, 0, buffer.bytes, buffer.pointer, 2);
			buffer.pointer += 2;
			floatBytes = BitConverter.GetBytes((Half)varVal.Y);
			Array.Copy(floatBytes, 0, buffer.bytes, buffer.pointer, 2);
			buffer.pointer += 2;
		}

		public static void Pack(HLBuffer buffer, Quaternion varVal, bool pack_type = false)
		{
			if (pack_type)
			{
				Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
				buffer.bytes[buffer.pointer] = (byte)Variant.Type.Quaternion;
				buffer.pointer += 1;
			}

			Array.Resize(ref buffer.bytes, buffer.bytes.Length + 8);
			byte[] floatBytes = BitConverter.GetBytes((Half)varVal.X);
			Array.Copy(floatBytes, 0, buffer.bytes, buffer.pointer, 2);
			buffer.pointer += 2;
			floatBytes = BitConverter.GetBytes((Half)varVal.Y);
			Array.Copy(floatBytes, 0, buffer.bytes, buffer.pointer, 2);
			buffer.pointer += 2;
			floatBytes = BitConverter.GetBytes((Half)varVal.Z);
			Array.Copy(floatBytes, 0, buffer.bytes, buffer.pointer, 2);
			buffer.pointer += 2;
			floatBytes = BitConverter.GetBytes((Half)varVal.W);
			Array.Copy(floatBytes, 0, buffer.bytes, buffer.pointer, 2);
			buffer.pointer += 2;
		}

		public static void Pack(HLBuffer buffer, float varVal, bool pack_type = false)
		{
			if (pack_type)
			{
				Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
				buffer.bytes[buffer.pointer] = (byte)Variant.Type.Float;
				buffer.pointer += 1;
			}

			Array.Resize(ref buffer.bytes, buffer.bytes.Length + 4);
			byte[] floatBytes = BitConverter.GetBytes(varVal);
			Array.Copy(floatBytes, 0, buffer.bytes, buffer.pointer, 4);
			buffer.pointer += 4;
		}

		public static void Pack(HLBuffer buffer, byte varVal)
		{
			Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
			buffer.bytes[buffer.pointer] = varVal;
			buffer.pointer += 1;
		}

		public static void Pack(HLBuffer buffer, long varVal, bool pack_type = false)
		{
			if (pack_type)
			{
				Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
				buffer.bytes[buffer.pointer] = (byte)Variant.Type.Int;
				buffer.pointer += 1;
			}

			Array.Resize(ref buffer.bytes, buffer.bytes.Length + 8);
			byte[] floatBytes = BitConverter.GetBytes(varVal);
			Array.Copy(floatBytes, 0, buffer.bytes, buffer.pointer, 8);
			buffer.pointer += 8;
		}

		public static void Pack(HLBuffer buffer, bool varVal, bool pack_type = false)
		{
			if (pack_type)
			{
				Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
				buffer.bytes[buffer.pointer] = (byte)Variant.Type.Bool;
				buffer.pointer += 1;
			}

			Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
			buffer.bytes[buffer.pointer] = (byte)(varVal ? 1 : 0);
			buffer.pointer += 1;
		}

		public static void Pack(HLBuffer buffer, Godot.Collections.Array varVal, bool pack_type = false)
		{
			if (pack_type)
			{
				Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
				buffer.bytes[buffer.pointer] = (byte)Variant.Type.Array;
				buffer.pointer += 1;
			}

			Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
			buffer.bytes[buffer.pointer] = (byte)varVal.Count;
			buffer.pointer += 1;
			foreach (var val in varVal)
			{
				if (val.VariantType == Variant.Type.Vector3)
				{
					Pack(buffer, (Vector3)val, pack_type);
				}
				else if (val.VariantType == Variant.Type.Vector2)
				{
					Pack(buffer, (Vector2)val, pack_type);
				}
				else if (val.VariantType == Variant.Type.Quaternion)
				{
					Pack(buffer, (Quaternion)val, pack_type);
				}
				else if (val.VariantType == Variant.Type.Float)
				{
					Pack(buffer, (float)val, pack_type);
				}
				else if (val.VariantType == Variant.Type.Int)
				{
					Pack(buffer, (long)val, pack_type);
				}
				else if (val.VariantType == Variant.Type.Bool)
				{
					Pack(buffer, (bool)val, pack_type);
				}
				else
				{
					GD.Print("HLBytes.Pack: Unhandled type: " + val.VariantType);
				}
			}
		}

		public static void Pack(HLBuffer buffer, int[] varVal, bool pack_type = false)
		{
			if (pack_type)
			{
				Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
				buffer.bytes[buffer.pointer] = (byte)Variant.Type.PackedInt32Array;
				buffer.pointer += 1;
			}
			Array.Resize(ref buffer.bytes, buffer.bytes.Length + 4);
			byte[] bytes = BitConverter.GetBytes(varVal.Length);
			Array.Copy(bytes, 0, buffer.bytes, buffer.pointer, 4);
			buffer.pointer += 4;
			foreach (var val in varVal)
			{
				Array.Resize(ref buffer.bytes, buffer.bytes.Length + 4);
				bytes = BitConverter.GetBytes(val);
				Array.Copy(bytes, 0, buffer.bytes, buffer.pointer, 4);
				buffer.pointer += 4;
			}
		}


		public static void Pack(HLBuffer buffer, byte[] varVal, bool pack_type = false)
		{
			if (pack_type)
			{
				Array.Resize(ref buffer.bytes, buffer.bytes.Length + 1);
				buffer.bytes[buffer.pointer] = (byte)Variant.Type.PackedInt32Array;
				buffer.pointer += 1;
			}
			Array.Resize(ref buffer.bytes, buffer.bytes.Length + varVal.Length);
			Array.Copy(varVal, 0, buffer.bytes, buffer.pointer, varVal.Length);
			buffer.pointer += varVal.Length;
		}

		public static Variant? UnpackVariant(HLBuffer buffer, Variant.Type? knownType = null)
		{
			Variant.Type type;
			if (knownType.HasValue)
			{
				type = knownType.Value;
			}
			else
			{
				type = (Variant.Type)buffer.bytes[buffer.pointer];
				buffer.pointer += 1;
			}
			if (type == Variant.Type.Vector3)
			{
				return UnpackVector3(buffer);
			}
			else if (type == Variant.Type.Vector2)
			{
				return UnpackVector2(buffer);
			}
			else if (type == Variant.Type.Quaternion)
			{
				return UnpackQuaternion(buffer);
			}
			else if (type == Variant.Type.Float)
			{
				return UnpackFloat(buffer);
			}
			else if (type == Variant.Type.Int)
			{
				return UnpackInt32(buffer);
			}
			else if (type == Variant.Type.Bool)
			{
				return UnpackBool(buffer);
			}
			else if (type == Variant.Type.Array)
			{
				return UnpackArray(buffer);
			}
			else if (type == Variant.Type.PackedByteArray)
			{
				return UnpackByteArray(buffer);
			}
			else
			{
				GD.Print("HLBytes.UnpackVariant: Unhandled type: " + type);
				return null;
			}
		}

		public static Vector2 UnpackVector2(HLBuffer buffer)
		{
			var result = new Vector2();
			var bytes = new byte[4];
			Array.Copy(buffer.bytes, buffer.pointer, bytes, 0, 4);
			result.X = (float)BitConverter.ToHalf(bytes, 0);
			buffer.pointer += 2;
			result.Y = (float)BitConverter.ToHalf(bytes, 2);
			buffer.pointer += 2;
			return result;
		}

		public static Vector3 UnpackVector3(HLBuffer buffer)
		{
			var result = new Vector3();
			var bytes = new byte[6];
			Array.Copy(buffer.bytes, buffer.pointer, bytes, 0, 6);
			result.X = (float)BitConverter.ToHalf(bytes, 0);
			buffer.pointer += 2;
			result.Y = (float)BitConverter.ToHalf(bytes, 2);
			buffer.pointer += 2;
			result.Z = (float)BitConverter.ToHalf(bytes, 4);
			buffer.pointer += 2;
			return result;
		}

		public static Quaternion UnpackQuaternion(HLBuffer buffer)
		{
			var result = new Quaternion();
			var bytes = new byte[8];
			Array.Copy(buffer.bytes, buffer.pointer, bytes, 0, 8);
			result.X = (float)BitConverter.ToHalf(bytes, 0);
			buffer.pointer += 2;
			result.Y = (float)BitConverter.ToHalf(bytes, 2);
			buffer.pointer += 2;
			result.Y = (float)BitConverter.ToHalf(bytes, 4);
			buffer.pointer += 2;
			result.Y = (float)BitConverter.ToHalf(bytes, 6);
			buffer.pointer += 2;
			return result;
		}

		public static float UnpackFloat(HLBuffer buffer)
		{
			var bytes = new byte[4];
			Array.Copy(buffer.bytes, buffer.pointer, bytes, 0, 4);
			buffer.pointer += 4;
			return BitConverter.ToSingle(bytes, 0);
		}
		public static byte UnpackInt8(HLBuffer buffer)
		{
			byte int8 = buffer.bytes[buffer.pointer];
			buffer.pointer += 1;
			return int8;
		}
		public static int UnpackInt32(HLBuffer buffer)
		{
			var bytes = new byte[4];
			Array.Copy(buffer.bytes, buffer.pointer, bytes, 0, 4);
			buffer.pointer += 4;
			return BitConverter.ToInt32(bytes, 0);
		}

		public static long UnpackInt64(HLBuffer buffer)
		{
			var bytes = new byte[8];
			Array.Copy(buffer.bytes, buffer.pointer, bytes, 0, 8);
			buffer.pointer += 8;
			var result = BitConverter.ToInt64(bytes, 0);
			return result;
		}

		public static bool UnpackBool(HLBuffer buffer)
		{
			var result = buffer.bytes[buffer.pointer] == 1;
			buffer.pointer += 1;
			return result;
		}

		public static Godot.Collections.Array UnpackArray(HLBuffer buffer)
		{
			var size = buffer.bytes[buffer.pointer];
			buffer.pointer += 1;
			var result = new Godot.Collections.Array();
			if (size == 0)
			{
				return result;
			}
			for (int i = 0; i < size; i++)
			{
				var variant = UnpackVariant(buffer);
				if (variant.HasValue)
				{
					result.Add(variant.Value);
				}
				else
				{
					GD.Print("Failed to unpack array element " + i);
				}
			}
			return result;
		}

		public static int[] UnpackInt32Array(HLBuffer buffer)
		{
			var size = BitConverter.ToInt32(buffer.bytes, buffer.pointer);
			buffer.pointer += 4;
			var result = new int[size];
			for (int i = 0; i < size; i++)
			{
				result[i] = BitConverter.ToInt32(buffer.bytes, buffer.pointer);
				buffer.pointer += 4;
			}
			return result;
		}

		public static byte[] UnpackByteArray(HLBuffer buffer)
		{
			var size = BitConverter.ToInt32(buffer.bytes, buffer.pointer);
			buffer.pointer += 4;
			var result = new byte[size];
			Array.Copy(buffer.bytes, buffer.pointer, result, 0, size);
			buffer.pointer += size;
			return result;
		}
	}
}