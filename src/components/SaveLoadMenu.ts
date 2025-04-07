import Phaser from 'phaser';
import { SaveManager } from '../utils/SaveManager';

/**
 * Interface for save slot data
 */
interface SaveSlot {
  id: string;
  name: string;
  timestamp: number;
  data: string;
}

/**
 * SaveLoadMenu component
 *
 * Provides a UI for saving and loading game state
 */
export class SaveLoadMenu extends Phaser.GameObjects.Container {
  private background!: Phaser.GameObjects.Rectangle;
  private titleText!: Phaser.GameObjects.Text;
  private closeButton!: Phaser.GameObjects.Text;
  private saveButton!: Phaser.GameObjects.Text;
  private loadButton!: Phaser.GameObjects.Text;
  private exportButton!: Phaser.GameObjects.Text;
  private importButton!: Phaser.GameObjects.Text;
  private deleteButton!: Phaser.GameObjects.Text;
  private slotContainer!: Phaser.GameObjects.Container;
  private slotTexts: Phaser.GameObjects.Text[] = [];
  private selectedSlot: number = -1;
  private saveSlots: SaveSlot[] = [];
  private isVisible: boolean = false;
  private inputField: HTMLInputElement | null = null;
  private fileInput: HTMLInputElement | null = null;

  // Maximum number of save slots
  private readonly MAX_SLOTS = 5;
  // Storage key for save slots
  private readonly SAVE_SLOTS_KEY = 'signal-lost-save-slots';

  constructor(scene: Phaser.Scene, x: number, y: number) {
    super(scene, x, y);

    // Create UI elements
    this.createBackground();
    this.createTitle();
    this.createCloseButton();
    this.createButtons();
    this.createSlotContainer();

    // Load save slots
    this.loadSaveSlots();

    // Create HTML elements for input
    this.createInputElements();

    // Add to scene
    scene.add.existing(this);

    // Hide initially
    this.setVisible(false);
    this.isVisible = false;
  }

  /**
   * Create the background rectangle
   */
  private createBackground(): void {
    this.background = this.scene.add.rectangle(0, 0, 600, 400, 0x000000, 0.9);
    this.background.setOrigin(0.5, 0.5);
    this.background.setInteractive();
    this.add(this.background);
  }

  /**
   * Create the title text
   */
  private createTitle(): void {
    this.titleText = this.scene.add.text(0, -170, 'Save / Load Game', {
      fontSize: '28px',
      color: '#ffffff',
      fontStyle: 'bold',
    });
    this.titleText.setOrigin(0.5, 0.5);
    this.add(this.titleText);
  }

  /**
   * Create the close button
   */
  private createCloseButton(): void {
    this.closeButton = this.scene.add.text(280, -170, 'X', {
      fontSize: '24px',
      color: '#ff0000',
      fontStyle: 'bold',
    });
    this.closeButton.setOrigin(0.5, 0.5);
    this.closeButton.setInteractive({ useHandCursor: true });
    this.closeButton.on('pointerdown', () => {
      this.hide();
    });
    this.add(this.closeButton);
  }

