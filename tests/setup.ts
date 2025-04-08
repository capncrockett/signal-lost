// Mock the Tone.js library
jest.mock('tone', () => {
  return {
    Destination: {
      volume: {
        value: 0,
      },
    },
    Noise: jest.fn().mockImplementation(() => {
      return {
        connect: jest.fn().mockReturnThis(),
        start: jest.fn(),
        stop: jest.fn(),
        dispose: jest.fn(),
      };
    }),
    Oscillator: jest.fn().mockImplementation(() => {
      return {
        connect: jest.fn().mockReturnThis(),
        start: jest.fn(),
        stop: jest.fn(),
        dispose: jest.fn(),
      };
    }),
    Filter: jest.fn().mockImplementation(() => {
      return {
        toDestination: jest.fn().mockReturnThis(),
        dispose: jest.fn(),
      };
    }),
    gainToDb: jest.fn().mockImplementation((gain) => gain * 20),
  };
});

// Mock the window.matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(), // Deprecated
    removeListener: jest.fn(), // Deprecated
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
});

// Mock the ResizeObserver
global.ResizeObserver = jest.fn().mockImplementation(() => ({
  observe: jest.fn(),
  unobserve: jest.fn(),
  disconnect: jest.fn(),
}));

// Mock the AudioContext
window.AudioContext = jest.fn().mockImplementation(() => {
  return {
    createOscillator: jest.fn().mockImplementation(() => {
      return {
        connect: jest.fn(),
        start: jest.fn(),
        stop: jest.fn(),
        frequency: {
          setValueAtTime: jest.fn(),
        },
      };
    }),
    createGain: jest.fn().mockImplementation(() => {
      return {
        connect: jest.fn(),
        gain: {
          setValueAtTime: jest.fn(),
        },
      };
    }),
    destination: {},
    currentTime: 0,
  };
});

// Mock the canvas context
HTMLCanvasElement.prototype.getContext = jest.fn().mockImplementation(() => {
  return {
    fillRect: jest.fn(),
    clearRect: jest.fn(),
    getImageData: jest.fn().mockReturnValue({
      data: new Array(4),
    }),
    putImageData: jest.fn(),
    createImageData: jest.fn().mockReturnValue([]),
    setTransform: jest.fn(),
    drawImage: jest.fn(),
    save: jest.fn(),
    restore: jest.fn(),
    scale: jest.fn(),
    rotate: jest.fn(),
    translate: jest.fn(),
    transform: jest.fn(),
    fillText: jest.fn(),
    strokeText: jest.fn(),
    measureText: jest.fn().mockReturnValue({ width: 0 }),
    createLinearGradient: jest.fn().mockReturnValue({
      addColorStop: jest.fn(),
    }),
    createRadialGradient: jest.fn().mockReturnValue({
      addColorStop: jest.fn(),
    }),
    createPattern: jest.fn().mockReturnValue({}),
    beginPath: jest.fn(),
    closePath: jest.fn(),
    moveTo: jest.fn(),
    lineTo: jest.fn(),
    bezierCurveTo: jest.fn(),
    quadraticCurveTo: jest.fn(),
    arc: jest.fn(),
    arcTo: jest.fn(),
    ellipse: jest.fn(),
    rect: jest.fn(),
    fill: jest.fn(),
    stroke: jest.fn(),
    clip: jest.fn(),
    isPointInPath: jest.fn(),
    isPointInStroke: jest.fn(),
    fillStyle: '',
    strokeStyle: '',
    lineWidth: 0,
    lineCap: '',
    lineJoin: '',
    miterLimit: 0,
    shadowOffsetX: 0,
    shadowOffsetY: 0,
    shadowBlur: 0,
    shadowColor: '',
    font: '',
    textAlign: '',
    textBaseline: '',
    globalAlpha: 0,
    globalCompositeOperation: '',
    imageSmoothingEnabled: false,
  };
});
