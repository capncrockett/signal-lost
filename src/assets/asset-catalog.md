# Signal Lost Asset Catalog

This document catalogs all assets used in the Signal Lost game, organized by type and usage.

## Images

### UI Elements
- `radio.png` - Radio tuner interface
- `signalStrengthIndicator.png` - Indicator for signal strength
- `signalDetector.png` - Device for detecting signals
- `messageDecoder.png` - Interface for decoding messages
- `journal.png` - Player's journal interface
- `staticOverlay.png` - Static overlay effect for radio interference

### Backgrounds
- `menuBackground.png` - Main menu background
- `gameTitleScreen.png` - Title screen background

### Field Scene
- `player.png` - Player character sprite
- `tiles.png` - Tileset for field environment
- `forestPath.png` - Forest path background
- `clearing.png` - Clearing area background
- `ruins.png` - Ruins area background
- `tower.png` - Radio tower background
- `weatherEffects.png` - Weather effect overlays

### Story Elements
- `mysteriousFigure.png` - Mysterious figure sprite
- `mysteriousSymbols.png` - Mysterious symbols found in-game
- `strangeArtifact.png` - Strange artifact item
- `signalWavePatterns.png` - Signal wave pattern visualizations

## Audio

### Sound Effects
- `static.mp3` - Radio static sound

## Data

### Game Data
- `events.json` - Narrative events data
- `new_events.json` - Updated narrative events data
- `field.json` - Field scene map data

## Usage Notes

1. All image assets should be loaded using React's import system for better bundling
2. Audio assets should be handled through the Web Audio API
3. Data files should be imported directly in the relevant components
4. Consider converting some static images to CSS/SVG for better performance
5. Add responsive alternatives for key UI elements
