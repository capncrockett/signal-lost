import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';
import { CombinedGameProvider } from './context/CombinedGameProvider';

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
    <CombinedGameProvider>
      <App />
    </CombinedGameProvider>
  </React.StrictMode>
);
