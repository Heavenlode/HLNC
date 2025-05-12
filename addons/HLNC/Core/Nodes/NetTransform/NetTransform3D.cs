using Godot;
using HLNC.Utility.Tools;

namespace HLNC.Utility.Nodes
{
	[GlobalClass]
	public partial class NetTransform3D : NetNode3D
	{
		[Export]
		public Node3D SourceNode { get; set; }

		[Export]
		public Node3D TargetNode { get; set; }

		[NetProperty]
		public bool IsTeleporting { get; set; }

		[NetProperty]
		public Vector3 NetPosition { get; set; }

		[NetProperty]
		public Vector3 NetRotation { get; set; }

		private bool _isTeleporting = false;
		public void OnNetworkChangeIsTeleporting(Tick tick, bool from, bool to)
		{
			_isTeleporting = true;
		}

		/// <inheritdoc/>
		public override void _WorldReady()
		{
			base._WorldReady();
			TargetNode ??= GetParent3D();
			SourceNode ??= GetParent3D();
			if (GetMeta("import_from_external", false).AsBool())
			{
				SourceNode.Position = NetPosition;
				SourceNode.Rotation = NetRotation;
				TargetNode.Position = NetPosition;
				TargetNode.Rotation = NetRotation;
			}
		}

		public Node3D GetParent3D()
		{
			var parent = GetParent();
			if (parent is Node3D)
			{
				return (Node3D)parent;
			}
			Debugger.Instance.Log("NetTransform parent is not a Node3D", Debugger.DebugLevel.ERROR);
			return null;
		}

		public void Face(Vector3 direction)
		{
			if (NetRunner.Instance.IsClient)
			{
				return;
			}
			var parent = GetParent3D();
			if (parent == null)
			{
				return;
			}
			parent.LookAt(direction, Vector3.Up, true);
		}

		bool teleportExported = false;

		/// <inheritdoc/>
		public override void _NetworkProcess(int tick)
		{
			base._NetworkProcess(tick);
			if (NetRunner.Instance.IsClient)
			{
				return;
			}
			NetPosition = SourceNode.Position;
			NetRotation = SourceNode.Rotation;
			if (IsTeleporting)
			{
				if (teleportExported)
				{
					IsTeleporting = false;
					teleportExported = false;
				}
				else
				{
					teleportExported = true;
				}
			}
		}

		public double NetworkLerpNetPosition(Variant from, Variant to, double weight)
		{
			if (_isTeleporting)
			{
				NetPosition = (Vector3)to;
				_isTeleporting = false;
				return 1;
			}

			return -1;
		}

		public double NetworkLerpNetRotation(Variant from, Variant to, double weight)
		{
			Vector3 start = from.AsVector3();
			Vector3 end = to.AsVector3();
			NetRotation = new Vector3((float)Mathf.LerpAngle(start.X, end.X, weight), (float)Mathf.LerpAngle(start.Y, end.Y, weight), (float)Mathf.LerpAngle(start.Z, end.Z, weight));
			return weight;
		}

		/// <inheritdoc/>
		public override void _PhysicsProcess(double delta)
		{
			if (Engine.IsEditorHint())
			{
				return;
			}
			base._PhysicsProcess(delta);
			if (NetRunner.Instance.IsServer)
			{
				return;
			}
			if (!Network.IsWorldReady) return;
			TargetNode.Position = NetPosition;
			TargetNode.Rotation = NetRotation;
		}

		public void Teleport(Vector3 incoming_position)
		{
			TargetNode.Position = incoming_position;
			IsTeleporting = true;
		}
	}
}
