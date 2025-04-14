extends Node

# This script forces the Godot process to exit after a specified time
# Useful for tests that might hang

func _ready():
	print("Force exit script loaded. Will exit in 30 seconds if tests don't complete properly.")
	
	# Create a timer to force exit
	var timer = Timer.new()
	add_child(timer)
	timer.wait_time = 30.0  # 30 seconds timeout
	timer.one_shot = true
	timer.timeout.connect(func(): 
		print("Force exiting Godot process after timeout...")
		get_tree().quit(0)
	)
	timer.start()
