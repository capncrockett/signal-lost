# Radio Dial Fix Plan

## Current Issues

1. The radio dial "flies all over the place" making it difficult to control
2. The slider works well but the dial needs improvement
3. Audio feedback is missing (no static, beeps, or radio sounds)
4. Visual feedback for signal detection is inadequate

## Root Causes

After examining the code in `PixelRadioInterface.cs`, the following issues were identified:

1. **Dial Sensitivity**: The current implementation calculates rotation based on mouse movement with a multiplier that's too high, causing erratic behavior.
2. **Angle Calculation**: The angle calculation in the `_GuiInput` method doesn't properly handle the circular nature of the dial.
3. **Audio Integration**: The audio system is implemented but not properly connected to the UI.
4. **Visual Feedback**: Signal detection visualization needs improvement.

## Fix Implementation Plan

### 1. Fix Radio Dial Control

```csharp
// Current problematic code in _GuiInput method:
if (_isDraggingKnob)
{
    // Calculate rotation based on mouse movement
    Vector2 knobCenter = new Vector2(
        _knobRect.Position.X + _knobRect.Size.X / 2,
        _knobRect.Position.Y + _knobRect.Size.Y / 2
    );

    Vector2 prevDir = (_lastMousePosition - knobCenter).Normalized();
    Vector2 newDir = (mousePos - knobCenter).Normalized();

    float angle = Mathf.Atan2(
        newDir.Y - prevDir.Y,
        newDir.X - prevDir.X
    );

    // Convert angle to frequency change
    float angleInDegrees = Mathf.RadToDeg(angle);
    float frequencyChange = angleInDegrees * 0.05f;

    // Apply frequency change
    ChangeFrequency(frequencyChange);

    _lastMousePosition = mousePos;
}
```

#### Proposed Fix:

1. Replace the angle calculation with a more intuitive approach:
   - Calculate the angle between the knob center and the current mouse position
   - Compare with the previous angle to determine rotation direction
   - Apply a dampening factor to make the control less sensitive

2. Add a "snap" feature to help users find frequencies more easily

3. Implement a maximum rotation speed to prevent erratic movement

```csharp
// Improved implementation
if (_isDraggingKnob)
{
    // Calculate rotation based on mouse position relative to knob center
    Vector2 knobCenter = new Vector2(
        _knobRect.Position.X + _knobRect.Size.X / 2,
        _knobRect.Position.Y + _knobRect.Size.Y / 2
    );

    // Calculate previous and current angles
    float prevAngle = Mathf.Atan2(_lastMousePosition.Y - knobCenter.Y, _lastMousePosition.X - knobCenter.X);
    float newAngle = Mathf.Atan2(mousePos.Y - knobCenter.Y, mousePos.X - knobCenter.X);
    
    // Calculate angle difference (handle wrapping around 2π)
    float angleDiff = newAngle - prevAngle;
    if (angleDiff > Mathf.Pi) angleDiff -= Mathf.Pi * 2;
    if (angleDiff < -Mathf.Pi) angleDiff += Mathf.Pi * 2;
    
    // Apply dampening factor to make control less sensitive
    float dampening = 0.3f;
    float frequencyChange = Mathf.RadToDeg(angleDiff) * dampening * 0.01f;
    
    // Limit maximum change per frame
    frequencyChange = Mathf.Clamp(frequencyChange, -0.2f, 0.2f);
    
    // Apply frequency change
    ChangeFrequency(frequencyChange);
    
    _lastMousePosition = mousePos;
}
```

### 2. Implement Audio Feedback

1. Ensure the AudioManager is properly initialized and connected
2. Add audio feedback for different actions:
   - Static noise when no signal is detected
   - Beeps or tones when a signal is found
   - Transition sounds when changing frequencies

