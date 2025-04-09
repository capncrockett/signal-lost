import React, { useState, useEffect } from 'react';
import { useGameState } from '../../context/GameStateContext';
import { useSignalState } from '../../context/SignalStateContext';
import { useEvent } from '../../context/EventContext';
import { useProgress } from '../../context/ProgressContext';
import { SaveManager, SaveFile } from '../../utils/SaveManager';
import './SaveLoadManager.css';

interface SaveLoadManagerProps {
  isOpen: boolean;
  onClose: () => void;
}

const SaveLoadManager: React.FC<SaveLoadManagerProps> = ({ isOpen, onClose }) => {
  const { state: gameState, dispatch: gameDispatch } = useGameState();
  const { state: signalState, dispatch: signalDispatch } = useSignalState();
  const { state: eventState, dispatch: eventDispatch } = useEvent();
  const { state: progressState, dispatch: progressDispatch } = useProgress();

  const [saveManager] = useState(() => new SaveManager());
  const [saveFiles, setSaveFiles] = useState<SaveFile[]>([]);
  const [selectedSaveId, setSelectedSaveId] = useState<string | null>(null);
  const [newSaveName, setNewSaveName] = useState<string>('');
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [message, setMessage] = useState<string>('');

  // Load save files when the component mounts or when isOpen changes
  useEffect(() => {
    if (isOpen) {
      setSaveFiles(saveManager.getSaveFiles());
      setNewSaveName(`Save Game ${new Date().toLocaleString()}`);
    }
  }, [isOpen, saveManager]);

  // Handle saving the game
  const handleSave = async (): Promise<void> => {
    if (!newSaveName.trim()) {
      setMessage('Please enter a save name');
      return;
    }

    setIsLoading(true);
    setMessage('Saving game...');

    try {
      // Take a screenshot if possible
      const screenshot = await saveManager.takeScreenshot();

      // Save the game
      const saveId = saveManager.saveGame(
        newSaveName,
        gameState,
        signalState,
        eventState,
        progressState,
        screenshot || undefined
      );

      // Update the save files list
      setSaveFiles(saveManager.getSaveFiles());
      setSelectedSaveId(saveId);
      setMessage('Game saved successfully!');
    } catch (error) {
      console.error('Error saving game:', error);
      setMessage('Error saving game. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  // Handle loading a save file
  const handleLoad = (): void => {
    if (!selectedSaveId) {
      setMessage('Please select a save file to load');
      return;
    }

    setIsLoading(true);
    setMessage('Loading game...');

    try {
      const saveFile = saveManager.loadGame(selectedSaveId);
      if (!saveFile) {
        setMessage('Error loading save file. File not found.');
        setIsLoading(false);
        return;
      }

      // Load game state
      gameDispatch({ type: 'LOAD_STATE', payload: saveFile.gameState });

      // Load signal state
      signalDispatch({ type: 'LOAD_STATE', payload: saveFile.signalState });

      // Load event state
      eventDispatch({ type: 'LOAD_STATE', payload: saveFile.eventState });

      // Load progress state
      progressDispatch({ type: 'LOAD_STATE', payload: saveFile.progressState });

      setMessage('Game loaded successfully!');

      // Close the dialog after a short delay
      setTimeout(() => {
        onClose();
      }, 1000);
    } catch (error) {
      console.error('Error loading game:', error);
      setMessage('Error loading game. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  // Handle deleting a save file
  const handleDelete = (): void => {
    if (!selectedSaveId) {
      setMessage('Please select a save file to delete');
      return;
    }

    if (window.confirm('Are you sure you want to delete this save file?')) {
      try {
        saveManager.deleteSave(selectedSaveId);
        setSaveFiles(saveManager.getSaveFiles());
        setSelectedSaveId(null);
        setMessage('Save file deleted successfully!');
      } catch (error) {
        console.error('Error deleting save file:', error);
        setMessage('Error deleting save file. Please try again.');
      }
    }
  };

  // Format a date for display
  const formatDate = (timestamp: number): string => {
    return new Date(timestamp).toLocaleString();
  };

  if (!isOpen) {
    return null;
  }

  return (
    <div className="save-load-overlay">
      <div className="save-load-container">
        <h2>Save / Load Game</h2>

        {message && <div className="save-load-message">{message}</div>}

        <div className="save-load-content">
          <div className="save-game-section">
            <h3>Save Game</h3>
            <div className="save-input-container">
              <input
                type="text"
                value={newSaveName}
                onChange={(e) => setNewSaveName(e.target.value)}
                placeholder="Enter save name"
                disabled={isLoading}
              />
              <button onClick={() => void handleSave()} disabled={isLoading || !newSaveName.trim()}>
                Save
              </button>
            </div>
          </div>

          <div className="save-files-section">
            <h3>Load Game</h3>
            {saveFiles.length === 0 ? (
              <div className="no-saves-message">No save files found</div>
            ) : (
              <div className="save-files-list">
                {saveFiles.map((saveFile) => (
                  <div
                    key={saveFile.id}
                    className={`save-file-item ${selectedSaveId === saveFile.id ? 'selected' : ''}`}
                    onClick={() => setSelectedSaveId(saveFile.id)}
                  >
                    <div className="save-file-info">
                      <div className="save-file-name">{saveFile.name}</div>
                      <div className="save-file-date">{formatDate(saveFile.timestamp)}</div>
                    </div>
                    {saveFile.screenshot && (
                      <div className="save-file-screenshot">
                        <img src={saveFile.screenshot} alt="Save screenshot" />
                      </div>
                    )}
                  </div>
                ))}
              </div>
            )}

            <div className="save-file-actions">
              <button
                onClick={handleLoad}
                disabled={isLoading || !selectedSaveId}
                className="load-button"
              >
                Load
              </button>
              <button
                onClick={handleDelete}
                disabled={isLoading || !selectedSaveId}
                className="delete-button"
              >
                Delete
              </button>
            </div>
          </div>
        </div>

        <div className="save-load-footer">
          <button onClick={onClose} disabled={isLoading} className="close-button">
            Close
          </button>
        </div>
      </div>
    </div>
  );
};

export default SaveLoadManager;
