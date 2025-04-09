/**
 * Utility functions for asset optimization
 */

/**
 * Image optimization settings
 */
export interface ImageOptimizationSettings {
  maxWidth?: number;
  maxHeight?: number;
  quality?: number;
  format?: 'webp' | 'jpeg' | 'png';
}

/**
 * Audio optimization settings
 */
export interface AudioOptimizationSettings {
  sampleRate?: number;
  channels?: number;
  bitRate?: number;
  format?: 'mp3' | 'ogg' | 'wav';
}

/**
 * Default image optimization settings
 */
export const DEFAULT_IMAGE_SETTINGS: ImageOptimizationSettings = {
  maxWidth: 1024,
  maxHeight: 1024,
  quality: 80,
  format: 'webp',
};

/**
 * Default audio optimization settings
 */
export const DEFAULT_AUDIO_SETTINGS: AudioOptimizationSettings = {
  sampleRate: 44100,
  channels: 2,
  bitRate: 128,
  format: 'mp3',
};

/**
 * Get optimized image URL with responsive size
 * @param imageName - The name of the image file
 * @param width - The desired width
 * @param height - The desired height
 * @returns The URL to the optimized image
 */
export const getOptimizedImageUrl = (
  imageName: string,
  width?: number,
  height?: number
): string => {
  // In a real implementation, this would use an image CDN or server-side
  // image optimization. For now, we'll just return the original image URL
  // with query parameters for documentation purposes.
  const baseUrl = `/assets/images/${imageName}`;

  if (!width && !height) {
    return baseUrl;
  }

  const params = new URLSearchParams();
  if (width) params.append('w', width.toString());
  if (height) params.append('h', height.toString());

  return `${baseUrl}?${params.toString()}`;
};

/**
 * Get the appropriate image size based on screen width
 * @param baseSize - The base size of the image
 * @param screenWidth - The current screen width
 * @returns The appropriate size for the current screen
 */
export const getResponsiveImageSize = (baseSize: number, screenWidth: number): number => {
  if (screenWidth < 640) {
    return Math.round(baseSize * 0.5); // 50% of original for small screens
  } else if (screenWidth < 1024) {
    return Math.round(baseSize * 0.75); // 75% of original for medium screens
  }
  return baseSize; // 100% of original for large screens
};

/**
 * Preload an image with optimized settings
 * @param imageName - The name of the image file
 * @param settings - Optimization settings
 * @returns A promise that resolves when the image is loaded
 */
export const preloadOptimizedImage = (
  imageName: string,
  settings: ImageOptimizationSettings = DEFAULT_IMAGE_SETTINGS
): Promise<HTMLImageElement> => {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.src = getOptimizedImageUrl(imageName, settings.maxWidth, settings.maxHeight);
    img.onload = () => resolve(img);
    img.onerror = (error) => reject(error);
  });
};

/**
 * Format JSON data for optimal loading
 * @param data - The data to format
 * @returns Formatted data
 */
export const formatGameData = <T>(data: T): T => {
  // In a real implementation, this would optimize the data structure
  // For now, we'll just return the original data
  return data;
};

/**
 * Create a responsive image srcset
 * @param imageName - The name of the image file
 * @param sizes - Array of sizes to include in the srcset
 * @returns A srcset string for responsive images
 */
export const createResponsiveSrcSet = (
  imageName: string,
  sizes: number[] = [320, 640, 960, 1280, 1920]
): string => {
  return sizes.map((size) => `${getOptimizedImageUrl(imageName, size)} ${size}w`).join(', ');
};

/**
 * Create a sizes attribute for responsive images
 * @param breakpoints - Object mapping CSS breakpoints to image widths
 * @returns A sizes string for responsive images
 */
export const createResponsiveSizes = (
  breakpoints: Record<string, string> = {
    '(max-width: 640px)': '100vw',
    '(max-width: 1024px)': '50vw',
    default: '33vw',
  }
): string => {
  return Object.entries(breakpoints)
    .map(([breakpoint, size]) => (breakpoint === 'default' ? size : `${breakpoint} ${size}`))
    .join(', ');
};
