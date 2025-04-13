extends Node

# This script is a wrapper for the AudioManager C# class
# It will be used as an autoload

var _audio_manager = null

func _ready():
	print("AudioManagerWrapper is initializing...")

	# Since we can't directly instantiate C# scripts from GDScript,
	# we'll create a dummy implementation for testing

	# Create a basic object to simulate AudioManager
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
	if _audio_manager:
		_audio_manager.SetVolume(vol)
		return true
	return false

func toggle_mute():
	if _audio_manager:
		_audio_manager.ToggleMute()
		return true
	return false

func play_static_noise(intensity):
	if _audio_manager:
		_audio_manager.PlayStaticNoise(intensity)
		return true
	return false

func stop_static_noise():
	if _audio_manager:
		_audio_manager.StopStaticNoise()
		return true
	return false

func play_signal(frequency, volume_scale = 1.0, waveform = "sine"):
	return _audio_manager.PlaySignal(frequency, volume_scale, waveform) if _audio_manager else null

func stop_signal():
	if _audio_manager:
		_audio_manager.StopSignal()
		return true
	return false

func play_effect(effect_name):
	if _audio_manager:
		_audio_manager.PlayEffect(effect_name)
		return true
	return false

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
