class_name HLBytes

static func pack(buffer: HLBuffer, var_val: Variant, pack_type: bool = false):
	var compute_type = typeof(var_val)
	if pack_type:
		buffer.bytes.resize(buffer.bytes.size() + 1)
		buffer.bytes.encode_u8(buffer.pointer, compute_type)
		buffer.pointer += 1
	if compute_type == TYPE_VECTOR3:
		buffer.bytes.resize(buffer.bytes.size() + 6)
		buffer.bytes.encode_half(buffer.pointer, var_val.x)
		buffer.pointer += 2
		buffer.bytes.encode_half(buffer.pointer, var_val.y)
		buffer.pointer += 2
		buffer.bytes.encode_half(buffer.pointer, var_val.z)
		buffer.pointer += 2
	elif compute_type == TYPE_VECTOR2:
		buffer.bytes.resize(buffer.bytes.size() + 4)
		buffer.bytes.encode_half(buffer.pointer, var_val.x)
		buffer.pointer += 2
		buffer.bytes.encode_half(buffer.pointer, var_val.y)
		buffer.pointer += 2
	elif compute_type == TYPE_QUATERNION:
		buffer.bytes.resize(buffer.bytes.size() + 8)
		buffer.bytes.encode_half(buffer.pointer, var_val.x)
		buffer.pointer += 2
		buffer.bytes.encode_half(buffer.pointer, var_val.y)
		buffer.pointer += 2
		buffer.bytes.encode_half(buffer.pointer, var_val.z)
		buffer.pointer += 2
		buffer.bytes.encode_half(buffer.pointer, var_val.w)
		buffer.pointer += 2
	elif compute_type == TYPE_FLOAT:
		buffer.bytes.resize(buffer.bytes.size() + 4)
		buffer.bytes.encode_float(buffer.pointer, var_val)
		buffer.pointer += 4
	elif compute_type == TYPE_INT:
		buffer.bytes.resize(buffer.bytes.size() + 8)
		buffer.bytes.encode_s64(buffer.pointer, var_val)
		buffer.pointer += 8
	elif compute_type == TYPE_BOOL:
		buffer.bytes.resize(buffer.bytes.size() + 1)
		buffer.bytes.encode_u8(buffer.pointer, 1 if var_val else 0)
		buffer.pointer += 1
	elif compute_type == TYPE_ARRAY:
		buffer.bytes.resize(buffer.bytes.size() + 1)
		buffer.bytes.encode_u8(buffer.pointer, var_val.size())
		buffer.pointer += 1
		for val in var_val:
			pack(buffer, val, true)
	elif compute_type == TYPE_PACKED_INT32_ARRAY:
		buffer.bytes.resize(buffer.bytes.size() + 2)
		buffer.bytes.encode_u16(buffer.pointer, var_val.size())
		buffer.pointer += 2
		for val in var_val:
			buffer.bytes.resize(buffer.bytes.size() + 4)
			buffer.bytes.encode_s32(buffer.pointer, val)
			buffer.pointer += 4
	elif compute_type == TYPE_PACKED_BYTE_ARRAY:
		buffer.bytes.resize(buffer.bytes.size() + 2)
		buffer.bytes.encode_u16(buffer.pointer, var_val.size())
		buffer.bytes.append_array(var_val)
		buffer.pointer += var_val.size() + 2
	else:
		return false
	return true

static func unpack(buffer: HLBuffer, type: Variant = null):
	var var_val = null
	if type == null:
		type = buffer.bytes.decode_u8(buffer.pointer)
		buffer.pointer += 1
	if type == TYPE_VECTOR3:
		var_val = Vector3()
		var_val.x = buffer.bytes.decode_half(buffer.pointer)
		buffer.pointer += 2
		var_val.y = buffer.bytes.decode_half(buffer.pointer)
		buffer.pointer += 2
		var_val.z = buffer.bytes.decode_half(buffer.pointer)
		buffer.pointer += 2
	elif type == TYPE_VECTOR2:
		var_val = Vector2()
		var_val.x = buffer.bytes.decode_half(buffer.pointer)
		buffer.pointer += 2
		var_val.y = buffer.bytes.decode_half(buffer.pointer)
		buffer.pointer += 2
	elif type == TYPE_QUATERNION:
		var_val = Quaternion()
		var_val.x = buffer.bytes.decode_half(buffer.pointer)
		buffer.pointer += 2
		var_val.y = buffer.bytes.decode_half(buffer.pointer)
		buffer.pointer += 2
		var_val.z = buffer.bytes.decode_half(buffer.pointer)
		buffer.pointer += 2
		var_val.w = buffer.bytes.decode_half(buffer.pointer)
		buffer.pointer += 2
	elif type == TYPE_FLOAT:
		var_val = buffer.bytes.decode_float(buffer.pointer)
		buffer.pointer += 4
	elif type == TYPE_INT:
		var_val = buffer.bytes.decode_s64(buffer.pointer)
		buffer.pointer += 8
	elif type == TYPE_BOOL:
		var_val = bool(buffer.bytes.decode_u8(buffer.pointer))
		buffer.pointer += 1
	elif type == TYPE_ARRAY:
		var size = buffer.bytes.decode_u8(buffer.pointer)
		buffer.pointer += 1
		var_val = []
		if size == 0:
			return var_val
		for i in range(size):
			var_val.push_back(unpack(buffer))
	elif type == TYPE_PACKED_INT32_ARRAY:
		var size = buffer.bytes.decode_u16(buffer.pointer)
		buffer.pointer += 2
		var_val = []
		if size == 0:
			return var_val
		for i in range(size):
			var_val.push_back(buffer.bytes.decode_s32(buffer.pointer))
			buffer.pointer += 4
	elif type == TYPE_PACKED_BYTE_ARRAY:
		var size = buffer.bytes.decode_u16(buffer.pointer)
		buffer.pointer += 2
		var_val = buffer.bytes.slice(buffer.pointer, buffer.pointer + size)
		buffer.pointer += size
	return var_val
