import React from 'react';
import Modal from './Modal';
import Button from './Button';
import './Dialog.css';

export type DialogType = 'info' | 'success' | 'warning' | 'error' | 'confirm';

export interface DialogProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  message: string;
  type?: DialogType;
  confirmLabel?: string;
  cancelLabel?: string;
  onConfirm?: () => void;
  onCancel?: () => void;
}

/**
 * Dialog component for alerts and confirmations
 */
const Dialog: React.FC<DialogProps> = ({
  isOpen,
  onClose,
  title,
  message,
  type = 'info',
  confirmLabel = 'OK',
  cancelLabel = 'Cancel',
  onConfirm,
  onCancel,
}) => {
  const handleConfirm = (): void => {
    if (onConfirm) {
      onConfirm();
    }
    onClose();
  };

  const handleCancel = (): void => {
    if (onCancel) {
      onCancel();
    }
    onClose();
  };

  const getIcon = (): React.ReactNode => {
    switch (type) {
      case 'success':
        return (
          <div className="dialog-icon dialog-icon-success">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
              <polyline points="22 4 12 14.01 9 11.01"></polyline>
            </svg>
          </div>
        );
      case 'warning':
        return (
          <div className="dialog-icon dialog-icon-warning">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
              <line x1="12" y1="9" x2="12" y2="13"></line>
              <line x1="12" y1="17" x2="12.01" y2="17"></line>
            </svg>
          </div>
        );
      case 'error':
        return (
          <div className="dialog-icon dialog-icon-error">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="15" y1="9" x2="9" y2="15"></line>
              <line x1="9" y1="9" x2="15" y2="15"></line>
            </svg>
          </div>
        );
      case 'confirm':
        return (
          <div className="dialog-icon dialog-icon-confirm">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="12" y1="8" x2="12" y2="12"></line>
              <line x1="12" y1="16" x2="12.01" y2="16"></line>
            </svg>
          </div>
        );
      default:
        return (
          <div className="dialog-icon dialog-icon-info">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="12" y1="16" x2="12" y2="12"></line>
              <line x1="12" y1="8" x2="12.01" y2="8"></line>
            </svg>
          </div>
        );
    }
  };

  const footer = (
    <>
      {type === 'confirm' && (
        <Button variant="secondary" onClick={handleCancel} data-testid="dialog-cancel-button">
          {cancelLabel}
        </Button>
      )}
      <Button
        variant={type === 'error' ? 'danger' : type === 'success' ? 'success' : 'primary'}
        onClick={handleConfirm}
        data-testid="dialog-confirm-button"
      >
        {confirmLabel}
      </Button>
    </>
  );

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={title}
      footer={footer}
      size="small"
      closeOnEsc={type !== 'confirm'}
      closeOnOverlayClick={type !== 'confirm'}
      showCloseButton={type !== 'confirm'}
    >
      <div className="dialog-content">
        {getIcon()}
        <p className="dialog-message">{message}</p>
      </div>
    </Modal>
  );
};

export default Dialog;
