@tool
extends Button

@export var data_frames_window: Window

func _on_pressed() -> void:
	data_frames_window.popup_centered()