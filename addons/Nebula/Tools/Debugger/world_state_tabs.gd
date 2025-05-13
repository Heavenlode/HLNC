@tool
extends TabContainer

var tab_states: Dictionary = {}

var tab_indices: Dictionary = {
    "Nodes": 1,
    "Calls": 2,
    "Logs": 3
}

func update_tab_state(tab_index: int, state: bool) -> void:
    if tab_states.has(tab_index) and tab_states[tab_index] == state:
        return

    tab_states[tab_index] = state

    if state:
        set_tab_title(tab_index, get_tab_title(tab_index) + " *")
    else:
        set_tab_title(tab_index, get_tab_title(tab_index).replace(" *", ""))

func _on_nodes_network_nodes_changed(state:bool) -> void:
    update_tab_state(tab_indices["Nodes"], state)

func _on_logs_log_changed(state:bool) -> void:
    update_tab_state(tab_indices["Logs"], state)

func _on_calls_changed(state:bool) -> void:
    update_tab_state(tab_indices["Calls"], state)
