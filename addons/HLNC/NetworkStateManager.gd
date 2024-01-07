extends Node

# Track the last tick number that we received an acknowledgement for each player
var latest_acknowledgements = {}
var network_properties_cache = {}
var consistency_buffers = {}
var spawn_buffers = {}
var despawn_buffers = {}
var peer_sync_state = {}
var interest_queue = {}

var is_loading = true

enum SYNC_STATE {
	INITIAL,
	LOADING,
	READY,
}

signal player_joined(peer_id: int)

func register_player(peer_id: int):
	latest_acknowledgements[peer_id] = 0
	consistency_buffers[peer_id] = {}
	spawn_buffers[peer_id] = {}
	despawn_buffers[peer_id] = {}
	peer_sync_state[peer_id] = SYNC_STATE.INITIAL
	network_properties_cache[peer_id] = {}
	for network_id in NetworkRunner.network_nodes:
		var network_node = NetworkRunner.network_nodes[network_id]
		for network_property in network_node.network_properties:
			if network_node.network_id not in network_properties_cache:
				network_properties_cache[network_node.network_id] = {}
			network_properties_cache[network_node.network_id][network_property.name] = network_property.default_value

func peer_aware_of_node(peer_id: int, node: NetworkNode3D):
	return (
		not node.dynamic_spawn or (
		latest_acknowledgements[peer_id] >= node.spawn_tick))

func export_state(current_tick: int):

	var active_peers = {}
	var peer_buffers = {}
	var new_spawns_count = {}
	var new_spawns_buffers = {}
	var interest_change_count = {}
	var interest_change_buffers = {}
	var interest_added_net_ids = {}
	for peer_id in NetworkRunner.multiplayer_api.get_peers():
		peer_buffers[peer_id] = HLBuffer.new()
		new_spawns_buffers[peer_id] = HLBuffer.new()
		interest_change_buffers[peer_id] = HLBuffer.new()
		interest_added_net_ids[peer_id] = {}
		new_spawns_count[peer_id] = 0
		interest_change_count[peer_id] = 0
		active_peers[peer_id] = true
	
	# Interest management
	for interest_queue_values in interest_queue.keys():
		var peer_id = interest_queue_values[0]
		if peer_id not in active_peers:
			continue
		var net_id = interest_queue_values[1]
		var has_interest: bool = interest_queue[interest_queue_values]
		var interest_node = NetworkRunner.network_nodes[net_id]
		interest_queue.erase(interest_queue_values)
		for network_node in NetworkRunner.get_all_network_nodes(interest_node):
			if network_node.restrict_interest:
				continue
			if has_interest:
				network_node.interest[peer_id] = true
				interest_added_net_ids[peer_id][net_id] = true
			else:
				network_node.interest.erase(peer_id)
			if network_node.has_method("on_interest_change"):
				network_node.on_interest_change(peer_id, has_interest)
		interest_change_count[peer_id] += 1
		interest_change_buffers[peer_id].pack(net_id)
		interest_change_buffers[peer_id].pack(has_interest)
		if has_interest and not interest_node.nested:
			if interest_node.despawned:
				NetworkRunner.notify_despawn(interest_node, peer_id)
			elif interest_node.dynamic_spawn:
				NetworkRunner.notify_spawn(interest_node, peer_id)

	# Inform clients of all interest changes
	for peer_id in interest_change_buffers:
		peer_buffers[peer_id].pack(interest_change_count[peer_id])
		peer_buffers[peer_id].bytes.append_array(interest_change_buffers[peer_id].bytes)
		peer_buffers[peer_id].pointer += interest_change_buffers[peer_id].bytes.size()

	# Spawns
	for peer_id in spawn_buffers:
		if peer_id not in active_peers:
			continue
		for tick_number in spawn_buffers[peer_id]:
			for new_spawn in spawn_buffers[peer_id][tick_number]:
				var class_id = new_spawn["class_id"]
				var position = new_spawn["position"]
				var input_authority = new_spawn["input_authority"]
				var net_ids: PackedInt32Array = new_spawn["generated_net_ids"]
				new_spawns_buffers[peer_id].pack(tick_number)
				new_spawns_buffers[peer_id].pack(class_id)
				new_spawns_buffers[peer_id].pack(position)
				new_spawns_buffers[peer_id].pack(input_authority)
				new_spawns_buffers[peer_id].pack(net_ids)
				new_spawns_count[peer_id] += 1
		peer_buffers[peer_id].pack(new_spawns_count[peer_id])
		peer_buffers[peer_id].bytes.append_array(new_spawns_buffers[peer_id].bytes)
		peer_buffers[peer_id].pointer += new_spawns_buffers[peer_id].bytes.size()

	# Despawns
	for peer_id in despawn_buffers:
		if peer_id not in active_peers:
			continue
		var despawn_ids: PackedInt32Array = []
		for tick_number in despawn_buffers[peer_id]:
			for network_id in despawn_buffers[peer_id][tick_number]:
				despawn_ids.push_back(network_id)
		peer_buffers[peer_id].pack(despawn_ids)

	var node_values_to_clear: Array = []
	# Inform clients of all network variables states
	for net_id in NetworkRunner.network_nodes:
		var node: NetworkNode3D = NetworkRunner.network_nodes[net_id]
		if node.despawned:
			continue
		for peer_id in NetworkRunner.multiplayer_api.get_peers():
			if peer_id not in active_peers:
				continue
			# Do not update nodes which the player has no interest in, or have not yet acknowledged existence of
			if peer_id not in node.interest:
				continue
			if not peer_aware_of_node(peer_id, node):
				continue
			var cache = network_properties_cache[peer_id]
			if net_id not in cache:
				cache[net_id] = {}
				
			var var_list = []
			for var_id in len(node.network_properties):
				var var_name = node.network_properties[var_id].name
				var var_val = node.get(var_name)
				var flags = node.network_properties[var_id].flags
				
				if var_name in cache[net_id] and cache[net_id][var_name] == var_val:
					continue

				if flags & NetworkPropertySetting.FLAG_SYNC_ON_INTEREST and net_id not in interest_added_net_ids[peer_id]:
					continue
					
				var_list.push_back([var_id, var_name, var_val, flags])
				if typeof(var_val) == TYPE_ARRAY:
					cache[net_id][var_name] = var_val.duplicate()
				else:
					cache[net_id][var_name] = var_val

			var list_size = var_list.size()
			if list_size == 0:
				continue
			peer_buffers[peer_id].pack(net_id)
			peer_buffers[peer_id].bytes.resize(peer_buffers[peer_id].bytes.size() + 1)
			peer_buffers[peer_id].bytes.encode_u8(peer_buffers[peer_id].pointer, list_size)
			peer_buffers[peer_id].pointer += 1
			for memo in var_list:
				var var_id = memo[0]
				var var_name = memo[1]
				var var_val = memo[2]
				var flags = memo[3]
				var consistency_buffer
				if flags & NetworkPropertySetting.FLAG_LOSSY_CONSISTENCY:
					if net_id not in consistency_buffers[peer_id]:
						consistency_buffers[peer_id][net_id] = {}
					if var_id not in consistency_buffers[peer_id][net_id]:
						consistency_buffers[peer_id][net_id][var_id] = []
					consistency_buffer = consistency_buffers[peer_id][net_id][var_id]
				peer_buffers[peer_id].pack_network_variable(current_tick, flags, consistency_buffer, var_id, var_name, var_val)

	for clearable in node_values_to_clear:
		var node = clearable[0]
		var var_name = clearable[1]
		node.set(var_name, [])

	return peer_buffers

