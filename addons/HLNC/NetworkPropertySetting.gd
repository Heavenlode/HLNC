class_name NetworkPropertySetting

# Client-side interpolate between value changes.
static var FLAG_LINEAR_INTERPOLATION = 1 << 0

# Repeatedly send the most recent value until acknowledged by the client.
static var FLAG_LOSSY_CONSISTENCY = 1 << 1

# Only send the most recent value when interest has been aquired by the client, then stop syncing.
static var FLAG_SYNC_ON_INTEREST = 1 << 2

var name: String = ""
var flags: int = 0
var interpolation_decider
var default_value: Variant

func _init(target_node: NetworkNode3D, _name: String, _flags: int = 0) -> void:
	self.name = _name
	self.flags = _flags
	self.default_value = target_node.get(_name)