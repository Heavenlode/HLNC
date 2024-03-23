using System.Diagnostics;
using Godot;

namespace HLNC
{
	public partial class NetworkTransform : NetworkNode3D
	{
		public bool teleporting = true;
		
		[Export]
		public Node3D TargetNode { get; set; }

		[NetworkProperty]
		public Vector3 NetPosition { get; set; }

		[NetworkProperty]
		public Vector3 NetRotation { get; set; }

		[Signal]
		public delegate void InterpolateNetPositionEventHandler(Vector3 next_value);

		// public int InterpolationDecider()
		// {
		// 	if (!teleporting)
		// 	{
		// 		return NetworkProperty.Flags.LinearInterpolation;
		// 	}
		// 	else
		// 	{
		// 		teleporting = false;
		// 		return 0;
		// 	}
		// }

		public override void _Ready()
		{
			base._Ready();
			if (TargetNode == null)
			{
				TargetNode = GetParent3D();
			}
			NetPosition = TargetNode.GlobalPosition;
			// net_pos_prop.InterpolationDecider = InterpolationDecider;
			// NetworkProperties.Add(net_pos_prop);
		}

		public Node3D GetParent3D()
		{
			var parent = GetParent();
			if (parent is Node3D)
			{
				return (Node3D)parent;
			}
			GD.PrintErr("NetworkTransform parent is not a Node3D");
			return null;
		}

		public void Face(Vector3 direction)
		{
			if (!NetworkRunner.Instance.IsServer)
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

		public override void _NetworkProcess(int tick)
		{
			base._NetworkProcess(tick);
			if (!NetworkRunner.Instance.IsServer)
			{
				return;
			}
			NetPosition = TargetNode.GlobalPosition;
			NetRotation = TargetNode.GlobalRotation;
		}

		public override void _PhysicsProcess(double delta)
		{
			base._PhysicsProcess(delta);
			if (NetworkRunner.Instance.IsServer)
			{
				return;
			}
			TargetNode.GlobalPosition = NetPosition;
			TargetNode.GlobalRotation = NetRotation;
		}

		public void Teleport(Vector3 incoming_position)
		{
			if (!NetworkRunner.Instance.IsServer)
			{
				teleporting = true;
				return;
			}
			var parent = GetParent3D();
			if (parent == null)
			{
				return;
			}
			parent.GlobalPosition = incoming_position;
		}
	}
}