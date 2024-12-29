using System;
using Godot;

namespace HLNC.Utils
{
	public partial class ServerClientConnector : Node
	{
		public override void _Ready()
		{
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
			NetworkRunner.Instance.StartServer();
			if (Env.Instance.InitialWorldScene != null)
			{
				Debugger.Log("Loading initial world scene: " + Env.Instance.InitialWorldScene);
				Debugger.Log("No existing zone data found. Create fresh zone instance.");
				var InitialWorldScene = GD.Load<PackedScene>(Env.Instance.InitialWorldScene);
				NetworkRunner.Instance.CreateWorldPacked(Env.Instance.InitialWorldId, InitialWorldScene);
				Debugger.Log("Server ready");
			}
			else
			{
				throw new Exception("No initial world scene specified. Provide either a worldId or initialWorldScene in the start args.");
			}
		}

		private void prepareClient()
		{
			NetworkRunner.Instance.StartClient();
		}
	}
}
