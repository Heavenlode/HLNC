class_name TickTimer

static func CreateFromSeconds(current_tick: int, seconds: float):
    return TickTimer.new(current_tick, current_tick + ceilf(seconds * NetworkRunner.TPS))

var start_tick: int
var end_tick: int

func _init(_start_tick, _end_tick):
    self.start_tick = _start_tick
    self.end_tick = _end_tick


func is_done(current_tick: int):
    return current_tick >= self.end_tick