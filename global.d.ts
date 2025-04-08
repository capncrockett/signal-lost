// Global type definitions
interface Window {
  webkitAudioContext: typeof AudioContext;
  // game is defined in multiple places, so we'll use a more specific type
  game: unknown;
  fpsValues?: number[];
  fpsMonitoringInterval?: number;
}
