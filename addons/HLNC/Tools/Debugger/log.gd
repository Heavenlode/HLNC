@tool
extends VBoxContainer

@export var follow_check_box: CheckBox
@export var worldDebug: Control

signal log_changed(state: bool)

func _on_world_debug_log(frameId:int, timestamp:String, level:String, message:String) -> void:
    if worldDebug.get("SelectedTickFrameId") != frameId:
        return

    $RichTextLabel.text += "%s: %s\n" % [level, message]
    log_changed.emit(true)

func _on_world_debug_tick_frame_selected(tickFrame: TickFrameUI) -> void:
    $RichTextLabel.text = ""
    var frameData = worldDebug.call("GetFrame", tickFrame.tick_frame_id)
    var logs: Array = frameData.logs
    for log in logs:
        $RichTextLabel.text += "%s: %s\n" % [log.level, log.message]
    log_changed.emit(logs.size() > 0)