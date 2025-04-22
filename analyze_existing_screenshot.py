#!/usr/bin/env python3
"""
Analyze an existing screenshot.

This script analyzes an existing screenshot using the analyze_screenshot.py script.

Usage:
python analyze_existing_screenshot.py <screenshot_path>
"""

import os
import sys
import subprocess
import json
import argparse

def analyze_screenshot(screenshot_path):
    """Analyze a screenshot using the analyze_screenshot.py script."""
    if not os.path.exists(screenshot_path):
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
            
            # Print extracted text if available
            if analysis.get('extracted_text') and not analysis['extracted_text'].startswith("Error"):
                print(f"Extracted Text: {analysis['extracted_text']}")
            
            print("================")
            
            return output_path
        else:
            print(f"Error: Analysis file not created.")
            return None
    except Exception as e:
        print(f"Error analyzing screenshot: {e}")
        return None

def main():
    parser = argparse.ArgumentParser(description='Analyze an existing screenshot.')
    parser.add_argument('screenshot_path', help='Path to the screenshot')
    
    args = parser.parse_args()
    
    # Analyze screenshot
    analysis_path = analyze_screenshot(args.screenshot_path)
    if not analysis_path:
        return 1
    
    print("Done.")
    return 0

if __name__ == "__main__":
    sys.exit(main())
