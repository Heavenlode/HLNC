class_name NetworkDebug extends Node

enum Message {
	BYTES_PER_SECOND,
	PING
}

var monitor_stream: PacketPeerStream
var sock: StreamPeerTCP
func connect_to_monitor():
	# Connect to a TCP Server
	sock = StreamPeerTCP.new()
	var err = sock.connect_to_host("127.0.0.1", 8887)
	if err != OK:
		print("Error connecting to monitor log:", err)
		return
	monitor_stream = PacketPeerStream.new()
	monitor_stream.set_stream_peer(sock)

func log(data: Variant):
	if sock.get_status() != StreamPeerTCP.STATUS_CONNECTED:
		return
	monitor_stream.put_var(data)

func _process(delta):
	if sock == null:
		return
	sock.poll()