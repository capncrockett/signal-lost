extends Node

class_name ErrorReportViewer

signal errors_loaded(error_count: int, warning_count: int)

var _errors: Array[String] = []
var _warnings: Array[String] = []

func _ready():
	load_latest_error_report()

func load_latest_error_report():
	var logs_dir = "res://logs"
	
	# Ensure the directory exists
	var dir = DirAccess.open("res://")
	if not dir.dir_exists("logs"):
		print("Logs directory not found: ", logs_dir)
		return
	
	# Get the latest error report
	dir = DirAccess.open(logs_dir)
	var report_files = []
	dir.list_dir_begin()
	var file_name = dir.get_next()
	while file_name != "":
		if not dir.current_is_dir() and file_name.begins_with("error_report_") and file_name.ends_with(".md"):
			report_files.append(logs_dir + "/" + file_name)
		file_name = dir.get_next()
	dir.list_dir_end()
	
	if report_files.is_empty():
		print("No error reports found")
		return
	
	# Sort by modification time (newest first)
	report_files.sort_custom(func(a, b): 
		var file_a = FileAccess.get_modified_time(a)
		var file_b = FileAccess.get_modified_time(b)
		return file_a > file_b
	)
	
	var latest_report = report_files[0]
	print("Loading error report: ", latest_report)
	
	# Parse the error report
	_errors.clear()
	_warnings.clear()
	
	var file = FileAccess.open(latest_report, FileAccess.READ)
	if file:
		var in_errors_section = false
		var in_warnings_section = false
		
		while not file.eof_reached():
			var line = file.get_line()
			
			if line.begins_with("## Errors"):
				in_errors_section = true
				in_warnings_section = false
				continue
			elif line.begins_with("## Warnings"):
				in_errors_section = false
				in_warnings_section = true
				continue
			elif line.begins_with("##"):
				in_errors_section = false
				in_warnings_section = false
				continue
			
			if in_errors_section and not line.strip_edges().is_empty():
				_errors.append(line)
			elif in_warnings_section and not line.strip_edges().is_empty():
				_warnings.append(line)
	
	print("Loaded ", _errors.size(), " errors and ", _warnings.size(), " warnings")
	errors_loaded.emit(_errors.size(), _warnings.size())

func get_errors() -> Array[String]:
	return _errors

func get_warnings() -> Array[String]:
	return _warnings

func print_summary():
	print("Error Report Summary: ", _errors.size(), " errors, ", _warnings.size(), " warnings")
	
	if _errors.size() > 0:
		print("\n=== ERRORS ===")
		for error in _errors:
			print(error)
	
	if _warnings.size() > 0:
		print("\n=== WARNINGS ===")
		for warning in _warnings:
			print(warning)
