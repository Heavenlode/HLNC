@tool
extends EditorPlugin

const AUTOLOAD_RUNNER = "NetworkRunner"
const AUTOLOAD_ENV = "Env"

func _build():
    return true

func _enter_tree():
    add_autoload_singleton(AUTOLOAD_ENV, "res://addons/HLNC/Utils/Env/Env.cs")
    add_autoload_singleton(AUTOLOAD_RUNNER, "res://addons/HLNC/Core/NetworkRunner.cs")
    var project_settings_controller = ProjectSettingsController.new()
    add_child(project_settings_controller)

func _exit_tree():
    remove_autoload_singleton(AUTOLOAD_RUNNER)
    remove_autoload_singleton(AUTOLOAD_ENV)
