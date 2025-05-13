@tool
extends VBoxContainer

@onready var tree: Tree = $Tree
@export var world_debug: Control
@export var world_inspector: Control

# Track items that need color transition
var transitioning_items: Dictionary = {}
const TRANSITION_SPEED: float = 3.0  # Adjust this to control transition speed
const WHITE_COLOR = Color(1, 1, 1)
const CHANGED_COLOR = Color(1, 0.5, 0)

signal network_node_inspected(node_data: Dictionary)
signal network_nodes_changed(state: bool)

func _process(delta: float) -> void:
    var items_to_remove = []
    
    for item in transitioning_items:
        if !is_instance_valid(item) or item.is_queued_for_deletion():
            items_to_remove.append(item)
            continue
        var current_color: Color = item.get_custom_color(0)
        var new_color = current_color.lerp(WHITE_COLOR, delta * TRANSITION_SPEED)
        
        item.set_custom_color(0, new_color)
        
        # If we're very close to white, remove from tracking
        if new_color.is_equal_approx(WHITE_COLOR):
            item.set_custom_color(0, WHITE_COLOR)
            items_to_remove.append(item)
    
    # Remove completed items
    for item in items_to_remove:
        transitioning_items.erase(item)

func _set_item_color(item: TreeItem, is_changed: bool) -> void:
    if is_changed:
        transitioning_items.erase(item)
        item.set_custom_color(0, CHANGED_COLOR)
    else:
        # Add to transitioning items instead of setting white directly
        transitioning_items[item] = true

func update_tree(frame_id: int, frame_data: Dictionary) -> void:
    var previous_frame = {}
    if frame_id > 0:
        previous_frame = world_debug.call("GetFrame", frame_id - 1)

    var world_state: Dictionary = frame_data.get("world_state")
    if world_state.is_empty():
        return
    
    # Create root node if it doesn't exist
    var root = tree.get_root()
    if not root:
        root = tree.create_item()
    
    # Update root node text
    root.set_text(0, world_state.get("nodeName", "Root"))
    var root_metadata = root.get_metadata(0)
    var changed = previous_frame != null and previous_frame.get("world_state", {}).hash() != world_state.hash()
    network_nodes_changed.emit(changed)
    _set_item_color(root, changed)
    root.set_metadata(0, world_state)

    _reconcile_children(root, world_state.get("children", {}), previous_frame.get("world_state", {}).get("children", {}))

    if tree.get_selected() != null:
        network_node_inspected.emit(tree.get_selected().get_metadata(0))

func _reconcile_children(parent_item: TreeItem, children: Dictionary, previous_children: Dictionary) -> void:
    var existing_children = {}
    var child = parent_item.get_first_child()

    while child:
        existing_children[child.get_text(0)] = child
        child = child.get_next()

    for child_name in children:
        var child_data = children[child_name]
        var child_item: TreeItem
        
        if existing_children.has(child_name):
            child_item = existing_children[child_name]
            existing_children.erase(child_name)
        else:
            child_item = tree.create_item(parent_item)
        
        child_item.set_text(0, child_data.get("nodeName", child_name))
        var child_metadata = child_item.get_metadata(0)
        var changed = previous_children != null and previous_children.get(child_name, {}).hash() != child_data.hash()
        network_nodes_changed.emit(changed)
        _set_item_color(child_item, changed)
        child_item.set_metadata(0, child_data)
        if child_data.has("children"):
            _reconcile_children(child_item, child_data["children"], previous_children.get(child_name, {}).get("children", {}))
    
    for child_item in existing_children.values():
        child_item.free()

func _on_world_debug_tick_frame_selected(tickFrame: TickFrameUI) -> void:
    var frame_data = world_debug.call("GetFrame", tickFrame.tick_frame_id)
    update_tree(tickFrame.tick_frame_id, frame_data)

func _on_world_debug_tick_frame_updated(id:int) -> void:
    if world_debug.get("IsLive"):
        var frame_data = world_debug.call("GetFrame", id)
        update_tree(id, frame_data)

func _on_tree_item_selected() -> void:
    network_node_inspected.emit(tree.get_selected().get_metadata(0))
