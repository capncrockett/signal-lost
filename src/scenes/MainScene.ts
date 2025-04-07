import Phaser from 'phaser';
import { RadioTuner } from '../components/RadioTuner';
import { SoundscapeManager } from '../audio/SoundscapeManager';
import { VolumeControl } from '../components/VolumeControl';
import { TestOverlay } from '../utils/TestOverlay';
import { NarrativeEngine } from '../narrative/NarrativeEngine';
import { NarrativeRenderer } from '../narrative/NarrativeRenderer';
import { SaveManager } from '../utils/SaveManager';
import { SaveLoadMenu } from '../components/SaveLoadMenu';

export class MainScene extends Phaser.Scene {
  private radioTuner!: RadioTuner;
  private soundscapeManager!: SoundscapeManager;
  private volumeControl!: VolumeControl;
  private narrativeEngine!: NarrativeEngine;
  private narrativeRenderer!: NarrativeRenderer;
  private saveLoadMenu!: SaveLoadMenu;

  constructor() {
    super({ key: 'MainScene' });
  }

  preload() {
    // Load assets with multiple path formats to handle different environments
    // Try with leading slash first
    this.load.image('radio', '/assets/images/radio.png');
    this.load.image('signalDetector', '/assets/images/signalDetector.png');
    this.load.image('menuBackground', '/assets/images/menuBackground.png');

    // Alternative paths without leading slash as fallback
    this.load.image('radio_alt', 'assets/images/radio.png');
    this.load.image('signalDetector_alt', 'assets/images/signalDetector.png');
    this.load.image('menuBackground_alt', 'assets/images/menuBackground.png');

    // Log asset loading events
    this.load.on('filecomplete', (key: string, type: string) => {
      console.log(`MainScene asset loaded: ${key} (${type})`);
    });

    this.load.on('loaderror', (file: Phaser.Loader.File) => {
      // Ensure url is a string
      const url = typeof file.url === 'string' ? file.url : String(file.url);
      console.error(`MainScene error loading asset: ${file.key} from ${url}`);
    });

    // Static noise is generated programmatically, no need to load an audio file
  }

