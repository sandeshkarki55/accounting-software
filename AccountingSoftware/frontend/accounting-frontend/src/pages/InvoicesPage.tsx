import React, { useState, useEffect } from 'react';
import { Invoice, InvoiceStatus, Customer, CompanyInfo, CreateInvoiceDto } from '../types';
import { invoiceService, customerService, companyInfoService } from '../services/api';
import InvoiceModal from '../components/InvoiceModal';

const InvoicesPage: React.FC = () => {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [companyInfos, setCompanyInfos] = useState<CompanyInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showInvoiceModal, setShowInvoiceModal] = useState(false);
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | undefined>();

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [invoicesData, customersData, companyInfosData] = await Promise.all([
        invoiceService.getInvoices(),
        customerService.getCustomers(),
        companyInfoService.getCompanyInfos()
      ]);
      setInvoices(invoicesData);
      setCustomers(customersData);
      setCompanyInfos(companyInfosData);
      setError(null);
    } catch (err) {
      setError('Failed to load data');
      console.error('Error loading data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddInvoice = () => {
    setSelectedInvoice(undefined);
    setShowInvoiceModal(true);
  };

  const handleViewInvoice = (invoice: Invoice) => {
    setSelectedInvoice(invoice);
    setShowInvoiceModal(true);
  };

  const handleSaveInvoice = async (invoiceData: CreateInvoiceDto) => {
    try {
      await invoiceService.createInvoice(invoiceData);
      await loadData(); // Reload invoices
      setShowInvoiceModal(false);
    } catch (err) {
      console.error('Error saving invoice:', err);
      throw err;
    }
  };

  const getStatusBadgeClass = (status: InvoiceStatus) => {
    switch (status) {
      case InvoiceStatus.Draft: return 'bg-secondary';
      case InvoiceStatus.Sent: return 'bg-primary';
      case InvoiceStatus.Paid: return 'bg-success';
      case InvoiceStatus.Overdue: return 'bg-danger';
      case InvoiceStatus.Cancelled: return 'bg-dark';
      default: return 'bg-secondary';
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', { 
      style: 'currency', 
      currency: 'USD' 
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  if (loading) return (
    <div className="d-flex justify-content-center align-items-center" style={{minHeight: '200px'}}>
      <div className="spinner-border text-primary" role="status">
        <span className="visually-hidden">Loading invoices...</span>
      </div>
    </div>
  );
  
  if (error) return (
    <div className="alert alert-danger" role="alert">
      <strong>Error:</strong> {error}
      <button className="btn btn-sm btn-outline-danger ms-2" onClick={loadData}>
        Try Again
      </button>
    </div>
  );

  return (
    <div className="container">
      <div className="row">
        <div className="col-12">
          <h1 className="mb-4 text-dark">Invoices</h1>
          
          <div className="mb-4">
            <button 
              className="btn btn-primary" 
              onClick={handleAddInvoice}
            >
              <i className="bi bi-plus-circle me-2"></i>
              Create New Invoice
            </button>
          </div>

          <div className="card shadow-sm">
            <div className="card-body p-0">
              <div className="table-responsive">
                <table className="table table-hover mb-0">
                  <thead className="table-dark">
                    <tr>
                      <th scope="col">Invoice #</th>
                      <th scope="col">Customer</th>
                      <th scope="col">Date</th>
                      <th scope="col">Due Date</th>
                      <th scope="col">Total</th>
                      <th scope="col">Status</th>
                      <th scope="col">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {invoices.map((invoice) => (
                      <tr key={invoice.id}>
                        <td className="fw-bold">{invoice.invoiceNumber}</td>
                        <td>{invoice.customerName}</td>
                        <td>{formatDate(invoice.invoiceDate)}</td>
                        <td>{formatDate(invoice.dueDate)}</td>
                        <td className="fw-bold">{formatCurrency(invoice.totalAmount)}</td>
                        <td>
                          <span className={`badge ${getStatusBadgeClass(invoice.status)}`}>
                            {invoice.statusName}
                          </span>
                        </td>
                        <td>
                          <div className="btn-group" role="group">
                            <button
                              className="btn btn-sm btn-outline-primary"
                              onClick={() => handleViewInvoice(invoice)}
                              title="View Invoice"
                            >
                              <i className="bi bi-eye"></i>
                            </button>
                            <button
                              className="btn btn-sm btn-outline-secondary"
                              title="Print Invoice"
                            >
                              <i className="bi bi-printer"></i>
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          {invoices.length === 0 && (
            <div className="text-center py-5">
              <div className="mb-3">
                <i className="bi bi-receipt display-1 text-muted"></i>
              </div>
              <h5 className="text-muted">No invoices found</h5>
              <p className="text-muted">Create your first invoice to get started.</p>
              <button 
                className="btn btn-primary" 
                onClick={handleAddInvoice}
              >
                <i className="bi bi-plus-circle me-2"></i>
                Create Invoice
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Invoice Modal for Add/View */}
      <InvoiceModal
        show={showInvoiceModal}
        onHide={() => setShowInvoiceModal(false)}
        onSave={handleSaveInvoice}
        invoice={selectedInvoice}
        customers={customers}
        companyInfos={companyInfos}
      />
    </div>
  );
};

export default InvoicesPage;