  /**
   * Create the action buttons
   */
  private createButtons(): void {
    // Save button
    this.saveButton = this.scene.add.text(-200, -120, 'Save Game', {
      fontSize: '20px',
      color: '#ffffff',
      backgroundColor: '#006600',
      padding: { x: 10, y: 5 },
    });
    this.saveButton.setOrigin(0.5, 0.5);
    this.saveButton.setInteractive({ useHandCursor: true });
    this.saveButton.on('pointerdown', () => {
      this.saveGame();
    });
    this.add(this.saveButton);

    // Load button
    this.loadButton = this.scene.add.text(0, -120, 'Load Game', {
      fontSize: '20px',
      color: '#ffffff',
      backgroundColor: '#000066',
      padding: { x: 10, y: 5 },
    });
    this.loadButton.setOrigin(0.5, 0.5);
    this.loadButton.setInteractive({ useHandCursor: true });
    this.loadButton.on('pointerdown', () => {
      this.loadGame();
    });
    this.add(this.loadButton);

    // Delete button
    this.deleteButton = this.scene.add.text(200, -120, 'Delete Save', {
      fontSize: '20px',
      color: '#ffffff',
      backgroundColor: '#660000',
      padding: { x: 10, y: 5 },
    });
    this.deleteButton.setOrigin(0.5, 0.5);
    this.deleteButton.setInteractive({ useHandCursor: true });
    this.deleteButton.on('pointerdown', () => {
      this.deleteGame();
    });
    this.add(this.deleteButton);

    // Export button
    this.exportButton = this.scene.add.text(-100, 150, 'Export Save', {
      fontSize: '18px',
      color: '#ffffff',
      backgroundColor: '#666600',
      padding: { x: 10, y: 5 },
    });
    this.exportButton.setOrigin(0.5, 0.5);
    this.exportButton.setInteractive({ useHandCursor: true });
    this.exportButton.on('pointerdown', () => {
      this.exportGame();
    });
    this.add(this.exportButton);

    // Import button
    this.importButton = this.scene.add.text(100, 150, 'Import Save', {
      fontSize: '18px',
      color: '#ffffff',
      backgroundColor: '#666600',
      padding: { x: 10, y: 5 },
    });
    this.importButton.setOrigin(0.5, 0.5);
    this.importButton.setInteractive({ useHandCursor: true });
    this.importButton.on('pointerdown', () => {
      this.importGame();
    });
    this.add(this.importButton);
  }

  /**
   * Create the slot container
   */
  private createSlotContainer(): void {
    this.slotContainer = this.scene.add.container(0, 0);
    this.add(this.slotContainer);
  }

  /**
   * Create HTML input elements
   */
  private createInputElements(): void {
    // Create input field for save name
    this.inputField = document.createElement('input');
    this.inputField.type = 'text';
    this.inputField.style.position = 'absolute';
    this.inputField.style.left = '-1000px'; // Hide off-screen initially
    this.inputField.style.top = '-1000px';
    this.inputField.style.zIndex = '1000';
    this.inputField.style.padding = '8px';
    this.inputField.style.width = '200px';
    this.inputField.style.fontFamily = 'Arial, sans-serif';
    document.body.appendChild(this.inputField);

    // Create file input for importing saves
    this.fileInput = document.createElement('input');
    this.fileInput.type = 'file';
    this.fileInput.accept = '.json';
    this.fileInput.style.position = 'absolute';
    this.fileInput.style.left = '-1000px'; // Hide off-screen initially
    this.fileInput.style.top = '-1000px';
    this.fileInput.style.zIndex = '1000';
    this.fileInput.addEventListener('change', (event) => {
      const target = event.target as HTMLInputElement;
      if (target.files && target.files.length > 0) {
        const file = target.files[0];
        const reader = new FileReader();
        reader.onload = (e) => {
          const result = e.target?.result;
          if (typeof result === 'string') {
            this.importSaveData(result);
          }
        };
        reader.readAsText(file);
      }
    });
    document.body.appendChild(this.fileInput);
  }

  /**
   * Load save slots from localStorage
   */
  private loadSaveSlots(): void {
    try {
      const savedSlots = localStorage.getItem(this.SAVE_SLOTS_KEY);
      if (savedSlots) {
        this.saveSlots = JSON.parse(savedSlots) as SaveSlot[];
      } else {
        this.saveSlots = [];
      }
    } catch (e) {
      console.error('Failed to load save slots:', e);
      this.saveSlots = [];
    }

    this.updateSlotDisplay();
  }

  /**
   * Save slots to localStorage
   */
  private saveSlotsToStorage(): void {
    try {
      localStorage.setItem(this.SAVE_SLOTS_KEY, JSON.stringify(this.saveSlots));
    } catch (e) {
      console.error('Failed to save slots to storage:', e);
    }
  }

