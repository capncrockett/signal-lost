// Create a mock for the AudioManager
export const mockAudioManager = {
  getInstance: jest.fn().mockReturnValue({
    setMasterVolume: jest.fn(),
    getMasterVolume: jest.fn().mockReturnValue(0.1),
    addVolumeChangeListener: jest.fn(),
    removeVolumeChangeListener: jest.fn(),
  }),
};
