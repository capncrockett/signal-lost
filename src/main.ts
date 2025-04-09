// This file is kept for compatibility but is no longer used
// The main entry point is now main.tsx

console.warn('main.ts is deprecated. The application now uses main.tsx as the entry point.');

// Add global error handler
window.addEventListener('error', (event) => {
  console.error('Global error:', event.message, event.filename, event.lineno, event.error);
});

export {}; // This makes the file a module