func import_state(incoming_tick, state_size, state_bytes):
	var changed_nodes = {}
	var old_tick = NetworkRunner.current_tick
	if state_size > 0:
		var state_buf = HLBuffer.new()
		state_buf.bytes = state_bytes.decompress(state_size)
		state_buf.pointer = 0

		var interest_changes = state_buf.unpack(TYPE_INT)
		for i in range(interest_changes):
			var net_id = state_buf.unpack(TYPE_INT)
			var has_interest = state_buf.unpack(TYPE_BOOL)
			if net_id in NetworkRunner.network_nodes:
				for network_node in NetworkRunner.get_all_network_nodes(NetworkRunner.network_nodes[net_id]):
					network_node.interest[NetworkRunner.local_player_id] = has_interest
					if has_interest:
						network_node.reveal_tick = incoming_tick
					if network_node.has_method("on_interest_change"):
						network_node.on_interest_change(NetworkRunner.local_player_id, has_interest)

		var spawn_count = state_buf.unpack(TYPE_INT)
		for i in range(spawn_count):
			var tick_number = state_buf.unpack(TYPE_INT)
			var class_id = state_buf.unpack(TYPE_INT)
			var position = state_buf.unpack(TYPE_VECTOR3)
			var input_authority = state_buf.unpack(TYPE_INT)
			var net_ids = state_buf.unpack(TYPE_PACKED_INT32_ARRAY)
			var already_spawned = false
			for net_id in net_ids:
				if net_id in NetworkRunner.network_nodes:
					already_spawned = true
					break
			if already_spawned:
				continue
			NetworkRunner.spawn_helper(class_id, position, input_authority, [], net_ids)

		var despawn_ids = state_buf.unpack(TYPE_PACKED_INT32_ARRAY)
		for network_id in despawn_ids:
			NetworkRunner.despawn_helper(network_id)

		var tick_changes = {
			incoming_tick: []
		}
		var unpacked_values = state_buf.unpack_network_variables()
		for unpacked in unpacked_values:
			var net_id = unpacked[0]
			var var_id = unpacked[1]
			var flags = unpacked[2]
			var var_val = unpacked[3]
			if flags & NetworkPropertySetting.FLAG_LOSSY_CONSISTENCY:
				for consistency_buffered_val in var_val:
					var consistency_tick = consistency_buffered_val[0]
					if consistency_tick < old_tick:
						continue
					var consistency_val = consistency_buffered_val[1]
					if consistency_tick not in tick_changes:
						tick_changes[consistency_tick] = []
					tick_changes[consistency_tick].push_back([net_id, var_id, consistency_val])
			else:
				tick_changes[incoming_tick].push_back([net_id, var_id, var_val])

		var ordered_ticks = tick_changes.keys()
		ordered_ticks.sort()
		for tick in ordered_ticks:
			NetworkRunner.current_tick = tick
			for change in tick_changes[tick]:
				var net_id = change[0]
				changed_nodes[net_id] = true
				var var_id = change[1]
				var var_val = change[2]
				NetworkRunner.network_nodes[net_id].queue_change(tick, var_id, var_val)
				NetworkRunner.network_nodes[net_id]._network_process(tick)

	NetworkRunner.current_tick = incoming_tick
	for net_id in NetworkRunner.network_nodes:
		if net_id not in changed_nodes:
			NetworkRunner.network_nodes[net_id]._network_process(incoming_tick)

