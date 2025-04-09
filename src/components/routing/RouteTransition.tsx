import React, { useRef, useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import './RouteTransition.css';

export interface RouteTransitionProps {
  children: React.ReactNode;
  timeout?: number;
  classNames?: {
    enter?: string;
    enterActive?: string;
    exit?: string;
    exitActive?: string;
  };
}

/**
 * RouteTransition component for animating route changes
 */
const RouteTransition: React.FC<RouteTransitionProps> = ({
  children,
  timeout = 300,
  classNames = {
    enter: 'fade-enter',
    enterActive: 'fade-enter-active',
    exit: 'fade-exit',
    exitActive: 'fade-exit-active',
  },
}) => {
  const location = useLocation();
  const [displayLocation, setDisplayLocation] = useState(location);
  const [transitionStage, setTransitionStage] = useState('enter');
  const prevLocationRef = useRef(location);

  useEffect(() => {
    // Skip transition on initial render
    if (prevLocationRef.current.pathname === location.pathname) {
      return;
    }

    // Start exit animation
    setTransitionStage('exit');

    // After exit animation completes, update the location and start enter animation
    const exitTimeout = setTimeout(() => {
      setDisplayLocation(location);
      setTransitionStage('enter');
      prevLocationRef.current = location;
    }, timeout);

    return () => clearTimeout(exitTimeout);
  }, [location, timeout]);

  // Determine the current transition class
  const transitionClass =
    transitionStage === 'enter'
      ? `${classNames.enter || ''} ${classNames.enterActive || ''}`
      : `${classNames.exit || ''} ${classNames.exitActive || ''}`;

  return (
    <div
      className={`route-transition ${transitionClass}`}
      style={{ animationDuration: `${timeout}ms` }}
      data-testid="route-transition"
    >
      {React.Children.map(children, (child) => {
        if (React.isValidElement(child)) {
          return React.cloneElement(child, {
            key: displayLocation.pathname,
          });
        }
        return child;
      })}
    </div>
  );
};

export default RouteTransition;
