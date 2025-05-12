@tool
class_name TickFrameUI extends Control

var bar_red_stylebox = preload("res://addons/HLNC/Tools/Debugger/Ticks/tick_frame_bar_color_red.tres")
var bar_yellow_stylebox = preload("res://addons/HLNC/Tools/Debugger/Ticks/tick_frame_bar_color_yellow.tres")
var bar_green_stylebox = preload("res://addons/HLNC/Tools/Debugger/Ticks/tick_frame_bar_color_green.tres")

var unselected_outline = preload("res://addons/HLNC/Tools/Debugger/Ticks/tick_frame_unselected.tres")
var selected_outline = preload("res://addons/HLNC/Tools/Debugger/Ticks/tick_frame_selected.tres")

const frame_height = 112
var previous_tick_frame: Control
var next_tick_frame: Control
var is_selected: bool = false

var tick_frame_id: int
var frame_size: float = 0.0

func set_frame_size(size: int, size_percent: float) -> void:
    var tick_frame_bar: Panel = $Bar
    frame_size = size
    tick_frame_bar.set_size(Vector2(0, frame_height * size_percent), false)
    tick_frame_bar.set_position(Vector2(0, frame_height - (frame_height * size_percent)), false)

    if size_percent > 0.75:
        tick_frame_bar.add_theme_stylebox_override("panel", bar_red_stylebox)
    elif size_percent > 0.5:
        tick_frame_bar.add_theme_stylebox_override("panel", bar_yellow_stylebox)
    else:
        tick_frame_bar.add_theme_stylebox_override("panel", bar_green_stylebox)

func select() -> void:
    $Outline.add_theme_stylebox_override("panel", selected_outline)
    is_selected = true

func deselect() -> void:
    $Outline.add_theme_stylebox_override("panel", unselected_outline)
    is_selected = false