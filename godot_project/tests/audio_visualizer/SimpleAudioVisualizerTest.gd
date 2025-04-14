extends Node

# Simple test for the AudioVisualizer

func _ready():
	print("Starting simple AudioVisualizer test...")
	
	# Create a new AudioVisualizer instance
	var audio_visualizer = ColorRect.new()
	audio_visualizer.set_script(load("res://Scenes/Radio/AudioVisualizer.gd"))
	add_child(audio_visualizer)
	
	# Set properties
	audio_visualizer.num_bars = 32
	audio_visualizer.bar_width = 4.0
	audio_visualizer.bar_spacing = 2.0
	audio_visualizer.min_bar_height = 5.0
	audio_visualizer.max_bar_height = 100.0
	audio_visualizer.signal_color = Color(0.0, 0.8, 0.0, 1.0)
	audio_visualizer.static_color = Color(0.8, 0.8, 0.8, 1.0)
	audio_visualizer.background_color = Color(0.1, 0.1, 0.1, 1.0)
	
	# Set size
	audio_visualizer.size = Vector2(400, 100)
	
	# Call _ready to initialize
	audio_visualizer._ready()
	
	# Test setting signal strength and static intensity
	print("Testing signal strength and static intensity...")
	audio_visualizer.set_signal_strength(0.5)
	audio_visualizer.set_static_intensity(0.7)
	
	# Test that values are set correctly
	if audio_visualizer._signal_strength == 0.5:
		print("PASS: Signal strength set correctly")
	else:
		print("FAIL: Signal strength not set correctly")
	
	if audio_visualizer._static_intensity == 0.7:
		print("PASS: Static intensity set correctly")
	else:
		print("FAIL: Static intensity not set correctly")
	
	# Test clamping of values
	print("Testing clamping of values...")
	audio_visualizer.set_signal_strength(1.5)
	if audio_visualizer._signal_strength == 1.0:
		print("PASS: Signal strength clamped correctly")
	else:
		print("FAIL: Signal strength not clamped correctly")
	
	audio_visualizer.set_static_intensity(-0.5)
	if audio_visualizer._static_intensity == 0.0:
		print("PASS: Static intensity clamped correctly")
	else:
		print("FAIL: Static intensity not clamped correctly")
	
	# Test noise generation
	print("Testing noise generation...")
	var noise1 = audio_visualizer.noise_at(0, 0.0)
	var noise2 = audio_visualizer.noise_at(10, 0.0)
	
	if noise1 != noise2:
		print("PASS: Different positions give different noise values")
	else:
		print("FAIL: Different positions give same noise values")
	
	var noise3 = audio_visualizer.noise_at(0, 0.0)
	var noise4 = audio_visualizer.noise_at(0, 1.0)
	
	if noise3 != noise4:
		print("PASS: Different times give different noise values")
	else:
		print("FAIL: Different times give same noise values")
	
	# Test bar height calculation
	print("Testing bar height calculation...")
	audio_visualizer.set_signal_strength(0.0)
	audio_visualizer.set_static_intensity(0.0)
	var height1 = audio_visualizer._calculate_bar_height(0)
	
	if height1 == audio_visualizer.min_bar_height:
		print("PASS: Height is min_bar_height with no signal or static")
	else:
		print("FAIL: Height is not min_bar_height with no signal or static")
	
	audio_visualizer.set_signal_strength(1.0)
	var height2 = audio_visualizer._calculate_bar_height(0)
	
	if height2 > audio_visualizer.min_bar_height:
		print("PASS: Height is greater than min_bar_height with full signal")
	else:
		print("FAIL: Height is not greater than min_bar_height with full signal")
	
	print("AudioVisualizer test completed successfully!")
	
	# Exit the application
	get_tree().quit()
