extends Node

@export var server_address = "10.2.100.2"
@export var port = 8888
@export var max_peers = 5

var netpeer = ENetMultiplayerPeer.new()
var multiplayer_api : MultiplayerAPI
var is_server = false
var net_started = false

var debug_scene = preload("res://addons/HLNC/NetworkDebug.tscn")

# Unfortunately RPCs cannot use a static enum for a channel name
# So we keep this here as documentation
# enum TRANSFER_CHANNEL {
# 	STATE_TRANSFER	= 0, (DEFAULT)
# 	CLIENT_INPUT	= 1,
# 	VISUAL_EFFECTS	= 2,
# }

var network_id_counter = 0
var network_nodes = {}
var net_ids_memo = {}
var local_player_id:
	get:
		return multiplayer_api.get_unique_id()

var input_store = {}

func DebugPrint(msg):
	print("{0}: {1}".format(["Server" if is_server else "Client", msg]))

var network_debug: NetworkDebug

func start_network():
	if is_server:
		start_server()
		# TODO Ensure this only happens in development
		network_debug = debug_scene.instantiate()
		add_child(network_debug)
		network_debug.connect_to_monitor()
	else:
		start_client()

func _register_static_spawn(node: NetworkNode3D):
	var parent_name
	if node.get_parent() != null:
		parent_name = node.get_parent().name
	else:
		parent_name = "root"
	(parent_name + node.name).hash()
	network_id_counter += 1
	while network_id_counter in network_nodes:
		DebugPrint("Hash collision detected: {0}".format([node.name]))
		network_id_counter += 1
	network_nodes[network_id_counter] = node
	node.network_id = network_id_counter

func start_server():
	get_tree().multiplayer_poll = false
	var err = netpeer.create_server(port, max_peers)
	if err != OK:
		DebugPrint("Error starting: {0}".format([err]))
		return
	multiplayer_api = MultiplayerAPI.create_default_interface()
	multiplayer_api.peer_connected.connect(_on_peer_connected)
	multiplayer_api.peer_disconnected.connect(_on_peer_disconnected)
	get_tree().set_multiplayer(multiplayer_api, "/root")
	multiplayer_api.multiplayer_peer = netpeer
	net_started = true
	DebugPrint("Started")

func start_client():
	get_tree().multiplayer_poll = false
	var err = netpeer.create_client(server_address, port)
	if err != OK:
		DebugPrint("Error connecting: {0}".format([err]))
		return
	multiplayer_api = MultiplayerAPI.create_default_interface()
	multiplayer_api.peer_connected.connect(_on_peer_connected)
	multiplayer_api.peer_disconnected.connect(_on_peer_disconnected)
	get_tree().set_multiplayer(multiplayer_api, "/root") 
	multiplayer_api.multiplayer_peer = netpeer
	net_started = true
	DebugPrint("Started")

var frame_counter = 0
const FRAMES_PER_SECOND = 60
const FRAMES_PER_TICK = 2
const TPS = FRAMES_PER_SECOND / FRAMES_PER_TICK
const MTU = 1400

var current_tick = 0

var network_properties_cache = {}

var debug_data_sizes = []
var debug_player_ping = {}

func notify_despawn(network_node: NetworkNode3D, peer_id: int):
	if peer_id in network_node.despawn_aware:
		return
	network_node.despawn_aware[peer_id] = true
	if NetworkRunner.current_tick not in NetworkStateManager.despawn_buffers[peer_id]:
		NetworkStateManager.despawn_buffers[peer_id][NetworkRunner.current_tick] = PackedInt32Array()
	NetworkStateManager.despawn_buffers[peer_id][NetworkRunner.current_tick].push_back(network_node.network_id)

func notify_spawn(network_node: NetworkNode3D, peer_id: int):
	if peer_id in network_node.spawn_aware:
		return
	network_node.spawn_aware[peer_id] = true
	if NetworkRunner.current_tick not in NetworkStateManager.despawn_buffers[peer_id]:
		NetworkStateManager.spawn_buffers[peer_id][NetworkRunner.current_tick] = []
	var position = network_node.global_position
	if "network_transform" in network_node:
		position = network_node.network_transform.net_position
	NetworkStateManager.spawn_buffers[peer_id][NetworkRunner.current_tick].push_back({
		"class_id": NetworkRegistry.PATH_PACK[network_node.scene_file_path],
		"position": position,  
		"input_authority": network_node.input_authority,
		"generated_net_ids": net_ids_memo[network_node.network_id],
	})

