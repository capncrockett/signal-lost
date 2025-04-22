extends Node

# This script has been deprecated.
# Please use the Python-based screenshot analysis tools instead:
# - analyze_screenshot.py
# - analyze_existing_screenshot.py
# - take_and_analyze_screenshot.py

func _ready():
	print("Game Scene Screenshot Tool - DEPRECATED")
	print("Please use the Python-based screenshot analysis tools instead.")
	print("See SCREENSHOT_ANALYSIS.md for more information.")

	# Quit after a short delay
	var quit_timer = get_tree().create_timer(0.5)
	quit_timer.timeout.connect(func(): get_tree().quit())
