import { create } from 'zustand';
import { NoiseType } from '../audio/NoiseType';

interface RadioState {
  // Radio state
  frequency: number;
  signalStrength: number;
  currentSignalId: string | null;
  staticIntensity: number;
  isScanning: boolean;
  showMessage: boolean;
  isDragging: boolean;
  
  // Actions
  setFrequency: (frequency: number) => void;
  setSignalStrength: (strength: number) => void;
  setCurrentSignalId: (id: string | null) => void;
  setStaticIntensity: (intensity: number) => void;
  setIsScanning: (isScanning: boolean) => void;
  setShowMessage: (show: boolean) => void;
  toggleShowMessage: () => void;
  setIsDragging: (isDragging: boolean) => void;
  
  // Reset state (for cleanup)
  resetState: () => void;
}

export const useRadioStore = create<RadioState>((set) => ({
  // Initial state
  frequency: 90.0,
  signalStrength: 0,
  currentSignalId: null,
  staticIntensity: 0.5,
  isScanning: false,
  showMessage: false,
  isDragging: false,
  
  // Actions
  setFrequency: (frequency) => set({ frequency }),
  setSignalStrength: (strength) => set({ signalStrength: strength }),
  setCurrentSignalId: (id) => set({ currentSignalId: id }),
  setStaticIntensity: (intensity) => set({ staticIntensity: intensity }),
  setIsScanning: (isScanning) => set({ isScanning }),
  setShowMessage: (show) => set({ showMessage: show }),
  toggleShowMessage: () => set((state) => ({ showMessage: !state.showMessage })),
  setIsDragging: (isDragging) => set({ isDragging }),
  
  // Reset state (for cleanup)
  resetState: () => set({
    signalStrength: 0,
    currentSignalId: null,
    staticIntensity: 0.5,
    isScanning: false,
    showMessage: false,
    isDragging: false,
  }),
}));
