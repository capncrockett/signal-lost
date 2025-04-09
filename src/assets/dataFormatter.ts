/**
 * Utility functions for formatting and optimizing game data
 */

/**
 * Format options for game data
 */
export interface DataFormatOptions {
  minify?: boolean;
  removeNulls?: boolean;
  removeEmptyStrings?: boolean;
  removeEmptyArrays?: boolean;
  removeEmptyObjects?: boolean;
  convertDatesToISOString?: boolean;
}

/**
 * Default format options
 */
export const DEFAULT_FORMAT_OPTIONS: DataFormatOptions = {
  minify: true,
  removeNulls: true,
  removeEmptyStrings: false,
  removeEmptyArrays: false,
  removeEmptyObjects: false,
  convertDatesToISOString: true,
};

/**
 * Format game data for optimal loading and storage
 * @param data - The data to format
 * @param options - Format options
 * @returns Formatted data
 */
export const formatGameDataForStorage = <T>(
  data: T,
  options: DataFormatOptions = DEFAULT_FORMAT_OPTIONS
): T => {
  if (!data) return data;

  // Handle arrays
  if (Array.isArray(data)) {
    const formattedArray = data
      .map((item) => formatGameDataForStorage(item, options))
      .filter((item: unknown) => {
        if (options.removeNulls && item === null) return false;
        if (options.removeEmptyStrings && item === '') return false;
        if (options.removeEmptyArrays && Array.isArray(item) && item.length === 0) return false;
        if (
          options.removeEmptyObjects &&
          typeof item === 'object' &&
          item !== null &&
          !Array.isArray(item) &&
          Object.keys(item).length === 0
        )
          return false;
        return true;
      });
    return formattedArray as T;
  }

  // Handle objects
  if (typeof data === 'object' && data !== null) {
    const formattedObject: Record<string, unknown> = {};

    for (const [key, value] of Object.entries(data)) {
      // Skip null values if removeNulls is true
      if (options.removeNulls && value === null) continue;

      // Skip empty strings if removeEmptyStrings is true
      if (options.removeEmptyStrings && value === '') continue;

      // Skip empty arrays if removeEmptyArrays is true
      if (options.removeEmptyArrays && Array.isArray(value) && value.length === 0) continue;

      // Skip empty objects if removeEmptyObjects is true
      if (
        options.removeEmptyObjects &&
        typeof value === 'object' &&
        value !== null &&
        !Array.isArray(value) &&
        Object.keys(value).length === 0
      )
        continue;

      // Convert dates to ISO strings if convertDatesToISOString is true
      if (options.convertDatesToISOString && value instanceof Date) {
        formattedObject[key] = value.toISOString();
      } else if (typeof value === 'object' && value !== null) {
        // Recursively format nested objects and arrays
        formattedObject[key] = formatGameDataForStorage(value, options);
      } else {
        formattedObject[key] = value;
      }
    }

    return formattedObject as T;
  }

  // Return primitive values as is
  return data;
};

/**
 * Minify JSON data by removing whitespace
 * @param jsonData - The JSON data to minify
 * @returns Minified JSON string
 */
export const minifyJson = (jsonData: string): string => {
  try {
    const parsed = JSON.parse(jsonData);
    return JSON.stringify(parsed);
  } catch (error) {
    console.error('Error minifying JSON:', error);
    return jsonData;
  }
};

/**
 * Pretty print JSON data with indentation
 * @param jsonData - The JSON data to format
 * @param spaces - Number of spaces for indentation
 * @returns Formatted JSON string
 */
export const prettyPrintJson = (jsonData: string, spaces = 2): string => {
  try {
    const parsed = JSON.parse(jsonData);
    return JSON.stringify(parsed, null, spaces);
  } catch (error) {
    console.error('Error formatting JSON:', error);
    return jsonData;
  }
};

/**
 * Optimize game data for storage
 * @param data - The data to optimize
 * @returns Optimized data as a string
 */
export const optimizeForStorage = <T extends Record<string, unknown>>(data: T): string => {
  const formatted = formatGameDataForStorage(data, {
    ...DEFAULT_FORMAT_OPTIONS,
    removeEmptyStrings: true,
    removeEmptyArrays: true,
    removeEmptyObjects: true,
  });
  return JSON.stringify(formatted);
};

/**
 * Parse optimized data from storage
 * @param data - The optimized data string
 * @returns Parsed data
 */
export const parseFromStorage = <T extends Record<string, unknown>>(data: string): T => {
  try {
    return JSON.parse(data) as T;
  } catch (error) {
    console.error('Error parsing data from storage:', error);
    throw error;
  }
};