  create() {
    // Initialize SaveManager
    SaveManager.initialize();

    // Initialize narrative engine
    this.initializeNarrativeEngine();

    // Add background - try both asset keys
    let bg;
    try {
      bg = this.add.image(400, 300, 'menuBackground');
    } catch (error) {
      console.log('Trying alternative background key');
      bg = this.add.image(400, 300, 'menuBackground_alt');
    }

    if (bg) {
      bg.setDisplaySize(800, 600);
      bg.setName('menuBackground'); // Add name for resize handler
    } else {
      console.error('Failed to load background with both keys');
      // Create a fallback background
      bg = this.add.rectangle(400, 300, 800, 600, 0x000000);
      bg.setName('menuBackground');
    }

    // Set up resize handler
    this.scale.on(
      'resize',
      (width: number, height: number) => this.handleResize(width, height),
      this
    );

    // Initial resize to set correct positions
    this.handleResize(this.scale.width, this.scale.height);

    // Initialize soundscape manager
    this.soundscapeManager = new SoundscapeManager(this);

    // Create the radio tuner component with larger size
    try {
      this.radioTuner = new RadioTuner(this, 400, 300, {
        width: 500, // Increase width
        height: 200, // Increase height
        backgroundColor: 0x444444, // Darker background for better visibility
        knobColor: 0xffff00, // Yellow knob for better visibility
        sliderColor: 0x888888, // Lighter slider for better visibility
        // Add asset keys for both path formats
        radioImageKey: 'radio',
        radioImageKeyAlt: 'radio_alt',
      });
      this.add.existing(this.radioTuner);
      console.log('RadioTuner created successfully');
    } catch (error) {
      console.error('Failed to create RadioTuner:', error);
      // Create a fallback radio tuner display
      const fallbackRadio = this.add.rectangle(400, 300, 500, 200, 0x444444);
      fallbackRadio.setName('fallbackRadio');

      // Add text to indicate the fallback
      this.add
        .text(400, 300, 'RADIO TUNER (FALLBACK)', {
          fontSize: '24px',
          fontStyle: 'bold',
          color: '#ffffff',
          align: 'center',
        })
        .setOrigin(0.5, 0.5);
    }

    // Add test overlay for the radio tuner
    TestOverlay.createOverlay(this, this.radioTuner, 'radio-tuner');

    // Add a text label above the radio
    const radioLabel = this.add.text(400, 150, 'RADIO TUNER', {
      fontSize: '32px',
      fontStyle: 'bold',
      color: '#ffffff',
      backgroundColor: '#000000',
      padding: { x: 10, y: 5 },
    });
    radioLabel.setOrigin(0.5, 0.5);

    // Define types for signal data
    interface LocationSignalData {
      locationId: string;
      coordinates: { x: number; y: number };
    }

    interface MessageSignalData {
      message: string;
    }

    // Define item signal data type
    interface ItemSignalData {
      itemId: string;
      name: string;
      description: string;
    }

    type SignalData = LocationSignalData | MessageSignalData | ItemSignalData;

    // Define the type for signal lock data
    interface SignalLockData {
      frequency: number;
      signalStrength: number;
      signalId: string;
      signalType: 'location' | 'message' | 'item';
      signalData: SignalData;
    }

    // Listen for signal lock events
    this.radioTuner.on('signalLock', (data: SignalLockData) => {
      console.log(
        `Signal locked at frequency: ${data.frequency}, ID: ${data.signalId}, Type: ${data.signalType}`
      );

      // Play a signal sound when locked
      this.soundscapeManager.playSignalSound();

      // Handle different signal types
      this.handleSignalLock(data);
    });

    // Initialize audio on first interaction
    this.input.once('pointerdown', () => {
      this.soundscapeManager.initialize();
      console.log('Audio initialized');
    });

    // Add volume control
    this.volumeControl = new VolumeControl(this, 700, 50, {
      width: 150,
      height: 30,
      initialVolume: 0.1, // Start at 10% volume
    });
    this.add.existing(this.volumeControl);

    // Add test overlay for the volume control
    TestOverlay.createOverlay(this, this.volumeControl, 'volume-control');

    // Add button to navigate to FieldScene
    const fieldButton = this.add.text(400, 500, 'Go to Field', {
      fontSize: '32px',
      fontStyle: 'bold',
      color: '#ffffff',
      backgroundColor: '#333333',
      padding: { x: 20, y: 10 },
      shadow: { offsetX: 2, offsetY: 2, color: '#000000', blur: 2, stroke: true, fill: true },
    });
    fieldButton.setOrigin(0.5, 0.5);
    fieldButton.setInteractive({ useHandCursor: true });
    fieldButton.setName('fieldButton'); // Add name for resize handler

    // Add a glow effect to make it more visible
    fieldButton.preFX?.addGlow(0x00ffff, 0, 0, false, 0.1, 16);

    // Make the button pulse to draw attention
    this.tweens.add({
      targets: fieldButton,
      scaleX: 1.1,
      scaleY: 1.1,
      duration: 1000,
      yoyo: true,
      repeat: -1,
      ease: 'Sine.easeInOut',
    });

    // Add click handler
    fieldButton.on('pointerdown', () => {
      console.log('Go to Field button clicked');
      this.scene.start('FieldScene');
    });

    // Add test overlay for E2E testing
    TestOverlay.createOverlay(this, fieldButton, 'go-to-field-button', () => {
      console.log('Go to Field button clicked via test overlay');
      this.scene.start('FieldScene');
    });

    // Note: DOM button removed to avoid duplication with the Phaser Text button

    // Add instructions
    const instructions = this.add.text(400, 400, 'Click and drag the radio tuner to find signals', {
      fontSize: '18px',
      color: '#ffffff',
      backgroundColor: '#000000',
      padding: { x: 10, y: 5 },
    });
    instructions.setOrigin(0.5, 0.5);
    instructions.setName('instructions'); // Add name for resize handler

    // Add volume instructions
    const volumeInstructions = this.add.text(700, 80, 'Adjust volume', {
      fontSize: '14px',
      color: '#ffffff',
      backgroundColor: '#000000',
      padding: { x: 5, y: 3 },
    });
    volumeInstructions.setOrigin(0.5, 0.5);
    volumeInstructions.setName('volumeInstructions'); // Add name for resize handler

    // Add save/load button
    const saveLoadButton = this.add.text(700, 120, 'Save/Load', {
      fontSize: '14px',
      color: '#ffffff',
      backgroundColor: '#000066',
      padding: { x: 5, y: 3 },
    });
    saveLoadButton.setOrigin(0.5, 0.5);
    saveLoadButton.setName('saveLoadButton');
    saveLoadButton.setInteractive({ useHandCursor: true });
    saveLoadButton.on('pointerdown', () => {
      this.toggleSaveLoadMenu();
    });

    // Create save/load menu
    this.saveLoadMenu = new SaveLoadMenu(this, 400, 300);
    this.saveLoadMenu.on('save', (saveId: string) => {
      console.log(`Game saved with ID: ${saveId}`);
    });
    this.saveLoadMenu.on('load', (saveId: string) => {
      console.log(`Game loaded with ID: ${saveId}`);
      this.refreshGameState();
    });
  }

