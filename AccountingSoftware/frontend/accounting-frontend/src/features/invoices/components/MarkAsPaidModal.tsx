import React, { useState, useEffect } from 'react';
import { MarkInvoiceAsPaidDto, Invoice } from '../../../types';

interface MarkAsPaidModalProps {
  show: boolean;
  onHide: () => void;
  onConfirm: (data: MarkInvoiceAsPaidDto) => Promise<void>;
  invoice?: Invoice;
}

const MarkAsPaidModal: React.FC<MarkAsPaidModalProps> = ({ 
  show, 
  onHide, 
  onConfirm, 
  invoice 
}) => {
  const [formData, setFormData] = useState<MarkInvoiceAsPaidDto>({
    paidDate: new Date().toISOString().split('T')[0],
    paymentReference: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (show) {
      // Reset form when modal opens
      setFormData({
        paidDate: new Date().toISOString().split('T')[0],
        paymentReference: ''
      });
      setError(null);
    }
  }, [show]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.paidDate) {
      setError('Payment date is required');
      return;
    }

    setLoading(true);
    setError(null);
    
    try {
      await onConfirm(formData);
      onHide();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to mark invoice as paid');
    } finally {
      setLoading(false);
    }
  };

  if (!show) return null;

  return (
    <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">
              <i className="bi bi-check-circle me-2"></i>
              Mark Invoice as Paid
            </h5>
            <button type="button" className="btn-close" onClick={onHide}></button>
          </div>
          
          <form onSubmit={handleSubmit}>
            <div className="modal-body">
              <div className="mb-3">
                <p className="text-muted">
                  You are marking invoice <strong>{invoice?.invoiceNumber}</strong> as paid.
                </p>
              </div>

              {error && (
                <div className="alert alert-danger" role="alert">
                  {error}
                </div>
              )}

              <div className="mb-3">
                <label htmlFor="paidDate" className="form-label">Payment Date *</label>
                <input
                  type="date"
                  className="form-control"
                  id="paidDate"
                  value={formData.paidDate}
                  onChange={(e) => setFormData({ ...formData, paidDate: e.target.value })}
                  max={new Date().toISOString().split('T')[0]} // Don't allow future dates
                  required
                />
              </div>

              <div className="mb-3">
                <label htmlFor="paymentReference" className="form-label">Payment Reference</label>
                <input
                  type="text"
                  className="form-control"
                  id="paymentReference"
                  value={formData.paymentReference}
                  onChange={(e) => setFormData({ ...formData, paymentReference: e.target.value })}
                  placeholder="e.g., Check #12345, Bank Transfer, Cash"
                  maxLength={100}
                />
                <div className="form-text">
                  Optional reference for tracking the payment method or transaction ID
                </div>
              </div>
            </div>
            
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={onHide} disabled={loading}>
                Cancel
              </button>
              <button type="submit" className="btn btn-success" disabled={loading}>
                {loading ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                    Marking as Paid...
                  </>
                ) : (
                  <>
                    <i className="bi bi-check-circle me-2"></i>
                    Mark as Paid
                  </>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default MarkAsPaidModal;