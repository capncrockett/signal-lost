import React, { ReactNode } from 'react';
import GameStateProvider from './GameStateContext';
import SignalStateProvider from './SignalStateContext';
import EventProvider from './EventContext';
import ProgressProvider from './ProgressContext';
import AudioProvider from './AudioContext';
import { TriggerProvider } from './TriggerContext';
import TriggerSystem from '../components/system/TriggerSystem';
import GameStateIntegration from '../components/system/GameStateIntegration';

interface CombinedGameProviderProps {
  children: ReactNode;
  persistState?: boolean;
  enableGameStateIntegration?: boolean;
  autoSaveInterval?: number;
  triggerConfigUrl?: string;
}

/**
 * Combined provider that wraps all game-related context providers
 * This ensures proper nesting and data flow between contexts
 */
export const CombinedGameProvider: React.FC<CombinedGameProviderProps> = ({
  children,
  persistState = true,
  enableGameStateIntegration = true,
  autoSaveInterval = 5 * 60 * 1000, // 5 minutes
  triggerConfigUrl = '/assets/config/triggers.json',
}) => {
  return (
    <GameStateProvider persistState={persistState}>
      <SignalStateProvider persistState={persistState}>
        <EventProvider persistState={persistState}>
          <ProgressProvider persistState={persistState}>
            <TriggerProvider persistState={persistState}>
              <AudioProvider>
                {enableGameStateIntegration && (
                  <GameStateIntegration
                    triggerUrls={['/assets/narrative/events.json']}
                    autoSaveInterval={autoSaveInterval}
                  />
                )}
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
