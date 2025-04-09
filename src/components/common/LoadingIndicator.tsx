import React from 'react';
import './LoadingIndicator.css';

export interface LoadingIndicatorProps {
  size?: 'small' | 'medium' | 'large';
  color?: string;
  fullPage?: boolean;
  text?: string;
}

/**
 * LoadingIndicator component for showing loading states
 */
const LoadingIndicator: React.FC<LoadingIndicatorProps> = ({
  size = 'medium',
  color,
  fullPage = false,
  text,
}) => {
  const containerClasses = [
    'loading-indicator-container',
    fullPage ? 'loading-indicator-fullpage' : '',
  ]
    .filter(Boolean)
    .join(' ');

  const spinnerClasses = [`loading-indicator-spinner`, `loading-indicator-${size}`]
    .filter(Boolean)
    .join(' ');

  const spinnerStyle = color ? { borderTopColor: color } : undefined;

  return (
    <div className={containerClasses} data-testid="loading-indicator">
      <div className={spinnerClasses} style={spinnerStyle} />
      {text && <p className="loading-indicator-text">{text}</p>}
    </div>
  );
};

export default LoadingIndicator;
