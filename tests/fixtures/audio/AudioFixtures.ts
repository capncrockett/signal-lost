import { AudioFixture, FixtureCollection } from '../types';

/**
 * Audio fixtures
 */
export const AudioFixtures: FixtureCollection<Record<string, unknown>> = {
  id: 'audio',
  fixtures: [
    {
      id: 'defaultSettings',
      data: {
        masterVolume: 0.8,
        musicVolume: 0.5,
        sfxVolume: 0.8,
        ambientVolume: 0.6,
        muted: false,
      },
      metadata: {
        description: 'Default audio settings',
        tags: ['audio', 'settings', 'default'],
      },
    },
    {
      id: 'mutedSettings',
      data: {
        masterVolume: 0.8,
        musicVolume: 0.5,
        sfxVolume: 0.8,
        ambientVolume: 0.6,
        muted: true,
        previousVolume: 0.8,
      },
      metadata: {
        description: 'Muted audio settings',
        tags: ['audio', 'settings', 'muted'],
      },
    },
    {
      id: 'lowVolume',
      data: {
        masterVolume: 0.2,
        musicVolume: 0.1,
        sfxVolume: 0.2,
        ambientVolume: 0.1,
        muted: false,
      },
      metadata: {
        description: 'Low volume audio settings',
        tags: ['audio', 'settings', 'low'],
      },
    },
    {
      id: 'noMusic',
      data: {
        masterVolume: 0.8,
        musicVolume: 0,
        sfxVolume: 0.8,
        ambientVolume: 0.6,
        muted: false,
      },
      metadata: {
        description: 'Audio settings with no music',
        tags: ['audio', 'settings', 'noMusic'],
      },
    },
    {
      id: 'soundscapeDefault',
      data: {
        staticVolume: 0.5,
        droneVolume: 0.3,
        blipVolume: 0.2,
        staticPan: 0,
        dronePan: 0,
        blipPan: 0,
        blipInterval: [5000, 15000],
        blipDuration: [200, 800],
        initialized: true,
      },
      metadata: {
        description: 'Default soundscape settings',
        tags: ['audio', 'soundscape', 'default'],
      },
    },
    {
      id: 'soundscapeHighInterference',
      data: {
        staticVolume: 0.8,
        droneVolume: 0.2,
        blipVolume: 0.4,
        staticPan: 0,
        dronePan: 0,
        blipPan: 0,
        blipInterval: [2000, 8000],
        blipDuration: [300, 1200],
        initialized: true,
      },
      metadata: {
        description: 'Soundscape settings with high interference',
        tags: ['audio', 'soundscape', 'interference'],
      },
    },
    {
      id: 'soundscapeClear',
      data: {
        staticVolume: 0.1,
        droneVolume: 0.5,
        blipVolume: 0.1,
        staticPan: 0,
        dronePan: 0,
        blipPan: 0,
        blipInterval: [10000, 20000],
        blipDuration: [100, 400],
        initialized: true,
      },
      metadata: {
        description: 'Soundscape settings with clear signal',
        tags: ['audio', 'soundscape', 'clear'],
      },
    },
    {
      id: 'audioBuffers',
      data: {
        buffers: {
          'static': {
            duration: 2.0,
            sampleRate: 44100,
            numberOfChannels: 1,
          },
          'drone': {
            duration: 10.0,
            sampleRate: 44100,
            numberOfChannels: 2,
          },
          'blip': {
            duration: 0.5,
            sampleRate: 44100,
            numberOfChannels: 1,
          },
          'click': {
            duration: 0.1,
            sampleRate: 44100,
            numberOfChannels: 1,
          },
        },
      },
      metadata: {
        description: 'Audio buffer mock data',
        tags: ['audio', 'buffers', 'mock'],
      },
    },
  ],
  metadata: {
    description: 'Audio fixtures for testing',
    author: 'Signal Lost Team',
    createdAt: '2023-06-01',
    updatedAt: '2023-06-15',
  },
};