```csharp
// In _Process method, after updating state
if (_radioSystem != null && _gameState != null)
{
    // Update audio based on signal strength
    if (_isPowerOn)
    {
        if (_signalStrength > 0.1f)
        {
            // Play signal sound with volume based on strength
            _radioSystem.PlaySignalSound(_currentFrequency, _signalStrength);
        }
        else
        {
            // Play static noise
            _radioSystem.PlayStaticNoise(1.0f - _signalStrength);
        }
    }
    else
    {
        // Ensure audio is stopped when radio is off
        _radioSystem.StopAllSounds();
    }
}
```

### 3. Improve Visual Feedback

1. Enhance the signal strength meter to be more responsive
2. Add visual effects for signal detection:
   - Pulsing glow when a signal is detected
   - Color changes based on signal strength
   - Visual noise effect that correlates with audio static

```csharp
// In DrawSignalStrengthMeter method
private void DrawSignalStrengthMeter()
{
    // ... existing code ...
    
    // Draw meter fill with color based on signal strength
    if (_isPowerOn)
    {
        Color meterColor = SignalMeterColor;
        
        // Change color based on signal strength
        if (_signalStrength > 0.7f)
        {
            // Strong signal - bright green
            meterColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        }
        else if (_signalStrength > 0.3f)
        {
            // Medium signal - yellow-green
            meterColor = new Color(0.5f, 0.8f, 0.0f, 1.0f);
        }
        else if (_signalStrength > 0.1f)
        {
            // Weak signal - yellow
            meterColor = new Color(0.8f, 0.8f, 0.0f, 1.0f);
        }
        else
        {
            // No signal - gray
            meterColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        }
        
        float fillWidth = meterWidth * _signalStrength;
        DrawRect(new Rect2(meterX, meterY, fillWidth, meterHeight), meterColor);
        
        // Add pulsing effect for strong signals
        if (_signalStrength > 0.7f)
        {
            float pulseIntensity = (float)Math.Sin(Time.GetTicksMsec() / 200.0) * 0.2f + 0.8f;
            DrawRect(new Rect2(meterX, meterY, fillWidth, meterHeight), 
                new Color(meterColor.R, meterColor.G, meterColor.B, pulseIntensity));
        }
    }
    
    // ... rest of the method ...
}
```

### 4. Add Frequency Snapping

Implement a feature to help users find signals more easily by adding subtle "snapping" to frequencies where signals exist:

```csharp
// In ChangeFrequency method
private void ChangeFrequency(float amount)
{
    if (_gameState == null || _radioSystem == null) return;

    float newFreq = _currentFrequency + amount;
    newFreq = Mathf.Clamp(newFreq, _minFrequency, _maxFrequency);
    
    // Check if we're close to a signal frequency
    var nearbySignal = _radioSystem.FindSignalAtFrequency(newFreq);
    if (nearbySignal != null)
    {
        float distance = Math.Abs(newFreq - nearbySignal.Frequency);
        if (distance < 0.2f)
        {
            // Apply subtle snapping effect
            float snapStrength = 1.0f - (distance / 0.2f);
            snapStrength *= 0.5f; // Reduce snap strength to make it subtle
            newFreq = Mathf.Lerp(newFreq, nearbySignal.Frequency, snapStrength);
        }
    }
    
    newFreq = Mathf.Snapped(newFreq, 0.1f);  // Round to nearest 0.1
    _gameState.SetFrequency(newFreq);
}
```

## Testing Plan

1. Test the radio dial control with different mouse movement speeds
2. Verify that the dial responds predictably to user input
3. Test audio feedback to ensure it correlates with signal strength
4. Verify visual feedback provides clear indication of signal detection
5. Test frequency snapping to ensure it helps find signals without being too aggressive
6. Test on both Windows and Mac platforms to ensure cross-platform compatibility

## Implementation Steps

1. Implement the radio dial control fix
2. Test and refine the control behavior
3. Implement audio feedback improvements
4. Implement visual feedback enhancements
5. Add frequency snapping feature
6. Perform comprehensive testing
7. Document the changes and update user documentation

## Success Criteria

1. The radio dial responds predictably to user input
2. Users can easily tune to specific frequencies
3. Audio feedback provides clear indication of signal detection
4. Visual feedback enhances the user experience
5. The overall radio tuning experience feels intuitive and satisfying