func queue_interest(network_node: NetworkNode3D, peer_id: int, _interest: bool):
	if peer_id in network_node.interest and network_node.interest[peer_id] == _interest:
		return
	if not _interest and peer_id in GlobalGameState.player_contractors and GlobalGameState.player_contractors[peer_id] == network_node:
		return
	NetworkStateManager.interest_queue[[peer_id, network_node.network_id]] = _interest

func process_debug_data():
	if network_debug == null:
		return
	if debug_data_sizes.size() >= TPS:
		var bytes_per_second = debug_data_sizes.reduce(func(collect, curr): return collect + curr, 0)
		var largest_tick_value = debug_data_sizes.reduce(func(collect, curr): return max(collect, curr), 0)
		network_debug.log([NetworkDebug.Message.BYTES_PER_SECOND, bytes_per_second, largest_tick_value])
		debug_data_sizes.clear()
	for peer_id in debug_player_ping:
		var ping = debug_player_ping[peer_id]
		if ping.size() >= TPS:
			var avg_ping = ping.reduce(func(collect, curr): return collect + curr, 0) / TPS
			network_debug.log([NetworkDebug.Message.PING, peer_id, avg_ping])
			debug_player_ping[peer_id].clear()

func server_process_tick():

	for net_id in network_nodes:
		var node = network_nodes[net_id]
		if node == null:
			continue
		if node.is_queued_for_deletion():
			network_nodes.erase(net_id)
			continue
		node._network_process(current_tick)

	var exported_state = NetworkStateManager.export_state(current_tick)
	for peer_id in multiplayer_api.get_peers():
		if peer_id == 1:
			continue
		if NetworkStateManager.peer_sync_state[peer_id] == NetworkStateManager.SYNC_STATE.INITIAL:
			NetworkStateManager.peer_sync_state[peer_id] = NetworkStateManager.SYNC_STATE.LOADING
			NetworkStateManager.rpc_id(peer_id, "receive_initial_state", current_tick, exported_state[peer_id].bytes.size(), exported_state[peer_id].bytes.compress())
		elif NetworkStateManager.peer_sync_state[peer_id] == NetworkStateManager.SYNC_STATE.READY:
			if peer_id not in exported_state:
				NetworkStateManager.rpc_id(peer_id, "tick", current_tick, 0, PackedByteArray())
			else:
				var compressed_payload = exported_state[peer_id].bytes.compress()
				var size = exported_state[peer_id].bytes.size()
				if network_debug != null:
					debug_data_sizes.push_back(compressed_payload.size())
				if size > MTU:
					DebugPrint("Warning: Data size {0} exceeds MTU {1}".format([size, MTU]))
					
				NetworkStateManager.rpc_id(peer_id, "tick", current_tick, size, compressed_payload)

func _physics_process(_delta: float) -> void:
	if not net_started:
		return
		
	if multiplayer_api.has_multiplayer_peer():
		multiplayer_api.poll()

	if is_server:
		frame_counter += 1
		if frame_counter < FRAMES_PER_TICK:
			return
		frame_counter = 0
		current_tick += 1
		server_process_tick()
		process_debug_data()

# Synchronizing state if a player joins or reconnects
# To do this, we keep the player in a "Loading" state and ignore inputs
# During this process, we send the entire game state to the player via TPC reliable connection
# This same process can be done thru a "reconnecting..." de-sync repair

func _on_peer_connected(peer_id):
	
	if not is_server:
		if peer_id == 1:
			DebugPrint("Connected to server")
		else:
			DebugPrint("Peer connected to server")
		return

	DebugPrint("Peer {0} joined".format([peer_id]))

	for node in get_tree().get_nodes_in_group("global_interest"):
		if node is NetworkNode3D:
			node.interest[peer_id] = true
	NetworkStateManager.register_player(peer_id)

func _on_peer_disconnected(peer_id):
	DebugPrint("Peer disconnected peer_id: {0}".format([peer_id]))

func Spawn(scene: NetworkRegistry.NETWORK_SCENES, position: Vector3, input_authority: int = 1, interest: PackedInt32Array = []):
	if not is_server:
		return
	return spawn_helper(scene, position, input_authority, interest)

func Despawn(node: NetworkNode3D):
	if node.network_id not in network_nodes:
		return
	
	node.despawned = true
	net_ids_memo.erase(node.network_id)
	despawn_helper(node.network_id)	

