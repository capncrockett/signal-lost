# Signal Lost Documentation

This directory contains documentation for the Signal Lost game project, which has been migrated to the Godot Engine.

## Documentation Structure

### Godot Implementation

- [Godot Migration](./godot-migration.md): Migration plan from browser-based to Godot implementation
- [Godot Workflow](./godot-workflow.md): Development workflow for Godot
- [Godot Testing](./godot-testing.md): Testing approach for Godot
- [Godot Audio System](./godot-audio-system.md): Audio system implementation in Godot
- [Godot Cleanup Plan](./godot-cleanup-plan.md): Cleanup plan for the Godot migration

### Project Documentation

- [Todo List](./todo.md): Overall project tasks and status
- [Agent Roles](./agent-roles.md): Roles and responsibilities for Alpha and Beta agents
- [Sprint Godot Migration](./sprint-godot-migration.md): Sprint plan for the Godot migration

## Agent Collaboration

- Alpha and Beta agents work in parallel on the Godot implementation
- Alpha agent focuses on core systems and audio implementation
- Beta agent focuses on narrative and game state integration
- Daily synchronization with develop branch

## Documentation Conventions

1. **Godot Documents**: Follow the naming convention `godot-feature.md`
2. **Checkmarks**: Use ✅ for completed items and ⬜ for pending items
3. **Code Examples**: Include relevant C# code examples with proper syntax highlighting
4. **Screenshots**: Include screenshots where helpful, stored in the `assets` directory
5. **Links**: Use relative links to reference other documents or code files

## Updating Documentation

When updating documentation:

1. Keep the documentation up to date with current progress
2. Update the README.md with any new documentation files
3. Ensure all links remain valid
4. Maintain consistent formatting and style
5. Focus on Godot-specific implementation details
