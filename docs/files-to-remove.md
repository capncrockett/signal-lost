# Files to Remove

The following files should be removed as part of the C# migration:

## GDScript Files

### Core Scripts
- `godot_project/scripts/AudioManagerWrapper.gd`
- `godot_project/scripts/GameStateWrapper.gd`
- `godot_project/scripts/FixedGameState.gd`

### Scene Scripts
- `godot_project/scenes/radio/AudioVisualizer.gd`
- `godot_project/scenes/radio/RadioTuner.gd`

### Test Scripts
- `godot_project/tests/unit/test_audio_manager.gd`
- `godot_project/tests/unit/test_audio_visualizer.gd`
- `godot_project/tests/unit/test_game_state.gd`
- `godot_project/tests/unit/test_radio_tuner.gd`
- `godot_project/tests/integration/test_radio_tuner_integration.gd`
- `godot_project/tests/scripts/base_test.gd`

## Other Files

- Any `.import` files associated with the removed GDScript files
- Any `.tres` files that reference the removed GDScript files

# Note

Before removing these files, ensure that all functionality has been properly migrated to C# equivalents. The C# scripts should be tested thoroughly to ensure they provide the same functionality as the GDScript versions.
