// Define the shape of our game state
export interface GameState {
  currentFrequency: number;
  discoveredFrequencies: number[];
  currentLocation: string;
  inventory: string[];
  gameProgress: number;
  isRadioOn: boolean;
}
