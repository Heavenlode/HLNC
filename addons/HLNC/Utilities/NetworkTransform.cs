using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace HLNC.Utilities
{
    public partial class NetworkTransform : NetworkNode3D
    {
        public bool teleporting = true;

        [Export]
        public Node3D TargetNode { get; set; }

        [NetworkProperty]
        public bool IsTeleporting { get; set; }

        [NetworkProperty]
        public Vector3 NetPosition { get; set; }

        [NetworkProperty]
        public Vector3 NetRotation { get; set; }

        private bool _isTeleporting = false;
        public void OnNetworkChangeIsTeleporting(Tick tick, bool from, bool to)
        {
            _isTeleporting = true;
        }

        public override void _Ready()
        {
            base._Ready();
            TargetNode ??= GetParent3D();
            NetPosition = TargetNode.GlobalPosition;
            NetRotation = TargetNode.GlobalRotation;
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

        bool teleportExported = false;

        public override void _NetworkProcess(int tick)
        {
            base._NetworkProcess(tick);
            if (!NetworkRunner.Instance.IsServer)
            {
                return;
            }
            NetPosition = TargetNode.GlobalPosition;
            NetRotation = TargetNode.GlobalRotation;
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
            TargetNode.GlobalPosition = incoming_position;
            IsTeleporting = true;
        }
    }
}