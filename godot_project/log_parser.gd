extends Node

class_name LogParser

class LogEntry:
	enum LogLevel {INFO, WARNING, ERROR, SCRIPT_ERROR}
	
	var level: int
	var message: String
	var location: String
	var line_number: int
	var timestamp: String
	
	func _init(p_level: int, p_message: String, p_location: String = "", p_line_number: int = 0):
		level = p_level
		message = p_message
		location = p_location
		line_number = p_line_number
		timestamp = Time.get_datetime_string_from_system()
	
	func _to_string() -> String:
		var level_str = ["INFO", "WARNING", "ERROR", "SCRIPT_ERROR"][level]
		var location_str = "" if location.is_empty() else " at: " + location
		var line_str = "" if line_number == 0 else " line: " + str(line_number)
		return "[%s] %s%s%s" % [level_str, message, location_str, line_str]

static func parse_latest_log() -> Array[LogEntry]:
	var log_entries: Array[LogEntry] = []
	var logs_dir = "res://logs"
	
	# Ensure the directory exists
	var dir = DirAccess.open("res://")
	if not dir.dir_exists("logs"):
		print("Logs directory not found: ", logs_dir)
		return log_entries
	
	# Get the latest log file
	dir = DirAccess.open(logs_dir)
	var log_files = []
	dir.list_dir_begin()
	var file_name = dir.get_next()
	while file_name != "":
		if not dir.current_is_dir() and file_name.begins_with("godot_log_") and file_name.ends_with(".log"):
			log_files.append(logs_dir + "/" + file_name)
		file_name = dir.get_next()
	dir.list_dir_end()
	
	if log_files.is_empty():
		print("No log files found")
		return log_entries
	
	# Sort by modification time (newest first)
	log_files.sort_custom(func(a, b): 
		var file_a = FileAccess.get_modified_time(a)
		var file_b = FileAccess.get_modified_time(b)
		return file_a > file_b
	)
	
	var latest_log_file = log_files[0]
	print("Parsing log file: ", latest_log_file)
	
	# Parse the log file
	var file = FileAccess.open(latest_log_file, FileAccess.READ)
	if file:
		while not file.eof_reached():
			var line = file.get_line()
			var entry = _parse_log_line(line)
			if entry:
				log_entries.append(entry)
	
	return log_entries

static func _parse_log_line(line: String) -> LogEntry:
	# Parse ERROR lines
	if line.contains("ERROR:"):
		var parts = line.split("ERROR:", true, 1)
		if parts.size() > 1:
			var message_parts = parts[1].strip_edges().split(" at: ", true, 1)
			var message = message_parts[0].strip_edges()
			var location = ""
			var line_number = 0
			
			if message_parts.size() > 1:
				location = message_parts[1].strip_edges()
				# Try to extract line number if present
				var line_match = location.find(":")
				if line_match != -1:
					var line_str = location.substr(line_match + 1).strip_edges()
					if line_str.is_valid_int():
						line_number = line_str.to_int()
			
			return LogEntry.new(LogEntry.LogLevel.ERROR, message, location, line_number)
	
	# Parse SCRIPT ERROR lines
	elif line.contains("SCRIPT ERROR:"):
		var parts = line.split("SCRIPT ERROR:", true, 1)
		if parts.size() > 1:
			var message_parts = parts[1].strip_edges().split(" at: ", true, 1)
			var message = message_parts[0].strip_edges()
			var location = ""
			var line_number = 0
			
			if message_parts.size() > 1:
				location = message_parts[1].strip_edges()
				# Try to extract line number if present
				var line_match = location.find(":")
				if line_match != -1:
					var line_str = location.substr(line_match + 1).strip_edges()
					if line_str.is_valid_int():
						line_number = line_str.to_int()
			
			return LogEntry.new(LogEntry.LogLevel.SCRIPT_ERROR, message, location, line_number)
	
	# Parse WARNING lines
	elif line.contains("WARNING:"):
		var parts = line.split("WARNING:", true, 1)
		if parts.size() > 1:
			var message_parts = parts[1].strip_edges().split(" at: ", true, 1)
			var message = message_parts[0].strip_edges()
			var location = ""
			var line_number = 0
			
			if message_parts.size() > 1:
				location = message_parts[1].strip_edges()
				# Try to extract line number if present
				var line_match = location.find(":")
				if line_match != -1:
					var line_str = location.substr(line_match + 1).strip_edges()
					if line_str.is_valid_int():
						line_number = line_str.to_int()
			
			return LogEntry.new(LogEntry.LogLevel.WARNING, message, location, line_number)
	
	return null

static func print_error_summary():
	var entries = parse_latest_log()
	
	print("Found ", entries.size(), " log entries")
	
	var errors = entries.filter(func(entry): return entry.level == LogEntry.LogLevel.ERROR or entry.level == LogEntry.LogLevel.SCRIPT_ERROR)
	var warnings = entries.filter(func(entry): return entry.level == LogEntry.LogLevel.WARNING)
	
	print("Errors: ", errors.size(), ", Warnings: ", warnings.size())
	
	if errors.size() > 0:
		print("\n=== ERRORS ===")
		for error in errors:
			print(error)
	
	if warnings.size() > 0:
		print("\n=== WARNINGS ===")
		for warning in warnings:
			print(warning)
