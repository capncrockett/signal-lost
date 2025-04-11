import React, { useState, useEffect } from 'react';
import './PlayerNotebook.css';

interface Note {
  id: string;
  title: string;
  content: string;
  timestamp: number;
  tags: string[];
}

interface PlayerNotebookProps {
  isOpen: boolean;
  onClose: () => void;
}

const PlayerNotebook: React.FC<PlayerNotebookProps> = ({ isOpen, onClose }) => {
  const [notes, setNotes] = useState<Note[]>([]);
  const [activeNoteId, setActiveNoteId] = useState<string | null>(null);
  const [editMode, setEditMode] = useState<boolean>(false);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [filteredNotes, setFilteredNotes] = useState<Note[]>([]);

  // Form state for editing or creating notes
  const [noteTitle, setNoteTitle] = useState<string>('');
  const [noteContent, setNoteContent] = useState<string>('');
  const [noteTags, setNoteTags] = useState<string>('');

  // Load notes from localStorage
  useEffect(() => {
    const savedNotes = localStorage.getItem('player-notebook');
    if (savedNotes) {
      try {
        const parsedNotes = JSON.parse(savedNotes) as Note[];
        setNotes(parsedNotes);
      } catch (error) {
        console.error('Error loading notes:', error);
      }
    }
  }, []);

  // Save notes to localStorage whenever they change
  useEffect(() => {
    localStorage.setItem('player-notebook', JSON.stringify(notes));
  }, [notes]);

  // Filter notes based on search term
  useEffect(() => {
    if (!searchTerm.trim()) {
      setFilteredNotes(notes);
      return;
    }

    const lowerSearchTerm = searchTerm.toLowerCase();
    const filtered = notes.filter(
      (note) =>
        note.title.toLowerCase().includes(lowerSearchTerm) ||
        note.content.toLowerCase().includes(lowerSearchTerm) ||
        note.tags.some((tag) => tag.toLowerCase().includes(lowerSearchTerm))
    );
    setFilteredNotes(filtered);
  }, [notes, searchTerm]);

  // Get the active note
  const getActiveNote = (): Note | undefined => {
    return notes.find((note) => note.id === activeNoteId);
  };

  // Create a new note
  const createNewNote = () => {
    setActiveNoteId(null);
    setNoteTitle('');
    setNoteContent('');
    setNoteTags('');
    setEditMode(true);
  };

  // Edit an existing note
  const editNote = (noteId: string) => {
    const note = notes.find((n) => n.id === noteId);
    if (note) {
      setActiveNoteId(noteId);
      setNoteTitle(note.title);
      setNoteContent(note.content);
      setNoteTags(note.tags.join(', '));
      setEditMode(true);
    }
  };

  // Delete a note
  const deleteNote = (noteId: string) => {
    if (window.confirm('Are you sure you want to delete this note?')) {
      setNotes(notes.filter((note) => note.id !== noteId));
      if (activeNoteId === noteId) {
        setActiveNoteId(null);
      }
    }
  };

  // Save the current note
  const saveNote = () => {
    if (!noteTitle.trim()) {
      alert('Please enter a title for your note');
      return;
    }

    const tagsArray = noteTags
      .split(',')
      .map((tag) => tag.trim())
      .filter((tag) => tag !== '');

    if (activeNoteId) {
      // Update existing note
      setNotes(
        notes.map((note) =>
          note.id === activeNoteId
            ? {
                ...note,
                title: noteTitle,
                content: noteContent,
                tags: tagsArray,
                timestamp: Date.now(),
              }
            : note
        )
      );
    } else {
      // Create new note
      const newNote: Note = {
        id: `note-${Date.now()}`,
        title: noteTitle,
        content: noteContent,
        tags: tagsArray,
        timestamp: Date.now(),
      };
      setNotes([...notes, newNote]);
      setActiveNoteId(newNote.id);
    }
    setEditMode(false);
  };

  // Cancel editing
  const cancelEdit = () => {
    setEditMode(false);
    if (activeNoteId) {
      const note = notes.find((n) => n.id === activeNoteId);
      if (note) {
        setNoteTitle(note.title);
        setNoteContent(note.content);
        setNoteTags(note.tags.join(', '));
      }
    } else {
      setNoteTitle('');
      setNoteContent('');
      setNoteTags('');
    }
  };

  // Format date for display
  const formatDate = (timestamp: number): string => {
    return new Date(timestamp).toLocaleString();
  };

  if (!isOpen) {
    return null;
  }

  return (
    <div className="player-notebook-overlay">
      <div className="player-notebook-container">
        <div className="notebook-header">
          <h2>Player Notebook</h2>
          <div className="notebook-search">
            <input
              type="text"
              placeholder="Search notes..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              disabled={editMode}
            />
          </div>
          <button className="close-button" onClick={onClose}>
            Close
          </button>
        </div>

        <div className="notebook-content">
          <div className="notebook-sidebar">
            <button className="new-note-button" onClick={createNewNote} disabled={editMode}>
              + New Note
            </button>
            <div className="note-list">
              {filteredNotes.length === 0 ? (
                <div className="no-notes">No notes found</div>
              ) : (
                filteredNotes.map((note) => (
                  <div
                    key={note.id}
                    className={`note-item ${activeNoteId === note.id ? 'active' : ''}`}
                    onClick={() => {
                      if (!editMode) {
                        setActiveNoteId(note.id);
                        setNoteTitle(note.title);
                        setNoteContent(note.content);
                        setNoteTags(note.tags.join(', '));
                      }
                    }}
                  >
                    <div className="note-item-title">{note.title}</div>
                    <div className="note-item-date">{formatDate(note.timestamp)}</div>
                  </div>
                ))
              )}
            </div>
          </div>

          <div className="notebook-main">
            {!activeNoteId && !editMode ? (
              <div className="no-note-selected">
                <p>Select a note from the sidebar or create a new one</p>
              </div>
            ) : editMode ? (
              <div className="note-editor">
                <div className="editor-field">
                  <label htmlFor="note-title">Title:</label>
                  <input
                    id="note-title"
                    type="text"
                    value={noteTitle}
                    onChange={(e) => setNoteTitle(e.target.value)}
                    placeholder="Note title"
                  />
                </div>
                <div className="editor-field">
                  <label htmlFor="note-content">Content:</label>
                  <textarea
                    id="note-content"
                    value={noteContent}
                    onChange={(e) => setNoteContent(e.target.value)}
                    placeholder="Write your note here..."
                    rows={10}
                  />
                </div>
                <div className="editor-field">
                  <label htmlFor="note-tags">Tags (comma separated):</label>
                  <input
                    id="note-tags"
                    type="text"
                    value={noteTags}
                    onChange={(e) => setNoteTags(e.target.value)}
                    placeholder="tag1, tag2, tag3"
                  />
                </div>
                <div className="editor-actions">
                  <button onClick={saveNote}>Save</button>
                  <button onClick={cancelEdit}>Cancel</button>
                </div>
              </div>
            ) : (
              <div className="note-viewer">
                {getActiveNote() && (
                  <>
                    <div className="note-header">
                      <h3>{getActiveNote()?.title}</h3>
                      <div className="note-actions">
                        <button onClick={() => editNote(activeNoteId!)}>Edit</button>
                        <button onClick={() => deleteNote(activeNoteId!)}>Delete</button>
                      </div>
                    </div>
                    <div className="note-date">
                      Last updated: {formatDate(getActiveNote()?.timestamp || 0)}
                    </div>
                    <div className="note-content">
                      <p>{getActiveNote()?.content}</p>
                    </div>
                    {getActiveNote()?.tags.length ? (
                      <div className="note-tags">
                        {getActiveNote()?.tags.map((tag) => (
                          <span key={tag} className="tag">
                            {tag}
                          </span>
                        ))}
                      </div>
                    ) : null}
                  </>
                )}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default PlayerNotebook;
