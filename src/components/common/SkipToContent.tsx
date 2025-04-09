import React from 'react';
import '../../styles/accessibility.css';

export interface SkipToContentProps {
  contentId: string;
  label?: string;
}

/**
 * SkipToContent component for keyboard accessibility
 * Allows keyboard users to skip navigation and go directly to main content
 */
const SkipToContent: React.FC<SkipToContentProps> = ({ contentId, label = 'Skip to content' }) => {
  return (
    <a href={`#${contentId}`} className="skip-to-content" data-testid="skip-to-content">
      {label}
    </a>
  );
};

export default SkipToContent;
