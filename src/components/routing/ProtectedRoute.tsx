import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useGameState } from '../../context/GameStateContext';

export interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredProgress?: number;
  requiredItems?: string[];
  redirectTo?: string;
}

/**
 * ProtectedRoute component for guarding routes based on game progress
 */
const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requiredProgress = 0,
  requiredItems = [],
  redirectTo = '/',
}) => {
  const { state } = useGameState();
  const location = useLocation();

  // Check if the player has the required progress
  const hasRequiredProgress = state.gameProgress >= requiredProgress;

  // Check if the player has all required items
  const hasRequiredItems = requiredItems.every((item) => state.inventory.includes(item));

  // If the player doesn't meet the requirements, redirect to the specified route
  if (!hasRequiredProgress || !hasRequiredItems) {
    return <Navigate to={redirectTo} state={{ from: location }} replace />;
  }

  // If the player meets the requirements, render the children
  return <>{children}</>;
};

export default ProtectedRoute;
