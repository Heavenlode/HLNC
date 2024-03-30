using System;
using System.Collections.Generic;
using Godot;

namespace HLNC
{

	public struct ConsistencyBufferItem
	{
		public Tick tick;
		public Variant value;
	}
	
	public struct UnpackedVariable
	{
		public NetworkId netId;
		public int varId;
		public int flags;
		public Variant varVal;
	}


	public class HLBuffer
	{
		public byte[] bytes;
		public int pointer;
		public bool pointerReachedEnd => pointer >= bytes.Length;
		public const int CONSISTENCY_BUFFER_SIZE_LIMIT = 256;
		public byte[] RemainingBytes => bytes[pointer..];

		public HLBuffer(byte[] bytes = null)
		{
			this.bytes = bytes ?? new byte[0];
			pointer = 0;
		}
	}
}