extends BaseTest

# Unit tests for the AudioVisualizer

var _audio_visualizer = null

func before_each():
	# Call parent before_each
	super.before_each()
	
	# Create a new AudioVisualizer instance
	_audio_visualizer = ColorRect.new()
	_audio_visualizer.set_script(load("res://Scenes/Radio/AudioVisualizer.gd"))
	add_child_autofree(_audio_visualizer)
	
	# Set default properties
	_audio_visualizer.num_bars = 32
	_audio_visualizer.bar_width = 4.0
	_audio_visualizer.bar_spacing = 2.0
	_audio_visualizer.min_bar_height = 5.0
	_audio_visualizer.max_bar_height = 100.0
	_audio_visualizer.signal_color = Color(0.0, 0.8, 0.0, 1.0)
	_audio_visualizer.static_color = Color(0.8, 0.8, 0.8, 1.0)
	_audio_visualizer.background_color = Color(0.1, 0.1, 0.1, 1.0)
	
	# Set size
	_audio_visualizer.size = Vector2(400, 100)
	
	# Call _ready to initialize
	_audio_visualizer._ready()

func after_each():
	# Call parent after_each
	super.after_each()
	
	# Clean up
	if _audio_visualizer:
		_audio_visualizer.queue_free()
		_audio_visualizer = null

func test_initialization():
	# Test that the visualizer initializes correctly
	assert_eq(_audio_visualizer.num_bars, 32, "Number of bars should be 32")
	assert_eq(_audio_visualizer.color, Color(0.1, 0.1, 0.1, 1.0), "Background color should be set")
	assert_eq(_audio_visualizer._bar_heights.size(), 32, "Bar heights array should be initialized")

func test_set_signal_strength():
	# Test setting signal strength
	_audio_visualizer.set_signal_strength(0.5)
	assert_eq(_audio_visualizer._signal_strength, 0.5, "Signal strength should be set to 0.5")
	
	# Test clamping of signal strength
	_audio_visualizer.set_signal_strength(1.5)
	assert_eq(_audio_visualizer._signal_strength, 1.0, "Signal strength should be clamped to 1.0")
	
	_audio_visualizer.set_signal_strength(-0.5)
	assert_eq(_audio_visualizer._signal_strength, 0.0, "Signal strength should be clamped to 0.0")

func test_set_static_intensity():
	# Test setting static intensity
	_audio_visualizer.set_static_intensity(0.7)
	assert_eq(_audio_visualizer._static_intensity, 0.7, "Static intensity should be set to 0.7")
	
	# Test clamping of static intensity
	_audio_visualizer.set_static_intensity(1.5)
	assert_eq(_audio_visualizer._static_intensity, 1.0, "Static intensity should be clamped to 1.0")
	
	_audio_visualizer.set_static_intensity(-0.5)
	assert_eq(_audio_visualizer._static_intensity, 0.0, "Static intensity should be clamped to 0.0")

func test_calculate_bar_height():
	# Test with no signal or static
	_audio_visualizer.set_signal_strength(0.0)
	_audio_visualizer.set_static_intensity(0.0)
	var height = _audio_visualizer._calculate_bar_height(0)
	assert_eq(height, _audio_visualizer.min_bar_height, "Height should be min_bar_height with no signal or static")
	
	# Test with full signal
	_audio_visualizer.set_signal_strength(1.0)
	_audio_visualizer.set_static_intensity(0.0)
	height = _audio_visualizer._calculate_bar_height(0)
	assert_gt(height, _audio_visualizer.min_bar_height, "Height should be greater than min_bar_height with full signal")
	
	# Test with full static
	_audio_visualizer.set_signal_strength(0.0)
	_audio_visualizer.set_static_intensity(1.0)
	height = _audio_visualizer._calculate_bar_height(0)
	assert_gt(height, _audio_visualizer.min_bar_height, "Height should be greater than min_bar_height with full static")

func test_noise_at():
	# Test that noise_at returns a value between 0 and 1
	var noise = _audio_visualizer.noise_at(0, 0.0)
	assert_between(noise, 0.0, 1.0, "Noise value should be between 0 and 1")
	
	# Test that different positions give different noise values
	var noise1 = _audio_visualizer.noise_at(0, 0.0)
	var noise2 = _audio_visualizer.noise_at(10, 0.0)
	assert_ne(noise1, noise2, "Different positions should give different noise values")
	
	# Test that different times give different noise values
	var noise3 = _audio_visualizer.noise_at(0, 0.0)
	var noise4 = _audio_visualizer.noise_at(0, 1.0)
	assert_ne(noise3, noise4, "Different times should give different noise values")
