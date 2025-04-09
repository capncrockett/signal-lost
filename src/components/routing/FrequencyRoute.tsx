import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useGameState } from '../../context/GameStateContext';

export interface FrequencyRouteProps {
  children: React.ReactNode;
  requiredFrequency: number;
  redirectTo?: string;
  tolerance?: number;
}

/**
 * FrequencyRoute component for guarding routes based on discovered frequencies
 */
const FrequencyRoute: React.FC<FrequencyRouteProps> = ({
  children,
  requiredFrequency,
  redirectTo = '/radio',
  tolerance = 0.1,
}) => {
  const { state } = useGameState();
  const location = useLocation();

  // Check if the player has discovered the required frequency
  const hasDiscoveredFrequency = state.discoveredFrequencies.some(
    (freq) => Math.abs(freq - requiredFrequency) <= tolerance
  );

  // If the player hasn't discovered the frequency, redirect to the specified route
  if (!hasDiscoveredFrequency) {
    return <Navigate to={redirectTo} state={{ from: location }} replace />;
  }

  // If the player has discovered the frequency, render the children
  return <>{children}</>;
};

export default FrequencyRoute;
