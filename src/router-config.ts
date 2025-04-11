// Configure React Router future flags
// This file should be imported before any React Router components are used

// Define the global types for React Router future flags
declare global {
  interface Window {
    __reactRouterVersion?: string;
    __reactRouterFuture?: {
      v7_startTransition?: boolean;
      v7_relativeSplatPath?: boolean;
    };
  }
}

// Set future flags to opt-in to React Router v7 behavior
window.__reactRouterVersion = '6.4.0';
window.__reactRouterFuture = {
  v7_startTransition: true,
  v7_relativeSplatPath: true,
};

export {};
