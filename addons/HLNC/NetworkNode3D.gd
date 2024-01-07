class_name NetworkNode3D extends Node3D

# Whenever a user gains interest of a node, they gain interest in all child nodes by default
# However, some child objects should have their interest static / managed manually
# For example to keep hidden values only available to one player
var restrict_interest: bool = false

# This is used client-side to determine if a node should have its network properties interpolated
# Specifically useful for when interest is added. We do not want to interpolate revealed properties.
var reveal_tick: int = -1

# When a node is despawned, we indicate that here for the Server
# As the server must keep track of a node for other users to know it has despawned
var despawned = false
var despawn_aware = {}

# When a user gains interest of a node, the server needs these to know if the user should be notified
# that the node must be created on their end
var dynamic_spawn = false
var spawn_aware = {}
var spawn_tick: int = -1

var nested = false
var network_id: int = -1;
var input_authority: int = -1;
var is_current_owner:
	get:
		return NetworkRunner.is_server or input_authority == NetworkRunner.local_player_id

var network_properties: Array[NetworkPropertySetting] = []

var interest = {}
var change_queue = {}

static func GetFromNetworkId(network_id):
	if network_id == -1:
		return null
	if network_id not in NetworkRunner.network_nodes:
		return null
	return NetworkRunner.network_nodes[network_id]

static func FindFromChild(node):
	while node != null:
		if node is NetworkNode3D:
			return node
		node = node.get_parent()
	return null

func despawn():
	if not NetworkRunner.is_server:
		return
	NetworkRunner.Despawn(self)

func queue_change(tick, var_id, var_value):
	var prop = network_properties[var_id]

	if self.has_method("_on_network_change_{0}".format([prop.name])):
		self.call("_on_network_change_{0}".format([prop.name]), tick, get(prop.name), var_value)

	change_queue[prop.name] = {
		"prop": prop,
		"from": get(prop.name),
		"to": var_value,
		"weight": 0.0
	}

func _ready():
	if dynamic_spawn:
		return
	NetworkRunner._register_static_spawn(self)

func _network_process(_tick: int):
	pass

func get_input():
	if input_authority not in NetworkRunner.input_store or not is_current_owner:
		return
	return NetworkRunner.input_store[input_authority]

func _physics_process(delta: float):
	if is_queued_for_deletion():
		return
	if NetworkRunner.is_server:
		return
	for var_name in change_queue:
		var to_lerp = change_queue[var_name]
		var next_value

		var flags = to_lerp["prop"].flags
		if to_lerp["prop"].interpolation_decider != null:
			flags &= ~NetworkPropertySetting.FLAG_LINEAR_INTERPOLATION
			flags |= to_lerp["prop"].interpolation_decider.call()

		if not (flags & NetworkPropertySetting.FLAG_LINEAR_INTERPOLATION) or NetworkRunner.current_tick <= reveal_tick:
			to_lerp["weight"] = 1.0
			next_value = to_lerp["to"]
			set(var_name, to_lerp["to"])
			change_queue.erase(var_name)

		else:
			if to_lerp["weight"] < 1.0:
				to_lerp["weight"] = min(to_lerp["weight"] + (delta * 10), 1.0)
				if typeof(to_lerp["from"]) == TYPE_QUATERNION:
					next_value = to_lerp["from"].normalized().slerp(to_lerp["to"].normalized(), to_lerp["weight"])
				else:
					next_value = lerp(to_lerp["from"], to_lerp["to"], to_lerp["weight"])
				set(var_name, next_value)
				if to_lerp["weight"] >= 1.0:
					change_queue.erase(var_name)

		if self.has_signal("interpolate_{0}".format([var_name])):
			self.emit_signal("interpolate_{0}".format([var_name]), next_value)
