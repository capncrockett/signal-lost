extends Node

# This script is a wrapper for the AudioManager C# class
# It will be used as an autoload

var _audio_manager = null

func _ready():
	print("AudioManagerWrapper is initializing...")
	
	# Try to load the C# script
	var audio_manager_script = load("res://scripts/AudioManager.cs")
	if audio_manager_script:
		print("AudioManager script loaded successfully")
		
		# Try to instantiate AudioManager
		_audio_manager = audio_manager_script.new()
		if _audio_manager:
			print("AudioManager instantiated successfully")
			add_child(_audio_manager)
		else:
			print("ERROR: Failed to instantiate AudioManager")
	else:
		print("ERROR: Failed to load AudioManager script")

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
