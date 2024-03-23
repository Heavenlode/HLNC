using Godot;

namespace HLNC
{
	public partial class NetworkAnimationPlayer : NetworkNode3D
	{
		[Export]
		public AnimationPlayer animation_player;

		private string[] animations;

		[NetworkProperty]
		public int active_animation {get; set;} = -1;

		[NetworkProperty]
		public float animation_position {get; set;} = 0.0f;

		public override void _Ready()
		{
			base._Ready();
			animations = animation_player.GetAnimationList();
		}

		public void OnNetworkChangeActiveAnimation(int tick, int old_val, int new_val)
		{
			if (new_val == -1)
			{
				animation_player.Stop();
			}
			else
			{
				animation_player.Play(animations[new_val]);
			}
		}

		public void OnNetworkChangeAnimationPosition(int tick, float old_val, float new_val)
		{
			if (active_animation == -1)
			{
				return;
			}
			GD.Print("Seeking to " + new_val);
			animation_player.Seek(new_val, true, true);
		}

		public void Play(string animation)
		{
			if (!NetworkRunner.Instance.IsServer)
			{
				return;
			}
			int animation_index = System.Array.IndexOf(animations, animation);
			if (animation_index == -1)
			{
				GD.Print("Animation not found: " + animation);
				return;
			}
			active_animation = animation_index;
			animation_player.Play(animations[animation_index]);
		}

		public override void _NetworkProcess(int tick)
		{
			base._NetworkProcess(tick);
			if (!NetworkRunner.Instance.IsServer)
			{
				return;
			}
			if (animation_player.IsPlaying())
			{
				animation_position = (float)animation_player.CurrentAnimationPosition;
			}
			else
			{
				animation_position = 0.0f;
			}
		}
	}
}