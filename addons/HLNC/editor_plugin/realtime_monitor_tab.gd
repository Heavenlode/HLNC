@tool
extends MarginContainer

@export var bandwidth_graph: Graph2D
@export var tick_bandwidth_graph: Graph2D
@export var ping_graph: Graph2D
var bytes_plot: Graph2D.PlotItem
var bytes_tick_plot: Graph2D.PlotItem
var bytes_largest_tick_plot: Graph2D.PlotItem
var ping_plots: Dictionary

func _ready():
	bytes_plot = bandwidth_graph.add_plot_item("Bytes per Second", Color.YELLOW, 1)
	bytes_tick_plot = tick_bandwidth_graph.add_plot_item("AVG per Tick", Color.GREEN, 1)
	bytes_largest_tick_plot = tick_bandwidth_graph.add_plot_item("Largest Tick", Color.RED, 1)

var server: TCPServer
var connections: Array[StreamPeerTCP] = []
var client: StreamPeerTCP
var stream: PacketPeerStream
func start_server():
	if server == null or not server.is_listening():
		server = TCPServer.new()
		var err = server.listen(8887)
		if err != OK:
			print("Error setting up monitoring server: " + str(err))
			return
	bytes_plot.clear()
	for i in ping_plots.keys():
		ping_graph.remove_plot_item(ping_plots[i])
		ping_plots[i].clear()
	ping_plots.clear()
	bytes_tick_plot.clear()
	bytes_largest_tick_plot.clear()
	bytes_plot_count = 0
	print("Monitoring server started on port 8887")

func array_to_string(arr: Array) -> String:
	var s = ""
	for i in arr:
		s += "{0}, ".format([i])
	return s

var bytes_plot_count = 0;

func _process(_delta):
	if not Engine.is_editor_hint():
		return
	if server == null or not server.is_listening():
		return
	if server.is_connection_available(): # check if someone's trying to connect
		client = server.take_connection() # accept connection
		stream = PacketPeerStream.new()
		stream.set_stream_peer(client)
		print("Now receiving server for realtime monitoring")
	if stream == null:
		return
	client.poll()
	if client.get_status() != StreamPeerTCP.STATUS_CONNECTED:
		print("Monitoring client disconnected")
		client = null
		stream = null
		return
	if stream.get_available_packet_count() > 0:
		var val = stream.get_var() # discard the packet
		if val[0] == NetworkDebug.Message.BYTES_PER_SECOND:
			bandwidth_graph.y_max = max(bandwidth_graph.y_max, val[1])
			bandwidth_graph.x_max = bytes_plot_count + 1
			tick_bandwidth_graph.y_max = max(tick_bandwidth_graph.y_max, val[2])
			tick_bandwidth_graph.x_max = bytes_plot_count + 1
			bytes_plot.add_point(Vector2(bytes_plot_count, val[1]))
			bytes_tick_plot.add_point(Vector2(bytes_plot_count, float(val[1]) / NetworkRunner.TPS))
			bytes_largest_tick_plot.add_point(Vector2(bytes_plot_count, val[2]))
			bytes_plot_count += 1
		elif val[0] == NetworkDebug.Message.PING:
			var peer_id = val[1]
			var avg_ping = val[2] * (1000 / NetworkRunner.TPS)
			if not ping_plots.has(peer_id):
				ping_plots[peer_id] = ping_graph.add_plot_item("Peer {0}".format([peer_id]), Color.YELLOW, 1)
			var plot = ping_plots[peer_id]
			ping_graph.y_max = max(ping_graph.y_max, avg_ping)
			ping_graph.x_max = bytes_plot_count + 1
			ping_plots[peer_id].add_point(Vector2(bytes_plot_count, avg_ping))
