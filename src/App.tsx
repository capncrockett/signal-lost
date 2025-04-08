import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import './App.css';
import RadioTuner from './components/radio/RadioTuner';
import { AssetLoader } from './components/common';
import { ESSENTIAL_ASSETS } from './assets';

// Placeholder components - will be replaced with actual components later
const Home = () => (
  <div className="page home-page" data-testid="home-page">
    Home Page
  </div>
);
const RadioPage = () => (
  <div className="page radio-tuner-page" data-testid="radio-page">
    <h2 data-testid="radio-page-title">Radio Tuner</h2>
    <RadioTuner data-testid="radio-tuner" />
  </div>
);
const FieldExploration = () => (
  <div className="page field-exploration-page" data-testid="field-page">
    Field Exploration
  </div>
);

const App: React.FC = () => {
  // Track loading state for future enhancements
  const [, setAssetsLoaded] = useState(false);

  const handleLoadComplete = (): void => {
    setAssetsLoaded(true);
    console.log('Assets loaded successfully');
  };

  return (
    <AssetLoader
      assets={ESSENTIAL_ASSETS}
      onLoadComplete={handleLoadComplete}
      loadingText="Loading Signal Lost..."
      data-testid="app-asset-loader"
    >
      <Router>
        <div className="app-container" data-testid="app-container">
          <header className="app-header" data-testid="app-header">
            <h1 data-testid="app-title">Signal Lost</h1>
            <nav data-testid="app-nav">
              <ul>
                <li>
                  <Link to="/" data-testid="nav-home">
                    Home
                  </Link>
                </li>
                <li>
                  <Link to="/radio" data-testid="nav-radio">
                    Radio
                  </Link>
                </li>
                <li>
                  <Link to="/field" data-testid="nav-field">
                    Field
                  </Link>
                </li>
              </ul>
            </nav>
          </header>
          <main className="app-content" data-testid="app-content">
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/radio" element={<RadioPage />} />
              <Route path="/field" element={<FieldExploration />} />
            </Routes>
          </main>
          <footer className="app-footer" data-testid="app-footer">
            <p>Signal Lost - A narrative exploration game</p>
          </footer>
        </div>
      </Router>
    </AssetLoader>
  );
};

export default App;
