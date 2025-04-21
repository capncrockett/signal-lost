# Script Deprecation Plan

This document outlines the plan for deprecating the old shell scripts (.sh) and batch files (.bat) in favor of the new consolidated scripts.

## Timeline

1. **Phase 1: Introduction (Current)** 
   - New scripts are added to the repository
   - Documentation is provided in `docs/scripts.md`
   - Old scripts continue to work as before

2. **Phase 2: Deprecation Warning (Next 2 weeks)**
   - Add deprecation warnings to all old scripts
   - Update READMEs to point to the new scripts
   - Encourage developers to start using the new scripts

3. **Phase 3: Removal (After 1 month)**
   - Remove all deprecated scripts
   - Update all documentation to reference only the new scripts

## Scripts to be Deprecated

### Root Directory Scripts

- `cleanup-docs.sh`
- `cleanup-gdscript.sh`
- `cleanup_gitignored.sh` / `cleanup_gitignored.bat`
- `fix-cross-platform.sh`
- `pixel_drawing_screenshot.sh`
- `run_csharp_tests.sh`
- `run_game.sh`
- `run_game_flow_test.sh`
- `run_game_with_radio.sh`
- `run_main_game.sh`
- `run_pixel_radio_interface.sh`
- `run_radio_audio_demo.sh`
- `run_radio_demo.sh`
- `run_radio_dial.sh`
- `run_radio_integration_test.sh`
- `run_radio_interface_demo.sh`
- `run_radio_main.sh`
- `run_radio_test.sh`
- `run_screenshot_test.sh`
- `run_simple_radio_test.sh`
- `take_pixel_drawing_screenshot.sh`
- `take_screenshot.sh` / `take_screenshot.bat`

### Godot Project Scripts

- `godot_project/install_gut.sh` / `godot_project/install_gut.bat`
- `godot_project/read_latest_log.sh` / `godot_project/read_latest_log.bat`
- `godot_project/run_audio_visualizer_test.sh` / `godot_project/run_audio_visualizer_test.bat`
- `godot_project/run_csharp_tests.sh` / `godot_project/run_csharp_tests.bat`
- `godot_project/run_custom_tests.sh` / `godot_project/run_custom_tests.bat`
- `godot_project/run_game.sh` / `godot_project/run_game.bat`
- `godot_project/run_gut_tests.sh`
- `godot_project/run_integration_tests.sh` / `godot_project/run_integration_tests.bat`
- `godot_project/run_pixel_drawing_demo.sh` / `godot_project/run_pixel_drawing_demo.bat`
- `godot_project/run_pixel_inventory_test.sh` / `godot_project/run_pixel_inventory_test.bat`
- `godot_project/run_pixel_map_test.sh` / `godot_project/run_pixel_map_test.bat`
- `godot_project/run_pixel_visualization_sandbox.sh` / `godot_project/run_pixel_visualization_sandbox.bat`
- `godot_project/run_project.sh` / `godot_project/run_project_windows.bat`
- `godot_project/run_radio_test.sh` / `godot_project/run_radio_test.bat` / `godot_project/run_radio_test_windows.bat`
- `godot_project/run_tests.sh` / `godot_project/run_tests.bat` / `godot_project/run_tests_windows.bat`
- `godot_project/run_with_error_report.sh` / `godot_project/run_with_error_report.bat`
- `godot_project/run_with_logs.sh` / `godot_project/run_with_logs.bat`
- `godot_project/take_game_screenshot.sh`

## Implementation of Deprecation Warnings

For shell scripts (.sh), add the following at the beginning of each script:

```bash
#!/bin/bash
echo "WARNING: This script is deprecated and will be removed in the future."
echo "Please use the new consolidated scripts in the 'scripts' directory."
echo "See 'docs/scripts.md' for more information."
echo ""
```

For batch files (.bat), add the following at the beginning of each script:

```batch
@echo off
echo WARNING: This script is deprecated and will be removed in the future.
echo Please use the new consolidated scripts in the 'scripts' directory.
echo See 'docs/scripts.md' for more information.
echo.
```

## Migration Guide

See `docs/scripts.md` for a comprehensive guide on how to migrate from the old scripts to the new consolidated scripts.
