/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-unsafe-call */
/* eslint-disable @typescript-eslint/no-unsafe-return */
import { create } from 'zustand';

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

export const useRadioStore = create<RadioState>((set: any) => ({
  // Initial state
  frequency: 90.0,
  signalStrength: 0,
  currentSignalId: null,
  staticIntensity: 0.5,
  isScanning: false,
  showMessage: false,
  isDragging: false,

  // Actions
  setFrequency: (frequency: number) => set({ frequency }),
  setSignalStrength: (strength: number) => set({ signalStrength: strength }),
  setCurrentSignalId: (id: string | null) => set({ currentSignalId: id }),
  setStaticIntensity: (intensity: number) => set({ staticIntensity: intensity }),
  setIsScanning: (isScanning: boolean) => set({ isScanning }),
  setShowMessage: (show: boolean) => set({ showMessage: show }),
  toggleShowMessage: () => set((state: RadioState) => ({ showMessage: !state.showMessage })),
  setIsDragging: (isDragging: boolean) => set({ isDragging }),

  // Reset state (for cleanup)
  resetState: () =>
    set({
      signalStrength: 0,
      currentSignalId: null,
      staticIntensity: 0.5,
      isScanning: false,
      showMessage: false,
      isDragging: false,
    }),
}));
