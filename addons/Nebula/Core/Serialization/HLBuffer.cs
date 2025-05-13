using Godot;

namespace Nebula.Serialization
{
    /// <summary>
    /// Standard object used to package data that will be transferred across the network.
    /// Used extensively by <see cref="HLBytes"/>.
    /// </summary>
    /// <param name="bytes"></param>
    public partial class HLBuffer: RefCounted
    {
        public HLBuffer()
        {
            bytes = [];
        }

        public HLBuffer(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public byte[] bytes;
        public int pointer = 0;
        public bool IsPointerEnd => pointer >= bytes.Length;
        public const int CONSISTENCY_BUFFER_SIZE_LIMIT = 256;
        public byte[] RemainingBytes => bytes[pointer..];
    }
}