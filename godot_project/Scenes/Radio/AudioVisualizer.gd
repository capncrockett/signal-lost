extends ColorRect

## Audio visualizer properties
@export var num_bars: int = 32
@export var bar_width: float = 4.0
@export var bar_spacing: float = 2.0
@export var min_bar_height: float = 5.0
@export var max_bar_height: float = 100.0
@export var signal_color: Color = Color(0.0, 0.8, 0.0, 1.0)
@export var static_color: Color = Color(0.8, 0.8, 0.8, 1.0)
@export var background_color: Color = Color(0.1, 0.1, 0.1, 1.0)

## Local state
var _bar_heights: Array = []
var _signal_strength: float = 0.0
var _static_intensity: float = 1.0
var _noise_seed: int = 0
var _time_passed: float = 0.0

## Called when the node enters the scene tree
func _ready() -> void:
	# Initialize bar heights
	_bar_heights.resize(num_bars)
	for i in range(num_bars):
		_bar_heights[i] = min_bar_height

	# Set background color
	color = background_color

	# Initialize random seed
	randomize()
	_noise_seed = randi()

## Process function called every frame
func _process(delta: float) -> void:
	_time_passed += delta
	queue_redraw()

## Custom drawing function
func _draw() -> void:
	var rect_width = size.x
	var rect_height = size.y

	# Calculate total width needed for all bars
	var total_width = num_bars * (bar_width + bar_spacing) - bar_spacing

	# Calculate starting x position to center the bars
	var start_x = (rect_width - total_width) / 2

	# Draw each bar
	for i in range(num_bars):
		# Calculate bar height based on signal and static
		var height = _calculate_bar_height(i)

		# Calculate bar position
		var x = start_x + i * (bar_width + bar_spacing)
		var y = rect_height - height

		# Calculate bar color based on signal strength and static intensity
		var bar_color = signal_color.lerp(static_color, _static_intensity)

		# Draw the bar
		draw_rect(Rect2(x, y, bar_width, height), bar_color)

## Calculate the height of a specific bar
func _calculate_bar_height(index: int) -> float:
	var height = min_bar_height

	# Signal component - smooth sine wave
	if _signal_strength > 0.0:
		var signal_freq = 2.0 + _signal_strength * 8.0  # Higher frequency with stronger signal
		var signal_phase = _time_passed * signal_freq
		var signal_value = sin(signal_phase + index * 0.2)
		# Ensure signal value is positive by using the square of the sine wave
		var signal_amplitude = signal_value * signal_value
		# Add a minimum amplitude to ensure height is always greater than min_bar_height when signal is present
		signal_amplitude = max(signal_amplitude, 0.2)
		height += signal_amplitude * _signal_strength * (max_bar_height - min_bar_height) * 0.5

	# Static component - random noise
	if _static_intensity > 0.0:
		var noise_value = noise_at(index, _time_passed)
		# Ensure noise value is at least 0.2 to make visible bars
		noise_value = max(noise_value, 0.2)
		height += noise_value * _static_intensity * (max_bar_height - min_bar_height) * 0.5

	# Ensure height is within bounds
	height = clamp(height, min_bar_height, max_bar_height)

	# Smooth transitions between frames
	var target_height = height
	var current_height = _bar_heights[index]
	var smoothing = 0.3
	height = current_height + (target_height - current_height) * smoothing

	# Update stored height
	_bar_heights[index] = height

	return height

## Generate noise value at a specific position and time
func noise_at(pos_index: int, time: float) -> float:
	var p = pos_index * 0.1
	var t = time * 2.0
	return (sin(p * 7.0 + t * 3.0) * 0.5 + 0.5) * (cos(p * 9.0 - t * 4.0) * 0.5 + 0.5)

## Set the signal strength (0.0 to 1.0)
func set_signal_strength(strength: float) -> void:
	_signal_strength = clamp(strength, 0.0, 1.0)

## Set the static intensity (0.0 to 1.0)
func set_static_intensity(intensity: float) -> void:
	_static_intensity = clamp(intensity, 0.0, 1.0)
