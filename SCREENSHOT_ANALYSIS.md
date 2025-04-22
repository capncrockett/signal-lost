# Screenshot Analysis Tools

This directory contains tools for analyzing screenshots taken during E2E tests.

## Tools

### 1. analyze_screenshot.py

This is the main script for analyzing screenshots. It extracts information about colors, dimensions, and potential UI elements.

```bash
py analyze_screenshot.py <screenshot_path> [--output <output_file>] [--pretty]
```

Options:
- `--output` or `-o`: Output file for the analysis (JSON format)
- `--pretty` or `-p`: Pretty print the JSON output

### 2. analyze_existing_screenshot.py

A simple wrapper script to analyze an existing screenshot and print a summary.

```bash
py analyze_existing_screenshot.py <screenshot_path>
```

### 3. take_and_analyze_screenshot.py

A script that takes a screenshot using Godot and then analyzes it.

```bash
py take_and_analyze_screenshot.py [test_name] [description]
```

### 4. create_test_image.py

A utility script to create a test image for screenshot analysis.

```bash
py create_test_image.py
```

## Analysis Information

The analysis provides the following information:

- **Basic Information**: Filename, path, file size
- **Image Properties**: Resolution, dimensions
- **Color Analysis**: Average color, brightness, dominant colors
- **UI Elements**: Potential UI elements based on color variance

## Example Output

```json
{
  "filename": "test_screenshot.png",
  "path": "screenshots/test_screenshot.png",
  "file_size_kb": 123.45,
  "image_size": {
    "width": 1920,
    "height": 1080
  },
  "average_color": {
    "r": 50,
    "g": 50,
    "b": 51
  },
  "is_dark_screen": true,
  "has_red_elements": false,
  "has_green_elements": false,
  "has_blue_elements": false,
  "potential_ui_elements": [
    {
      "x": 300,
      "y": 200,
      "width": 200,
      "height": 50,
      "position": "0.16,0.19 to 0.26,0.23",
      "variance": 1234.56
    },
    ...
  ]
}
```

## Usage in E2E Tests

These tools can be used in E2E tests to verify the visual state of the game. For example, you can:

1. Take a screenshot before and after a UI interaction
2. Analyze both screenshots to detect changes
3. Verify that the expected changes occurred

## Requirements

- Python 3.6+
- Pillow (PIL)
- numpy

To install the required packages:

```bash
py -m pip install Pillow numpy
```

For OCR functionality (optional):
```bash
py -m pip install pytesseract
```

You also need to install Tesseract OCR for the OCR functionality to work.
