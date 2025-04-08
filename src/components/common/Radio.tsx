import React, { forwardRef } from 'react';
import './Radio.css';

export interface RadioOption {
  value: string;
  label: string;
  disabled?: boolean;
}

export interface RadioProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label?: string;
  options: RadioOption[];
  error?: string;
  helperText?: string;
  direction?: 'horizontal' | 'vertical';
}

/**
 * Radio component for single selection from multiple options
 */
const Radio = forwardRef<HTMLInputElement, RadioProps>(
  (
    { label, options, error, helperText, direction = 'vertical', className = '', name, ...props },
    ref
  ) => {
    const radioGroupId = `radio-group-${Math.random().toString(36).substring(2, 9)}`;
    const radioClasses = ['radio', error ? 'radio-error' : '', className].filter(Boolean).join(' ');

    return (
      <div className="radio-container">
        {label && (
          <div className="radio-group-label" id={radioGroupId}>
            {label}
          </div>
        )}
        <div
          className={`radio-options radio-options-${direction}`}
          role="radiogroup"
          aria-labelledby={radioGroupId}
        >
          {options.map((option, index) => {
            const optionId = `radio-${name}-${option.value}`;
            return (
              <div key={option.value} className="radio-option">
                <input
                  ref={index === 0 ? ref : undefined}
                  type="radio"
                  id={optionId}
                  name={name}
                  value={option.value}
                  className={radioClasses}
                  disabled={option.disabled}
                  aria-invalid={!!error}
                  data-testid={`radio-${option.value}`}
                  {...props}
                />
                <label htmlFor={optionId} className="radio-label">
                  {option.label}
                </label>
              </div>
            );
          })}
        </div>
        {(error || helperText) && (
          <div className={`radio-message ${error ? 'radio-error-message' : 'radio-helper-text'}`}>
            {error || helperText}
          </div>
        )}
      </div>
    );
  }
);

Radio.displayName = 'Radio';

export default Radio;
