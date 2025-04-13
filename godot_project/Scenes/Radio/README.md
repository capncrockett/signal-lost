# Radio Tuner Implementation

This directory contains the implementation of the radio tuner for the Signal Lost game.

## Scene Structure

The RadioTuner scene is structured as follows:

- **RadioTuner** (Control)
  - **Background** (TextureRect)
  - **FrequencyDisplay** (Label)
  - **PowerButton** (Button)
  - **FrequencySlider** (HSlider)
  - **SignalStrengthMeter** (ProgressBar)
  - **StaticVisualization** (TextureRect)
  - **MessageContainer** (Control)
    - **MessageButton** (Button)
    - **MessageDisplay** (Control)
  - **ScanButton** (Button)
  - **TuneDownButton** (Button)
  - **TuneUpButton** (Button)
  - **RadioDial** (TextureRect)
  - **RadioKnob** (TextureRect)

## Implementation Details

The radio tuner is implemented using C# and Godot's UI controls. The main script is `RadioTuner.cs`, which handles the following functionality:

- Tuning the radio to different frequencies
- Detecting signals at specific frequencies
- Displaying signal strength
- Playing static noise and signal audio
- Scanning for signals
- Displaying messages associated with signals

## Required Assets

The following assets are needed for the radio tuner:

- `radio_background.png` - Background image for the radio
- `radio_dial.png` - Image for the radio dial
- `radio_knob.png` - Image for the tuning knob
- `static_overlay.png` - Overlay for the static visualization

## How to Use

1. Open the RadioTuner scene in the Godot Editor
2. Press the power button to turn the radio on
3. Use the frequency slider or tune buttons to change the frequency
4. When a signal is detected, the signal strength meter will increase
5. If a message is associated with the signal, the message button will become enabled
6. Press the message button to view the message
7. Press the scan button to automatically scan for signals

## Testing

The radio tuner can be tested using the `RadioTunerTests.cs` script in the `Tests` directory. The tests cover the following functionality:

- Power button functionality
- Frequency change
- Signal detection
- Scanning functionality
- Message display
- Radio behavior when turned off
