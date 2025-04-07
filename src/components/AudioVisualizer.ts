import Phaser from 'phaser';

interface AudioVisualizerConfig {
  width?: number;
  height?: number;
  backgroundColor?: number;
  barColor?: number;
  lineColor?: number;
  showBars?: boolean;
  showWaveform?: boolean;
  barWidth?: number;
  barSpacing?: number;
  fftSize?: number;
}

type VisualizationMode = 'bars' | 'waveform' | 'both' | 'none';

/**
 * AudioVisualizer component for Phaser 3
 * Provides a visualization of audio frequency data
 */
export class AudioVisualizer extends Phaser.GameObjects.Container {
  private config: Required<AudioVisualizerConfig>;
  private graphics: Phaser.GameObjects.Graphics;
  private analyser: AnalyserNode | null = null;
  private dataArray: Uint8Array | null = null;
  private timeDataArray: Uint8Array | null = null;

  constructor(scene: Phaser.Scene, x: number, y: number, config: AudioVisualizerConfig = {}) {
    super(scene, x, y);

    // Default configuration
    this.config = {
      width: config.width || 300,
      height: config.height || 100,
      backgroundColor: config.backgroundColor || 0x222222,
      barColor: config.barColor || 0x00ff00,
      lineColor: config.lineColor || 0xff0000,
      showBars: config.showBars !== undefined ? config.showBars : true,
      showWaveform: config.showWaveform !== undefined ? config.showWaveform : true,
      barWidth: config.barWidth || 4,
      barSpacing: config.barSpacing || 1,
      fftSize: config.fftSize || 256,
    };

    // Create graphics object for visualization
    this.graphics = this.scene.add.graphics();
    this.add(this.graphics);

    // Set up update event
    this.scene.events.on('preupdate', this.preUpdate, this);
  }

  /**
   * Connect the visualizer to an audio node
   * @param audioNode The audio node to visualize
   */
  public connectTo(audioNode: AudioNode): void {
    try {
      // Create analyzer node
      const audioContext = audioNode.context;
      this.analyser = audioContext.createAnalyser();
      this.analyser.fftSize = this.config.fftSize;

      // Connect the audio node to the analyzer
      audioNode.connect(this.analyser);

      // Create data arrays for frequency and time domain data
      this.dataArray = new Uint8Array(this.analyser.frequencyBinCount);
      this.timeDataArray = new Uint8Array(this.analyser.frequencyBinCount);

      console.log('AudioVisualizer connected to audio node');
    } catch (error) {
      console.error('Failed to connect AudioVisualizer:', error);
    }
  }

  /**
   * Update the visualization
   */
  public preUpdate(): void {
    if (!this.analyser || !this.dataArray || !this.timeDataArray) return;

    // Get frequency data
    this.analyser.getByteFrequencyData(this.dataArray);
    this.analyser.getByteTimeDomainData(this.timeDataArray);

    // Clear previous frame
    this.clear();

    // Draw visualizations
    if (this.config.showBars) {
      this.drawBars();
    }

    if (this.config.showWaveform) {
      this.drawWaveform();
    }
  }

  /**
   * Clear the graphics
   */
  private clear(): void {
    this.graphics.clear();
    this.graphics.fillStyle(this.config.backgroundColor, 1);
    this.graphics.fillRect(
      -this.config.width / 2,
      -this.config.height / 2,
      this.config.width,
      this.config.height
    );
  }

  /**
   * Draw frequency bars
   */
  private drawBars(): void {
    if (!this.dataArray) return;

    const barCount = Math.floor(
      this.config.width / (this.config.barWidth + this.config.barSpacing)
    );
    const step = Math.floor(this.dataArray.length / barCount);

    this.graphics.fillStyle(this.config.barColor, 1);

    for (let i = 0; i < barCount; i++) {
      // Get average value for this frequency range
      let sum = 0;
      for (let j = 0; j < step; j++) {
        const index = i * step + j;
        if (index < this.dataArray.length) {
          sum += this.dataArray[index];
        }
      }
      const value = sum / step;

      // Calculate bar height (0-255 mapped to 0-height)
      const barHeight = (value / 255) * this.config.height;

      // Draw bar
      this.graphics.fillRect(
        -this.config.width / 2 + i * (this.config.barWidth + this.config.barSpacing),
        this.config.height / 2 - barHeight,
        this.config.barWidth,
        barHeight
      );
    }
  }

  /**
   * Draw waveform
   */
  private drawWaveform(): void {
    if (!this.timeDataArray) return;

    const sliceWidth = this.config.width / this.timeDataArray.length;

    this.graphics.lineStyle(2, this.config.lineColor, 1);
    this.graphics.beginPath();

    // Start at the left side
    this.graphics.moveTo(
      -this.config.width / 2,
      -this.config.height / 2 + (this.timeDataArray[0] / 255) * this.config.height
    );

    // Draw line segments
    for (let i = 1; i < this.timeDataArray.length; i++) {
      const x = -this.config.width / 2 + i * sliceWidth;
      const y = -this.config.height / 2 + (this.timeDataArray[i] / 255) * this.config.height;
      this.graphics.lineTo(x, y);
    }

    this.graphics.strokePath();
  }

  /**
   * Set the visualization mode
   * @param mode The visualization mode to set
   */
  public setMode(mode: VisualizationMode): void {
    switch (mode) {
      case 'bars':
        this.config.showBars = true;
        this.config.showWaveform = false;
        break;
      case 'waveform':
        this.config.showBars = false;
        this.config.showWaveform = true;
        break;
      case 'both':
        this.config.showBars = true;
        this.config.showWaveform = true;
        break;
      case 'none':
        this.config.showBars = false;
        this.config.showWaveform = false;
        break;
    }
  }

  /**
   * Clean up resources when destroying
   */
  public destroy(fromScene?: boolean): void {
    // Remove event listener
    this.scene.events.off('preupdate', this.preUpdate, this);

    // Call parent destroy
    super.destroy(fromScene);
  }
}
