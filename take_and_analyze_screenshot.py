#!/usr/bin/env python3
"""
Take a screenshot using Godot and analyze it.

This script takes a screenshot using Godot and then analyzes it using the
analyze_screenshot.py script.

Usage:
python take_and_analyze_screenshot.py [test_name] [description]
"""

import os
import sys
import subprocess
import json
import argparse
import time
from datetime import datetime

def find_godot_executable():
    """Find the Godot executable."""
    # Check common locations
    possible_paths = [
        r"C:\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64.exe",
        r"/Applications/Godot_mono.app/Contents/MacOS/Godot",
        "godot"
    ]

    for path in possible_paths:
        if os.path.exists(path):
            return path

    # Try to find in PATH
    try:
        result = subprocess.run(["where", "godot"], capture_output=True, text=True, check=False)
        if result.returncode == 0:
            return "godot"
    except:
        pass

    # Not found
    return None

def take_screenshot(godot_executable, test_name):
    """Take a screenshot using Godot."""
    print(f"Taking screenshot with name: {test_name}")

    # Create screenshots directory if it doesn't exist
    os.makedirs("screenshots", exist_ok=True)

    # Generate a timestamp
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    filename = f"{test_name}_{timestamp}.png"
    screenshot_path = os.path.join("screenshots", filename)

    # Run the screenshot tool
    try:
        result = subprocess.run([
            godot_executable,
            "--path", "godot_project",
            "--script", "tools/SimpleScreenshot.cs",
            test_name
        ], capture_output=True, text=True, check=False)

        print(result.stdout)
        if result.stderr:
            print(f"Error: {result.stderr}")

        # Wait a moment for the screenshot to be saved
        time.sleep(1)

        # Find the most recent screenshot
        screenshots = [os.path.join("screenshots", f) for f in os.listdir("screenshots") if f.endswith(".png")]
        if not screenshots:
            print("Error: No screenshots found.")
            return None

        latest_screenshot = max(screenshots, key=os.path.getmtime)
        print(f"Screenshot taken: {latest_screenshot}")
        return latest_screenshot
    except Exception as e:
        print(f"Error taking screenshot: {e}")
        return None

def analyze_screenshot(screenshot_path):
    """Analyze a screenshot using the analyze_screenshot.py script."""
    if not screenshot_path or not os.path.exists(screenshot_path):
        print(f"Error: Screenshot not found: {screenshot_path}")
        return None

    print(f"Analyzing screenshot: {screenshot_path}")

    # Generate output path
    output_path = os.path.splitext(screenshot_path)[0] + ".analysis.json"

    # Run the analysis script
    try:
        result = subprocess.run([
            sys.executable,
            "analyze_screenshot.py",
            screenshot_path,
            "--output", output_path,
            "--pretty"
        ], capture_output=True, text=True, check=False)

        print(result.stdout)
        if result.stderr:
            print(f"Error: {result.stderr}")

        if os.path.exists(output_path):
            print(f"Analysis saved to: {output_path}")

            # Load and print the analysis
            with open(output_path, "r") as f:
                analysis = json.load(f)

            print("\nAnalysis Summary:")
            print("================")
            print(f"Resolution: {analysis['image_size']['width']}x{analysis['image_size']['height']}")
            print(f"Average Color: R={analysis['average_color']['r']}, G={analysis['average_color']['g']}, B={analysis['average_color']['b']}")
            print(f"Is Dark Screen: {analysis['is_dark_screen']}")
            print(f"Has Red Elements: {analysis['has_red_elements']}")
            print(f"Has Green Elements: {analysis['has_green_elements']}")
            print(f"Has Blue Elements: {analysis['has_blue_elements']}")
            print(f"Potential UI Elements: {len(analysis['potential_ui_elements'])}")
            print("================")

            return output_path
        else:
            print(f"Error: Analysis file not created.")
            return None
    except Exception as e:
        print(f"Error analyzing screenshot: {e}")
        return None

def main():
    parser = argparse.ArgumentParser(description='Take a screenshot and analyze it.')
    parser.add_argument('test_name', nargs='?', default="test", help='Name of the test')
    parser.add_argument('description', nargs='?', default="Screenshot taken during E2E test", help='Description of the screenshot')

    args = parser.parse_args()

    # Find Godot executable
    godot_executable = find_godot_executable()
    if not godot_executable:
        print("Error: Godot executable not found.")
        return 1

    print(f"Using Godot executable: {godot_executable}")

    # Take screenshot
    screenshot_path = take_screenshot(godot_executable, args.test_name)
    if not screenshot_path:
        return 1

    # Create a description file
    description_file = os.path.splitext(screenshot_path)[0] + ".description.txt"
    with open(description_file, "w") as f:
        f.write(f"Test: {args.test_name}\n")
        f.write(f"Description: {args.description}\n")
        f.write(f"Date: {datetime.now()}\n")

    print(f"Description saved to: {description_file}")

    # Analyze screenshot
    analysis_path = analyze_screenshot(screenshot_path)
    if not analysis_path:
        return 1

    print("Done.")
    return 0

if __name__ == "__main__":
    sys.exit(main())
