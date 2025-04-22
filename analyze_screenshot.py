#!/usr/bin/env python3
"""
Screenshot Analysis Tool

This script analyzes screenshots and extracts text and basic visual information
that can be understood by Augment.

Requirements:
- Python 3.6+
- pytesseract
- Pillow (PIL)
- numpy

Installation:
pip install pytesseract Pillow numpy

You also need to install Tesseract OCR:
- Windows: https://github.com/UB-Mannheim/tesseract/wiki
- macOS: brew install tesseract
- Linux: apt-get install tesseract-ocr

Usage:
python analyze_screenshot.py path/to/screenshot.png
"""

import os
import json
import argparse
from PIL import Image
import numpy as np

try:
    import pytesseract
    HAS_TESSERACT = True
except ImportError:
    HAS_TESSERACT = False
    print("Warning: pytesseract not installed. OCR functionality will be disabled.")

def extract_text_from_image(image_path):
    """Extract text from an image using OCR."""
    if not HAS_TESSERACT:
        return "OCR not available. Install pytesseract."

    try:
        text = pytesseract.image_to_string(Image.open(image_path))
        return text
    except Exception as e:
        return f"Error extracting text: {str(e)}"

def analyze_colors(image_path):
    """Analyze the dominant colors in the image."""
    try:
        image = Image.open(image_path)
        # Resize image to speed up processing
        image = image.resize((100, 100))
        # Convert to RGB if not already
        if image.mode != 'RGB':
            image = image.convert('RGB')

        # Get image data as numpy array
        pixels = np.array(image)
        pixels = pixels.reshape(-1, 3)

        # Simple color analysis - get average color
        avg_color = pixels.mean(axis=0)

        # Check for common UI states
        is_dark = avg_color.mean() < 128
        has_red = avg_color[0] > 150 and avg_color[0] > avg_color[1] + 50 and avg_color[0] > avg_color[2] + 50
        has_green = avg_color[1] > 150 and avg_color[1] > avg_color[0] + 50 and avg_color[1] > avg_color[2] + 50
        has_blue = avg_color[2] > 150 and avg_color[2] > avg_color[0] + 50 and avg_color[2] > avg_color[1] + 50

        return {
            "average_color": {
                "r": int(avg_color[0]),
                "g": int(avg_color[1]),
                "b": int(avg_color[2])
            },
            "is_dark_screen": is_dark,
            "has_red_elements": has_red,
            "has_green_elements": has_green,
            "has_blue_elements": has_blue
        }
    except Exception as e:
        return {"error": f"Error analyzing colors: {str(e)}"}

def detect_ui_elements(image_path):
    """Detect potential UI elements in the image."""
    try:
        # This is a simplified version that doesn't use ImageFilter
        image = Image.open(image_path)
        # Convert to RGB if not already
        if image.mode != 'RGB':
            image = image.convert('RGB')

        width, height = image.size

        # Simple analysis of image regions
        # Divide the image into a grid and check for color variations
        grid_size = 10
        cell_width = width // grid_size
        cell_height = height // grid_size

        ui_elements = []

        # Analyze each grid cell for color variance (high variance might indicate UI elements)
        for i in range(grid_size):
            for j in range(grid_size):
                x1 = i * cell_width
                y1 = j * cell_height
                x2 = min((i + 1) * cell_width, width)
                y2 = min((j + 1) * cell_height, height)

                # Get the region
                region = image.crop((x1, y1, x2, y2))
                region_array = np.array(region)

                # Calculate color variance
                variance = np.var(region_array)

                # If variance is high, it might be a UI element
                if variance > 1000:  # Arbitrary threshold
                    ui_elements.append({
                        "x": x1,
                        "y": y1,
                        "width": x2 - x1,
                        "height": y2 - y1,
                        "position": f"{x1/width:.2f},{y1/height:.2f} to {x2/width:.2f},{y2/height:.2f}",
                        "variance": float(variance)
                    })

        return {
            "image_size": {"width": width, "height": height},
            "potential_ui_elements": sorted(ui_elements, key=lambda x: x["variance"], reverse=True)[:10]  # Top 10 by variance
        }
    except Exception as e:
        return {"error": f"Error detecting UI elements: {str(e)}"}

def analyze_screenshot(image_path):
    """Analyze a screenshot and return a text description."""
    if not os.path.exists(image_path):
        return {"error": f"Image file not found: {image_path}"}

    results = {
        "filename": os.path.basename(image_path),
        "path": image_path,
        "file_size_kb": os.path.getsize(image_path) / 1024,
    }

    # Extract text using OCR
    text = extract_text_from_image(image_path)
    results["extracted_text"] = text

    # Analyze colors
    color_analysis = analyze_colors(image_path)
    results.update(color_analysis)

    # Try to detect UI elements
    ui_analysis = detect_ui_elements(image_path)
    results.update(ui_analysis)

    return results

def main():
    parser = argparse.ArgumentParser(description='Analyze screenshots and extract text/visual information.')
    parser.add_argument('image_path', help='Path to the screenshot image')
    parser.add_argument('--output', '-o', help='Output file for the analysis (JSON format)')
    parser.add_argument('--pretty', '-p', action='store_true', help='Pretty print the JSON output')

    args = parser.parse_args()

    results = analyze_screenshot(args.image_path)

    # Convert any non-serializable objects to strings
    def json_serialize(obj):
        if isinstance(obj, (bool, int, float, str, list, dict, type(None))):
            return obj
        return str(obj)

    # Recursively process the results dictionary
    def process_dict(d):
        result = {}
        for k, v in d.items():
            if isinstance(v, dict):
                result[k] = process_dict(v)
            elif isinstance(v, list):
                result[k] = [process_dict(item) if isinstance(item, dict) else json_serialize(item) for item in v]
            else:
                result[k] = json_serialize(v)
        return result

    # Process the results
    processed_results = process_dict(results) if isinstance(results, dict) else results

    if args.output:
        with open(args.output, 'w') as f:
            if args.pretty:
                json.dump(processed_results, f, indent=2)
            else:
                json.dump(processed_results, f)
    else:
        if args.pretty:
            print(json.dumps(processed_results, indent=2))
        else:
            print(json.dumps(processed_results))

if __name__ == "__main__":
    main()
