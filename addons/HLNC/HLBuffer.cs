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


    public class HLBuffer(byte[] bytes = null)
    {
        public byte[] bytes = bytes ?? [];
        public int pointer = 0;
        public bool IsPointerEnd => pointer >= bytes.Length;
        public const int CONSISTENCY_BUFFER_SIZE_LIMIT = 256;
        public byte[] RemainingBytes => bytes[pointer..];
    }
}