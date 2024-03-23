using Godot;

namespace HLNC
{
	public partial class NetworkDebug : Node
	{
		public enum Message
		{
			BYTES_PER_SECOND,
			PING
		}

		private PacketPeerStream monitorStream;
		private StreamPeerTcp sock;

		public void ConnectToMonitor()
		{
			// Connect to a TCP Server
			sock = new StreamPeerTcp();
			var err = sock.ConnectToHost("127.0.0.1", 8887);
			if (err != Error.Ok)
			{
				GD.Print("Error connecting to monitor log:", err);
				return;
			}
            monitorStream = new PacketPeerStream
            {
                StreamPeer = sock
            };
        }

		public void Log(Variant data)
		{
			if (sock.GetStatus() != StreamPeerTcp.Status.Connected)
			{
				return;
			}
			monitorStream.PutVar(data);
		}

		public override void _Process(double delta)
		{
			if (sock == null)
			{
				return;
			}
			sock.Poll();
		}
	}
}