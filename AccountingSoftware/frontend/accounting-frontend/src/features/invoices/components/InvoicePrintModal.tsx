import React from 'react';
import { Invoice, CompanyInfo, Customer } from '../../../types';
import InvoicePrintView from './InvoicePrintView';

interface InvoicePrintModalProps {
  show: boolean;
  onHide: () => void;
  invoice?: Invoice;
  companyInfo?: CompanyInfo;
  customer?: Customer;
}

const InvoicePrintModal: React.FC<InvoicePrintModalProps> = ({
  show,
  onHide,
  invoice,
  companyInfo,
  customer
}) => {
  const handlePrint = () => {
    window.print();
  };

  const getInvoiceStyles = () => {
    // Get the CSS from the InvoicePrintView.css file
    const styleSheets = Array.from(document.styleSheets);
    let invoiceStyles = '';
    
    styleSheets.forEach(sheet => {
      try {
        const rules = Array.from(sheet.cssRules || sheet.rules || []);
        rules.forEach(rule => {
          if (rule.cssText && (
            rule.cssText.includes('.invoice-print-view') ||
            rule.cssText.includes('@media print')
          )) {
            invoiceStyles += rule.cssText + '\n';
          }
        });
      } catch (e) {
        // Handle CORS issues with external stylesheets
        console.warn('Could not access stylesheet:', e);
      }
    });

    // Fallback basic styles if we can't extract them
    if (!invoiceStyles) {
      invoiceStyles = `
        .invoice-print-view {
          max-width: 8.5in;
          margin: 0 auto;
          padding: 20px;
          font-family: Arial, sans-serif;
          background: white;
          color: #333;
          line-height: 1.4;
        }
        @media print {
          .invoice-print-view {
            max-width: none;
            margin: 0;
            padding: 0.5in;
          }
          .no-print { display: none !important; }
        }
      `;
    }

    return invoiceStyles;
  };

  if (!show || !invoice) return null;

  return (
    <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
      <div className="modal-dialog modal-fullscreen">
        <div className="modal-content">
          <div className="modal-header no-print">
            <h5 className="modal-title">
              <i className="bi bi-printer me-2"></i>
              Print Invoice - {invoice.invoiceNumber}
            </h5>
            <button type="button" className="btn-close" onClick={onHide}></button>
          </div>
          
          <div className="modal-body p-0">
            <div className="d-flex justify-content-center gap-2 p-3 bg-light border-bottom no-print">
              <button 
                className="btn btn-primary"
                onClick={handlePrint}
                title="Print Invoice"
              >
                <i className="bi bi-printer me-2"></i>
                Print
              </button>
              <button 
                className="btn btn-outline-secondary"
                onClick={onHide}
                title="Close Preview"
              >
                <i className="bi bi-x-circle me-2"></i>
                Close
              </button>
            </div>
            
            <div className="overflow-auto" style={{ height: 'calc(100vh - 150px)' }}>
              <InvoicePrintView 
                invoice={invoice}
                companyInfo={companyInfo}
                customer={customer}
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default InvoicePrintModal;