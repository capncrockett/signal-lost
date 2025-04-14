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

	# Fallback to GDScript test runner if C# runner is not available
	print("Falling back to GDScript test runner")

	# Check if GUT is installed
	if not DirAccess.dir_exists_absolute("res://addons/gut"):
		print("ERROR: GUT addon not found. Please install it from the Asset Library.")
		quit(1)
		return

	# Create GUT instance
	var gut_script = load("res://addons/gut/gut.gd")
	var gut = gut_script.new()
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

		# Exit with appropriate code
		quit(1 if failed > 0 else 0)
	)
