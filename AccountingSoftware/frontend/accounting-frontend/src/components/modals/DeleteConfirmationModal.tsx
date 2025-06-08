import React from 'react';
import './DeleteConfirmationModal.scss';

interface DeleteConfirmationModalProps {
  show: boolean;
  onHide: () => void;
  onConfirm: () => Promise<void>;
  accountName: string;
  loading?: boolean;
}

const DeleteConfirmationModal: React.FC<DeleteConfirmationModalProps> = ({ 
  show, 
  onHide, 
  onConfirm, 
  accountName, 
  loading = false 
}) => {
  if (!show) return null;

  return (
    <div className="modal show d-block delete-confirmation-modal" tabIndex={-1}>
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title text-vibrant-danger">
              <i className="bi bi-exclamation-triangle me-2"></i>
              Confirm Delete
            </h5>
            <button type="button" className="btn-close" onClick={onHide}></button>
          </div>
          <div className="modal-body">
            <p>Are you sure you want to delete the account:</p>
            <p className="fw-bold text-vibrant-danger account-name">"{accountName}"</p>
            <div className="alert alert-warning" role="alert">
              <small>
                <i className="bi bi-info-circle me-1"></i>
                This action cannot be undone. Make sure this account has no transactions before deleting.
              </small>
            </div>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-secondary" onClick={onHide}>
              <i className="bi bi-x-circle me-2"></i>
              Cancel
            </button>
            <button 
              type="button" 
              className="btn btn-danger delete-btn" 
              onClick={onConfirm}
              disabled={loading}
            >
              {loading ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                  Deleting...
                </>
              ) : (
                <>
                  <i className="bi bi-trash me-2"></i>
                  Delete Account
                </>
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default DeleteConfirmationModal;