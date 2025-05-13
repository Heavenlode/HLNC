@tool
class_name InfiniteScrollContainer extends ScrollContainer

# NOTE: This requires that the children of the FrameContainer are all the same size.
# It fetches the width of the first child

var scroll_direction: Vector2 = Vector2.ZERO
var previous_scroll_position: float = 0
var current_page: int = 0
var item_size: int = 0
var page_size: int = 0
var num_pages: int = 0
var is_live: bool = true
var left_end: bool = false
var right_end: bool = false

const MAX_PAGES: int = 2
const DEFAULT_PAGE_SIZE: int = 30

signal on_scroll(direction: Vector2, percentage: float)
signal on_previous_page(start_id: int, num_children: int)
signal on_next_page(start_id: int, num_children: int)

func _ready() -> void:
    on_scroll.connect(handle_scroll)

func _process(delta: float) -> void:
    if get_h_scroll_bar().value > previous_scroll_position:
        scroll_direction = Vector2.RIGHT
    elif get_h_scroll_bar().value < previous_scroll_position:
        scroll_direction = Vector2.LEFT

    previous_scroll_position = get_h_scroll_bar().value

    emit_signal("on_scroll", scroll_direction, get_h_scroll_bar().value / (get_h_scroll_bar().max_value - size.x))

func paginate_child(node: Node) -> void:
    $FrameContainer.add_child(node)
    if not _prepare_pagination():
        return
    while num_pages > MAX_PAGES:
        if not _prepare_pagination():
            return
        if $FrameContainer.get_child_count() == 0:
            break
        var previous_child = $FrameContainer.get_child(0)
        $FrameContainer.remove_child(previous_child)
        previous_child.queue_free()

func scroll_to_item(item_index):
    # Get the ScrollContainer and its viewport
    var viewport = get_viewport()
    
    # Calculate the total scrollable width
    var total_scrollable_width = get_h_scroll_bar().max_value
    
    # Calculate target scroll position based on item index
    var target_position = item_size * item_index
    
    # Clamp the target position to valid scroll range
    target_position = clamp(target_position, 0, total_scrollable_width)
    
    # Get the scroll bar position and dimensions
    var scroll_bar = get_h_scroll_bar()
    var scroll_bar_rect = Rect2(scroll_bar.get_global_rect().position, scroll_bar.get_global_rect().size)
    
    # Calculate the ratio of our target position within the scrollable area
    var scroll_ratio = target_position / total_scrollable_width if total_scrollable_width > 0 else 0
    
    # Map this ratio to a position on the scroll bar
    var mouse_x = scroll_bar_rect.position.x + (scroll_ratio * scroll_bar_rect.size.x)
    var mouse_y = scroll_bar_rect.position.y + (scroll_bar_rect.size.y / 2)
    
    # Warp the mouse to this position
    viewport.warp_mouse(Vector2(mouse_x, mouse_y))

func load_previous_page(nodes: Array, is_end: bool) -> void:
    right_end = false
    if is_end:
        left_end = true

    for node in nodes:
        $FrameContainer.add_child(node)
        $FrameContainer.move_child(node, 0)

    _prepare_pagination()

    while num_pages > MAX_PAGES:
        if not _prepare_pagination():
            return
        if $FrameContainer.get_child_count() == 0:
            break
        var next_child = $FrameContainer.get_child($FrameContainer.get_child_count() - 1)
        $FrameContainer.remove_child(next_child)
        next_child.queue_free()


    scroll_to_item(nodes.size() - 1)

func load_next_page(nodes: Array, is_end: bool) -> void:
    left_end = false
    if is_end:
        right_end = true

    for node in nodes:
        $FrameContainer.add_child(node)

    _prepare_pagination()

    while num_pages > MAX_PAGES:
        if not _prepare_pagination():
            return
        if $FrameContainer.get_child_count() == 0:
            break
        var previous_child = $FrameContainer.get_child(0)
        $FrameContainer.remove_child(previous_child)
        previous_child.queue_free()

    scroll_to_item($FrameContainer.get_child_count() - nodes.size())

func handle_scroll(direction: Vector2, percentage: float) -> void:
    if is_live:
        return

    if not _prepare_pagination():
        return

    if scroll_direction == Vector2.LEFT and percentage < 0.05 and not left_end:
        on_previous_page.emit($FrameContainer.get_child(0).tick_frame_id, page_size)
    elif scroll_direction == Vector2.RIGHT and percentage > 0.95 and not right_end:
        on_next_page.emit($FrameContainer.get_child($FrameContainer.get_child_count() - 1).tick_frame_id, page_size)

func _prepare_pagination() -> bool:
    if item_size == 0:
        if $FrameContainer.get_child_count() == 0:
            return false

        var child = $FrameContainer.get_child(0) as TickFrameUI
        item_size = child.size.x
    if item_size <= 0:
        return false
    if $FrameContainer.get_child_count() == 0:
        return false
    page_size = max(DEFAULT_PAGE_SIZE, ceil(size.x / item_size))
    num_pages = floor($FrameContainer.get_child_count() / page_size)
    return true
