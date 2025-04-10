import React, { ReactNode } from 'react';
import { GameStateProvider } from './GameStateContext';
import { SignalStateProvider } from './SignalStateContext';
import { EventProvider } from './EventContext';
import { ProgressProvider } from './ProgressContext';
import { AudioProvider } from './AudioContext';
import { TriggerProvider } from './TriggerContext';
import TriggerSystem from '../components/system/TriggerSystem';

// Make sure all providers are properly exported

interface CombinedGameProviderProps {
  children: ReactNode;
  persistState?: boolean;
  triggerConfigUrl?: string;
}

/**
 * Combined provider that wraps all game-related context providers
 * This ensures proper nesting and data flow between contexts
 */
export const CombinedGameProvider: React.FC<CombinedGameProviderProps> = ({
  children,
  persistState = true,
  triggerConfigUrl = '/assets/config/triggers.json',
}) => {
  return (
    <GameStateProvider persistState={persistState}>
      <SignalStateProvider persistState={persistState}>
        <EventProvider persistState={persistState}>
          <ProgressProvider persistState={persistState}>
            <TriggerProvider persistState={persistState}>
              <AudioProvider>
                <TriggerSystem triggerConfigUrl={triggerConfigUrl}>{children}</TriggerSystem>
              </AudioProvider>
            </TriggerProvider>
          </ProgressProvider>
        </EventProvider>
      </SignalStateProvider>
    </GameStateProvider>
  );
};

export default CombinedGameProvider;
