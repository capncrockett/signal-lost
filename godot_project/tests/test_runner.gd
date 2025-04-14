extends SceneTree

# This script runs all tests from the command line
# Usage: godot --path /path/to/project --script tests/test_runner.gd

func _initialize():
	print("Starting GDScript test runner...")

	# Try to load the C# test runner
	# Note: We can't directly instantiate C# scripts from GDScript
	# So we'll check if the class exists in the global scope
	if ClassDB.class_exists("SignalLost.Tests.TestRunner"):
		print("C# TestRunner class found, but we can't instantiate it directly from GDScript")
		# Instead, we'll run the ComprehensiveTestRunner which can work with both C# and GDScript
		var scene = load("res://tests/ComprehensiveTestRunnerScene.tscn")
		if scene:
			var instance = scene.instantiate()
			get_root().add_child(instance)
			return
		else:
			print("ERROR: Could not load ComprehensiveTestRunnerScene.tscn")
			quit(1)
			return

	# Fallback to GDScript test runner if C# runner is not available
	print("Falling back to GDScript test runner")

	# Check if GUT is installed
	if not DirAccess.dir_exists_absolute("res://addons/gut"):
		print("ERROR: GUT addon not found. Please install it from the Asset Library.")
		print("Running the install_gut.sh script might fix this issue.")
		quit(1)
		return

	# Check if gut.gd exists
	if not FileAccess.file_exists("res://addons/gut/gut.gd"):
		print("ERROR: GUT addon is installed but gut.gd is missing.")
		print("Try reinstalling the GUT addon using the install_gut.sh script.")
		quit(1)
		return

	# Create GUT instance
	var gut_script = load("res://addons/gut/gut.gd")
	if gut_script == null:
		print("ERROR: Failed to load GUT script. The file exists but could not be loaded.")
		print("This might be because the project hasn't been opened in the Godot editor yet.")
		print("Try opening the project in the Godot editor first.")
		quit(1)
		return

	var gut = gut_script.new()
	if gut == null:
		print("ERROR: Failed to create GUT instance.")
		quit(1)
		return

	get_root().add_child(gut)

	# Configure GUT
	gut.set_yield_between_tests(true)
	gut.set_log_level(1)  # GUT.LOG_LEVEL_ALL_ASSERTS
	gut.set_inner_class_name_prefix("Test_")

	# Add test directory
	gut.add_directory("res://tests")

	# Run the tests
	print("Running tests...")
	gut.test_scripts()

	# Wait for tests to complete
	gut.connect("tests_finished", func():
		# Get results
		var failed = gut.get_fail_count()
		var passed = gut.get_pass_count()
		var pending = gut.get_pending_count()
		var _total = failed + passed + pending

		print("Tests completed: %d passed, %d failed, %d pending" % [passed, failed, pending])

		# Add a timer to ensure all processes are completed before exiting
		var timer = Timer.new()
		get_root().add_child(timer)
		timer.wait_time = 0.5
		timer.one_shot = true
		timer.timeout.connect(func(): quit(1 if failed > 0 else 0))
		timer.start()
	)