  /**
   * Update the slot display
   */
  private updateSlotDisplay(): void {
    // Clear existing slots
    this.slotTexts.forEach((text) => text.destroy());
    this.slotTexts = [];
    this.slotContainer.removeAll();

    // Create slot background
    const slotBackground = this.scene.add.rectangle(0, 0, 550, 200, 0x222222);
    this.slotContainer.add(slotBackground);

    // Add empty slots message if no saves
    if (this.saveSlots.length === 0) {
      const emptyText = this.scene.add.text(0, 0, 'No saved games found', {
        fontSize: '18px',
        color: '#aaaaaa',
      });
      emptyText.setOrigin(0.5, 0.5);
      this.slotContainer.add(emptyText);
      this.slotTexts.push(emptyText);
      return;
    }

    // Add slots
    for (let i = 0; i < this.saveSlots.length; i++) {
      const slot = this.saveSlots[i];
      const date = new Date(slot.timestamp);
      const dateString = `${date.toLocaleDateString()} ${date.toLocaleTimeString()}`;

      const slotText = this.scene.add.text(
        0,
        -80 + i * 40,
        `${i + 1}. ${slot.name} (${dateString})`,
        {
          fontSize: '18px',
          color: '#ffffff',
          padding: { x: 10, y: 5 },
        }
      );
      slotText.setOrigin(0.5, 0.5);
      slotText.setInteractive({ useHandCursor: true });

      // Highlight selected slot
      if (i === this.selectedSlot) {
        slotText.setBackgroundColor('#444444');
      }

      // Add click handler
      slotText.on('pointerdown', () => {
        this.selectSlot(i);
      });

      this.slotContainer.add(slotText);
      this.slotTexts.push(slotText);
    }
  }

  /**
   * Select a slot
   * @param index Slot index
   */
  private selectSlot(index: number): void {
    // Deselect previous slot
    if (this.selectedSlot >= 0 && this.selectedSlot < this.slotTexts.length) {
      this.slotTexts[this.selectedSlot].setBackgroundColor('');
    }

    // Select new slot
    this.selectedSlot = index;

    // Highlight new slot
    if (this.selectedSlot >= 0 && this.selectedSlot < this.slotTexts.length) {
      this.slotTexts[this.selectedSlot].setBackgroundColor('#444444');
    }
  }

  /**
   * Save the current game state
   */
  private saveGame(): void {
    // Position input field
    if (this.inputField) {
      const canvas = this.scene.game.canvas;
      const bounds = canvas.getBoundingClientRect();
      const x = bounds.left + this.x + canvas.width / 2;
      const y = bounds.top + this.y + canvas.height / 2;

      this.inputField.style.left = `${x - 100}px`;
      this.inputField.style.top = `${y}px`;
      this.inputField.value = `Save ${this.saveSlots.length + 1}`;
      this.inputField.focus();

      // Handle input confirmation
      const handleKeyDown = (event: KeyboardEvent): void => {
        if (event.key === 'Enter' && this.inputField) {
          const saveName = this.inputField.value.trim() || `Save ${this.saveSlots.length + 1}`;
          this.createSaveSlot(saveName);

          // Hide input field
          this.inputField.style.left = '-1000px';
          this.inputField.style.top = '-1000px';

          // Remove event listener
          this.inputField.removeEventListener('keydown', handleKeyDown);
        } else if (event.key === 'Escape' && this.inputField) {
          // Hide input field
          this.inputField.style.left = '-1000px';
          this.inputField.style.top = '-1000px';

          // Remove event listener
          this.inputField.removeEventListener('keydown', handleKeyDown);
        }
      };

      this.inputField.addEventListener('keydown', handleKeyDown);
    }
  }

  /**
   * Create a new save slot
   * @param name Save name
   */
  private createSaveSlot(name: string): void {
    // Get current game state
    const gameState = SaveManager.exportState();

    // Create new save slot
    const newSlot: SaveSlot = {
      id: `save_${Date.now()}`,
      name: name,
      timestamp: Date.now(),
      data: gameState,
    };

    // Add to save slots
    if (this.saveSlots.length >= this.MAX_SLOTS) {
      // Remove oldest save if at max capacity
      this.saveSlots.shift();
    }

    this.saveSlots.push(newSlot);

    // Save to storage
    this.saveSlotsToStorage();

    // Update display
    this.updateSlotDisplay();

    // Select the new slot
    this.selectSlot(this.saveSlots.length - 1);

    // Emit save event
    this.emit('save', newSlot.id);

    console.log(`Game saved to slot "${name}" (${newSlot.id})`);
  }

