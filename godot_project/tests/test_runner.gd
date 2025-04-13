extends SceneTree

# This script runs all tests from the command line
# Usage: godot --path /path/to/project --script tests/test_runner.gd

func _init():
    print("Starting test runner...")
    
    # Check if GUT is installed
    var gut_dir = Directory.new()
    if not gut_dir.dir_exists("res://addons/gut"):
        print("ERROR: GUT addon not found. Please install it from the Asset Library.")
        quit(1)
        return
    
    # Create GUT instance
    var gut = load("res://addons/gut/gut.gd").new()
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
    yield(gut, "tests_finished")
    
    # Get results
    var failed = gut.get_fail_count()
    var passed = gut.get_pass_count()
    var pending = gut.get_pending_count()
    var total = failed + passed + pending
    
    print("Tests completed: %d passed, %d failed, %d pending" % [passed, failed, pending])
    
    # Exit with appropriate code
    quit(1 if failed > 0 else 0)
