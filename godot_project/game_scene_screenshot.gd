extends Node

var _screenshot_taken = false
var _main_scene = null

func _ready():
	print("Game Scene Screenshot Tool - Starting...")
	
	# Load and instance the main scene
	print("Loading main game scene...")
	var main_scene_resource = load("res://PixelMainScene.tscn")
	if main_scene_resource:
		_main_scene = main_scene_resource.instantiate()
		add_child(_main_scene)
		print("Main scene added to tree")
	else:
		print("Failed to load main scene")
		return
	
	# Take a screenshot after a longer delay to ensure the scene is fully loaded and rendered
	print("Waiting 5 seconds for the scene to fully render...")
	var timer = get_tree().create_timer(5.0)
	timer.timeout.connect(_take_screenshot)

func _take_screenshot():
	if _screenshot_taken:
		return
		
	_screenshot_taken = true
	
	# Get the user data directory
	var user_dir = OS.get_user_data_dir()
	print("User data directory: ", user_dir)
	
	# Create the Screenshots directory if it doesn't exist
	var screenshots_dir = user_dir + "/Screenshots"
	var dir = DirAccess.open("user://")
	if not dir.dir_exists("Screenshots"):
		dir.make_dir("Screenshots")
		print("Created directory: ", screenshots_dir)
	
	# Generate a filename with timestamp
	var datetime = Time.get_datetime_dict_from_system()
	var timestamp = "%04d%02d%02d_%02d%02d%02d" % [
		datetime.year, datetime.month, datetime.day,
		datetime.hour, datetime.minute, datetime.second
	]
	var filename = "game_screenshot_%s.png" % timestamp
	var path = "user://Screenshots/" + filename
	
	# Take a screenshot
	print("Taking screenshot...")
	var viewport = get_viewport()
	var image = viewport.get_texture().get_image()
	
	# Save the screenshot
	var err = image.save_png(path)
	
	if err == OK:
		print("Screenshot saved successfully to: ", path)
		print("Full path: ", user_dir + "/Screenshots/" + filename)
	else:
		print("Failed to save screenshot: ", err)
	
	# Quit after a short delay
	var quit_timer = get_tree().create_timer(0.5)
	quit_timer.timeout.connect(func(): get_tree().quit())
