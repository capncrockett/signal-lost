extends Node

# This script is executed after all tests have run
# It can be used to perform cleanup or generate reports

func _init():
	print("Post-run script executed")
	
	# Get the GUT instance
	var gut = get_parent()
	
	# Print test results
	print("\n=== TEST RESULTS ===")
	print("Tests run: %d" % (gut.get_pass_count() + gut.get_fail_count() + gut.get_pending_count()))
	print("Passed: %d" % gut.get_pass_count())
	print("Failed: %d" % gut.get_fail_count())
	print("Pending: %d" % gut.get_pending_count())
	
	# Generate a report file
	var report = "# Signal Lost Test Report\n\n"
	report += "Generated: %s\n\n" % Time.get_datetime_string_from_system()
	report += "## Summary\n\n"
	report += "- Tests run: %d\n" % (gut.get_pass_count() + gut.get_fail_count() + gut.get_pending_count())
	report += "- Passed: %d\n" % gut.get_pass_count()
	report += "- Failed: %d\n" % gut.get_fail_count()
	report += "- Pending: %d\n\n" % gut.get_pending_count()
	
	# Add details of failed tests
	if gut.get_fail_count() > 0:
		report += "## Failed Tests\n\n"
		var test_scripts = gut.get_test_collector().get_test_scripts()
		
		for script in test_scripts:
			var script_instance = load(script).new()
			var test_methods = gut.get_test_collector().get_test_methods_for(script_instance)
			
			for method in test_methods:
				var result = gut.get_test_result(script, method)
				if result and result.status == 2:  # FAIL status
					report += "### %s.%s\n\n" % [script.get_file(), method]
					report += "- Error: %s\n" % result.assertion_results[0].message
					report += "- Line: %d\n\n" % result.assertion_results[0].line_number
		
	# Save the report
	var file = FileAccess.open("res://test_results.md", FileAccess.WRITE)
	if file:
		file.store_string(report)
		print("Test report saved to res://test_results.md")
	else:
		print("Failed to save test report")
