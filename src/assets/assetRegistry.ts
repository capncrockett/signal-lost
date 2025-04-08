import { AssetType, AssetEntry } from './assetUtils';

/**
 * Registry of all game assets
 */

// Image assets
export const IMAGE_ASSETS = {
  // UI Elements
  RADIO: 'radio.png',
  SIGNAL_STRENGTH_INDICATOR: 'signalStrengthIndicator.png',
  SIGNAL_DETECTOR: 'signalDetector.png',
  MESSAGE_DECODER: 'messageDecoder.png',
  JOURNAL: 'journal.png',
  STATIC_OVERLAY: 'staticOverlay.png',

  // Backgrounds
  MENU_BACKGROUND: 'menuBackground.png',
  GAME_TITLE_SCREEN: 'gameTitleScreen.png',

  // Field Scene
  PLAYER: 'player.png',
  TILES: 'tiles.png',
  FOREST_PATH: 'forestPath.png',
  CLEARING: 'clearing.png',
  RUINS: 'ruins.png',
  TOWER: 'tower.png',
  WEATHER_EFFECTS: 'weatherEffects.png',

  // Story Elements
  MYSTERIOUS_FIGURE: 'mysteriousFigure.png',
  MYSTERIOUS_SYMBOLS: 'mysteriousSymbols.png',
  STRANGE_ARTIFACT: 'strangeArtifact.png',
  SIGNAL_WAVE_PATTERNS: 'signalWavePatterns.png',
};

// Audio assets
export const AUDIO_ASSETS = {
  STATIC: 'static.mp3',
};

// Data assets
export const DATA_ASSETS = {
  EVENTS: '/assets/narrative/events.json',
  NEW_EVENTS: '/assets/narrative/new_events.json',
  FIELD_MAP: '/assets/maps/field.json',
};

/**
 * Complete list of all assets for preloading
 */
export const ALL_ASSETS: AssetEntry[] = [
  // Images
  ...Object.entries(IMAGE_ASSETS).map(([_, name]) => ({
    type: AssetType.IMAGE,
    name,
    path: `/assets/images/${name}`,
  })),

  // Audio
  ...Object.entries(AUDIO_ASSETS).map(([_, name]) => ({
    type: AssetType.AUDIO,
    name,
    path: `/assets/audio/${name}`,
  })),

  // Data
  ...Object.entries(DATA_ASSETS).map(([_, path]) => ({
    type: AssetType.DATA,
    name: path.split('/').pop() || '',
    path,
  })),
];

/**
 * Essential assets that should be preloaded before the game starts
 */
export const ESSENTIAL_ASSETS: AssetEntry[] = [
  // Essential UI images
  {
    type: AssetType.IMAGE,
    name: IMAGE_ASSETS.RADIO,
    path: `/assets/images/${IMAGE_ASSETS.RADIO}`,
  },
  {
    type: AssetType.IMAGE,
    name: IMAGE_ASSETS.SIGNAL_STRENGTH_INDICATOR,
    path: `/assets/images/${IMAGE_ASSETS.SIGNAL_STRENGTH_INDICATOR}`,
  },
  {
    type: AssetType.IMAGE,
    name: IMAGE_ASSETS.STATIC_OVERLAY,
    path: `/assets/images/${IMAGE_ASSETS.STATIC_OVERLAY}`,
  },

  // Essential audio
  {
    type: AssetType.AUDIO,
    name: AUDIO_ASSETS.STATIC,
    path: `/assets/audio/${AUDIO_ASSETS.STATIC}`,
  },

  // Essential data
  {
    type: AssetType.DATA,
    name: 'events.json',
    path: DATA_ASSETS.EVENTS,
  },
];
