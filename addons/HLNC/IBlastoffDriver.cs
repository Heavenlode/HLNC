using System;

namespace HLNC {

    // This interface provides the logic to handle Blastoff communication
    // https://github.com/Heavenlode/Blastoff
    public interface IBlastoffServerDriver {
        public bool BlastoffValidatePeer(Guid zoneId, string token);

    }

    public interface IBlastoffClientDriver {
        public string BlastoffGetToken();
        public Guid BlastoffGetZoneId();
    }
}