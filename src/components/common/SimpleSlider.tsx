import React from 'react';
import './SimpleSlider.css';

interface SimpleSliderProps {
  min: number;
  max: number;
  step: number;
  value: number;
  onChange: (value: number) => void;
  disabled?: boolean;
  className?: string;
}

const SimpleSlider: React.FC<SimpleSliderProps> = ({
  min,
  max,
  step,
  value,
  onChange,
  disabled = false,
  className = '',
}) => {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>): void => {
    onChange(parseFloat(e.target.value));
  };

  return (
    <input
      type="range"
      min={min}
      max={max}
      step={step}
      value={value}
      onChange={handleChange}
      disabled={disabled}
      className={`simple-slider ${className}`}
      style={{ width: '100%' }}
    />
  );
};

export default SimpleSlider;