  /**
   * Load a saved game
   */
  private loadGame(): void {
    // Check if a slot is selected
    if (this.selectedSlot < 0 || this.selectedSlot >= this.saveSlots.length) {
      console.warn('No save slot selected');
      return;
    }

    // Get selected slot
    const slot = this.saveSlots[this.selectedSlot];

    // Import game state
    const success = SaveManager.importState(slot.data);

    if (success) {
      console.log(`Game loaded from slot "${slot.name}" (${slot.id})`);

      // Emit load event
      this.emit('load', slot.id);

      // Hide menu
      this.hide();
    } else {
      console.error(`Failed to load game from slot "${slot.name}" (${slot.id})`);
    }
  }

  /**
   * Delete a saved game
   */
  private deleteGame(): void {
    // Check if a slot is selected
    if (this.selectedSlot < 0 || this.selectedSlot >= this.saveSlots.length) {
      console.warn('No save slot selected');
      return;
    }

    // Get selected slot
    const slot = this.saveSlots[this.selectedSlot];

    // Remove slot
    this.saveSlots.splice(this.selectedSlot, 1);

    // Save to storage
    this.saveSlotsToStorage();

    // Update display
    this.updateSlotDisplay();

    // Reset selection
    this.selectedSlot = -1;

    console.log(`Deleted save slot "${slot.name}" (${slot.id})`);
  }

  /**
   * Export the current game or selected save
   */
  private exportGame(): void {
    let dataToExport: string;

    // If a slot is selected, export that save
    if (this.selectedSlot >= 0 && this.selectedSlot < this.saveSlots.length) {
      dataToExport = this.saveSlots[this.selectedSlot].data;
    } else {
      // Otherwise export current game state
      dataToExport = SaveManager.exportState();
    }

    // Create a blob and download link
    const blob = new Blob([dataToExport], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'signal-lost-save.json';
    document.body.appendChild(a);
    a.click();

    // Clean up
    setTimeout(() => {
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    }, 0);

    console.log('Game state exported');
  }

  /**
   * Import a saved game
   */
  private importGame(): void {
    // Trigger file input
    if (this.fileInput) {
      this.fileInput.click();
    }
  }

  /**
   * Import save data from file
   * @param data Save data
   */
  private importSaveData(data: string): void {
    try {
      // Validate JSON by parsing it
      JSON.parse(data);

      // Create a new save slot
      const saveName = `Imported Save ${new Date().toLocaleDateString()}`;

      // Create new save slot
      const newSlot: SaveSlot = {
        id: `import_${Date.now()}`,
        name: saveName,
        timestamp: Date.now(),
        data: data,
      };

      // Add to save slots
      if (this.saveSlots.length >= this.MAX_SLOTS) {
        // Remove oldest save if at max capacity
        this.saveSlots.shift();
      }

      this.saveSlots.push(newSlot);

      // Save to storage
      this.saveSlotsToStorage();

      // Update display
      this.updateSlotDisplay();

      // Select the new slot
      this.selectSlot(this.saveSlots.length - 1);

      console.log(`Save data imported as "${saveName}"`);
    } catch (e) {
      console.error('Failed to import save data:', e);
    }
  }

  /**
   * Show the menu
   */
  public show(): void {
    if (this.isVisible) return;

    this.setVisible(true);
    this.isVisible = true;

    // Refresh save slots
    this.loadSaveSlots();

    // Emit show event
    this.emit('show');
  }

  /**
   * Hide the menu
   */
  public hide(): void {
    if (!this.isVisible) return;

    this.setVisible(false);
    this.isVisible = false;

    // Hide input field
    if (this.inputField) {
      this.inputField.style.left = '-1000px';
      this.inputField.style.top = '-1000px';
    }

    // Emit hide event
    this.emit('hide');
  }

  /**
   * Toggle menu visibility
   */
  public toggle(): void {
    if (this.isVisible) {
      this.hide();
    } else {
      this.show();
    }
  }

  /**
   * Clean up resources
   */
  public destroy(fromScene?: boolean): void {
    // Remove HTML elements
    if (this.inputField && document.body.contains(this.inputField)) {
      document.body.removeChild(this.inputField);
    }

    if (this.fileInput && document.body.contains(this.fileInput)) {
      document.body.removeChild(this.fileInput);
    }

    super.destroy(fromScene);
  }
}
