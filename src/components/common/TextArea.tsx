import React, { forwardRef } from 'react';
import './TextArea.css';

export interface TextAreaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  error?: string;
  helperText?: string;
  fullWidth?: boolean;
}

/**
 * TextArea component for multi-line text entry
 */
const TextArea = forwardRef<HTMLTextAreaElement, TextAreaProps>(
  ({ label, error, helperText, fullWidth = false, className = '', id, ...props }, ref) => {
    const textAreaId = id || `textarea-${Math.random().toString(36).substring(2, 9)}`;
    const textAreaClasses = [
      'textarea',
      error ? 'textarea-error' : '',
      fullWidth ? 'textarea-full-width' : '',
      className,
    ]
      .filter(Boolean)
      .join(' ');

    return (
      <div className={`textarea-container ${fullWidth ? 'textarea-container-full-width' : ''}`}>
        {label && (
          <label htmlFor={textAreaId} className="textarea-label">
            {label}
          </label>
        )}
        <textarea
          ref={ref}
          id={textAreaId}
          className={textAreaClasses}
          aria-invalid={!!error}
          data-testid="textarea"
          {...props}
        />
        {(error || helperText) && (
          <div
            className={`textarea-message ${
              error ? 'textarea-error-message' : 'textarea-helper-text'
            }`}
          >
            {error || helperText}
          </div>
        )}
      </div>
    );
  }
);

TextArea.displayName = 'TextArea';

export default TextArea;
