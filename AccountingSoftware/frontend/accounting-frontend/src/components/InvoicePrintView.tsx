import React from 'react';
import { Invoice, CompanyInfo, Customer } from '../types';
import './InvoicePrintView.css';

interface InvoicePrintViewProps {
  invoice: Invoice;
  companyInfo?: CompanyInfo;
  customer?: Customer;
}

const InvoicePrintView: React.FC<InvoicePrintViewProps> = ({ 
  invoice, 
  companyInfo, 
  customer 
}) => {
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', { 
      style: 'currency', 
      currency: companyInfo?.currency || 'USD' 
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  return (
    <div className="invoice-print-view">
      {/* Header */}
      <div className="invoice-header">
        <div className="company-info">
          {companyInfo?.logoUrl && (
            <img 
              src={companyInfo.logoUrl} 
              alt={companyInfo.companyName} 
              className="company-logo"
            />
          )}
          <div className="company-details">
            <h1 className="company-name">{companyInfo?.companyName || 'Your Company Name'}</h1>
            {companyInfo?.address && (
              <div className="company-address">
                <p>{companyInfo.address}</p>
                <p>
                  {companyInfo.city && `${companyInfo.city}, `}
                  {companyInfo.state && `${companyInfo.state} `}
                  {companyInfo.postalCode}
                </p>
                {companyInfo.country && <p>{companyInfo.country}</p>}
              </div>
            )}
            {companyInfo?.phone && <p>Phone: {companyInfo.phone}</p>}
            {companyInfo?.email && <p>Email: {companyInfo.email}</p>}
            {companyInfo?.website && <p>Website: {companyInfo.website}</p>}
          </div>
        </div>

        <div className="invoice-title-section">
          <h2 className="invoice-title">INVOICE</h2>
          <div className="invoice-meta">
            <p><strong>Invoice #:</strong> {invoice.invoiceNumber}</p>
            <p><strong>Date:</strong> {formatDate(invoice.invoiceDate)}</p>
            <p><strong>Due Date:</strong> {formatDate(invoice.dueDate)}</p>
            <p><strong>Status:</strong> <span className={`status-${invoice.status}`}>{invoice.statusName}</span></p>
          </div>
        </div>
      </div>

      {/* Bill To Section */}
      <div className="billing-section">
        <div className="bill-to">
          <h3>Bill To:</h3>
          <div className="customer-details">
            <p><strong>{customer?.companyName || invoice.customerName}</strong></p>
            {customer?.contactPersonName && <p>{customer.contactPersonName}</p>}
            {customer?.address && (
              <>
                <p>{customer.address}</p>
                <p>
                  {customer.city && `${customer.city}, `}
                  {customer.state && `${customer.state} `}
                  {customer.postalCode}
                </p>
                {customer.country && <p>{customer.country}</p>}
              </>
            )}
            {customer?.phone && <p>Phone: {customer.phone}</p>}
            {customer?.email && <p>Email: {customer.email}</p>}
          </div>
        </div>

        <div className="invoice-summary">
          <table className="summary-table">
            <tbody>
              <tr>
                <td>Subtotal:</td>
                <td>{formatCurrency(invoice.subTotal)}</td>
              </tr>
              <tr>
                <td>Tax ({invoice.taxRate}%):</td>
                <td>{formatCurrency(invoice.taxAmount)}</td>
              </tr>
              {invoice.discountAmount > 0 && (
                <tr>
                  <td>Discount:</td>
                  <td>-{formatCurrency(invoice.discountAmount)}</td>
                </tr>
              )}
              <tr className="total-row">
                <td><strong>Total:</strong></td>
                <td><strong>{formatCurrency(invoice.totalAmount)}</strong></td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      {/* Items Table */}
      <div className="items-section">
        <table className="items-table">
          <thead>
            <tr>
              <th className="item-description">Description</th>
              <th className="item-quantity">Qty</th>
              <th className="item-price">Unit Price</th>
              <th className="item-amount">Amount</th>
            </tr>
          </thead>
          <tbody>
            {invoice.items.map((item, index) => (
              <tr key={index}>
                <td className="item-description">{item.description}</td>
                <td className="item-quantity">{item.quantity}</td>
                <td className="item-price">{formatCurrency(item.unitPrice)}</td>
                <td className="item-amount">{formatCurrency(item.amount)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Footer Section */}
      <div className="invoice-footer">
        {invoice.notes && (
          <div className="notes-section">
            <h4>Notes:</h4>
            <p>{invoice.notes}</p>
          </div>
        )}

        {companyInfo?.bankName && (
          <div className="payment-info">
            <h4>Payment Information:</h4>
            <p><strong>Bank:</strong> {companyInfo.bankName}</p>
            {companyInfo.bankAccountNumber && (
              <p><strong>Account Number:</strong> {companyInfo.bankAccountNumber}</p>
            )}
            {companyInfo.bankRoutingNumber && (
              <p><strong>Routing Number:</strong> {companyInfo.bankRoutingNumber}</p>
            )}
          </div>
        )}

        {companyInfo?.taxNumber && (
          <div className="tax-info">
            <p><strong>Tax ID:</strong> {companyInfo.taxNumber}</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default InvoicePrintView;