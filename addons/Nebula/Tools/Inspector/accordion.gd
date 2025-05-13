@tool
class_name Accordion extends VBoxContainer

func _on_button_pressed() -> void:
    $Container.visible = !$Container.visible
    var text = $Button.text
    text[0] = "▼" if $Container.visible else "▶"
    $Button.text = text