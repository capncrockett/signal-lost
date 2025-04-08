import React, { forwardRef } from 'react';
import './Checkbox.css';

export interface CheckboxProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label: string;
  error?: string;
  helperText?: string;
}

/**
 * Checkbox component for boolean selection
 */
const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
  ({ label, error, helperText, className = '', id, ...props }, ref) => {
    const checkboxId = id || `checkbox-${Math.random().toString(36).substring(2, 9)}`;
    const checkboxClasses = ['checkbox', error ? 'checkbox-error' : '', className]
      .filter(Boolean)
      .join(' ');

    return (
      <div className="checkbox-container">
        <div className="checkbox-wrapper">
          <input
            ref={ref}
            type="checkbox"
            id={checkboxId}
            className={checkboxClasses}
            aria-invalid={!!error}
            data-testid="checkbox"
            {...props}
          />
          <label htmlFor={checkboxId} className="checkbox-label">
            {label}
          </label>
        </div>
        {(error || helperText) && (
          <div
            className={`checkbox-message ${
              error ? 'checkbox-error-message' : 'checkbox-helper-text'
            }`}
          >
            {error || helperText}
          </div>
        )}
      </div>
    );
  }
);

Checkbox.displayName = 'Checkbox';

export default Checkbox;
