using Godot;
using System;
using System.Threading.Tasks;

namespace SignalLost.Tests
{
    public partial class TestRunner : SceneTree
    {
        // This script runs all tests from the command line
        // Usage: godot --path /path/to/project --script Tests/TestRunner.cs

        public override void _Initialize()
        {
            GD.Print("Starting test runner...");
            
            // Check if GUT is installed
            var gutDir = new DirAccess();
            if (!DirAccess.DirExistsAbsolute("res://addons/gut"))
            {
                GD.Print("ERROR: GUT addon not found. Please install it from the Asset Library.");
                Quit(1);
                return;
            }
            
            // Create GUT instance
            var gutScript = GD.Load<Script>("res://addons/gut/gut.gd");
            var gut = (Node)gutScript.New();
            GetRoot().AddChild(gut);
            
            // Configure GUT
            gut.Call("set_yield_between_tests", true);
            gut.Call("set_log_level", 1);  // GUT.LOG_LEVEL_ALL_ASSERTS
            gut.Call("set_inner_class_name_prefix", "Test_");
            
            // Add test directory
            gut.Call("add_directory", "res://tests");
            
            // Run the tests
            GD.Print("Running tests...");
            gut.Call("test_scripts");
            
            // Wait for tests to complete
            gut.Connect("tests_finished", Callable.From(() => 
            {
                // Get results
                int failed = (int)gut.Call("get_fail_count");
                int passed = (int)gut.Call("get_pass_count");
                int pending = (int)gut.Call("get_pending_count");
                int total = failed + passed + pending;
                
                GD.Print($"Tests completed: {passed} passed, {failed} failed, {pending} pending");
                
                // Exit with appropriate code
                Quit(failed > 0 ? 1 : 0);
            }));
        }
    }
}