  update(_time: number, _delta: number) {
    // Update game objects

    // Update soundscape based on radio tuner signal strength
    if (this.radioTuner && this.soundscapeManager) {
      const signalStrength = this.radioTuner.getSignalStrengthValue();
      this.soundscapeManager.updateLayers(signalStrength);
    }
  }

  /**
   * Handle signal lock events
   * @param data Signal lock data
   */
  private handleSignalLock(data: SignalLockData): void {
    // Create a visual effect to indicate signal lock
    this.createSignalLockEffect();

    // Display signal information
    this.displaySignalInfo(data);

    // Handle different signal types
    switch (data.signalType) {
      case 'location':
        this.handleLocationSignal(data.signalData as LocationSignalData);
        break;
      case 'message':
        this.handleMessageSignal(data.signalData as MessageSignalData);
        break;
      case 'item':
        this.handleItemSignal(data.signalData as ItemSignalData);
        break;
      default:
        console.log(`Unknown signal type: ${data.signalType}`);
        break;
    }

    // Trigger narrative events based on signal ID
    switch (data.signalId) {
      case 'signal1':
        this.narrativeEngine.triggerEvent('signal1_discovery');
        break;
      case 'signal2':
        this.narrativeEngine.triggerEvent('signal2_discovery');
        break;
      case 'signal3':
        this.narrativeEngine.triggerEvent('signal3_discovery');
        break;
      case 'signal4':
        this.narrativeEngine.triggerEvent('signal4_discovery');
        break;
      case 'signal5':
        this.narrativeEngine.triggerEvent('signal5_discovery');
        break;
      case 'signal6':
        this.narrativeEngine.triggerEvent('signal6_discovery');
        break;
    }
  }

  /**
   * Create a visual effect to indicate signal lock
   */
  private createSignalLockEffect(): void {
    // Create a flash effect
    const flash = this.add.rectangle(400, 300, 800, 600, 0xffffff);
    flash.setAlpha(0.8);
    flash.setDepth(100);

    // Fade out the flash
    this.tweens.add({
      targets: flash,
      alpha: 0,
      duration: 500,
      ease: 'Power2',
      onComplete: () => {
        flash.destroy();
      },
    });
  }

  /**
   * Display signal information
   * @param data Signal lock data
   */
  private displaySignalInfo(data: SignalLockData): void {
    // Create a text object to display signal information
    const infoText = this.add.text(
      400,
      200,
      `Signal Detected!\n` +
        `Frequency: ${data.frequency.toFixed(1)} MHz\n` +
        `Signal Strength: ${(data.signalStrength * 100).toFixed(0)}%\n` +
        `Type: ${data.signalType.toUpperCase()}`,
      {
        fontSize: '24px',
        fontStyle: 'bold',
        color: '#ffffff',
        backgroundColor: '#000000',
        padding: { x: 20, y: 10 },
        align: 'center',
      }
    );
    infoText.setOrigin(0.5, 0.5);
    infoText.setDepth(101);

    // Add a glow effect
    infoText.preFX?.addGlow(0x00ffff, 0, 0, false, 0.1, 16);

    // Fade out after a few seconds
    this.time.delayedCall(5000, () => {
      this.tweens.add({
        targets: infoText,
        alpha: 0,
        duration: 1000,
        ease: 'Power2',
        onComplete: () => {
          infoText.destroy();
        },
      });
    });
  }

