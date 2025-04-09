import React, { useEffect, useState } from 'react';
import { preloadAssets, AssetEntry } from '../../assets/assetUtils';
import { LoadingIndicator } from './index';
import './AssetLoader.css';

export interface AssetLoaderProps {
  assets: AssetEntry[];
  children: React.ReactNode;
  loadingText?: string;
  onLoadComplete?: () => void;
  fallback?: React.ReactNode;
}

/**
 * Component for preloading assets before rendering children
 */
const AssetLoader: React.FC<AssetLoaderProps> = ({
  assets,
  children,
  loadingText = 'Loading assets...',
  onLoadComplete,
  fallback,
}) => {
  const [isLoading, setIsLoading] = useState(true);
  const [progress, setProgress] = useState(0);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadAssets = async (): Promise<void> => {
      if (assets.length === 0) {
        setIsLoading(false);
        if (onLoadComplete) onLoadComplete();
        return;
      }

      try {
        // Load assets one by one to track progress
        let loaded = 0;
        for (const asset of assets) {
          try {
            await preloadAssets([asset]);
            loaded++;
            setProgress(Math.floor((loaded / assets.length) * 100));
          } catch (assetError) {
            console.error(`Failed to load asset ${asset.name}:`, assetError);
            // Continue loading other assets even if one fails
          }
        }

        setIsLoading(false);
        if (onLoadComplete) onLoadComplete();
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Unknown error loading assets';
        setError(errorMessage);
        console.error('Asset loading failed:', err);
      }
    };

    loadAssets().catch((err) => {
      console.error('Unexpected error in loadAssets:', err);
    });
  }, [assets, onLoadComplete]);

  if (error) {
    return (
      <div className="asset-loader-error" data-testid="asset-loader-error">
        <h2>Error Loading Assets</h2>
        <p>{error}</p>
        <button
          onClick={() => window.location.reload()}
          className="asset-loader-retry-button"
          data-testid="asset-loader-retry-button"
        >
          Retry
        </button>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="asset-loader-container" data-testid="asset-loader">
        {fallback || (
          <>
            <LoadingIndicator size="large" />
            <p className="asset-loader-text">{loadingText}</p>
            <div
              className="asset-loader-progress-container"
              data-testid="asset-loader-progress-container"
            >
              <div
                className="asset-loader-progress-bar"
                style={{ width: `${progress}%` }}
                data-testid="asset-loader-progress-bar"
              />
              <span className="asset-loader-progress-text" data-testid="asset-loader-progress-text">
                {progress}%
              </span>
            </div>
          </>
        )}
      </div>
    );
  }

  return <>{children}</>;
};

export default AssetLoader;
