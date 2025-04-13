import { create } from 'zustand';

interface RadioState {
  // UI state
  showMessage: boolean;
  isScanning: boolean;
  signalStrength: number;
  staticIntensity: number;
  currentSignalId: string | null;

  // Actions
  setShowMessage: (show: boolean) => void;
  toggleMessage: () => void;
  setIsScanning: (scanning: boolean) => void;
  toggleScanning: () => void;
  setSignalStrength: (strength: number) => void;
  setStaticIntensity: (intensity: number) => void;
  setCurrentSignalId: (id: string | null) => void;
}

export const useRadioStore = create<RadioState>((set) => ({
  // UI state
  showMessage: false,
  isScanning: false,
  signalStrength: 0,
  staticIntensity: 0.5,
  currentSignalId: null,

  // Actions
  setShowMessage: (show) => set({ showMessage: show }),
  toggleMessage: () => set((state) => ({ showMessage: !state.showMessage })),
  setIsScanning: (scanning) => set({ isScanning: scanning }),
  toggleScanning: () => set((state) => ({ isScanning: !state.isScanning })),
  setSignalStrength: (strength) => set({ signalStrength: strength }),
  setStaticIntensity: (intensity) => set({ staticIntensity: intensity }),
  setCurrentSignalId: (id) => set({ currentSignalId: id }),
}));
