import Phaser from 'phaser';
import { PerformanceMonitor, FpsData, MemoryData } from '../utils/PerformanceMonitor';

/**
 * Performance display component for showing FPS and memory usage
 */
export class PerformanceDisplay extends Phaser.GameObjects.Container {
  // Performance monitor
  private performanceMonitor: PerformanceMonitor;

  // UI elements
  private background!: Phaser.GameObjects.Rectangle;
  private fpsText!: Phaser.GameObjects.Text;
  private memoryText!: Phaser.GameObjects.Text;
  private fpsGraph!: Phaser.GameObjects.Graphics;
  private memoryGraph!: Phaser.GameObjects.Graphics;

  // Configuration
  private config: PerformanceDisplayConfig;

  // Default configuration
  private static readonly DEFAULT_CONFIG: PerformanceDisplayConfig = {
    width: 200,
    height: 120,
    padding: 10,
    backgroundColor: 0x000000,
    backgroundAlpha: 0.5,
    textColor: '#ffffff',
    fpsGraphColor: 0x00ff00,
    memoryGraphColor: 0xff0000,
    graphHeight: 40,
    graphWidth: 180,
    showFps: true,
    showMemory: true,
    showGraphs: true,
    updateInterval: 1000,
  };

  /**
   * Create a new performance display
   * @param scene Scene to add the display to
   * @param x X position
   * @param y Y position
   * @param config Configuration options
   */
  constructor(
    scene: Phaser.Scene,
    x: number,
    y: number,
    config?: Partial<PerformanceDisplayConfig>
  ) {
    super(scene, x, y);

    // Get performance monitor
    this.performanceMonitor = PerformanceMonitor.getInstance();

    // Merge configuration
    this.config = { ...PerformanceDisplay.DEFAULT_CONFIG, ...config };

    // Create UI
    this.createUI();

    // Add to scene
    scene.add.existing(this);

    // Set up event listeners
    this.setupEventListeners();

    // Set update interval
    this.scene.time.addEvent({
      delay: this.config.updateInterval,
      callback: () => this.updateDisplay(),
      callbackScope: this,
      loop: true,
    });

    // Set data-testid for testing
    this.setDataEnabled();
    this.data.set('testid', 'performance-display');
  }

  /**
   * Create UI elements
   */
  private createUI(): void {
    // Calculate dimensions
    const width = this.config.width;
    const height = this.config.height;
    const padding = this.config.padding;

    // Create background
    this.background = this.scene.add.rectangle(
      0,
      0,
      width,
      height,
      this.config.backgroundColor,
      this.config.backgroundAlpha
    );
    this.background.setOrigin(0);
    this.add(this.background);

    // Create FPS text
    if (this.config.showFps) {
      this.fpsText = this.scene.add.text(padding, padding, 'FPS: 0', {
        color: this.config.textColor,
        fontSize: '14px',
      });
      this.fpsText.setOrigin(0);
      this.add(this.fpsText);
    }

    // Create memory text
    if (this.config.showMemory) {
      this.memoryText = this.scene.add.text(
        padding,
        this.config.showFps ? padding + 20 : padding,
        'Memory: 0 MB',
        {
          color: this.config.textColor,
          fontSize: '14px',
        }
      );
      this.memoryText.setOrigin(0);
      this.add(this.memoryText);
    }

    // Create FPS graph
    if (this.config.showGraphs && this.config.showFps) {
      this.fpsGraph = this.scene.add.graphics();
      this.add(this.fpsGraph);
    }

    // Create memory graph
    if (this.config.showGraphs && this.config.showMemory) {
      this.memoryGraph = this.scene.add.graphics();
      this.add(this.memoryGraph);
    }
  }

  /**
   * Set up event listeners
   */
  private setupEventListeners(): void {
    // Listen for FPS updates
    if (this.config.showFps) {
      this.performanceMonitor.addEventListener('fps', ((data: unknown) => {
        this.handleFpsUpdate(data as FpsData);
      }) as (data: unknown) => void);
    }

    // Listen for memory updates
    if (this.config.showMemory) {
      this.performanceMonitor.addEventListener('memory', ((data: unknown) => {
        this.handleMemoryUpdate(data as MemoryData);
      }) as (data: unknown) => void);
    }
  }

  /**
   * Handle FPS update
   * @param data FPS data
   */
  private handleFpsUpdate(data: FpsData): void {
    // Update FPS text
    if (this.config.showFps && this.fpsText) {
      this.fpsText.setText(
        `FPS: ${data.average.toFixed(1)} ` +
          `(Min: ${data.min.toFixed(1)}, Max: ${data.max.toFixed(1)})`
      );
    }

    // Update FPS graph
    if (this.config.showGraphs && this.config.showFps && this.fpsGraph) {
      this.updateFpsGraph(data);
    }
  }

