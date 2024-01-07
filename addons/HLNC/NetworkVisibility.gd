class_name NetworkVisibility extends Node3D

@export var nodes: Array[Node] = []

func _enter_tree():
	var net_parent = NetworkNode3D.FindFromChild(self)
	if net_parent.is_current_owner and not NetworkRunner.is_server:
		return

	while nodes.size() > 0:
		var node = nodes.pop_back()
		node.process_mode = Node.PROCESS_MODE_DISABLED
		for child in node.get_children():
			nodes.push_back(child)
		remove_child(node)
		node.set_script(null)
		node.queue_free()
