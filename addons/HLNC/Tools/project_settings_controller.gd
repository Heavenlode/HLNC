@tool
class_name ProjectSettingsController extends EditorPlugin

func _enter_tree():
    ## Config Settings
    ProjectSettings.set_setting("HLNC/config/log_level", ProjectSettings.get_setting("HLNC/config/log_level", 0))
    ProjectSettings.add_property_info({
        "name": "HLNC/config/log_level",
        "type": TYPE_INT,
        "hint": PROPERTY_HINT_ENUM,
        "hint_string": "Error:1,Warn:2,Info:4,Verbose:8",
        "usage": PROPERTY_USAGE_DEFAULT,
    })

    ## Network Settings
    ProjectSettings.set_setting("HLNC/network/IP", ProjectSettings.get_setting("HLNC/network/IP", "127.0.0.1"))
    ProjectSettings.add_property_info({
        "name": "HLNC/network/IP",
        "type": TYPE_STRING,
        "usage": PROPERTY_USAGE_DEFAULT,
    })
    ProjectSettings.set_setting("HLNC/network/default_port", ProjectSettings.get_setting("HLNC/network/default_port", 8888))
    ProjectSettings.add_property_info({
        "name": "HLNC/network/default_port",
        "type": TYPE_INT,
        "hint": PROPERTY_HINT_RANGE,
        "hint_string": "1000,65535,1",
        "usage": PROPERTY_USAGE_DEFAULT,
    })
    ProjectSettings.set_setting("HLNC/network/MTU", ProjectSettings.get_setting("HLNC/network/MTU", 1400))
    ProjectSettings.add_property_info({
        "name": "HLNC/network/MTU",
        "type": TYPE_INT,
        "hint": PROPERTY_HINT_RANGE,
        "hint_string": "100,65535,1",
        "usage": PROPERTY_USAGE_DEFAULT,
    })

    ## World Settings
    ProjectSettings.set_setting("HLNC/world/default_scene",
        ProjectSettings.get_setting("HLNC/world/default_scene", ProjectSettings.get_setting("application/run/main_scene", "")))
    ProjectSettings.add_property_info({
        "name": "HLNC/world/default_scene",
        "type": TYPE_STRING,
        "hint": PROPERTY_HINT_FILE,
        "hint_string": "*.tscn",
        "usage": PROPERTY_USAGE_DEFAULT,
    })

    ProjectSettings.set_setting("HLNC/world/managed_entrypoint", ProjectSettings.get_setting("HLNC/world/managed_entrypoint", true))
    ProjectSettings.add_property_info({
        "name": "HLNC/world/managed_entrypoint",
        "type": TYPE_BOOL,
        "usage": PROPERTY_USAGE_DEFAULT,
    })

    if ProjectSettings.get_setting("HLNC/world/managed_entrypoint", true):
        ProjectSettings.set_setting("application/run/main_scene", "res://addons/HLNC/Utils/ServerClientConnector/default_server_client_connector.tscn")
    ProjectSettings.save()

func _build():
    NetRunner.Port = ProjectSettings.get_setting("HLNC/network/default_port")
    NetRunner.ServerAddress = ProjectSettings.get_setting("HLNC/network/IP")
    return true

func _exit_tree():
    pass
    # ProjectSettings.set_setting("application/run/main_scene", ProjectSettings.get_setting("HLNC/world/default_scene"))
    # ProjectSettings.clear("HLNC/config/log_level")
    # ProjectSettings.clear("HLNC/network/default_port")
    # ProjectSettings.clear("HLNC/network/IP")
    # ProjectSettings.clear("HLNC/world/default_scene")
    # ProjectSettings.save()