  /**
   * Handle location signal
   * @param data Location data
   */
  private handleLocationSignal(data: LocationSignalData): void {
    console.log(
      `Location signal detected: ${data.locationId} at coordinates (${data.coordinates.x}, ${data.coordinates.y})`
    );

    // Create a marker on the map
    const marker = this.add.text(
      400,
      400,
      `Location Marked on Map:\n` + `Coordinates: (${data.coordinates.x}, ${data.coordinates.y})`,
      {
        fontSize: '20px',
        color: '#ffff00',
        backgroundColor: '#333333',
        padding: { x: 15, y: 8 },
        align: 'center',
      }
    );
    marker.setOrigin(0.5, 0.5);
    marker.setDepth(101);

    // Define interface for SaveManager
    interface SaveManagerInterface {
      setFlag: (key: string, value: boolean) => void;
      setData: (key: string, value: unknown) => void;
    }

    // Define interface for window with SaveManager property
    interface WindowWithSaveManager {
      SaveManager: SaveManagerInterface;
    }

    // Save the location to the game state
    const SaveManager = (window as unknown as WindowWithSaveManager).SaveManager;
    if (SaveManager) {
      SaveManager.setFlag(`discovered_${data.locationId}`, true);
      SaveManager.setData(`location_${data.locationId}`, data.coordinates);
    }

    // Fade out after a few seconds
    this.time.delayedCall(8000, () => {
      this.tweens.add({
        targets: marker,
        alpha: 0,
        duration: 1000,
        ease: 'Power2',
        onComplete: () => {
          marker.destroy();
        },
      });
    });
  }

  /**
   * Handle message signal
   * @param data Message data
   */
  private handleMessageSignal(data: MessageSignalData): void {
    console.log(`Message signal detected: ${data.message}`);

    // Create a text object to display the message
    const messageText = this.add.text(400, 400, `Incoming Message:\n` + `"${data.message}"`, {
      fontSize: '20px',
      color: '#00ff00',
      backgroundColor: '#333333',
      padding: { x: 15, y: 8 },
      align: 'center',
    });
    messageText.setOrigin(0.5, 0.5);
    messageText.setDepth(101);

    // Add a typewriter effect
    const originalText = messageText.text;
    messageText.setText('');

    let currentChar = 0;
    const typewriterEvent = this.time.addEvent({
      delay: 50,
      callback: () => {
        messageText.text += originalText[currentChar];
        currentChar++;

        if (currentChar === originalText.length) {
          typewriterEvent.destroy();
        }
      },
      repeat: originalText.length - 1,
    });

    // Fade out after a few seconds
    this.time.delayedCall(10000, () => {
      this.tweens.add({
        targets: messageText,
        alpha: 0,
        duration: 1000,
        ease: 'Power2',
        onComplete: () => {
          messageText.destroy();
        },
      });
    });
  }

  /**
   * Handle item signal
   * @param data Item data
   */
  private handleItemSignal(data: ItemSignalData): void {
    console.log(`Item signal detected: ${data.itemId} - ${data.name}`);

    // Create a text object to display the item information
    const itemText = this.add.text(
      400,
      400,
      `Item Discovered!\n` + `${data.name}\n` + `${data.description}`,
      {
        fontSize: '20px',
        color: '#ffcc00',
        backgroundColor: '#333333',
        padding: { x: 15, y: 8 },
        align: 'center',
      }
    );
    itemText.setOrigin(0.5, 0.5);
    itemText.setDepth(101);

    // Save the item to the game state
    SaveManager.setFlag(`item_${data.itemId}`, true);

    // Create a visual effect for item discovery
    const itemEffect = this.add.circle(400, 400, 50, 0xffcc00, 0.5);
    itemEffect.setDepth(100);

    // Animate the effect
    this.tweens.add({
      targets: itemEffect,
      radius: 100,
      alpha: 0,
      duration: 1000,
      ease: 'Power2',
      onComplete: () => {
        itemEffect.destroy();
      },
    });

    // Add a typewriter effect
    const originalText = itemText.text;
    itemText.setText('');

    let currentChar = 0;
    const typewriterEvent = this.time.addEvent({
      delay: 50,
      callback: () => {
        itemText.text += originalText[currentChar];
        currentChar++;

        if (currentChar === originalText.length) {
          typewriterEvent.destroy();
        }
      },
      repeat: originalText.length - 1,
    });

    // Fade out the text after a few seconds
    this.time.delayedCall(10000, () => {
      this.tweens.add({
        targets: itemText,
        alpha: 0,
        duration: 1000,
        ease: 'Power2',
        onComplete: () => {
          itemText.destroy();
        },
      });
    });
  }

