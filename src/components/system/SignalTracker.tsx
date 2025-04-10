import React, { useEffect } from 'react';
import { useSignalState } from '../../context/SignalStateContext';
import { useEvent } from '../../context/EventContext';
import { useGameState } from '../../context/GameStateContext';
import { Signal } from '../../types/signal.d';

interface SignalTrackerProps {
  onSignalDiscovered?: (signal: Signal) => void;
  onSignalUpdated?: (signal: Signal) => void;
  onActiveSignalChanged?: (signal: Signal | undefined) => void;
}

/**
 * SignalTracker component that manages signal state tracking
 * This component doesn't render anything but provides functionality
 * for tracking and processing signals
 */
const SignalTracker: React.FC<SignalTrackerProps> = ({
  onSignalDiscovered,
  onSignalUpdated,
  onActiveSignalChanged,
}) => {
  const {
    state: signalState,
    getActiveSignal,
    getSignalById,
    updateSignal,
    discoverSignal,
  } = useSignalState();

  const { state: gameState } = useGameState();
  const { dispatchEvent } = useEvent();

  // Track active signal changes
  useEffect(() => {
    const activeSignal = getActiveSignal();

    if (onActiveSignalChanged && activeSignal) {
      onActiveSignalChanged(activeSignal);
    }
  }, [signalState.activeSignalId, getActiveSignal, onActiveSignalChanged]);

  // Track newly discovered signals
  useEffect(() => {
    if (signalState.lastDiscoveredTimestamp === null) return;

    // Find the most recently discovered signal
    const lastDiscoveredId =
      signalState.discoveredSignalIds[signalState.discoveredSignalIds.length - 1];
    if (!lastDiscoveredId) return;

    const signal = getSignalById(lastDiscoveredId);
    if (!signal) return;

    // Notify about the discovered signal
    if (onSignalDiscovered) {
      onSignalDiscovered(signal);
    }

    // Dispatch an event for the discovered signal
    dispatchEvent('signal', {
      type: 'signal_discovered',
      signalId: signal.id,
      frequency: signal.frequency,
      signalType: signal.type,
      timestamp: Date.now(),
    });

    // If this is a message signal, also dispatch a message event
    if (signal.type === 'message') {
      dispatchEvent('signal', {
        type: 'new_message',
        signalId: signal.id,
        content: signal.content,
        timestamp: Date.now(),
      });
    }
  }, [
    signalState.lastDiscoveredTimestamp,
    signalState.discoveredSignalIds,
    getSignalById,
    dispatchEvent,
    onSignalDiscovered,
  ]);

  // Update signal strength based on current frequency
  useEffect(() => {
    // Get all signals
    const allSignals = Object.values(signalState.signals);

    // Update signal strength based on proximity to current frequency
    allSignals.forEach((signal) => {
      const frequencyDiff = Math.abs(signal.frequency - gameState.currentFrequency);
      const proximityThreshold = 0.5; // MHz

      if (frequencyDiff <= proximityThreshold) {
        // Calculate strength based on proximity (closer = stronger)
        const normalizedDiff = frequencyDiff / proximityThreshold;
        const newStrength = 1 - normalizedDiff;

        // Only update if strength has changed significantly
        if (Math.abs(newStrength - signal.strength) > 0.1) {
          updateSignal(signal.id, { strength: newStrength });

          // Notify about the updated signal
          if (onSignalUpdated) {
            onSignalUpdated({
              ...signal,
              strength: newStrength,
            });
          }

          // If signal is strong enough and not yet discovered, discover it
          if (newStrength > 0.8 && !signal.discovered) {
            discoverSignal(signal.id);
          }
        }
      }
    });
  }, [
    gameState.currentFrequency,
    signalState.signals,
    updateSignal,
    discoverSignal,
    onSignalUpdated,
  ]);

  // This component doesn't render anything
  return null;
};

export default SignalTracker;
