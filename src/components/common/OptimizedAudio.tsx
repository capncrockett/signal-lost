import React, { useRef, useEffect, useState } from 'react';
import { getAudioUrl } from '../../assets/assetUtils';
import './OptimizedAudio.css';

export interface OptimizedAudioProps {
  audioName: string;
  autoPlay?: boolean;
  loop?: boolean;
  controls?: boolean;
  volume?: number;
  muted?: boolean;
  onEnded?: () => void;
  onPlay?: () => void;
  onPause?: () => void;
  preload?: 'auto' | 'metadata' | 'none';
  className?: string;
}

/**
 * OptimizedAudio component for playing optimized audio files
 */
const OptimizedAudio: React.FC<OptimizedAudioProps> = ({
  audioName,
  autoPlay = false,
  loop = false,
  controls = true,
  volume = 1,
  muted = false,
  onEnded,
  onPlay,
  onPause,
  preload = 'metadata',
  className = '',
}) => {
  const audioRef = useRef<HTMLAudioElement>(null);
  const [isLoaded, setIsLoaded] = useState(false);
  const [isPlaying, setIsPlaying] = useState(autoPlay);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);

  // Set volume and muted state
  useEffect(() => {
    if (audioRef.current) {
      audioRef.current.volume = volume;
      audioRef.current.muted = muted;
    }
  }, [volume, muted]);

  // Handle audio events
  useEffect(() => {
    const audioElement = audioRef.current;
    if (!audioElement) return;

    const handleLoadedMetadata = (): void => {
      setDuration(audioElement.duration);
      setIsLoaded(true);
    };

    const handleTimeUpdate = (): void => {
      setCurrentTime(audioElement.currentTime);
    };

    const handlePlay = (): void => {
      setIsPlaying(true);
      if (onPlay) onPlay();
    };

    const handlePause = (): void => {
      setIsPlaying(false);
      if (onPause) onPause();
    };

    const handleEnded = (): void => {
      setIsPlaying(false);
      if (onEnded) onEnded();
    };

    // Add event listeners
    audioElement.addEventListener('loadedmetadata', handleLoadedMetadata);
    audioElement.addEventListener('timeupdate', handleTimeUpdate);
    audioElement.addEventListener('play', handlePlay);
    audioElement.addEventListener('pause', handlePause);
    audioElement.addEventListener('ended', handleEnded);

    // Clean up event listeners
    return () => {
      audioElement.removeEventListener('loadedmetadata', handleLoadedMetadata);
      audioElement.removeEventListener('timeupdate', handleTimeUpdate);
      audioElement.removeEventListener('play', handlePlay);
      audioElement.removeEventListener('pause', handlePause);
      audioElement.removeEventListener('ended', handleEnded);
    };
  }, [onEnded, onPlay, onPause]);

  // Custom play/pause controls
  const togglePlayPause = (): void => {
    if (audioRef.current) {
      if (isPlaying) {
        audioRef.current.pause();
      } else {
        audioRef.current.play().catch((error) => {
          console.error('Error playing audio:', error);
        });
      }
    }
  };

  // Format time in MM:SS format
  const formatTime = (timeInSeconds: number): string => {
    const minutes = Math.floor(timeInSeconds / 60);
    const seconds = Math.floor(timeInSeconds % 60);
    return `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
  };

  // Handle seek
  const handleSeek = (e: React.ChangeEvent<HTMLInputElement>): void => {
    const newTime = parseFloat(e.target.value);
    if (audioRef.current) {
      audioRef.current.currentTime = newTime;
      setCurrentTime(newTime);
    }
  };

  // Get audio source URL
  const audioSrc = getAudioUrl(audioName);

  // Combine classes
  const audioClasses = ['optimized-audio', className].filter(Boolean).join(' ');

  return (
    <div className={audioClasses} data-testid={`audio-${audioName.replace(/\.[^/.]+$/, '')}`}>
      <audio
        ref={audioRef}
        src={audioSrc}
        autoPlay={autoPlay}
        loop={loop}
        controls={controls}
        preload={preload}
        data-testid="audio-element"
      />

      {!controls && isLoaded && (
        <div className="custom-audio-controls" data-testid="custom-audio-controls">
          <button
            className={`play-pause-button ${isPlaying ? 'playing' : 'paused'}`}
            onClick={togglePlayPause}
            aria-label={isPlaying ? 'Pause' : 'Play'}
            data-testid="play-pause-button"
          >
            {isPlaying ? '❚❚' : '▶'}
          </button>

          <div className="time-controls">
            <span className="current-time" data-testid="current-time">
              {formatTime(currentTime)}
            </span>

            <input
              type="range"
              min="0"
              max={duration}
              value={currentTime}
              step="0.1"
              onChange={handleSeek}
              className="seek-slider"
              aria-label="Seek"
              data-testid="seek-slider"
            />

            <span className="duration" data-testid="duration">
              {formatTime(duration)}
            </span>
          </div>
        </div>
      )}
    </div>
  );
};

export default OptimizedAudio;
