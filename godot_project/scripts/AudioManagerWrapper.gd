extends Node

# This script is a wrapper for the AudioManager C# class
# It will be used as an autoload

var _audio_manager = null

func _ready():
	print("AudioManagerWrapper is initializing...")

	# Try to get the C# AudioManager if it exists as an autoload
	var audio_manager_autoload = get_node_or_null("/root/AudioManager")
	if audio_manager_autoload:
		print("Found AudioManager autoload, using it")
		_audio_manager = audio_manager_autoload
		return

	# Try to instantiate the C# AudioManager class
	if ClassDB.class_exists("SignalLost.AudioManager"):
		print("Found AudioManager class, instantiating it")
		_audio_manager = Node.new()
		_audio_manager.set_script(load("res://scripts/AudioManager.cs"))
		add_child(_audio_manager)
		return

	# Fallback to dummy implementation for testing
	print("Creating dummy AudioManager for testing")
	_audio_manager = Node.new()
	_audio_manager.name = "AudioManager"
	add_child(_audio_manager)

	# Add properties
	_audio_manager.set("_volume", 0.8)
	_audio_manager.set("_isMuted", false)

	# Create audio players
	var static_player = AudioStreamPlayer.new()
	static_player.name = "StaticPlayer"
	_audio_manager.add_child(static_player)
	_audio_manager.set("_staticPlayer", static_player)

	var signal_player = AudioStreamPlayer.new()
	signal_player.name = "SignalPlayer"
	_audio_manager.add_child(signal_player)
	_audio_manager.set("_signalPlayer", signal_player)

	var effect_player = AudioStreamPlayer.new()
	effect_player.name = "EffectPlayer"
	_audio_manager.add_child(effect_player)
	_audio_manager.set("_effectPlayer", effect_player)

	# Add methods
	_audio_manager.set_script(create_script_with_methods())

	print("Dummy AudioManager created for testing")

# Forward methods to the C# instance
func set_volume(vol):
	# Use a simple fallback approach
	if _audio_manager == null:
		return false

	# Try direct method call
	if _audio_manager.has_method("SetVolume"):
		_audio_manager.call("SetVolume", vol)
		return true

	# Fallback - directly set the property
	_audio_manager.set("_volume", vol)
	return true

func toggle_mute():
	# Use a simple fallback approach
	if _audio_manager == null:
		return false

	# Try direct method call
	if _audio_manager.has_method("ToggleMute"):
		_audio_manager.call("ToggleMute")
		return true

	# Fallback - directly toggle the property
	var is_muted = _audio_manager.get("_isMuted")
	if is_muted != null:
		_audio_manager.set("_isMuted", !is_muted)
	else:
		_audio_manager.set("_isMuted", true)
	return true

func play_static_noise(intensity):
	# Use a simple fallback approach
	if _audio_manager == null:
		return false

	# Try direct method call
	if _audio_manager.has_method("PlayStaticNoise"):
		_audio_manager.call("PlayStaticNoise", intensity)
		return true

	# Fallback - directly play the static player
	var static_player = _audio_manager.get("_staticPlayer")
	if static_player and static_player is AudioStreamPlayer:
		static_player.stop()
		static_player.play()
	return true

func stop_static_noise():
	# Use a simple fallback approach
	if _audio_manager == null:
		return false

	# Try direct method call
	if _audio_manager.has_method("StopStaticNoise"):
		_audio_manager.call("StopStaticNoise")
		return true

	# Fallback - directly stop the static player
	var static_player = _audio_manager.get("_staticPlayer")
	if static_player and static_player is AudioStreamPlayer:
		static_player.stop()
	return true

func play_signal(frequency, volume_scale = 1.0, waveform = "sine"):
	# Use a simple fallback approach
	if _audio_manager == null:
		return null

	# Try direct method call
	if _audio_manager.has_method("PlaySignal"):
		return _audio_manager.call("PlaySignal", frequency, volume_scale, waveform)

	# Fallback - directly play the signal player
	var signal_player = _audio_manager.get("_signalPlayer")
	if signal_player and signal_player is AudioStreamPlayer:
		signal_player.stop()
		signal_player.play()
		return true
	return null

func stop_signal():
	# Use a simple fallback approach
	if _audio_manager == null:
		return false

	# Try direct method call
	if _audio_manager.has_method("StopSignal"):
		_audio_manager.call("StopSignal")
		return true

	# Fallback - directly stop the signal player
	var signal_player = _audio_manager.get("_signalPlayer")
	if signal_player and signal_player is AudioStreamPlayer:
		signal_player.stop()
	return true

func play_effect(effect_name):
	# Use a simple fallback approach
	if _audio_manager == null:
		return false

	# Try direct method call
	if _audio_manager.has_method("PlayEffect"):
		_audio_manager.call("PlayEffect", effect_name)
		return true

	# Fallback - directly play the effect player
	var effect_player = _audio_manager.get("_effectPlayer")
	if effect_player and effect_player is AudioStreamPlayer:
		effect_player.stop()
		effect_player.play()
	return true

# Create a script with methods to simulate the C# AudioManager class
func create_script_with_methods():
	var script = GDScript.new()
	script.source_code = """
extends Node

# Properties
var _volume = 0.8
var _isMuted = false
var _staticPlayer = null
var _signalPlayer = null
var _effectPlayer = null

# Methods
func _ready():
	# Get references to audio players
	_staticPlayer = get_node("StaticPlayer")
	_signalPlayer = get_node("SignalPlayer")
	_effectPlayer = get_node("EffectPlayer")


func SetVolume(vol):
	_volume = clamp(vol, 0.0, 1.0)


func ToggleMute():
	_isMuted = !_isMuted


func PlayStaticNoise(intensity):
	if _staticPlayer:
		_staticPlayer.stop()
		_staticPlayer.play()


func StopStaticNoise():
	if _staticPlayer:
		_staticPlayer.stop()


func PlaySignal(frequency, volume_scale = 1.0, waveform = "sine"):
	if _signalPlayer:
		_signalPlayer.stop()
		_signalPlayer.play()
		return true
	return null


func StopSignal():
	if _signalPlayer:
		_signalPlayer.stop()


func PlayEffect(effect_name):
	if _effectPlayer:
		_effectPlayer.stop()
		_effectPlayer.play()
"""
	script.reload()
	return script
