/**
 * Utility functions for asset management
 */

/**
 * Get the URL for an image asset
 * @param imageName - The name of the image file
 * @returns The URL to the image
 */
export const getImageUrl = (imageName: string): string => {
  return `/assets/images/${imageName}`;
};

/**
 * Get the URL for an audio asset
 * @param audioName - The name of the audio file
 * @returns The URL to the audio file
 */
export const getAudioUrl = (audioName: string): string => {
  return `/assets/audio/${audioName}`;
};

/**
 * Preload an image
 * @param imageName - The name of the image file
 * @returns A promise that resolves when the image is loaded
 */
export const preloadImage = (imageName: string): Promise<HTMLImageElement> => {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.src = getImageUrl(imageName);
    img.onload = () => resolve(img);
    img.onerror = (error) => reject(error);
  });
};

/**
 * Preload multiple images
 * @param imageNames - Array of image file names
 * @returns A promise that resolves when all images are loaded
 */
export const preloadImages = (imageNames: string[]): Promise<HTMLImageElement[]> => {
  return Promise.all(imageNames.map(preloadImage));
};

/**
 * Preload an audio file
 * @param audioName - The name of the audio file
 * @returns A promise that resolves when the audio is loaded
 */
export const preloadAudio = (audioName: string): Promise<AudioBuffer> => {
  return new Promise((resolve, reject) => {
    const audioContext = new AudioContext();
    fetch(getAudioUrl(audioName))
      .then((response) => response.arrayBuffer())
      .then((arrayBuffer) => audioContext.decodeAudioData(arrayBuffer))
      .then((audioBuffer) => resolve(audioBuffer))
      .catch((error) => reject(error));
  });
};

/**
 * Load a JSON data file
 * @param path - The path to the JSON file
 * @returns A promise that resolves with the parsed JSON data
 */
export const loadJsonData = async <T>(path: string): Promise<T> => {
  const response = await fetch(path);
  if (!response.ok) {
    throw new Error(`Failed to load data from ${path}: ${response.statusText}`);
  }
  return response.json() as Promise<T>;
};

/**
 * Asset types that can be preloaded
 */
export enum AssetType {
  IMAGE = 'image',
  AUDIO = 'audio',
  DATA = 'data',
}

/**
 * Asset entry for preloading
 */
export interface AssetEntry {
  type: AssetType;
  name: string;
  path: string;
}

/**
 * Preload multiple assets of different types
 * @param assets - Array of asset entries
 * @returns A promise that resolves when all assets are loaded
 */
export const preloadAssets = async (assets: AssetEntry[]): Promise<void> => {
  const promises = assets.map((asset) => {
    switch (asset.type) {
      case AssetType.IMAGE:
        return preloadImage(asset.name);
      case AssetType.AUDIO:
        return preloadAudio(asset.name);
      case AssetType.DATA:
        return loadJsonData(asset.path);
      default:
        return Promise.resolve();
    }
  });

  await Promise.all(promises);
};
