@tool
extends EditorPlugin

# const AUTOLOAD_RUNNER = "NetworkRunner"
# const AUTOLOAD_STATE_MANAGER = "NetworkStateManager"
# const MainPanel = preload("res://addons/HLNC/editor_plugin/main_screen.tscn")

# var main_panel_instance

func _build():
	# main_panel_instance.get_node("TabBar/RealtimeMonitor").start_server()
	return true

# func _enter_tree():
# 	var editor_file_system = get_editor_interface().get_resource_filesystem()
# 	var dir = DirAccess.open("res://")
# 	dir.make_dir_recursive("res://addons/HLNC/generated")
# 	if not FileAccess.file_exists("res://addons/HLNC/generated/registered_nodes.cs"):
# 		var template = FileAccess.open("res://addons/HLNC/RegisteredNodesTemplate.cs.template", FileAccess.READ)
# 		var content = template.get_as_text()
# 		template.close()
# 		content.replace("$SCENES_ENUM", "NONE")
# 		content.replace("$SCENES_MAP", "")
# 		content.replace("$PATH_PACK", "")
# 		var file = FileAccess.open("res://addons/HLNC/generated/registered_nodes.cs",FileAccess.WRITE)
# 		file.store_string(content)
# 		file.close()
# 		editor_file_system.reimport_files(["res://addons/HLNC/generated/registered_nodes.cs"])
# 		editor_file_system.scan()
# 	add_autoload_singleton(AUTOLOAD_RUNNER, "res://addons/HLNC/NetworkRunner.cs")
# 	add_autoload_singleton(AUTOLOAD_STATE_MANAGER, "res://addons/HLNC/NetworkStateManager.cs")
# 	main_panel_instance = MainPanel.instantiate()
# 	# Add the main panel to the editor's main viewport.
# 	get_editor_interface().get_editor_main_screen().add_child(main_panel_instance)
# 	var network_nodes_controller = main_panel_instance.get_node("TabBar/Settings/NetworkNodes")
# 	network_nodes_controller.dock_file_system = get_editor_interface().get_file_system_dock()
# 	network_nodes_controller.editor_file_system = editor_file_system
# 	# network_nodes_controller.refresh()
# 	# Hide the main panel. Very much required.
# 	_make_visible(false)


# func _exit_tree():
# 	if main_panel_instance:
# 		main_panel_instance.queue_free()
# 	remove_autoload_singleton(AUTOLOAD_STATE_MANAGER)
# 	remove_autoload_singleton(AUTOLOAD_RUNNER)


# func _has_main_screen():
# 	return true


# func _make_visible(visible):
# 	if main_panel_instance:
# 		main_panel_instance.visible = visible


# func _get_plugin_name():
# 	return "Network"


# func _get_plugin_icon():
# 	# Must return some kind of Texture for the icon.
# 	return get_editor_interface().get_base_control().get_theme_icon("Signals", "EditorIcons")
