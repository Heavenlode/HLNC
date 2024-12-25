using System.Threading.Tasks;

namespace HLNC.Addons.Lazy
{
    public interface ILazyPeerStatesLoader
    {
        public Task<byte[]> LoadPeerValues(NetPeer peer);
    }
}