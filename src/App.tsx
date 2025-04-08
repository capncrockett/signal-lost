import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import './App.css';
import RadioTuner from './components/radio/RadioTuner';

// Placeholder components - will be replaced with actual components later
const Home = () => <div className="page home-page">Home Page</div>;
const RadioPage = () => (
  <div className="page radio-tuner-page">
    <h2>Radio Tuner</h2>
    <RadioTuner />
  </div>
);
const FieldExploration = () => <div className="page field-exploration-page">Field Exploration</div>;

const App: React.FC = () => {
  return (
    <Router>
      <div className="app-container">
        <header className="app-header">
          <h1>Signal Lost</h1>
          <nav>
            <ul>
              <li><a href="/">Home</a></li>
              <li><a href="/radio">Radio</a></li>
              <li><a href="/field">Field</a></li>
            </ul>
          </nav>
        </header>
        <main className="app-content">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/radio" element={<RadioPage />} />
            <Route path="/field" element={<FieldExploration />} />
          </Routes>
        </main>
        <footer className="app-footer">
          <p>Signal Lost - A narrative exploration game</p>
        </footer>
      </div>
    </Router>
  );
};

export default App;
