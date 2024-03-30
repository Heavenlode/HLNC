class_name GDNetworkNode3D extends Node3D

signal NetworkPropertyChanged(node_path: String, property_name: String)

var parent_scene = null

func _init():
	set_meta("is_network_node", true)

func _ready():
	parent_scene = self
	while parent_scene != null:
		if parent_scene.has_meta("is_network_scene"):
			break
		parent_scene = parent_scene.get_parent()

	if parent_scene == null:
		printerr("Failed to find parent network scene for " + self.get_name())

func set_network_property(property: String, value: Variant):
	if !parent_scene:
		return
	var property_name = "network_" + property
	if property_name not in self:
		printerr("Property " + property_name + " does not exist in " + self.get_name())
		return
	set(property_name, value)
	NetworkPropertyChanged.emit(parent_scene.get_path_to(self), property_name)
	return

func _network_process(tick: int) -> void:
	pass
