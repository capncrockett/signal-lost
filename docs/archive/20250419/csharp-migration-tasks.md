# C# Migration Tasks

This document outlines the remaining tasks needed to complete the migration from GDScript to C#.

## Scene Files

The scene files have been updated to reference C# scripts, but there are issues with namespaces. C# classes are in the `SignalLost` or `SignalLost.Tests` namespaces, but the scene files are looking for classes without namespaces.

### Tasks:

- [x] Update the scene files to properly reference the C# classes with their namespaces
- [x] Add GlobalClass attribute to all C# classes
- [ ] Test each scene to ensure it loads correctly
- [ ] Fix any issues with script references

## Test Runner

The C# test runner is failing because it can't find the classes. This is likely due to the namespace issue.

### Tasks:

- [x] Update the test runner to properly handle namespaces
- [x] Add GlobalClass attribute to test classes
- [ ] Fix any issues with test class discovery
- [ ] Ensure all tests can be run from the command line

## C# Implementations

We have the following C# implementations:

- `AudioManager.cs`
- `AudioVisualizer.cs`
- `GameState.cs`
- `RadioTuner.cs`

### Tasks:

- [x] Add GlobalClass attribute to all C# classes
- [ ] Verify that all necessary GDScript functionality has been migrated to C#
- [ ] Add any missing C# implementations
- [ ] Test each implementation to ensure it works correctly

## Project Settings

There may be project settings that need to be updated for C#.

### Tasks:

- [ ] Check if there are any project settings that need to be updated
- [ ] Update the build configuration for C#
- [ ] Ensure the project can be built and run with C#

## Documentation

The documentation has been updated to reflect the C# migration, but there may be additional updates needed.

### Tasks:

- [x] Create csharp-migration.md with details on the migration process
- [x] Create files-to-remove.md with a list of files to remove
- [x] Update README.md with C# migration information
- [ ] Add examples of how to use the C# classes

## Testing

We need to ensure that all functionality works correctly with the C# implementation.

### Tasks:

- [ ] Run all tests to ensure they pass
- [ ] Test the game manually to ensure all features work
- [ ] Fix any issues that arise during testing

## Cleanup

We've removed most GDScript files and cleaned up gitignored files, but there may be additional cleanup needed.

### Tasks:

- [x] Remove all GDScript files (except in addons)
- [x] Clean up gitignored files
- [x] Create cleanup scripts for both Linux/Mac and Windows
- [ ] Ensure the repository is clean and organized

## Next Steps

After completing these tasks, the migration to C# will be complete. The next steps would be to continue development using C#.

### Tasks:

- [ ] Plan the next features to implement in C#
- [ ] Set up a development workflow for C#
- [ ] Train team members on C# development in Godot
