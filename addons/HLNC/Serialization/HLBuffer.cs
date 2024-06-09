using System;
using System.Collections.Generic;
using Godot;

namespace HLNC.Serialization
{
    public class HLBuffer(byte[] bytes = null)
    {
        public byte[] bytes = bytes ?? [];
        public int pointer = 0;
        public bool IsPointerEnd => pointer >= bytes.Length;
        public const int CONSISTENCY_BUFFER_SIZE_LIMIT = 256;
        public byte[] RemainingBytes => bytes[pointer..];
    }
}