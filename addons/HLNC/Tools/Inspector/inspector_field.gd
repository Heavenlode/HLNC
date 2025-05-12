@tool
extends MarginContainer

func set_value(value: String) -> void:
    $HBoxContainer/RichTextLabel.text = value
