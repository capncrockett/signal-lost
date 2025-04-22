#!/usr/bin/env python3
"""
Create a test image for screenshot analysis.

This script creates a simple test image that can be used for screenshot analysis.
"""

import os
from PIL import Image, ImageDraw

def create_test_image(output_path, width=1920, height=1080):
    """Create a test image with some UI elements."""
    # Create a new image with a dark background
    image = Image.new('RGB', (width, height), (32, 33, 33))
    draw = ImageDraw.Draw(image)

    # Draw some UI elements
    # Header
    draw.rectangle([(0, 0), (width, 100)], fill=(40, 40, 40))

    # Sidebar
    draw.rectangle([(0, 100), (200, height)], fill=(45, 45, 45))

    # Content area
    draw.rectangle([(220, 120), (width - 20, height - 20)], fill=(50, 50, 50))

    # Buttons
    button_positions = [
        (300, 200, 500, 250, "Button 1", (100, 100, 200)),
        (300, 300, 500, 350, "Button 2", (200, 100, 100)),
        (300, 400, 500, 450, "Button 3", (100, 200, 100)),
        (600, 200, 800, 250, "Button 4", (150, 150, 150)),
        (600, 300, 800, 350, "Button 5", (120, 120, 180)),
    ]

    for x1, y1, x2, y2, text, color in button_positions:
        draw.rectangle([(x1, y1), (x2, y2)], fill=color, outline=(255, 255, 255))
        # Use a simpler approach for text positioning
        text_x = (x1 + x2) // 2 - 30  # Approximate text width
        text_y = (y1 + y2) // 2 - 10
        draw.text((text_x, text_y), text, fill=(255, 255, 255))

    # Radio UI element
    draw.rectangle([(900, 200), (1200, 450)], fill=(60, 60, 60), outline=(255, 255, 255))
    draw.ellipse([(950, 250), (1150, 350)], fill=(80, 80, 80), outline=(255, 255, 255))
    draw.rectangle([(950, 370), (1150, 400)], fill=(100, 100, 100), outline=(255, 255, 255))

    # Save the image
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    print(f"Saving test image to: {output_path}")
    image.save(output_path)
    print(f"Test image created: {output_path}")
    return output_path

if __name__ == "__main__":
    output_path = "screenshots/test_screenshot.png"
    create_test_image(output_path)
