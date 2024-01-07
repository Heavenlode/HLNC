class_name NetworkAnimationPlayer extends NetworkNode3D

@export var animation_player: AnimationPlayer
@onready var animations: PackedStringArray = animation_player.get_animation_list()

var active_animation: int = -1
var animation_position: float = 0.0

func _on_network_change_active_animation(tick, old_val, new_val):
	if new_val == -1:
		animation_player.stop()
	else:
		animation_player.play(animations[new_val])

func _on_network_change_animation_position(tick, old_val, new_val):
	if active_animation == -1:
		return
	print("Seeking to ", new_val)
	animation_player.seek(new_val, true, true)

func play(animation: String):
	if not NetworkRunner.is_server:
		return
	var animation_index = animations.find(animation)
	if animation_index == -1:
		print("Animation not found: " + animation)
		return
	active_animation = animation_index
	animation_player.play(animations[animation_index])

func _ready():
	super()
	network_properties.append(
		NetworkPropertySetting.new(self, "active_animation", NetworkPropertySetting.FLAG_LOSSY_CONSISTENCY)
	)
	network_properties.append(
		NetworkPropertySetting.new(self, "animation_position", NetworkPropertySetting.FLAG_LOSSY_CONSISTENCY | NetworkPropertySetting.FLAG_SYNC_ON_INTEREST)
	)

func _network_process(_tick):
	super(_tick)
	if not NetworkRunner.is_server:
		return
	if animation_player.is_playing():
		animation_position = animation_player.current_animation_position
	else:
		animation_position = 0.0
