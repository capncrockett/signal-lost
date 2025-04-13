# Signal Lost Assets

This directory contains all the assets for the Signal Lost game.

## Directory Structure

- **audio/** - Contains all audio files
  - **static/** - Static noise samples
  - **signals/** - Signal audio samples
  - **music/** - Background music
  - **sfx/** - Sound effects
- **images/** - Contains all image files
  - **radio/** - Radio tuner images
  - **ui/** - UI elements
  - **backgrounds/** - Background images
- **fonts/** - Contains all font files
- **maps/** - Contains all map data
- **narrative/** - Contains all narrative content

## Asset Requirements

### Radio Tuner

The radio tuner requires the following assets:

- `radio_background.png` - Background image for the radio
- `radio_dial.png` - Image for the radio dial
- `radio_knob.png` - Image for the tuning knob
- `static_overlay.png` - Overlay for the static visualization

### Audio

The audio system requires the following assets:

- Static noise samples in various intensities
- Signal audio samples for different frequencies
- Sound effects for button presses, tuning, etc.

## Asset Creation Guidelines

- All images should be in PNG format
- All audio should be in WAV format
- Images should be created at 2x the intended display size for high DPI displays
- Audio should be normalized to -3dB
- All assets should be properly attributed in the CREDITS.md file