  /**
   * Handle memory update
   * @param data Memory data
   */
  private handleMemoryUpdate(data: MemoryData): void {
    // Update memory text
    if (this.config.showMemory && this.memoryText) {
      const usedMB = data.current.usedJSHeapSize / 1024 / 1024;
      const totalMB = data.current.totalJSHeapSize / 1024 / 1024;
      const limitMB = data.current.jsHeapSizeLimit / 1024 / 1024;
      this.memoryText.setText(
        `Memory: ${usedMB.toFixed(1)} MB / ${totalMB.toFixed(1)} MB ` +
          `(${((usedMB / limitMB) * 100).toFixed(1)}%)`
      );
    }

    // Update memory graph
    if (this.config.showGraphs && this.config.showMemory && this.memoryGraph) {
      this.updateMemoryGraph(data);
    }
  }

  /**
   * Update FPS graph
   * @param data FPS data
   */
  private updateFpsGraph(data: FpsData): void {
    const padding = this.config.padding;
    const graphWidth = this.config.graphWidth;
    const graphHeight = this.config.graphHeight;
    const graphY = this.config.showMemory ? padding + 40 : padding + 20;

    // Clear graph
    this.fpsGraph.clear();

    // Draw graph background
    this.fpsGraph.fillStyle(0x222222, 0.5);
    this.fpsGraph.fillRect(padding, graphY, graphWidth, graphHeight);

    // Draw FPS line
    this.fpsGraph.lineStyle(1, this.config.fpsGraphColor, 1);
    this.fpsGraph.beginPath();

    // Draw FPS history
    const history = data.history;
    const maxFps = Math.max(60, ...history); // At least 60 FPS for scale
    const step = graphWidth / (history.length - 1);

    for (let i = 0; i < history.length; i++) {
      const x = padding + i * step;
      const y = graphY + graphHeight - (history[i] / maxFps) * graphHeight;
      if (i === 0) {
        this.fpsGraph.moveTo(x, y);
      } else {
        this.fpsGraph.lineTo(x, y);
      }
    }

    this.fpsGraph.strokePath();

    // Draw 60 FPS line
    this.fpsGraph.lineStyle(1, 0xffff00, 0.5);
    const y60fps = graphY + graphHeight - (60 / maxFps) * graphHeight;
    this.fpsGraph.beginPath();
    this.fpsGraph.moveTo(padding, y60fps);
    this.fpsGraph.lineTo(padding + graphWidth, y60fps);
    this.fpsGraph.strokePath();
  }

  /**
   * Update memory graph
   * @param data Memory data
   */
  private updateMemoryGraph(data: MemoryData): void {
    const padding = this.config.padding;
    const graphWidth = this.config.graphWidth;
    const graphHeight = this.config.graphHeight;
    const graphY = this.config.showFps ? padding + 40 + this.config.graphHeight + 10 : padding + 20;

    // Clear graph
    this.memoryGraph.clear();

    // Draw graph background
    this.memoryGraph.fillStyle(0x222222, 0.5);
    this.memoryGraph.fillRect(padding, graphY, graphWidth, graphHeight);

    // Draw memory line
    this.memoryGraph.lineStyle(1, this.config.memoryGraphColor, 1);
    this.memoryGraph.beginPath();

    // Draw memory history
    const history = data.history;
    const maxMemory = data.current.jsHeapSizeLimit;
    const step = graphWidth / (history.length - 1);

    for (let i = 0; i < history.length; i++) {
      const x = padding + i * step;
      const y = graphY + graphHeight - (history[i].usedJSHeapSize / maxMemory) * graphHeight;
      if (i === 0) {
        this.memoryGraph.moveTo(x, y);
      } else {
        this.memoryGraph.lineTo(x, y);
      }
    }

    this.memoryGraph.strokePath();
  }

  /**
   * Update the display
   */
  private updateDisplay(): void {
    // This is called by the timer event
    // The actual updates are handled by the event listeners
  }

  /**
   * Show the display
   */
  public show(): void {
    this.setVisible(true);
  }

  /**
   * Hide the display
   */
  public hide(): void {
    this.setVisible(false);
  }

  /**
   * Toggle the display
   */
  public toggle(): void {
    this.setVisible(!this.visible);
  }

  /**
   * Clean up resources
   */
  public destroy(): void {
    // Remove event listeners
    // Note: We're using new wrapper functions here since we can't access the original wrappers
    this.performanceMonitor.removeEventListener('fps', ((data: unknown) => {
      this.handleFpsUpdate(data as FpsData);
    }) as (data: unknown) => void);

    this.performanceMonitor.removeEventListener('memory', ((data: unknown) => {
      this.handleMemoryUpdate(data as MemoryData);
    }) as (data: unknown) => void);

    // Call parent destroy
    super.destroy();
  }
}

/**
 * Performance display configuration
 */
export interface PerformanceDisplayConfig {
  width: number;
  height: number;
  padding: number;
  backgroundColor: number;
  backgroundAlpha: number;
  textColor: string;
  fpsGraphColor: number;
  memoryGraphColor: number;
  graphHeight: number;
  graphWidth: number;
  showFps: boolean;
  showMemory: boolean;
  showGraphs: boolean;
  updateInterval: number;
}
