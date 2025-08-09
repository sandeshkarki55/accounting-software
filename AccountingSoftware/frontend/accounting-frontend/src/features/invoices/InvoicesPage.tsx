import React, { useState, useEffect } from 'react';
import { Invoice, InvoiceStatus, Customer, CompanyInfo, CreateInvoiceDto, MarkInvoiceAsPaidDto, PaginationParams, SortingParams } from '../../types/index';
import Pagination from '../../components/common/Pagination';
import SortableTableHeader, { SortableColumn } from '../../components/common/SortableTableHeader';
import DebouncedSearchInput from '../../components/common/DebouncedSearchInput';
import { InvoiceFilteringParams } from '../../types/invoices';
import { invoiceService, customerService, companyInfoService } from '../../services/api';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData';
import InvoiceModal from './components/InvoiceModal';
import InvoicePrintModal from './components/InvoicePrintModal';
import MarkAsPaidModal from './components/MarkAsPaidModal';
import GenericDeleteConfirmationModal from '../../components/shared/GenericDeleteConfirmationModal';

const InvoicesPage: React.FC = () => {
  usePageTitle('Invoices');

  // Paged invoices
  const {
    data: invoices,
    loading,
    error,
    pagination,
    setPagination,
    sorting,
    setSorting,
    filtering,
    setFiltering,
    totalCount,
  } = usePagedData<Invoice, PaginationParams, SortingParams, InvoiceFilteringParams>({
    fetchData: invoiceService.getInvoices,
    initialPagination: { pageNumber: 1, pageSize: 10 },
    initialSorting: { orderBy: 'invoiceDate', descending: true },
    initialFiltering: { searchTerm: '', statusFilter: 'all' },
  });

  // Customers and companyInfos (not paged)
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [companyInfos, setCompanyInfos] = useState<CompanyInfo[]>([]);
  const [showInvoiceModal, setShowInvoiceModal] = useState(false);
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | undefined>();
  const [showPrintModal, setShowPrintModal] = useState(false);
  const [invoiceToPrint, setInvoiceToPrint] = useState<Invoice | undefined>();
  const [showMarkAsPaidModal, setShowMarkAsPaidModal] = useState(false);
  const [selectedInvoiceForPayment, setSelectedInvoiceForPayment] = useState<Invoice | undefined>();
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [invoiceToDelete, setInvoiceToDelete] = useState<Invoice | undefined>();
  const [deleteLoading, setDeleteLoading] = useState(false);

  useEffect(() => {
    const loadMeta = async () => {
      try {
        const [customersData, companyInfosPaged] = await Promise.all([
          customerService.getCustomers(),
          companyInfoService.getCompanyInfos({ pageNumber: 1, pageSize: 1000 }, { orderBy: 'companyName', descending: false }, { searchTerm: '' })
        ]);
        setCustomers(customersData);
        setCompanyInfos(companyInfosPaged.items);
      } catch (err) {
        // Optionally handle error
      }
    };
    loadMeta();
  }, []);

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
      setPagination({ ...pagination }); // Trigger reload
      setShowInvoiceModal(false);
    } catch (err) {
      console.error('Error saving invoice:', err);
      throw err;
    }
  };

  const handlePrintInvoice = (invoice: Invoice) => {
    setInvoiceToPrint(invoice);
    setShowPrintModal(true);
  };

  const handleMarkInvoiceAsPaid = (invoice: Invoice) => {
    setSelectedInvoiceForPayment(invoice);
    setShowMarkAsPaidModal(true);
  };

  const handleConfirmMarkAsPaid = async (invoiceData: MarkInvoiceAsPaidDto) => {
    try {
      await invoiceService.markInvoiceAsPaid(selectedInvoiceForPayment!.id, invoiceData);
      setPagination({ ...pagination }); // Trigger reload
      setShowMarkAsPaidModal(false);
    } catch (err) {
      console.error('Error marking invoice as paid:', err);
      throw err;
    }
  };

  const handleDeleteInvoice = (invoice: Invoice) => {
    setInvoiceToDelete(invoice);
    setShowDeleteModal(true);
  };

  const handleConfirmDelete = async () => {
    if (!invoiceToDelete) return;

    try {
      setDeleteLoading(true);
      await invoiceService.deleteInvoice(invoiceToDelete.id);
      setPagination({ ...pagination }); // Trigger reload
      setShowDeleteModal(false);
      setInvoiceToDelete(undefined);
    } catch (err) {
      // Optionally set error state
      console.error('Error deleting invoice:', err);
    } finally {
      setDeleteLoading(false);
    }
  };

  const getCustomerForInvoice = (customerId: number) => {
    return customers.find(customer => customer.id === customerId);
  };

  const getCompanyInfoForInvoice = (companyInfoId?: number) => {
    if (!companyInfoId) {
      return companyInfos.find(company => company.isDefault);
    }
    return companyInfos.find(company => company.id === companyInfoId);
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

  // Columns for SortableTableHeader
  const columns: SortableColumn[] = [
    { key: 'invoiceNumber', label: 'Invoice #', sortable: true },
    { key: 'invoiceDate', label: 'Date', sortable: true },
    { key: 'customer', label: 'Customer', sortable: false },
    { key: 'company', label: 'Company', sortable: false },
    { key: 'totalAmount', label: 'Total', sortable: true },
    { key: 'status', label: 'Status', sortable: true },
    { key: 'actions', label: 'Actions', sortable: false },
  ];

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
      <button className="btn btn-sm btn-outline-danger ms-2" onClick={() => setPagination({ ...pagination })}>
        Try Again
      </button>
    </div>
  );



  // Helper for rendering sort icons (Bootstrap style)
  const renderSortIcon = (column: string) => {
    if (sorting.orderBy !== column) return null;
    return (
      <i className={`bi ms-1 bi-sort-${sorting.descending ? 'down' : 'up'}-alt`}></i>
    );
  };

  // Handler for sorting (toggle if same column, ascending if new)
  const handleSort = (column: string) => {
    setSorting({
      orderBy: column,
      descending: sorting.orderBy === column ? !sorting.descending : false
    });
  };

  return (
    <div className="container">
      <div className="row">
        <div className="col-12">
          <h1 className="mb-4 text-dark">Invoices</h1>
          {/* Search and Filter Controls */}
          <div className="row mb-4">
            <div className="col-md-6">
              <div className="input-group">
                <span className="input-group-text">
                  <i className="bi bi-search"></i>
                </span>
                <DebouncedSearchInput
                  placeholder="Search invoices, descriptions, or customers..."
                  defaultValue={filtering.searchTerm || ''}
                  onSearch={value => setFiltering(f => ({ ...f, searchTerm: value }))}
                />
              </div>
            </div>
            <div className="col-md-3">
              <select
                className="form-select"
                value={filtering.statusFilter}
                onChange={e => setFiltering({ ...filtering, statusFilter: e.target.value as 'all' | 'paid' | 'unpaid' | 'overdue' | 'draft' | 'cancelled' })}
              >
                <option value="all">All Invoices</option>
                <option value="paid">Paid Only</option>
                <option value="unpaid">Unpaid Only</option>
                <option value="overdue">Overdue Only</option>
                <option value="draft">Draft Only</option>
                <option value="cancelled">Cancelled Only</option>
              </select>
            </div>
            <div className="col-md-3">
              <button 
                className="btn btn-primary w-100" 
                onClick={handleAddInvoice}
              >
                <i className="bi bi-plus-circle me-2"></i>
                Create Invoice
              </button>
            </div>
          </div>

          <div className="card shadow-sm">
            <div className="card-body p-0">
              <div className="table-responsive">
                <table className="table table-hover mb-0">
                  <SortableTableHeader
                    columns={[
                      { key: 'invoiceNumber', label: 'Invoice #', sortable: true },
                      { key: 'customer', label: 'Customer', sortable: false },
                      { key: 'invoiceDate', label: 'Date', sortable: true },
                      { key: 'dueDate', label: 'Due Date', sortable: true },
                      { key: 'totalAmount', label: 'Total', sortable: true },
                      { key: 'status', label: 'Status', sortable: true },
                      { key: 'actions', label: 'Actions', sortable: false },
                    ]}
                    sorting={{ orderBy: sorting.orderBy || '', descending: sorting.descending ?? false }}
                    onSort={handleSort}
                  />
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
                              onClick={() => handlePrintInvoice(invoice)}
                              title="Print Invoice"
                            >
                              <i className="bi bi-printer"></i>
                            </button>
                            {invoice.status !== InvoiceStatus.Paid && invoice.status !== InvoiceStatus.Cancelled && (
                              <button
                                className="btn btn-sm btn-outline-success"
                                onClick={() => handleMarkInvoiceAsPaid(invoice)}
                                title="Mark as Paid"
                              >
                                <i className="bi bi-check-circle"></i>
                              </button>
                            )}
                            {invoice.status !== InvoiceStatus.Paid && (
                              <button
                                className="btn btn-sm btn-outline-danger"
                                onClick={() => handleDeleteInvoice(invoice)}
                                title="Delete Invoice"
                              >
                                <i className="bi bi-trash"></i>
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          {invoices.length === 0 && !loading && (
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

          {/* Results count */}
          {totalCount > 0 && (
            <div className="mt-3 text-muted text-center">
              Showing {(pagination.pageNumber - 1) * pagination.pageSize + 1} to {Math.min(pagination.pageNumber * pagination.pageSize, totalCount)} of {totalCount} invoices
            </div>
          )}

          {/* Pagination Controls */}
          <Pagination
            pageNumber={pagination.pageNumber}
            pageSize={pagination.pageSize}
            totalCount={totalCount}
            onPageChange={(page: number) => setPagination({ ...pagination, pageNumber: page })}
            ariaLabel="Invoice Pagination"
          />

          {/* Invoice Modal for Add/View */}
      <InvoiceModal
        show={showInvoiceModal}
        onHide={() => setShowInvoiceModal(false)}
        onSave={handleSaveInvoice}
        invoice={selectedInvoice}
        customers={customers}
        companyInfos={companyInfos}
      />

      {/* Invoice Print Modal */}
      <InvoicePrintModal
        show={showPrintModal}
        onHide={() => setShowPrintModal(false)}
        invoice={invoiceToPrint}
        customer={invoiceToPrint ? getCustomerForInvoice(invoiceToPrint.customerId) : undefined}
        companyInfo={invoiceToPrint ? getCompanyInfoForInvoice(invoiceToPrint.companyInfoId) : undefined}
      />

      {/* Mark As Paid Modal */}
      <MarkAsPaidModal
        show={showMarkAsPaidModal}
        onHide={() => setShowMarkAsPaidModal(false)}
        onConfirm={handleConfirmMarkAsPaid}
        invoice={selectedInvoiceForPayment}
      />

      {/* Delete Confirmation Modal */}
      <GenericDeleteConfirmationModal
        show={showDeleteModal}
        onHide={() => setShowDeleteModal(false)}
        onConfirm={handleConfirmDelete}
        itemName={invoiceToDelete?.invoiceNumber || ''}
        itemType="invoice"
        loading={deleteLoading}
        warningMessage="This will soft delete the invoice and all its items. The invoice data will be preserved but hidden from normal views."
      />
    </div>
  );
};

export default InvoicesPage;