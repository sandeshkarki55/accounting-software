
import React, { useState } from 'react';
import Pagination from '../../components/common/Pagination';
import SortableTableHeader, { SortableColumn } from '../../components/common/SortableTableHeader';
import { Customer, CreateCustomerDto, UpdateCustomerDto, PaginationParams, SortingParams, CustomerFilteringParams } from '../../types';
import { customerService } from '../../services/customerService';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData';
import CustomerModal from '../../components/modals/CustomerModal';
import GenericDeleteConfirmationModal from '../../components/modals/GenericDeleteConfirmationModal';


const CustomersPage: React.FC = () => {
  usePageTitle('Customers');

  // Paged customers
  const {
    data: customers,
    loading,
    error,
    pagination,
    setPagination,
    sorting,
    setSorting,
    filtering,
    setFiltering,
    totalCount,
  } = usePagedData<Customer, PaginationParams, SortingParams, CustomerFilteringParams>({
    fetchData: customerService.getCustomersPaged,
    initialPagination: { pageNumber: 1, pageSize: 10 },
    initialSorting: { orderBy: 'companyName', descending: false },
    initialFiltering: { searchTerm: '', isActive: undefined },
  });

  const [showCustomerModal, setShowCustomerModal] = useState(false);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | undefined>();
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [customerToDelete, setCustomerToDelete] = useState<Customer | undefined>();
  const [deleteLoading, setDeleteLoading] = useState(false);

  const handleAddCustomer = () => {
    setSelectedCustomer(undefined);
    setShowCustomerModal(true);
  };

  const handleEditCustomer = (customer: Customer) => {
    setSelectedCustomer(customer);
    setShowCustomerModal(true);
  };


  const handleSaveCustomer = async (customerData: CreateCustomerDto | UpdateCustomerDto) => {
    try {
      if (selectedCustomer) {
        await customerService.updateCustomer(selectedCustomer.id, customerData as UpdateCustomerDto);
      } else {
        await customerService.createCustomer(customerData as CreateCustomerDto);
      }
      setPagination({ ...pagination }); // reload
      setShowCustomerModal(false);
    } catch (err) {
      console.error('Error saving customer:', err);
      throw err;
    }
  };

  const handleDeleteCustomer = (customer: Customer) => {
    setCustomerToDelete(customer);
    setShowDeleteModal(true);
  };


  const handleConfirmDelete = async () => {
    if (!customerToDelete) return;

    try {
      setDeleteLoading(true);
      await customerService.deleteCustomer(customerToDelete.id);
      setShowDeleteModal(false);
      setCustomerToDelete(undefined);
      setPagination({ ...pagination }); // reload
    } catch (err) {
      alert('Failed to delete customer');
      console.error('Error deleting customer:', err);
    } finally {
      setDeleteLoading(false);
    }
  };


  // Columns for SortableTableHeader
  const columns: SortableColumn[] = [
    { key: 'customerCode', label: 'Customer Code', sortable: true },
    { key: 'companyName', label: 'Company Name', sortable: true },
    { key: 'contactPersonName', label: 'Contact Person', sortable: true },
    { key: 'email', label: 'Email', sortable: true },
    { key: 'phone', label: 'Phone', sortable: true },
    { key: 'status', label: 'Status', sortable: false },
    { key: 'actions', label: 'Actions', sortable: false },
  ];

  // Sorting handler
  const handleSort = (column: string) => {
    setSorting({
      orderBy: column,
      descending: sorting.orderBy === column ? !sorting.descending : false
    });
  };

  if (loading) return (
    <div className="d-flex justify-content-center align-items-center" style={{minHeight: '200px'}}>
      <div className="spinner-border text-primary" role="status">
        <span className="visually-hidden">Loading customers...</span>
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

  return (
    <div className="container">
      <div className="row">
        <div className="col-12">
          <h1 className="mb-4 text-dark">Customers</h1>
          

          {/* Search and Add Controls */}
          <div className="row mb-4">
            <div className="col-md-6">
              <div className="input-group">
                <span className="input-group-text">
                  <i className="bi bi-search"></i>
                </span>
                <input
                  type="text"
                  className="form-control"
                  placeholder="Search customers..."
                  value={filtering.searchTerm || ''}
                  onChange={e => setFiltering({ ...filtering, searchTerm: e.target.value })}
                />
              </div>
            </div>
            <div className="col-md-3">
              <select
                className="form-select"
                value={filtering.isActive === undefined ? '' : filtering.isActive ? 'active' : 'inactive'}
                onChange={e => {
                  const val = e.target.value;
                  setFiltering({ ...filtering, isActive: val === '' ? undefined : val === 'active' });
                }}
              >
                <option value="">All Statuses</option>
                <option value="active">Active Only</option>
                <option value="inactive">Inactive Only</option>
              </select>
            </div>
            <div className="col-md-3">
              <button
                className="btn btn-primary w-100"
                onClick={handleAddCustomer}
              >
                <i className="bi bi-plus-circle me-2"></i>
                Add New Customer
              </button>
            </div>
          </div>

          <div className="card shadow-sm">
            <div className="card-body p-0">
              <div className="table-responsive">
                <table className="table table-hover mb-0">
                  <SortableTableHeader
                    columns={columns}
                    sorting={{ orderBy: sorting.orderBy || '', descending: sorting.descending ?? false }}
                    onSort={handleSort}
                  />
                  <tbody>
                    {customers.map((customer) => (
                      <tr key={customer.id}>
                        <td className="fw-bold">{customer.customerCode}</td>
                        <td>{customer.companyName}</td>
                        <td>{customer.contactPersonName || '-'}</td>
                        <td>{customer.email || '-'}</td>
                        <td>{customer.phone || '-'}</td>
                        <td>
                          <span className={`badge ${customer.isActive ? 'bg-success' : 'bg-secondary'}`}>
                            {customer.isActive ? 'Active' : 'Inactive'}
                          </span>
                        </td>
                        <td>
                          <div className="btn-group" role="group">
                            <button
                              className="btn btn-sm btn-outline-primary"
                              onClick={() => handleEditCustomer(customer)}
                              title="Edit Customer"
                            >
                              <i className="bi bi-pencil"></i>
                            </button>
                            <button
                              className="btn btn-sm btn-outline-danger"
                              onClick={() => handleDeleteCustomer(customer)}
                              title="Delete Customer"
                            >
                              <i className="bi bi-trash"></i>
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

          {/* Results count */}
          {totalCount > 0 && (
            <div className="mt-3 text-muted text-center">
              Showing {(pagination.pageNumber - 1) * pagination.pageSize + 1} to {Math.min(pagination.pageNumber * pagination.pageSize, totalCount)} of {totalCount} customers
            </div>
          )}

          {/* Pagination Controls */}
          <Pagination
            pageNumber={pagination.pageNumber}
            pageSize={pagination.pageSize}
            totalCount={totalCount}
            onPageChange={(page: number) => setPagination({ ...pagination, pageNumber: page })}
            ariaLabel="Customer Pagination"
          />

          {customers.length === 0 && !loading && (
            <div className="text-center py-5">
              <div className="mb-3">
                <i className="bi bi-people display-1 text-muted"></i>
              </div>
              <h5 className="text-muted">No customers found</h5>
              <p className="text-muted">Add your first customer to get started.</p>
              <button
                className="btn btn-primary"
                onClick={handleAddCustomer}
              >
                <i className="bi bi-plus-circle me-2"></i>
                Add Customer
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Customer Modal for Add/Edit */}
      <CustomerModal
        show={showCustomerModal}
        onHide={() => setShowCustomerModal(false)}
        onSave={handleSaveCustomer}
        customer={selectedCustomer}
      />

      {/* Delete Confirmation Modal */}
      <GenericDeleteConfirmationModal
        show={showDeleteModal}
        onHide={() => setShowDeleteModal(false)}
        onConfirm={handleConfirmDelete}
        itemName={customerToDelete?.companyName || ''}
        itemType="customer"
        loading={deleteLoading}
        warningMessage="This will soft delete the customer. The customer and their data will be preserved but hidden from normal views."
      />
    </div>
  );
};

export default CustomersPage;