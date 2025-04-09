import React, { Suspense } from 'react';
import { LoadingIndicator } from '../common';

export interface LazyRouteProps {
  component: React.LazyExoticComponent<React.ComponentType<unknown>>;
  fallback?: React.ReactNode;
  props?: Record<string, unknown>;
}

/**
 * LazyRoute component for lazy loading route components
 */
const LazyRoute: React.FC<LazyRouteProps> = ({ component: Component, fallback, props = {} }) => {
  return (
    <Suspense
      fallback={
        fallback || (
          <div className="lazy-route-loading" data-testid="lazy-route-loading">
            <LoadingIndicator size="large" text="Loading..." />
          </div>
        )
      }
    >
      <Component {...props} />
    </Suspense>
  );
};

export default LazyRoute;
