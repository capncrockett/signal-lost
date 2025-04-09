import React, { useState, lazy } from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import './App.css';
import './styles/accessibility.css';
import RadioTuner from './components/radio/RadioTuner';
import { AssetLoader, SkipToContent } from './components/common';
import GameMenuButton from './components/system/GameMenuButton';
import { RouteTransition, LazyRoute } from './components/routing';
import { ESSENTIAL_ASSETS } from './assets';

// Lazy load page components
const Home = lazy(() => import('./pages/Home'));
const RadioPage = lazy(() => import('./pages/RadioPage'));
const FieldExploration = lazy(() => import('./pages/FieldExploration'));

// Fallback components for development until pages are implemented
const HomeFallback = () => (
  <div className="page home-page" data-testid="home-page">
    Home Page
  </div>
);
const RadioPageFallback = () => (
  <div className="page radio-tuner-page" data-testid="radio-page">
    <h2 data-testid="radio-page-title">Radio Tuner</h2>
    <RadioTuner data-testid="radio-tuner" />
  </div>
);
const FieldExplorationFallback = () => (
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
          <SkipToContent contentId="main-content" />
          <GameMenuButton />
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
          <main className="app-content" id="main-content" data-testid="app-content" tabIndex={-1}>
            <RouteTransition>
              <Routes>
                <Route
                  path="/"
                  element={<LazyRoute component={Home} fallback={<HomeFallback />} />}
                />
                <Route
                  path="/radio"
                  element={<LazyRoute component={RadioPage} fallback={<RadioPageFallback />} />}
                />
                <Route
                  path="/field"
                  element={
                    <LazyRoute
                      component={FieldExploration}
                      fallback={<FieldExplorationFallback />}
                    />
                  }
                />
              </Routes>
            </RouteTransition>
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
