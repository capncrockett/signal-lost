# Signal Lost Documentation

This directory contains documentation for the Signal Lost game project, which has been migrated to the Godot Engine using C#.

## Documentation Files

- **agent-roles.md**: Defines the roles and responsibilities of Agent Alpha and Beta agents in the development process.
- **csharp-migration.md**: Details the migration from GDScript to C#.
- **files-to-remove.md**: List of files removed during the C# migration.
- **godot-audio-system.md**: Details the audio system implementation in Godot, including radio static and signal processing.
- **godot-cleanup-plan.md**: The plan for cleaning up the repository during the migration to Godot (completed).
- **godot-migration.md**: Overview of the migration process from React to Godot.
- **godot-testing.md**: Guidelines for testing the Godot implementation.
- **godot-workflow.md**: Detailed workflow for Godot development.
- **logging.md**: Documentation for the logging system.
- **sprint-godot-migration.md**: Sprint plan for the Godot migration.
- **todo.md**: Current and upcoming tasks for the project.

## Godot Project

The Godot project is located in the `godot_project` directory at the root of the repository. See the README.md file in that directory for more information about the Godot implementation.

## Agent Collaboration

- Alpha and Beta agents work in parallel on the Godot implementation
- Alpha agent focuses on core systems and audio implementation
- Beta agent focuses on narrative and game state integration
- Daily synchronization with develop branch
- All new development is done in C#

## Documentation Conventions

1. **Godot Documents**: Follow the naming convention `godot-feature.md`
2. **C# Documents**: Follow the naming convention `csharp-feature.md`
3. **Checkmarks**: Use ✅ for completed items and ⬜ for pending items
4. **Code Examples**: Include relevant C# code examples with proper syntax highlighting
5. **Screenshots**: Include screenshots where helpful, stored in the `assets` directory
6. **Links**: Use relative links to reference other documents or code files

## Updating Documentation

When updating documentation:

1. Keep the documentation up to date with current progress
2. Update the README.md with any new documentation files
3. Ensure all links remain valid
4. Maintain consistent formatting and style
5. Focus on Godot-specific implementation details
6. Document C# implementation details and patterns
7. Update test documentation to reflect C# testing approach
