using System;

namespace HLNC {

    /// <summary>
    /// Provides server logic for Blastoff communication.
    /// <see cref="NetworkRunner">NetworkRunner</see> calls these functions to handle Blastoff communications.
    /// For more info on Blastoff, visit the repo: https://github.com/Heavenlode/Blastoff
    /// </summary>
    public interface IBlastoffServerDriver {

        /// <summary>
        /// Validate whether the user is allowed to join the desired zone (i.e. server instance) or not.
        /// </summary>
        /// <param name="token">The token sent from the client. This may be a JWT, HMAC, or anything else desired for authentication.</param>
        /// <param name="redirect">An optional output parameter to ask Blastoff connect the client to. Only used if the user is not valid and false is returned.</param>
        /// <returns>Return true to allow the user to join. Return false to reject.</returns>
        public bool BlastoffValidatePeer(string token, out Guid redirect);

    }

    /// <summary>
    /// Provides client logic for Blastoff communication.
    /// <see cref="NetworkRunner">NetworkRunner</see> calls these functions to handle Blastoff communications.
    /// For more info on Blastoff, visit the repo:
    /// </summary>
    public interface IBlastoffClientDriver {

        /// <summary>
        /// NetworkRunner uses this to request the user's authentication token to send along to the server.
        /// </summary>
        /// <returns>The user's authentication token which is validated in <see cref="IBlastoffServerDriver.BlastoffValidatePeer(Guid, string, out Guid)"/></returns>
        public string BlastoffGetToken();
    }
}