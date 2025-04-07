/**
 * Mock implementation of Web Audio API for testing
 */

// Mock AudioContext
export class AudioContext {
  // Properties
  public destination: AudioDestinationNode;
  public currentTime: number = 0;
  public sampleRate: number = 44100;
  public state: AudioContextState = 'running';

  constructor() {
    this.destination = new AudioDestinationNode(this);
  }

  // Methods
  public createGain(): GainNode {
    return new GainNode(this);
  }

  public createOscillator(): OscillatorNode {
    return new OscillatorNode(this);
  }

  public createStereoPanner(): StereoPannerNode {
    return new StereoPannerNode(this);
  }

  public createAnalyser(): AnalyserNode {
    return new AnalyserNode(this);
  }

  public createBufferSource(): AudioBufferSourceNode {
    return new AudioBufferSourceNode(this);
  }

  public createBuffer(
    numberOfChannels: number,
    length: number,
    sampleRate: number
  ): AudioBuffer {
    return new AudioBuffer({
      numberOfChannels,
      length,
      sampleRate,
    });
  }

  public resume(): Promise<void> {
    this.state = 'running';
    return Promise.resolve();
  }

  public suspend(): Promise<void> {
    this.state = 'suspended';
    return Promise.resolve();
  }

  public close(): Promise<void> {
    this.state = 'closed';
    return Promise.resolve();
  }
}

// Mock AudioNode
export class AudioNode {
  public context: AudioContext;

  constructor(context: AudioContext) {
    this.context = context;
  }

  public connect(destination: AudioNode): AudioNode {
    return destination;
  }

  public disconnect(): void {
    // No-op in mock
  }
}

// Mock AudioParam
export class AudioParam {
  public value: number = 0;

  constructor(defaultValue: number = 0) {
    this.value = defaultValue;
  }

  public setValueAtTime(value: number, _startTime: number): AudioParam {
    this.value = value;
    return this;
  }

  public linearRampToValueAtTime(value: number, _endTime: number): AudioParam {
    this.value = value;
    return this;
  }

  public exponentialRampToValueAtTime(value: number, _endTime: number): AudioParam {
    this.value = value;
    return this;
  }

  public setTargetAtTime(target: number, _startTime: number, _timeConstant: number): AudioParam {
    this.value = target;
    return this;
  }
}

// Mock GainNode
export class GainNode extends AudioNode {
  public gain: AudioParam;

  constructor(context: AudioContext) {
    super(context);
    this.gain = new AudioParam(1);
  }
}

// Mock OscillatorNode
export class OscillatorNode extends AudioNode {
  public frequency: AudioParam;
  public detune: AudioParam;
  public type: OscillatorType = 'sine';

  constructor(context: AudioContext) {
    super(context);
    this.frequency = new AudioParam(440);
    this.detune = new AudioParam(0);
  }

  public start(_when?: number): void {
    // No-op in mock
  }

  public stop(_when?: number): void {
    // No-op in mock
  }
}

// Mock StereoPannerNode
export class StereoPannerNode extends AudioNode {
  public pan: AudioParam;

  constructor(context: AudioContext) {
    super(context);
    this.pan = new AudioParam(0);
  }
}

// Mock AnalyserNode
export class AnalyserNode extends AudioNode {
  public fftSize: number = 2048;
  public frequencyBinCount: number = 1024;
  public minDecibels: number = -100;
  public maxDecibels: number = -30;
  public smoothingTimeConstant: number = 0.8;

  constructor(context: AudioContext) {
    super(context);
  }

  public getFloatFrequencyData(array: Float32Array): void {
    // Fill with random data
    for (let i = 0; i < array.length; i++) {
      array[i] = Math.random() * (this.maxDecibels - this.minDecibels) + this.minDecibels;
    }
  }

  public getByteFrequencyData(array: Uint8Array): void {
    // Fill with random data
    for (let i = 0; i < array.length; i++) {
      array[i] = Math.floor(Math.random() * 256);
    }
  }

  public getFloatTimeDomainData(array: Float32Array): void {
    // Fill with random data
    for (let i = 0; i < array.length; i++) {
      array[i] = Math.random() * 2 - 1;
    }
  }

  public getByteTimeDomainData(array: Uint8Array): void {
    // Fill with random data
    for (let i = 0; i < array.length; i++) {
      array[i] = Math.floor(Math.random() * 256);
    }
  }
}

// Mock AudioBufferSourceNode
export class AudioBufferSourceNode extends AudioNode {
  public buffer: AudioBuffer | null = null;
  public loop: boolean = false;
  public loopStart: number = 0;
  public loopEnd: number = 0;
  public playbackRate: AudioParam;

  constructor(context: AudioContext) {
    super(context);
    this.playbackRate = new AudioParam(1);
  }

  public start(_when?: number, _offset?: number, _duration?: number): void {
    // No-op in mock
  }

  public stop(_when?: number): void {
    // No-op in mock
  }
}

// Mock AudioBuffer
export class AudioBuffer {
  public length: number;
  public duration: number;
  public sampleRate: number;
  public numberOfChannels: number;
  private channels: Float32Array[];

  constructor(options: {
    numberOfChannels: number;
    length: number;
    sampleRate: number;
  }) {
    this.numberOfChannels = options.numberOfChannels;
    this.length = options.length;
    this.sampleRate = options.sampleRate;
    this.duration = options.length / options.sampleRate;
    this.channels = Array(options.numberOfChannels)
      .fill(0)
      .map(() => new Float32Array(options.length));
  }

  public getChannelData(channel: number): Float32Array {
    if (channel >= this.numberOfChannels) {
      throw new Error('Channel index out of bounds');
    }
    return this.channels[channel];
  }

  public copyFromChannel(destination: Float32Array, channelNumber: number, startInChannel?: number): void {
    const source = this.getChannelData(channelNumber);
    const start = startInChannel || 0;
    for (let i = 0; i < destination.length; i++) {
      if (i + start < source.length) {
        destination[i] = source[i + start];
      }
    }
  }

  public copyToChannel(source: Float32Array, channelNumber: number, startInChannel?: number): void {
    const destination = this.getChannelData(channelNumber);
    const start = startInChannel || 0;
    for (let i = 0; i < source.length; i++) {
      if (i + start < destination.length) {
        destination[i + start] = source[i];
      }
    }
  }
}

// Mock AudioDestinationNode
export class AudioDestinationNode extends AudioNode {
  public maxChannelCount: number = 2;
  public channelCount: number = 2;
  public channelCountMode: ChannelCountMode = 'explicit';
  public channelInterpretation: ChannelInterpretation = 'speakers';

  constructor(context: AudioContext) {
    super(context);
  }
}

// Type definitions
export type AudioContextState = 'suspended' | 'running' | 'closed';
export type OscillatorType = 'sine' | 'square' | 'sawtooth' | 'triangle' | 'custom';
export type ChannelCountMode = 'max' | 'clamped-max' | 'explicit';
export type ChannelInterpretation = 'speakers' | 'discrete';
