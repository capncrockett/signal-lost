import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import RadioTuner from './RadioTuner';
import { GameStateProvider } from '../../context/GameStateContext';
import { AudioProvider } from '../../context/AudioContext';

// Mock the audio context
jest.mock('../../context/AudioContext', () => {
  const originalModule = jest.requireActual('../../context/AudioContext');
  return {
    ...originalModule,
    useAudio: () => ({
      playStaticNoise: jest.fn(),
      stopStaticNoise: jest.fn(),
      playSignal: jest.fn(),
      stopSignal: jest.fn(),
      volume: 0.5,
      isMuted: false,
      setVolume: jest.fn(),
      toggleMute: jest.fn(),
      setNoiseType: jest.fn(),
      currentNoiseType: 'pink',
    }),
  };
});

// Mock the Zustand store
jest.mock('../../stores/radioStore', () => {
  const actualStore = jest.requireActual('../../stores/radioStore');
  const mockStore = {
    ...actualStore,
    useRadioStore: () => ({
      frequency: 90.0,
      signalStrength: 0,
      currentSignalId: null,
      staticIntensity: 0.5,
      isScanning: false,
      showMessage: false,
      isDragging: false,
      setFrequency: jest.fn(),
      setSignalStrength: jest.fn(),
      setCurrentSignalId: jest.fn(),
      setStaticIntensity: jest.fn(),
      setIsScanning: jest.fn(),
      setShowMessage: jest.fn(),
      toggleShowMessage: jest.fn(),
      setIsDragging: jest.fn(),
      resetState: jest.fn(),
    }),
  };
  return mockStore;
});

describe('RadioTuner Diagnostic Test', () => {
  const renderRadioTuner = () => {
    return render(
      <GameStateProvider>
        <AudioProvider>
          <RadioTuner />
        </AudioProvider>
      </GameStateProvider>
    );
  };

  test('Dial interaction diagnostic', () => {
    // Render the component
    renderRadioTuner();

    // Get the dial element
    const dial = screen.getByRole('slider', { name: /frequency dial/i });

    // Log initial state
    console.log('Initial dial position:', dial.querySelector('.tuner-dial-knob')?.style.left);

    // Turn on the radio
    const powerButton = screen.getByRole('button', { name: /turn radio on/i });
    fireEvent.click(powerButton);

    // Log state after turning on
    console.log(
      'Dial position after turning on:',
      dial.querySelector('.tuner-dial-knob')?.style.left
    );

    // Simulate mouse events on the dial
    fireEvent.mouseDown(dial);

    // Log state after mouse down
    console.log('isDragging after mouseDown:', (window as any).isDragging);

    // Simulate mouse move
    fireEvent.mouseMove(dial, { clientX: 100, clientY: 50 });

    // Log state after mouse move
    console.log(
      'Dial position after mouse move:',
      dial.querySelector('.tuner-dial-knob')?.style.left
    );

    // Simulate mouse up
    fireEvent.mouseUp(dial);

    // Log final state
    console.log('Final dial position:', dial.querySelector('.tuner-dial-knob')?.style.left);
    console.log('isDragging after mouseUp:', (window as any).isDragging);
  });
});
