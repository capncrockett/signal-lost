import Phaser from 'phaser';
import { NarrativeEngine, NarrativeEvent } from './NarrativeEngine';

/**
 * NarrativeRenderer
 *
 * Renders narrative events in a Phaser scene
 */
export class NarrativeRenderer extends Phaser.GameObjects.Container {
  // Reference to the narrative engine
  private engine: NarrativeEngine;

  // UI elements
  private background: Phaser.GameObjects.Rectangle;
  private messageText: Phaser.GameObjects.Text;
  private choiceTexts: Phaser.GameObjects.Text[] = [];

  // Current event
  private currentEvent: NarrativeEvent | null = null;

  // UI configuration
  private config = {
    width: 600,
    height: 400,
    padding: 20,
    backgroundColor: 0x000000,
    backgroundAlpha: 0.8,
    textColor: '#ffffff',
    highlightColor: '#ffff00',
    fontSize: 18,
    choiceFontSize: 16,
    lineSpacing: 10,
  };

  /**
   * Create a new narrative renderer
   * @param scene Reference to the scene
   * @param x X position
   * @param y Y position
   * @param engine Narrative engine
   * @param config UI configuration
   */
  constructor(
    scene: Phaser.Scene,
    x: number,
    y: number,
    engine: NarrativeEngine,
    config?: Partial<typeof NarrativeRenderer.prototype.config>
  ) {
    super(scene, x, y);

    this.engine = engine;

    // Apply custom configuration
    if (config) {
      this.config = { ...this.config, ...config };
    }

    // Create UI elements
    this.createUI();

    // Set up event listeners
    this.setupEventListeners();

    // Hide initially
    this.setVisible(false);
  }

  /**
   * Create UI elements
   */
  private createUI(): void {
    // Create background
    this.background = this.scene.add.rectangle(
      0,
      0,
      this.config.width,
      this.config.height,
      this.config.backgroundColor,
      this.config.backgroundAlpha
    );
    this.add(this.background);

    // Create message text
    this.messageText = this.scene.add.text(
      -this.config.width / 2 + this.config.padding,
      -this.config.height / 2 + this.config.padding,
      '',
      {
        color: this.config.textColor,
        fontSize: `${this.config.fontSize}px`,
        wordWrap: { width: this.config.width - this.config.padding * 2 },
      }
    );
    this.add(this.messageText);
  }

  /**
   * Set up event listeners
   */
  private setupEventListeners(): void {
    // Listen for narrative events
    this.engine.on('narrativeEvent', (event: NarrativeEvent) => {
      this.showEvent(event);
    });

    // Listen for narrative choices
    this.engine.on('narrativeChoice', () => {
      // Hide after a choice is made
      this.hide();
    });
  }

  /**
   * Show a narrative event
   * @param event Narrative event to show
   */
  showEvent(event: NarrativeEvent): void {
    this.currentEvent = event;

    // Set message text
    this.messageText.setText(event.message);

    // Clear existing choice texts
    for (const choiceText of this.choiceTexts) {
      choiceText.destroy();
    }
    this.choiceTexts = [];

    // Create choice texts
    const choiceStartY = this.messageText.y + this.messageText.height + this.config.lineSpacing * 2;

    for (let i = 0; i < event.choices.length; i++) {
      const choice = event.choices[i];
      const choiceText = this.scene.add.text(
        this.messageText.x,
        choiceStartY + i * (this.config.choiceFontSize + this.config.lineSpacing),
        `${i + 1}. ${choice.text}`,
        {
          color: this.config.textColor,
          fontSize: `${this.config.choiceFontSize}px`,
          wordWrap: { width: this.config.width - this.config.padding * 2 },
        }
      );

      // Make choice interactive
      choiceText.setInteractive({ useHandCursor: true });

      // Add hover effect
      choiceText.on('pointerover', () => {
        choiceText.setColor(this.config.highlightColor);
      });

      choiceText.on('pointerout', () => {
        choiceText.setColor(this.config.textColor);
      });

      // Add click handler
      choiceText.on('pointerdown', () => {
        this.engine.makeChoice(i);
      });

      this.add(choiceText);
      this.choiceTexts.push(choiceText);
    }

    // Show the renderer
    this.setVisible(true);
  }

  /**
   * Hide the renderer
   */
  hide(): void {
    this.setVisible(false);
    this.currentEvent = null;
  }

  /**
   * Check if the renderer is visible
   * @returns True if the renderer is visible
   */
  isVisible(): boolean {
    return this.visible;
  }

  /**
   * Get the current event
   * @returns Current event or null if no event is active
   */
  getCurrentEvent(): NarrativeEvent | null {
    return this.currentEvent;
  }

  /**
   * Update the renderer
   * @param time Current time
   * @param delta Time since last update
   */
  update(_time: number, _delta: number): void {
    // Check for keyboard input
    if (this.visible && this.currentEvent && this.currentEvent.choices.length > 0) {
      const keyboard = this.scene.input.keyboard;

      if (keyboard) {
        // Check for number keys 1-9
        for (let i = 0; i < Math.min(this.currentEvent.choices.length, 9); i++) {
          const key = keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ONE + i);

          if (Phaser.Input.Keyboard.JustDown(key)) {
            this.engine.makeChoice(i);
            break;
          }
        }
      }
    }
  }
}
