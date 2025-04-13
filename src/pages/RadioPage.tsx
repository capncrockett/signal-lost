import React from 'react';
import { Link } from 'react-router-dom';
import ZustandRadioTuner from '../components/radio/ZustandRadioTuner';
import './Pages.css';

/**
 * Radio page component
 */
const RadioPage: React.ComponentType<unknown> = () => {
  return (
    <div className="page radio-page" data-testid="radio-page">
      <h1 className="page-title" data-testid="radio-page-title">
        Radio Tuner
      </h1>

      <div className="page-content" data-testid="radio-page-content">
        <p className="page-description">
          Use the radio tuner to search for signals. Adjust the frequency to find hidden messages
          and locations.
        </p>

        <div className="radio-tuner-container" data-testid="radio-tuner-container">
          <ZustandRadioTuner data-testid="radio-tuner" />
        </div>

        <div className="page-actions" data-testid="radio-page-actions">
          <Link to="/" className="page-button secondary" data-testid="radio-to-home-button">
            Back to Home
          </Link>
          <Link to="/field" className="page-button" data-testid="radio-to-field-button">
            Explore Field
          </Link>
        </div>
      </div>
    </div>
  );
};

export default RadioPage;