  /**
   * Toggle the save/load menu visibility
   */
  private toggleSaveLoadMenu(): void {
    this.saveLoadMenu.toggle();
  }

  /**
   * Refresh game state after loading a save
   */
  private refreshGameState(): void {
    // Refresh narrative engine state
    this.narrativeEngine.loadEventHistory();

    // Refresh radio tuner state
    // If there's a saved frequency, set it
    const savedFrequency = SaveManager.getData('last_frequency');
    if (typeof savedFrequency === 'number') {
      this.radioTuner.setFrequency(savedFrequency);
    }

    // Refresh any other game state as needed
    // For example, update UI based on discovered locations
    const allFlags = SaveManager.getAllFlags();
    console.log('Refreshed game state with flags:', allFlags);
  }

  /**
   * Initialize the narrative engine
   */
  private initializeNarrativeEngine(): void {
    // Create narrative engine
    this.narrativeEngine = new NarrativeEngine();

    // Load narrative events
    fetch('/assets/narrative/events.json')
      .then((response) => response.json())
      .then((data) => {
        this.narrativeEngine.loadEvents(JSON.stringify(data));
        console.log('Narrative events loaded');
      })
      .catch((error) => {
        console.error('Failed to load narrative events:', error);
      });

    // Create narrative renderer
    this.narrativeRenderer = new NarrativeRenderer(this, 400, 300, this.narrativeEngine);
    this.add.existing(this.narrativeRenderer);
    this.narrativeRenderer.setVisible(false);
    this.narrativeRenderer.setDepth(200);

    // Listen for narrative events
    this.narrativeEngine.on('narrativeEvent', (event) => {
      console.log(`Narrative event triggered: ${event.id}`);
      this.narrativeRenderer.setVisible(true);
    });

    // Listen for narrative choices
    this.narrativeEngine.on('narrativeChoice', (data) => {
      console.log(`Choice made: ${data.choice.text}`);
      this.narrativeRenderer.setVisible(false);
    });

    // Listen for location discoveries
    this.narrativeEngine.on('locationDiscovered', (data) => {
      console.log(`Location discovered: ${data.id} at (${data.x}, ${data.y})`);
      SaveManager.setFlag(`discovered_${data.id}`, true);
      SaveManager.setData(`location_${data.id}`, { x: data.x, y: data.y });
    });
  }

  /**
   * Handle resize events
   * @param width New width of the scene
   * @param height New height of the scene
   */
  private handleResize(width: number, height: number): void {
    // Resize and reposition background
    const bg = this.children.getByName('menuBackground') as Phaser.GameObjects.Image;
    if (bg) {
      bg.setPosition(width / 2, height / 2);
      // Use setScale instead of setDisplaySize to maintain aspect ratio
      const scaleX = width / 800;
      const scaleY = height / 600;
      const scale = Math.max(scaleX, scaleY);
      bg.setScale(scale);
    }

    // Reposition volume control
    if (this.volumeControl) {
      this.volumeControl.onResize(width, height);
    }

    // Reposition radio tuner
    if (this.radioTuner) {
      this.radioTuner.x = width / 2;
      this.radioTuner.y = height / 2;
    }

    // Reposition field button
    const fieldButton = this.children.getByName('fieldButton') as Phaser.GameObjects.Text;
    if (fieldButton) {
      fieldButton.x = width / 2;
      fieldButton.y = height - 100;
    }

    // Reposition instructions
    const instructions = this.children.getByName('instructions') as Phaser.GameObjects.Text;
    if (instructions) {
      instructions.x = width / 2;
      instructions.y = height / 2 + 100;
    }

    // Reposition volume instructions
    const volumeInstructions = this.children.getByName(
      'volumeInstructions'
    ) as Phaser.GameObjects.Text;
    if (volumeInstructions) {
      volumeInstructions.x = width / 2;
      volumeInstructions.y = height / 2 + 130;
    }

    // Reposition narrative renderer
    if (this.narrativeRenderer) {
      this.narrativeRenderer.x = width / 2;
      this.narrativeRenderer.y = height / 2;
    }
  }
}
