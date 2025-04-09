import React from 'react';
import {
  getOptimizedImageUrl,
  createResponsiveSrcSet,
  createResponsiveSizes,
} from '../../assets/assetOptimizer';
import './ResponsiveImage.css';

export interface ResponsiveImageProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  imageName: string;
  alt: string;
  sizes?: string;
  widths?: number[];
  fallbackWidth?: number;
  lazyLoad?: boolean;
  objectFit?: 'contain' | 'cover' | 'fill' | 'none' | 'scale-down';
}

/**
 * ResponsiveImage component for displaying optimized, responsive images
 */
const ResponsiveImage: React.FC<ResponsiveImageProps> = ({
  imageName,
  alt,
  sizes,
  widths = [320, 640, 960, 1280, 1920],
  fallbackWidth = 640,
  lazyLoad = true,
  objectFit = 'cover',
  className = '',
  ...props
}) => {
  // Generate srcset for responsive images
  const srcSet = createResponsiveSrcSet(imageName, widths);

  // Use provided sizes or generate default
  const sizeAttr = sizes || createResponsiveSizes();

  // Get fallback src for browsers that don't support srcset
  const src = getOptimizedImageUrl(imageName, fallbackWidth);

  // Combine classes
  const imageClasses = ['responsive-image', `object-fit-${objectFit}`, className]
    .filter(Boolean)
    .join(' ');

  return (
    <img
      src={src}
      srcSet={srcSet}
      sizes={sizeAttr}
      alt={alt}
      loading={lazyLoad ? 'lazy' : undefined}
      className={imageClasses}
      data-testid={`image-${imageName.replace(/\.[^/.]+$/, '')}`}
      {...props}
    />
  );
};

export default ResponsiveImage;
