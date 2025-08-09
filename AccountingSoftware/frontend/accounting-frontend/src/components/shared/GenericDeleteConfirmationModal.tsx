import React from 'react';

interface GenericDeleteConfirmationModalProps {
  show: boolean;
  onHide: () => void;
  onConfirm: () => Promise<void>;
  itemName: string;
  itemType: string;
  loading?: boolean;
  warningMessage?: string;
}

const GenericDeleteConfirmationModal: React.FC<GenericDeleteConfirmationModalProps> = ({ 
  show, 
  onHide, 
  onConfirm, 
  itemName, 
  itemType,
  loading = false,
  warningMessage 
}) => {
  if (!show) return null;

  return (
    <div className="modal show d-block" tabIndex={-1}>
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title text-danger">
              <i className="bi bi-exclamation-triangle me-2"></i>
              Confirm Delete
            </h5>
            <button type="button" className="btn-close" onClick={onHide}></button>
          </div>
          <div className="modal-body">
            <p>Are you sure you want to delete this {itemType}:</p>
            <p className="fw-bold text-danger">"{itemName}"</p>
            <div className="alert alert-warning" role="alert">
              <small>
                <i className="bi bi-info-circle me-1"></i>
                {warningMessage || "This action will soft delete the item. It can be recovered if needed."}
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
              className="btn btn-danger" 
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
                  Delete {itemType}
                </>
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default GenericDeleteConfirmationModal;