func get_all_network_nodes(node) -> Array[NetworkNode3D]:
	var nodes: Array[NetworkNode3D] = []
	if is_instance_of(node, NetworkNode3D):
		nodes.append(node)
	for N in node.get_children():
		var child_nodes: Array[NetworkNode3D] = get_all_network_nodes(N)
		for child_node in child_nodes:
			if is_instance_of(child_node, NetworkNode3D):
				nodes.append(child_node)
	return nodes

func spawn_helper(class_id: NetworkRegistry.NETWORK_SCENES, position: Vector3, input_authority: int = 1, interest: PackedInt32Array = [], net_ids: PackedInt32Array = []):
	var network_node: NetworkNode3D = NetworkRegistry.SCENES_MAP[class_id].instantiate()
	var nodes = get_all_network_nodes(network_node)
	var generated_net_ids: PackedInt32Array = []
	for idx in len(nodes):
		var net_node = nodes[idx]
		var net_id
		net_node.spawn_tick = current_tick
		net_node.reveal_tick = current_tick
		if is_server:
			while network_id_counter in network_nodes:
				network_id_counter += 1
			net_id = network_id_counter
			generated_net_ids.append(net_id)
		else:
			net_id = net_ids[idx]


		net_node.dynamic_spawn = true
		net_node.nested = idx > 0
		net_node.network_id = net_id
		net_node.input_authority = input_authority
		for interest_id in interest:
			if net_node.restrict_interest and interest_id != input_authority:
				continue
			net_node.interest[interest_id] = true
		
		network_nodes[net_id] = net_node

	network_node.ready.connect(func() -> void:
		if "network_transform" in network_node: 
			network_node.network_transform.net_position = position
		network_node.global_position = position
	)
	get_tree().current_scene.add_child(network_node)

	net_ids_memo[network_node.network_id] = generated_net_ids

	if is_server:
		for peer_id in interest:
			if current_tick not in NetworkStateManager.spawn_buffers[peer_id]:
				NetworkStateManager.spawn_buffers[peer_id][current_tick] = []
			NetworkStateManager.spawn_buffers[peer_id][current_tick].push_back({
				"class_id": class_id,
				"position": position,  
				"input_authority": input_authority,
				"generated_net_ids": generated_net_ids,
			})
	else:
		if network_node is Contractor:
			GlobalGameState.player_contractors[input_authority] = network_node

	return network_node

func despawn_helper(net_id: int):
	if net_id not in network_nodes:
		return
	var node = network_nodes[net_id]
	if is_server:
		for child in node.get_children():
			if "collision_layer" in child and child.collision_layer & GlobalGameConfig.LAYER_VISIBLE_SHAPE:
				# Ensure anything that would have been visible now only exists for the purpose of
				# notifying players that it has despawned when they would "reveal it"
				child.collision_layer = GlobalGameConfig.LAYER_VISIBLE_SHAPE
			else:
				# Remove everything else
				child.queue_free()
		for peer_id in node.interest:
			if current_tick not in NetworkStateManager.despawn_buffers[peer_id]:
				NetworkStateManager.despawn_buffers[peer_id][current_tick] = PackedInt32Array()
			NetworkStateManager.despawn_buffers[peer_id][current_tick].push_back(node.network_id)
	else:
		node.queue_free()
		network_nodes.erase(net_id)

@rpc("any_peer", "call_local", "reliable", 1)
func transfer_input(tick, incoming_input):
	var sender = multiplayer.get_remote_sender_id()
	if sender not in input_store:
		input_store[sender] = {}
	for key in incoming_input:
		var incoming = incoming_input[key]
		if incoming is Vector2 or incoming is Vector3:
			incoming = incoming.normalized()
		input_store[sender][key] = incoming_input[key]
	

var visual_gather_ore_sprite = preload("res://ui/currency/gather_ore_sprite.tscn")

@rpc("authority", "call_remote", "reliable", 2)
func collect_money(from_position: Vector3, to_node: int, amount: int):
	for i in range(amount):
		var gather_sprite = visual_gather_ore_sprite.instantiate()
		gather_sprite.target_node = network_nodes[to_node].get_node("CharacterBody3D")
		gather_sprite.position = from_position
		gather_sprite.original_position = from_position
		add_child(gather_sprite)

var burst_scene = preload("res://ingame_objects/ore/malenite/burst.tscn")

@rpc("authority", "call_remote", "reliable", 2)
func ore_burst(type: Ore.ORE_TYPES, position: Vector3):
	var particle_burst = burst_scene.instantiate()
	get_tree().get_root().add_child(particle_burst)
	particle_burst.global_position = position
	particle_burst.start()

@rpc("authority", "call_remote", "reliable", 2)
func notice(message: String):
	print("NOTICE: {0}".format([message]))
