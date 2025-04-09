import React from 'react';
import { Link } from 'react-router-dom';
import './Pages.css';

/**
 * Field exploration page component
 */
const FieldExploration: React.FC = () => {
  return (
    <div className="page field-page" data-testid="field-page">
      <h1 className="page-title" data-testid="field-page-title">
        Field Exploration
      </h1>

      <div className="page-content" data-testid="field-page-content">
        <p className="page-description">
          Explore the field to find clues and items related to the mysterious signals. Navigate
          through different areas and interact with objects.
        </p>

        <div className="field-placeholder" data-testid="field-placeholder">
          <div className="field-grid" data-testid="field-grid">
            {/* Placeholder for field grid */}
            <div className="field-cell player" data-testid="field-player"></div>
            {Array(24)
              .fill(null)
              .map((_, index) => (
                <div key={index} className="field-cell" data-testid={`field-cell-${index}`}></div>
              ))}
          </div>
        </div>

        <div className="page-actions" data-testid="field-page-actions">
          <Link to="/radio" className="page-button secondary" data-testid="field-to-radio-button">
            Back to Radio
          </Link>
          <Link to="/" className="page-button" data-testid="field-to-home-button">
            Return Home
          </Link>
        </div>
      </div>
    </div>
  );
};

export default FieldExploration;
