import React from 'react';
import { Link } from 'react-router-dom';
import './Pages.css';

/**
 * Home page component
 */
const Home: React.FC = () => {
  return (
    <div className="page home-page" data-testid="home-page">
      <h1 className="page-title" data-testid="home-page-title">
        Signal Lost
      </h1>

      <div className="page-content" data-testid="home-page-content">
        <p className="page-description">
          Welcome to Signal Lost, a narrative exploration game where you uncover the mystery behind
          strange radio signals and their connection to an abandoned research facility.
        </p>

        <div className="page-actions" data-testid="home-page-actions">
          <Link to="/radio" className="page-button" data-testid="home-to-radio-button">
            Start Tuning
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Home;
