@tool
extends Window

@export var follow_check_box: CheckBox
@export var world_debug: Node

var _thread: Thread
var _logs_loaded: bool = false

func _on_close_requested() -> void:
    hide()
    $Panel/LogBox.text = ""
    if _thread and _thread.is_started():
        _thread.wait_to_finish()

func _on_open() -> void:
    popup_centered()
    _logs_loaded = false
    _thread = Thread.new()
    _thread.start(_load_logs_thread)

func _load_logs_thread() -> void:
    var logs = world_debug.GetLogs()
    call_deferred("_process_logs", logs)

func _process_logs(logs: Array) -> void:
    var log_text = ""
    for log in logs:
        log_text += "%s [Tick %d] %s: %s\n" % [log["timestamp"], log["id"], log["level"], log["message"]]
    $Panel/LogBox.text = log_text
    _logs_loaded = true
    if _thread and _thread.is_started():
        _thread.wait_to_finish()

func _on_world_debug_log(frameId:int, timestamp:String, level:String, message:String) -> void:
    if not visible:
        return
    $Panel/LogBox.text += "%s [Tick %d] %s: %s\n" % [timestamp, frameId, level, message]
    if follow_check_box.button_pressed:
        $Panel/LogBox.scroll_to_line($Panel/LogBox.get_line_count() - 1)