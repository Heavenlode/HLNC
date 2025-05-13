@tool
extends VBoxContainer

@export var item_container: ItemList
@export var worldDebug: Control

var calls: Dictionary = {}

signal calls_changed(state: bool)

func _on_world_debug_network_function_called(frameId: int, functionIndex: String) -> void:
    if worldDebug.get("SelectedTickFrameId") != frameId:
        return

    if not calls.has(frameId):
        calls[frameId] = []
    calls[frameId].append(functionIndex)
    item_container.add_item(functionIndex)
    calls_changed.emit(true)
func _on_world_debug_tick_frame_selected(tickFrame: TickFrameUI) -> void:
    item_container.clear()
    var frameData = worldDebug.call("GetFrame", tickFrame.tick_frame_id)
    var calls = frameData.network_function_calls
    for call_data in calls:
        item_container.add_item(call_data.name)
    calls_changed.emit(calls.size() > 0)