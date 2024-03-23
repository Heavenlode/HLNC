@tool
extends VFlowContainer

var editor_file_system: EditorFileSystem
var dock_file_system: FileSystemDock
var active_item: int = -1

var path_classes: Dictionary = {}
var class_paths: Dictionary = {}

func class_is_network_node(class_path: String):
	while true:
		if class_path in path_classes:
			if path_classes[class_path] == "NetworkNode3D":
				return true
			var klass = path_classes[class_path]
			if klass not in class_paths:
				return false
			class_path = class_paths[path_classes[class_path]]
		else:
			return false

func find_network_scenes(dir: EditorFileSystemDirectory) -> Array[Dictionary]:

	var node_memo: Dictionary = {}

	var classes = ProjectSettings.get_global_class_list()
	for klass in classes:
		path_classes[klass["path"]] = klass["base"]
		class_paths[klass["class"]] = klass["path"]

	var node_regex = RegEx.new()
	node_regex.compile("\\[node name=\"(\\w+)\" type=\"Node3D\".*\\]")
	var script_regex = RegEx.new()
	script_regex.compile("script = ExtResource\\(\"(\\w+)\"\\)")
	var results: Array[Dictionary] = []
	for idx in range(dir.get_file_count()):
		var file_type = dir.get_file_type(idx)
		if file_type != "PackedScene":
			continue
		
		var scene = FileAccess.open(dir.get_file_path(idx), FileAccess.READ)
		var content = scene.get_as_text()
		scene.close()
		var nodes: Array[RegExMatch] = node_regex.search_all(content)
		for result in nodes:
			if result.get_string(0).find("parent=") != -1:
				continue
			var script_result = script_regex.search(content, result.get_end())
			if script_result:
				var resource_regex = RegEx.new()
				resource_regex.compile("\\[ext_resource type=\"Script\" path=\"(.*)\" id=\"" + script_result.get_string(1) + "\"\\]")
				var resource_result = resource_regex.search(content)
				if resource_result:
					if node_memo.has(result.get_string(1)):
						continue
					node_memo[result.get_string(1)] = true
					if class_is_network_node(resource_result.get_string(1)):
						results.push_back({
							"name": result.get_string(1),
							"path": dir.get_file_path(idx)
						})
						break

	for idx in range(dir.get_subdir_count()):
		results = results + find_network_scenes(dir.get_subdir(idx))
	
	return results

func normalizeNameForEnum(name: String):
	var result = ""
	for char in name:
		if not char.is_valid_int() and char == char.to_upper() and result.length() > 0:
			result += "_"
		result += char
	return result.replace(" ", "_").to_upper()

func _on_refresh_pressed():
	refresh()

func refresh():
	$List.clear()
	# var network_scenes = find_network_scenes(editor_file_system.get_filesystem())
	# var class_names = {}

	# var template = FileAccess.open("res://addons/HLNC/RegisteredNodesTemplate.cs.template", FileAccess.READ)
	# var content = template.get_as_text()
	# template.close()

	# var file = FileAccess.open("res://addons/HLNC/generated/registered_nodes.cs",FileAccess.WRITE)
	# var enum_content = network_scenes.reduce(func(accum, scene):
	# 	return accum + "\t" + normalizeNameForEnum(scene["name"]) + ",\n"
	# , "")
	# if enum_content.length() == 0:
	# 	enum_content = "\tNONE\n"
	# content = content.replace("$SCENES_ENUM", enum_content)
	# var map_content = network_scenes.reduce(func(accum, scene):
	# 	return accum + "\t{ NETWORK_SCENES." + normalizeNameForEnum(scene["name"]) + ", GD.Load<PackedScene>(\"" + scene["path"] + "\") },\n"
	# , "")
	# content = content.replace("$SCENES_MAP", map_content)
	# map_content = network_scenes.reduce(func(accum, scene):
	# 	return accum + "\t\"{ " + scene["path"] + "\", NETWORK_SCENES." + normalizeNameForEnum(scene["name"]) + " },\n"
	# , "")
	# content = content.replace("$PATH_PACK", map_content)

	# file.store_string(content)
	# file.close()
	# editor_file_system.reimport_files(["res://addons/HLNC/generated/registered_nodes.cs"])
	# editor_file_system.scan()
	# var registryScript = load("res://addons/HLNC/generated/registered_nodes.cs")
	# var NetworkRegistry = registryScript.new()
	# for scene_name in NetworkRegistry.NETWORK_SCENES.keys():
	# 	$List.add_item(scene_name.capitalize())

func _on_registered_network_nodes_item_selected(index:int):
	active_item = index

func _on_list_item_clicked(index:int, at_position:Vector2, mouse_button_index:int):
	active_item = index
	$List.select(index)
	if mouse_button_index == MOUSE_BUTTON_RIGHT:
		var mouse_pos = get_global_mouse_position()
		$PopupMenu.popup(Rect2i(mouse_pos.x, mouse_pos.y, 0, 0))


func _on_popup_menu_id_pressed(id:int):
	print("not implemented")
	# var registryScript = load("res://addons/HLNC/generated/registered_nodes.cs")
	# var NetworkRegistry = registryScript.new()
	# dock_file_system.navigate_to_path(NetworkRegistry.SCENES_MAP[active_item].resource_path)
