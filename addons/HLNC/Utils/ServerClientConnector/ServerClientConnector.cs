using System;
using Godot;

namespace HLNC.Utils
{
	public partial class ServerClientConnector : Node
	{
		public override void _Ready()
		{
			GD.Print("ServerClientConnector _Ready");
			if (Env.Instance.HasServerFeatures)
			{
				prepareServer();
			}
			else
			{
				prepareClient();
			}
		}

		private void prepareServer()
		{
			NetRunner.Instance.StartServer();
			if (Env.Instance.InitialWorldScene != null)
			{
				Debugger.Instance.Log("Loading initial world scene: " + Env.Instance.InitialWorldScene);
				Debugger.Instance.Log("No existing zone data found. Create fresh zone instance.");
				var InitialWorldScene = GD.Load<PackedScene>(Env.Instance.InitialWorldScene);
				NetRunner.Instance.CreateWorldPacked(Env.Instance.InitialWorldId, InitialWorldScene);
				Debugger.Instance.Log("Server ready");
			}
			else
			{
				throw new Exception("No initial world scene specified. Provide either a worldId or initialWorldScene in the start args.");
			}
		}

		private void prepareClient()
		{
			GD.Print("ServerClientConnector prepareClient");
			NetRunner.Instance.StartClient();
		}
	}
}
