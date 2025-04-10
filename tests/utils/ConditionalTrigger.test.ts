import {
  evaluateCondition,
  evaluateTrigger,
  isTriggerOnCooldown,
  hasOneTimeTriggerFired,
  processTriggers,
  createFrequencyTrigger,
  createProgressTrigger,
  createLocationTrigger,
  createInventoryTrigger,
  Condition,
  Trigger,
  TriggerState,
} from '../../src/utils/ConditionalTrigger';

describe('ConditionalTrigger', () => {
  describe('evaluateCondition', () => {
    test('evaluates equals condition correctly', () => {
      const condition: Condition = {
        type: 'equals',
        path: 'gameState.currentFrequency',
        value: 91.5,
      };

      const state = {
        gameState: {
          currentFrequency: 91.5,
        },
      };

      expect(evaluateCondition(condition, state)).toBe(true);

      const stateFalse = {
        gameState: {
          currentFrequency: 92.0,
        },
      };

      expect(evaluateCondition(condition, stateFalse)).toBe(false);
    });

    test('evaluates notEquals condition correctly', () => {
      const condition: Condition = {
        type: 'notEquals',
        path: 'gameState.currentFrequency',
        value: 91.5,
      };

      const state = {
        gameState: {
          currentFrequency: 92.0,
        },
      };

      expect(evaluateCondition(condition, state)).toBe(true);

      const stateFalse = {
        gameState: {
          currentFrequency: 91.5,
        },
      };

      expect(evaluateCondition(condition, stateFalse)).toBe(false);
    });

    test('evaluates greaterThan condition correctly', () => {
      const condition: Condition = {
        type: 'greaterThan',
        path: 'progressState.currentProgress',
        value: 5,
      };

      const state = {
        progressState: {
          currentProgress: 6,
        },
      };

      expect(evaluateCondition(condition, state)).toBe(true);

      const stateFalse = {
        progressState: {
          currentProgress: 4,
        },
      };

      expect(evaluateCondition(condition, stateFalse)).toBe(false);
    });

    test('evaluates lessThan condition correctly', () => {
      const condition: Condition = {
        type: 'lessThan',
        path: 'progressState.currentProgress',
        value: 5,
      };

      const state = {
        progressState: {
          currentProgress: 4,
        },
      };

      expect(evaluateCondition(condition, state)).toBe(true);

      const stateFalse = {
        progressState: {
          currentProgress: 6,
        },
      };

      expect(evaluateCondition(condition, stateFalse)).toBe(false);
    });

    test('evaluates contains condition correctly for arrays', () => {
      const condition: Condition = {
        type: 'contains',
        path: 'gameState.inventory',
        value: 'radio',
      };

      const state = {
        gameState: {
          inventory: ['radio', 'map', 'compass'],
        },
      };

      expect(evaluateCondition(condition, state)).toBe(true);

      const stateFalse = {
        gameState: {
          inventory: ['map', 'compass'],
        },
      };

      expect(evaluateCondition(condition, stateFalse)).toBe(false);
    });

    test('evaluates contains condition correctly for strings', () => {
      const condition: Condition = {
        type: 'contains',
        path: 'gameState.message',
        value: 'signal',
      };

      const state = {
        gameState: {
          message: 'Found a signal at 91.5 MHz',
        },
      };

      expect(evaluateCondition(condition, state)).toBe(true);

      const stateFalse = {
        gameState: {
          message: 'No transmission detected',
        },
      };

      expect(evaluateCondition(condition, stateFalse)).toBe(false);
    });

    test('evaluates notContains condition correctly', () => {
      const condition: Condition = {
        type: 'notContains',
        path: 'gameState.inventory',
        value: 'radio',
      };

      const state = {
        gameState: {
          inventory: ['map', 'compass'],
        },
      };

      expect(evaluateCondition(condition, state)).toBe(true);

      const stateFalse = {
        gameState: {
          inventory: ['radio', 'map', 'compass'],
        },
      };

      expect(evaluateCondition(condition, stateFalse)).toBe(false);
    });

    test('handles invalid paths gracefully', () => {
      const condition: Condition = {
        type: 'equals',
        path: 'gameState.nonExistentProperty.deeperProperty',
        value: 'something',
      };

      const state = {
        gameState: {
          currentFrequency: 91.5,
        },
      };

      expect(evaluateCondition(condition, state)).toBe(false);
    });
  });

  describe('evaluateTrigger', () => {
    test('evaluates trigger with multiple conditions correctly', () => {
      const trigger: Trigger = {
        id: 'test-trigger',
        conditions: [
          {
            type: 'equals',
            path: 'gameState.currentFrequency',
            value: 91.5,
          },
          {
            type: 'equals',
            path: 'gameState.isRadioOn',
            value: true,
          },
        ],
        event: {
          type: 'signal',
          payload: { message: 'Signal detected' },
        },
        oneTime: true,
      };

      const state = {
        gameState: {
          currentFrequency: 91.5,
          isRadioOn: true,
        },
      };

      expect(evaluateTrigger(trigger, state)).toBe(true);

      const stateFalse1 = {
        gameState: {
          currentFrequency: 92.0,
          isRadioOn: true,
        },
      };

      expect(evaluateTrigger(trigger, stateFalse1)).toBe(false);

      const stateFalse2 = {
        gameState: {
          currentFrequency: 91.5,
          isRadioOn: false,
        },
      };

      expect(evaluateTrigger(trigger, stateFalse2)).toBe(false);
    });
  });

  describe('isTriggerOnCooldown', () => {
    test('returns false when trigger has no cooldown', () => {
      const trigger: Trigger = {
        id: 'test-trigger',
        conditions: [],
        event: {
          type: 'signal',
          payload: {},
        },
        oneTime: false,
      };

      const triggerState: TriggerState = {
        triggeredIds: [],
        lastTriggeredTimestamps: {},
      };

      expect(isTriggerOnCooldown(trigger, triggerState)).toBe(false);
    });

    test('returns false when trigger has not been triggered before', () => {
      const trigger: Trigger = {
        id: 'test-trigger',
        conditions: [],
        event: {
          type: 'signal',
          payload: {},
        },
        oneTime: false,
        cooldown: 1000,
      };

      const triggerState: TriggerState = {
        triggeredIds: [],
        lastTriggeredTimestamps: {},
      };

      expect(isTriggerOnCooldown(trigger, triggerState)).toBe(false);
    });

    test('returns true when trigger is on cooldown', () => {
      const now = Date.now();
      const trigger: Trigger = {
        id: 'test-trigger',
        conditions: [],
        event: {
          type: 'signal',
          payload: {},
        },
        oneTime: false,
        cooldown: 1000,
      };

      const triggerState: TriggerState = {
        triggeredIds: ['test-trigger'],
        lastTriggeredTimestamps: {
          'test-trigger': now - 500, // Triggered 500ms ago
        },
      };

      expect(isTriggerOnCooldown(trigger, triggerState)).toBe(true);
    });

    test('returns false when cooldown has expired', () => {
      const now = Date.now();
      const trigger: Trigger = {
        id: 'test-trigger',
        conditions: [],
        event: {
          type: 'signal',
          payload: {},
        },
        oneTime: false,
        cooldown: 1000,
      };

      const triggerState: TriggerState = {
        triggeredIds: ['test-trigger'],
        lastTriggeredTimestamps: {
          'test-trigger': now - 1500, // Triggered 1500ms ago
        },
      };

      expect(isTriggerOnCooldown(trigger, triggerState)).toBe(false);
    });
  });

  describe('hasOneTimeTriggerFired', () => {
    test('returns true when one-time trigger has fired', () => {
      const trigger: Trigger = {
        id: 'test-trigger',
        conditions: [],
        event: {
          type: 'signal',
          payload: {},
        },
        oneTime: true,
      };

      const triggerState: TriggerState = {
        triggeredIds: ['test-trigger'],
        lastTriggeredTimestamps: {},
      };

      expect(hasOneTimeTriggerFired(trigger, triggerState)).toBe(true);
    });

    test('returns false when one-time trigger has not fired', () => {
      const trigger: Trigger = {
        id: 'test-trigger',
        conditions: [],
        event: {
          type: 'signal',
          payload: {},
        },
        oneTime: true,
      };

      const triggerState: TriggerState = {
        triggeredIds: ['other-trigger'],
        lastTriggeredTimestamps: {},
      };

      expect(hasOneTimeTriggerFired(trigger, triggerState)).toBe(false);
    });

    test('returns false when trigger is not one-time', () => {
      const trigger: Trigger = {
        id: 'test-trigger',
        conditions: [],
        event: {
          type: 'signal',
          payload: {},
        },
        oneTime: false,
      };

      const triggerState: TriggerState = {
        triggeredIds: ['test-trigger'],
        lastTriggeredTimestamps: {},
      };

      expect(hasOneTimeTriggerFired(trigger, triggerState)).toBe(false);
    });
  });

  describe('processTriggers', () => {
    test('processes triggers and returns events to dispatch', () => {
      const triggers: Trigger[] = [
        {
          id: 'frequency-trigger',
          conditions: [
            {
              type: 'equals',
              path: 'gameState.currentFrequency',
              value: 91.5,
            },
          ],
          event: {
            type: 'signal',
            payload: { message: 'Signal detected at 91.5 MHz' },
          },
          oneTime: false,
        },
        {
          id: 'inventory-trigger',
          conditions: [
            {
              type: 'contains',
              path: 'gameState.inventory',
              value: 'map',
            },
          ],
          event: {
            type: 'narrative',
            payload: { message: 'You can now navigate using the map' },
          },
          oneTime: true,
        },
      ];

      const state = {
        gameState: {
          currentFrequency: 91.5,
          inventory: ['radio', 'map'],
        },
      };

      const triggerState: TriggerState = {
        triggeredIds: [],
        lastTriggeredTimestamps: {},
      };

      const events = processTriggers(triggers, state, triggerState);

      // Both triggers should fire
      expect(events.length).toBe(2);
      expect(events[0].type).toBe('signal');
      expect(events[1].type).toBe('narrative');

      // Trigger state should be updated
      expect(triggerState.triggeredIds).toContain('frequency-trigger');
      expect(triggerState.triggeredIds).toContain('inventory-trigger');
      expect(triggerState.lastTriggeredTimestamps['frequency-trigger']).toBeDefined();
      expect(triggerState.lastTriggeredTimestamps['inventory-trigger']).toBeDefined();
    });

    test('skips one-time triggers that have already fired', () => {
      const triggers: Trigger[] = [
        {
          id: 'one-time-trigger',
          conditions: [
            {
              type: 'equals',
              path: 'gameState.currentFrequency',
              value: 91.5,
            },
          ],
          event: {
            type: 'signal',
            payload: { message: 'Signal detected at 91.5 MHz' },
          },
          oneTime: true,
        },
      ];

      const state = {
        gameState: {
          currentFrequency: 91.5,
        },
      };

      const triggerState: TriggerState = {
        triggeredIds: ['one-time-trigger'],
        lastTriggeredTimestamps: {},
      };

      const events = processTriggers(triggers, state, triggerState);

      // Trigger should be skipped
      expect(events.length).toBe(0);
    });

    test('skips triggers that are on cooldown', () => {
      const now = Date.now();
      const triggers: Trigger[] = [
        {
          id: 'cooldown-trigger',
          conditions: [
            {
              type: 'equals',
              path: 'gameState.currentFrequency',
              value: 91.5,
            },
          ],
          event: {
            type: 'signal',
            payload: { message: 'Signal detected at 91.5 MHz' },
          },
          oneTime: false,
          cooldown: 1000,
        },
      ];

      const state = {
        gameState: {
          currentFrequency: 91.5,
        },
      };

      const triggerState: TriggerState = {
        triggeredIds: ['cooldown-trigger'],
        lastTriggeredTimestamps: {
          'cooldown-trigger': now - 500, // Triggered 500ms ago
        },
      };

      const events = processTriggers(triggers, state, triggerState);

      // Trigger should be skipped
      expect(events.length).toBe(0);
    });
  });

  describe('createFrequencyTrigger', () => {
    test('creates a frequency trigger with correct conditions', () => {
      const trigger = createFrequencyTrigger(
        'freq-91.5',
        91.5,
        {
          strength: 0.8,
          type: 'message',
          content: 'Signal detected at 91.5 MHz',
          discovered: false,
        },
        true,
        1000
      );

      expect(trigger.id).toBe('freq-91.5');
      expect(trigger.conditions).toHaveLength(2);
      expect(trigger.conditions[0].type).toBe('equals');
      expect(trigger.conditions[0].path).toBe('gameState.currentFrequency');
      expect(trigger.conditions[0].value).toBe(91.5);
      expect(trigger.conditions[1].type).toBe('equals');
      expect(trigger.conditions[1].path).toBe('gameState.isRadioOn');
      expect(trigger.conditions[1].value).toBe(true);
      expect(trigger.event.type).toBe('signal');
      expect(trigger.oneTime).toBe(true);
      expect(trigger.cooldown).toBe(1000);
    });
  });

  describe('createProgressTrigger', () => {
    test('creates a progress trigger with correct conditions', () => {
      const trigger = createProgressTrigger(
        'progress-5',
        5,
        { message: 'You have reached level 5' },
        true
      );

      expect(trigger.id).toBe('progress-5');
      expect(trigger.conditions).toHaveLength(1);
      expect(trigger.conditions[0].type).toBe('greaterThan');
      expect(trigger.conditions[0].path).toBe('progressState.currentProgress');
      expect(trigger.conditions[0].value).toBe(4.9); // With offset
      expect(trigger.event.type).toBe('narrative');
      expect(trigger.oneTime).toBe(true);
    });
  });

  describe('createLocationTrigger', () => {
    test('creates a location trigger with correct conditions', () => {
      const trigger = createLocationTrigger(
        'location-tower',
        'radio-tower',
        { message: 'You have reached the radio tower' },
        true
      );

      expect(trigger.id).toBe('location-tower');
      expect(trigger.conditions).toHaveLength(1);
      expect(trigger.conditions[0].type).toBe('equals');
      expect(trigger.conditions[0].path).toBe('gameState.currentLocation');
      expect(trigger.conditions[0].value).toBe('radio-tower');
      expect(trigger.event.type).toBe('narrative');
      expect(trigger.oneTime).toBe(true);
    });
  });

  describe('createInventoryTrigger', () => {
    test('creates an inventory trigger with correct conditions', () => {
      const trigger = createInventoryTrigger(
        'inventory-map',
        'map',
        { message: 'You can now navigate using the map' },
        true
      );

      expect(trigger.id).toBe('inventory-map');
      expect(trigger.conditions).toHaveLength(1);
      expect(trigger.conditions[0].type).toBe('contains');
      expect(trigger.conditions[0].path).toBe('gameState.inventory');
      expect(trigger.conditions[0].value).toBe('map');
      expect(trigger.event.type).toBe('narrative');
      expect(trigger.oneTime).toBe(true);
    });
  });
});
