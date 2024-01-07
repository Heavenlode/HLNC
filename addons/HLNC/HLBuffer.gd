class_name HLBuffer

var bytes: PackedByteArray
var pointer: int
var CONSISTENCY_BUFFER_SIZE_LIMIT = 256

func _init():
	bytes = PackedByteArray()
	pointer = 0

var warned = false

func pack_network_variable(
	tick: int,
	flags: int,
	consistency_buffer,
	var_id: int,
	var_name: String,
	var_val: Variant
):
	bytes.resize(bytes.size() + 1)
	bytes.encode_u8(pointer, var_id)
	pointer += 1
	var result = false
	if flags & NetworkPropertySetting.FLAG_LOSSY_CONSISTENCY:
		var intermediate_buffer = HLBuffer.new()
		consistency_buffer.clear()
		consistency_buffer.push_back([tick, var_val])
		while true:
			var buffer_size = intermediate_buffer.bytes.size()
			if buffer_size > CONSISTENCY_BUFFER_SIZE_LIMIT:
				if consistency_buffer.size() == 1:
					if not warned:
						print("WARNING, BUFFER COULD NOT CONTAIN A SINGLE VALUE", var_id, var_name, var_val)
						warned = true
					break
				consistency_buffer.pop_front()
			elif buffer_size > 0:
				break
			intermediate_buffer.bytes.clear()
			intermediate_buffer.pack(consistency_buffer)
		result = pack(consistency_buffer)
	else:
		result = pack(var_val)

	if not result:
		print("Failed to pack variable {0} with value {1}".format([var_name, var_val]))

func unpack_network_variables(node: NetworkNode3D = null):
	var target_node = node
	var result = []
	while pointer < bytes.size():
		if node == null:
			var target_net_id = bytes.decode_s64(pointer)
			target_node = NetworkNode3D.GetFromNetworkId(target_net_id)
			if target_node == null:
				print("Failed to unpack network variables, node not found: " + str(target_net_id))
			pointer += 8
		var var_count = bytes.decode_u8(pointer)
		pointer += 1
		for i in range(var_count):
			var unpacked = unpack_variable(target_node)
			result.push_back([target_node.network_id] + unpacked)
	return result

func pack(var_val: Variant = null):
	return HLBytes.pack(self, var_val)

func unpack(type):
	return HLBytes.unpack(self, type)

func unpack_variable(node: NetworkNode3D):
	var var_id = bytes.decode_u8(pointer)
	pointer += 1
	var flags = node.network_properties[var_id].flags
	var var_name = node.network_properties[var_id].name
	if flags & NetworkPropertySetting.FLAG_LOSSY_CONSISTENCY:
		var buffer_values = []
		buffer_values.push_back(unpack(TYPE_ARRAY))
		return [var_id, flags] + buffer_values
	else:
		return [var_id, flags, unpack(typeof(node.get(var_name)))]