func update_consistency_buffers(peer_id, previous_acknowledgement, client_current_tick):
	for tick_number in spawn_buffers[peer_id].keys():
		if tick_number <= client_current_tick:
			spawn_buffers[peer_id].erase(tick_number)
	for tick_number in despawn_buffers[peer_id].keys():
		if tick_number <= client_current_tick:
			despawn_buffers[peer_id].erase(tick_number)
	for acknowledged_tick in range(previous_acknowledgement, client_current_tick):
		for net_id in consistency_buffers[peer_id]:
			for var_id in consistency_buffers[peer_id][net_id]:
				var buffer = consistency_buffers[peer_id][net_id][var_id]
				var idx = 0
				for buffered_var_value in buffer:
					var current_tick = buffered_var_value[0]
					if current_tick <= acknowledged_tick:
						idx += 1
						continue
					else:
						break
				consistency_buffers[peer_id][net_id][var_id] = buffer.slice(idx)

@rpc("authority", "call_remote", "unreliable_ordered")
func tick(incoming_tick: int, state_size: int, state_bytes: PackedByteArray):
	if incoming_tick <= NetworkRunner.current_tick:
		return
	import_state(incoming_tick, state_size, state_bytes)
	rpc_id(1, "tick_acknowledge", incoming_tick)
	NetworkRunner.current_tick = incoming_tick
	

# TODO How to ensure this can only be received by the server (similar to InputHandler)
@rpc("any_peer", "call_remote", "unreliable_ordered")
func tick_acknowledge(client_current_tick):
	if not NetworkRunner.is_server:
		return
	var peer_id = multiplayer.get_remote_sender_id()
	var previous_acknowledgement = latest_acknowledgements[peer_id]
	latest_acknowledgements[peer_id] = client_current_tick
	update_consistency_buffers(peer_id, previous_acknowledgement, client_current_tick)
	
	if NetworkRunner.network_debug != null:
		if peer_id not in NetworkRunner.debug_player_ping:
			NetworkRunner.debug_player_ping[peer_id] = []
			
		NetworkRunner.debug_player_ping[peer_id].push_back(NetworkRunner.current_tick - client_current_tick)


@rpc("authority", "call_remote", "reliable", 3)
func receive_initial_state(incoming_tick: int, state_size: int, state_bytes: PackedByteArray):
	import_state(incoming_tick, state_size, state_bytes)
	NetworkRunner.current_tick = incoming_tick
	rpc_id(1, "sync_acknowledge", incoming_tick)	

# TODO How to ensure this can only be received by the server (similar to InputHandler)
@rpc("any_peer", "call_remote", "reliable", 3)
func sync_acknowledge(client_current_tick):
	if not NetworkRunner.is_server:
		return
	var peer_id = multiplayer.get_remote_sender_id()
	var previous_acknowledgement = latest_acknowledgements[peer_id]
	latest_acknowledgements[peer_id] = client_current_tick
	update_consistency_buffers(peer_id, previous_acknowledgement, client_current_tick)
	print("SYNC ACK", client_current_tick)
	peer_sync_state[peer_id] = SYNC_STATE.READY
	player_joined.emit(peer_id)
