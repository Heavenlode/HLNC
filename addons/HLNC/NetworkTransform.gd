class_name NetworkTransform extends NetworkNode3D

var teleporting = true
@onready var net_position = get_parent().global_position
@onready var net_rotation = get_parent().quaternion

signal interpolate_net_position(next_value)

func interpolation_decider():
	if not teleporting:
		return NetworkPropertySetting.FLAG_LINEAR_INTERPOLATION
	else:
		teleporting = false
		return 0

func _ready():
	super._ready()
	var net_pos_prop = NetworkPropertySetting.new(self, "net_position", NetworkPropertySetting.FLAG_LINEAR_INTERPOLATION | NetworkPropertySetting.FLAG_LOSSY_CONSISTENCY)
	net_pos_prop.interpolation_decider = interpolation_decider
	network_properties.append(net_pos_prop)
	network_properties.append(NetworkPropertySetting.new(self, "net_rotation", NetworkPropertySetting.FLAG_LINEAR_INTERPOLATION | NetworkPropertySetting.FLAG_LOSSY_CONSISTENCY))

func face(direction: Vector3):
	if not NetworkRunner.is_server:
		return
	get_parent().look_at(direction, Vector3.UP, true)

func _network_process(delta):
	super._network_process(delta)
	if not NetworkRunner.is_server:
		return
	net_position = get_parent().global_position
	net_rotation = get_parent().quaternion

func _physics_process(delta):
	super._physics_process(delta)
	if NetworkRunner.is_server:
		return
	get_parent().global_position = net_position
	get_parent().quaternion = net_rotation

func teleport(incoming_position: Vector3):
	if not NetworkRunner.is_server:
		teleporting = true
		return
	get_parent().global_position = incoming_position
	net_position = incoming_position
