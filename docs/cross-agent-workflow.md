# Cross-Agent Workflow Best Practices

This document outlines best practices for collaboration between Agent Alpha and Agent Beta in the Signal Lost development process.

## Communication Protocols

### Pull Request Process

1. **PR Title Format**: Use the format `[Alpha|Beta] Brief description of changes`
2. **PR Description Template**:
   ```
   ## Changes Made
   - Detailed list of changes

   ## Testing Done
   - Tests performed

   ## Screenshots (if applicable)
   
   ## Notes for Reviewer
   - Specific areas to focus on
   ```
3. **PR Review Checklist**:
   - Code follows project style guidelines
   - Tests are included and passing
   - Documentation is updated
   - No unnecessary code or comments
   - TypeScript types are properly defined
   - Performance considerations addressed

### Issue Reporting

1. **Issue Title Format**: Use the format `[Component] Brief description of issue`
2. **Issue Description Template**:
   ```
   ## Description
   Clear description of the issue

   ## Steps to Reproduce
   1. Step 1
   2. Step 2
   3. ...

   ## Expected Behavior
   What should happen

   ## Actual Behavior
   What actually happens

   ## Environment
   - Browser: 
   - OS: 
   - Device: 

   ## Screenshots/Logs
   If applicable
   ```
3. **Issue Assignment**:
   - Assign to Agent Alpha for feature bugs
   - Assign to Agent Beta for test/infrastructure issues
   - Use "needs triage" label for unclear ownership

## Code Handoff Process

### Feature Implementation to Testing Handoff

1. **Agent Alpha Responsibilities**:
   - Complete feature implementation
   - Write unit and integration tests
   - Document component interfaces
   - Add data-testid attributes for E2E testing
   - Create PR and request Agent Beta's review

2. **Agent Beta Responsibilities**:
   - Review code for quality and test coverage
   - Write E2E tests for the feature
   - Verify component interfaces
   - Provide feedback on code organization
   - Approve PR if all checks pass

### Testing to Feature Implementation Handoff

1. **Agent Beta Responsibilities**:
   - Identify issues through testing
   - Document reproducible test cases
   - Create issue with clear steps to reproduce
   - Assign to Agent Alpha

2. **Agent Alpha Responsibilities**:
   - Acknowledge issue receipt
   - Fix the issue
   - Update tests to prevent regression
   - Request verification from Agent Beta

## Conflict Resolution

### Interface Contract Conflicts

1. Create a contract resolution branch: `feature/contract/resolve-conflict`
2. Document the conflict and proposed resolution
3. Get approval from both agents
4. Merge the resolution

### Code Style Conflicts

1. Defer to established style guide
2. For new patterns, create a proposal
3. Discuss and document the decision
4. Update style guide documentation

## Performance Monitoring

### Agent Alpha Responsibilities

- Implement performance-critical features
- Optimize render cycles
- Reduce unnecessary re-renders
- Profile component performance

### Agent Beta Responsibilities

- Monitor overall application performance
- Identify performance bottlenecks
- Document performance issues
- Implement performance monitoring tools

## Memory Leak Detection

### Agent Alpha Responsibilities

- Ensure proper cleanup in useEffect hooks
- Dispose of resources properly
- Fix memory leaks in components

### Agent Beta Responsibilities

- Monitor for memory leaks
- Create reproducible test cases
- Document memory usage patterns
- Implement memory profiling tools

## Documentation Standards

1. **Component Documentation**:
   ```typescript
   /**
    * ComponentName - Brief description
    * 
    * @component
    * @example
    * <ComponentName prop1="value" prop2={42} />
    * 
    * @param {object} props - Component props
    * @param {string} props.prop1 - Description of prop1
    * @param {number} props.prop2 - Description of prop2
    * @returns {React.ReactElement} Component JSX
    */
   ```

2. **Interface Documentation**:
   ```typescript
   /**
    * Interface for data shared between Alpha and Beta components
    * 
    * @interface
    * @property {string} id - Unique identifier
    * @property {number} value - Numeric value
    * @property {boolean} isActive - Activity state
    */
   ```

3. **Test Documentation**:
   ```typescript
   /**
    * Test suite for ComponentName
    * 
    * Tests:
    * - Initial rendering
    * - User interactions
    * - Edge cases
    * - Error handling
    */
   ```

## Regular Sync Points

1. **PR Reviews**: Both agents review each other's PRs
2. **Issue Triage**: Joint review of new issues
3. **Sprint Planning**: Collaborative planning for next sprint
4. **Documentation Updates**: Regular review of documentation

## Continuous Improvement

1. Document lessons learned after each sprint
2. Update workflow documentation with new best practices
3. Refine communication protocols based on experience
4. Share knowledge between agents
