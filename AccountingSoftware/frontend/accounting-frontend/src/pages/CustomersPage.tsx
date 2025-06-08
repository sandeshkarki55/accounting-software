import React, { useState, useEffect } from 'react';
import { Customer, CreateCustomerDto, UpdateCustomerDto } from '../types';
import { customerService } from '../services/api';
import CustomerModal from '../components/modals/CustomerModal';

const CustomersPage: React.FC = () => {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCustomerModal, setShowCustomerModal] = useState(false);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | undefined>();

  useEffect(() => {
    loadCustomers();
  }, []);

  const loadCustomers = async () => {
    try {
      setLoading(true);
      const data = await customerService.getCustomers();
      setCustomers(data);
      setError(null);
    } catch (err) {
      setError('Failed to load customers');
      console.error('Error loading customers:', err);
    } finally {
      setLoading(false);
    }
  };

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
      await loadCustomers();
      setShowCustomerModal(false);
    } catch (err) {
      console.error('Error saving customer:', err);
      throw err;
    }
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
      <button className="btn btn-sm btn-outline-danger ms-2" onClick={loadCustomers}>
        Try Again
      </button>
    </div>
  );

  return (
    <div className="container">
      <div className="row">
        <div className="col-12">
          <h1 className="mb-4 text-dark">Customers</h1>
          
          <div className="mb-4">
            <button 
              className="btn btn-primary" 
              onClick={handleAddCustomer}
            >
              <i className="bi bi-plus-circle me-2"></i>
              Add New Customer
            </button>
          </div>

          <div className="card shadow-sm">
            <div className="card-body p-0">
              <div className="table-responsive">
                <table className="table table-hover mb-0">
                  <thead className="table-dark">
                    <tr>
                      <th scope="col">Customer Code</th>
                      <th scope="col">Company Name</th>
                      <th scope="col">Contact Person</th>
                      <th scope="col">Email</th>
                      <th scope="col">Phone</th>
                      <th scope="col">Status</th>
                      <th scope="col">Actions</th>
                    </tr>
                  </thead>
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
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          {customers.length === 0 && (
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
    </div>
  );
};

export default CustomersPage;