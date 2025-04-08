import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';
import { GameStateProvider } from './context/GameStateContext';
import { AudioProvider } from './context/AudioContext';

// Create root element if it doesn't exist
const rootElement =
  document.getElementById('root') ||
  (() => {
    const root = document.createElement('div');
    root.id = 'root';
    document.body.appendChild(root);
    return root;
  })();

// Render the React application
ReactDOM.createRoot(rootElement).render(
  <React.StrictMode>
    <GameStateProvider>
      <AudioProvider>
        <App />
      </AudioProvider>
    </GameStateProvider>
  </React.StrictMode>
);
