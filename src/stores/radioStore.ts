import React, { createContext, useContext, useState } from 'react';

interface RadioState {
  // Radio state properties
  frequency: number;
  signalStrength: number;
  currentSignalId: string | null;
  staticIntensity: number;
  isScanning: boolean;
  showMessage: boolean;
  isDragging: boolean;

  // Methods
  setFrequency: (frequency: number) => void;
  setSignalStrength: (strength: number) => void;
  setCurrentSignalId: (id: string | null) => void;
  setStaticIntensity: (intensity: number) => void;
  setIsScanning: (isScanning: boolean) => void;
  setShowMessage: (show: boolean) => void;
  toggleShowMessage: () => void;
  setIsDragging: (isDragging: boolean) => void;
  resetState: () => void;
}

// Create context with a default undefined value
const RadioContext = createContext<RadioState | undefined>(undefined);

// Provider component
export const RadioProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [frequency, setFrequency] = useState<number>(90.0);
  const [signalStrength, setSignalStrength] = useState<number>(0);
  const [currentSignalId, setCurrentSignalId] = useState<string | null>(null);
  const [staticIntensity, setStaticIntensity] = useState<number>(0.5);
  const [isScanning, setIsScanning] = useState<boolean>(false);
  const [showMessage, setShowMessage] = useState<boolean>(false);
  const [isDragging, setIsDragging] = useState<boolean>(false);

  // Method implementations
  const toggleShowMessage = (): void => setShowMessage(!showMessage);

  const resetState = (): void => {
    setFrequency(90.0);
    setSignalStrength(0);
    setCurrentSignalId(null);
    setStaticIntensity(0.5);
    setIsScanning(false);
    setShowMessage(false);
    setIsDragging(false);
  };

  const value: RadioState = {
    frequency,
    signalStrength,
    currentSignalId,
    staticIntensity,
    isScanning,
    showMessage,
    isDragging,
    setFrequency,
    setSignalStrength,
    setCurrentSignalId,
    setStaticIntensity,
    setIsScanning,
    setShowMessage,
    toggleShowMessage,
    setIsDragging,
    resetState,
  };

  return React.createElement(RadioContext.Provider, { value }, children);
};

// Hook to use the radio context
export const useRadioStore = (): RadioState => {
  const context = useContext(RadioContext);
  if (context === undefined) {
    throw new Error('useRadioStore must be used within a RadioProvider');
  }
  return context;
};
