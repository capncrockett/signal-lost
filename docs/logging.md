# Logging and Error Reporting in Signal Lost

This document outlines the logging system for the Signal Lost Godot project.

## Quick Start

1. **Run with logs**: `run_with_logs.bat` (Windows) or `run_with_logs.sh` (Linux/macOS)
2. **View logs**: `read_latest_log.bat` (Windows) or `read_latest_log.sh` (Linux/macOS)
3. **Generate error report**: `run_with_error_report.bat` (Windows) or `run_with_error_report.sh` (Linux/macOS)

## How It Works

- Logs are stored in the `godot_project/logs` directory
- Log files use the format: `godot_log_YYYYMMDD-HHMMSS.log`
- Error reports use the format: `error_report_YYYYMMDD-HHMMSS.md`
- The scripts automatically filter for errors and warnings

## For AI Assistants

To share logs with AI assistants:

1. Run with error reporting: `run_with_error_report.bat`
2. Copy the error report to clipboard:
   ```bash
   # Windows
   type godot_project\logs\error_report_*.md | clip
   ```
3. Paste into the chat with the AI assistant

## Common Issues

- **Missing GUT Addon**: Install using `install_gut.bat`
- **Audio Driver Issues**: Try running with `--headless` flag
- **UID Duplicates**: Open and resave scene files in Godot Editor
- **Script Parsing Errors**: Check parent classes and dependencies

## Tips

- Run with logs during development
- Check error reports before committing code
- Clean up old logs periodically
