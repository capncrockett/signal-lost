import React, { ReactNode } from 'react';
import { GameStateProvider } from './GameStateContext';
import { SignalStateProvider } from './SignalStateContext';
import { EventProvider } from './EventContext';
import { ProgressProvider } from './ProgressContext';
import { AudioProvider } from './AudioContext';

// Make sure all providers are properly exported

interface CombinedGameProviderProps {
  children: ReactNode;
  persistState?: boolean;
}

/**
 * Combined provider that wraps all game-related context providers
 * This ensures proper nesting and data flow between contexts
 */
export const CombinedGameProvider: React.FC<CombinedGameProviderProps> = ({
  children,
  persistState = true,
}) => {
  return (
    <GameStateProvider persistState={persistState}>
      <SignalStateProvider persistState={persistState}>
        <EventProvider persistState={persistState}>
          <ProgressProvider persistState={persistState}>
            <AudioProvider>{children}</AudioProvider>
          </ProgressProvider>
        </EventProvider>
      </SignalStateProvider>
    </GameStateProvider>
  );
};

export default CombinedGameProvider;
