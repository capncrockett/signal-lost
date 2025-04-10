import React, { useEffect, useState } from 'react';
import { TriggerProvider } from '../../context/TriggerContext';
import TriggerManager from './TriggerManager';
import { Trigger } from '../../utils/ConditionalTrigger';

interface TriggerSystemProps {
  triggerConfigUrl?: string; // URL to load trigger configuration from
  initialTriggers?: Trigger[]; // Initial triggers to use
  checkInterval?: number; // How often to check triggers in milliseconds
  children?: React.ReactNode;
}

/**
 * TriggerSystem component that sets up the trigger system
 * This component loads trigger configurations and manages the trigger system
 */
const TriggerSystem: React.FC<TriggerSystemProps> = ({
  triggerConfigUrl,
  initialTriggers = [],
  checkInterval = 1000,
  children,
}) => {
  const [triggers, setTriggers] = useState<Trigger[]>(initialTriggers);
  const [isLoading, setIsLoading] = useState<boolean>(!!triggerConfigUrl);
  const [error, setError] = useState<string | null>(null);

  // Load trigger configuration from URL if provided
  useEffect(() => {
    if (!triggerConfigUrl) {
      return;
    }

    const loadTriggerConfig = async (): Promise<void> => {
      try {
        setIsLoading(true);
        const response = await fetch(triggerConfigUrl);
        if (!response.ok) {
          throw new Error(`Failed to load trigger configuration: ${response.statusText}`);
        }

        const config = (await response.json()) as Trigger[];
        setTriggers((prevTriggers) => [...prevTriggers, ...config]);
        setError(null);
      } catch (err) {
        console.error('Error loading trigger configuration:', err);
        setError(
          err instanceof Error ? err.message : 'Unknown error loading trigger configuration'
        );
      } finally {
        setIsLoading(false);
      }
    };

    void loadTriggerConfig();
  }, [triggerConfigUrl]);

  return (
    <TriggerProvider initialTriggers={triggers}>
      {!isLoading && (
        <TriggerManager triggers={triggers} checkInterval={checkInterval}>
          {error && <div className="trigger-system-error">{error}</div>}
          {children}
        </TriggerManager>
      )}
    </TriggerProvider>
  );
};

export default TriggerSystem;
