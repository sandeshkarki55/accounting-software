
import React, { useEffect } from 'react';
import { Customer, CreateCustomerDto, UpdateCustomerDto } from '../../types';
import BaseModal from './BaseModal';
import { useFormState } from '../../hooks/useFormState';
import { useValidation } from '../../hooks/useValidation';

interface CustomerModalProps {
  show: boolean;
  onHide: () => void;
  onSave: (customer: CreateCustomerDto | UpdateCustomerDto) => Promise<void>;
  customer?: Customer;
}

const CustomerModal: React.FC<CustomerModalProps> = ({ show, onHide, onSave, customer }) => {
  const initialForm = {
    companyName: '',
    contactPersonName: '',
    email: '',
    phone: '',
    address: '',
    city: '',
    state: '',
    postalCode: '',
    country: '',
    isActive: true,
    notes: ''
  };
  const { formData, setFormData, handleChange, resetForm } = useFormState(initialForm);
  const [loading, setLoading] = React.useState(false);
  const { errors, setErrors, validate, clearErrors } = useValidation<typeof initialForm>();

  const isEditMode = !!customer;

  useEffect(() => {
    if (customer) {
      setFormData({
        companyName: customer.companyName,
        contactPersonName: customer.contactPersonName || '',
        email: customer.email || '',
        phone: customer.phone || '',
        address: customer.address || '',
        city: customer.city || '',
        state: customer.state || '',
        postalCode: customer.postalCode || '',
        country: customer.country || '',
        isActive: customer.isActive,
        notes: customer.notes || ''
      });
    } else {
      resetForm();
    }
    clearErrors();
  }, [customer, show, setFormData, resetForm, clearErrors]);

  const validateForm = () =>
    validate(() => {
      const newErrors: { [key: string]: string } = {};
      if (!formData.companyName.trim()) {
        newErrors.companyName = 'Company name is required';
      }
      if (formData.email && !/\S+@\S+\.\S+/.test(formData.email)) {
        newErrors.email = 'Invalid email format';
      }
      return newErrors;
    });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;
    setLoading(true);
    try {
      if (isEditMode) {
        const updateData: UpdateCustomerDto = {
          companyName: formData.companyName,
          contactPersonName: formData.contactPersonName || undefined,
          email: formData.email || undefined,
          phone: formData.phone || undefined,
          address: formData.address || undefined,
          city: formData.city || undefined,
          state: formData.state || undefined,
          postalCode: formData.postalCode || undefined,
          country: formData.country || undefined,
          isActive: formData.isActive,
          notes: formData.notes || undefined
        };
        await onSave(updateData);
      } else {
        const createData: CreateCustomerDto = {
          companyName: formData.companyName,
          contactPersonName: formData.contactPersonName || undefined,
          email: formData.email || undefined,
          phone: formData.phone || undefined,
          address: formData.address || undefined,
          city: formData.city || undefined,
          state: formData.state || undefined,
          postalCode: formData.postalCode || undefined,
          country: formData.country || undefined,
          notes: formData.notes || undefined
        };
        await onSave(createData);
      }
      onHide();
    } catch (error) {
      console.error('Error saving customer:', error);
    } finally {
      setLoading(false);
    }
  };

  if (!show) return null;

  const modalTitle = (
    <>
      <i className="bi bi-person-plus me-2"></i>
      {isEditMode ? 'Edit Customer' : 'Add New Customer'}
    </>
  );

  const modalFooter = (
    <>
      <button type="button" className="btn btn-secondary" onClick={onHide}>
        <i className="bi bi-x-circle me-2"></i>
        Cancel
      </button>
      <button type="submit" className="btn btn-primary save-btn" disabled={loading}>
        {loading ? (
          <>
            <span className="spinner-border spinner-border-sm me-2" role="status"></span>
            {isEditMode ? 'Updating...' : 'Creating...'}
          </>
        ) : (
          <>
            <i className="bi bi-check-circle me-2"></i>
            {isEditMode ? 'Update Customer' : 'Create Customer'}
          </>
        )}
      </button>
    </>
  );

  return (
    <BaseModal
      show={show}
      onHide={onHide}
      title={modalTitle}
      footer={null}
      size="lg"
      className="customer-modal"
      ariaLabel={isEditMode ? 'Edit Customer Modal' : 'Add Customer Modal'}
    >
      <form onSubmit={handleSubmit}>
        {isEditMode && (
          <div className="alert alert-info mb-3">
            <i className="bi bi-info-circle me-2"></i>
            Customer Code: <strong>{customer?.customerCode}</strong>
          </div>
        )}
        <div className="row">
          <div className="col-md-12">
            <div className="mb-3">
              <label htmlFor="companyName" className="form-label">Company Name *</label>
              <input
                type="text"
                className={`form-control ${errors.companyName ? 'is-invalid' : ''}`}
                id="companyName"
                name="companyName"
                value={formData.companyName}
                onChange={handleChange}
                placeholder="e.g., Acme Corp"
              />
              {errors.companyName && <div className="invalid-feedback">{errors.companyName}</div>}
            </div>
          </div>
        </div>

        <div className="row">
          <div className="col-md-6">
            <div className="mb-3">
              <label htmlFor="contactPersonName" className="form-label">Contact Person</label>
              <input
                type="text"
                className="form-control"
                id="contactPersonName"
                name="contactPersonName"
                value={formData.contactPersonName}
                onChange={handleChange}
                placeholder="e.g., John Doe"
              />
            </div>
          </div>
          <div className="col-md-6">
            <div className="mb-3">
              <label htmlFor="email" className="form-label">Email</label>
              <input
                type="email"
                className={`form-control ${errors.email ? 'is-invalid' : ''}`}
                id="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                placeholder="e.g., john@acme.com"
              />
              {errors.email && <div className="invalid-feedback">{errors.email}</div>}
            </div>
          </div>
        </div>

        <div className="row">
          <div className="col-md-6">
            <div className="mb-3">
              <label htmlFor="phone" className="form-label">Phone</label>
              <input
                type="tel"
                className="form-control"
                id="phone"
                name="phone"
                value={formData.phone}
                onChange={handleChange}
                placeholder="e.g., +1 555-1234"
              />
            </div>
          </div>
          <div className="col-md-6">
            <div className="mb-3">
              <label htmlFor="address" className="form-label">Address</label>
              <input
                type="text"
                className="form-control"
                id="address"
                name="address"
                value={formData.address}
                onChange={handleChange}
                placeholder="e.g., 123 Main St"
              />
            </div>
          </div>
        </div>

        <div className="row">
          <div className="col-md-4">
            <div className="mb-3">
              <label htmlFor="city" className="form-label">City</label>
              <input
                type="text"
                className="form-control"
                id="city"
                name="city"
                value={formData.city}
                onChange={handleChange}
                placeholder="e.g., New York"
              />
            </div>
          </div>
          <div className="col-md-4">
            <div className="mb-3">
              <label htmlFor="state" className="form-label">State</label>
              <input
                type="text"
                className="form-control"
                id="state"
                name="state"
                value={formData.state}
                onChange={handleChange}
                placeholder="e.g., NY"
              />
            </div>
          </div>
          <div className="col-md-4">
            <div className="mb-3">
              <label htmlFor="postalCode" className="form-label">Postal Code</label>
              <input
                type="text"
                className="form-control"
                id="postalCode"
                name="postalCode"
                value={formData.postalCode}
                onChange={handleChange}
                placeholder="e.g., 10001"
              />
            </div>
          </div>
        </div>

        <div className="row">
          <div className="col-md-6">
            <div className="mb-3">
              <label htmlFor="country" className="form-label">Country</label>
              <input
                type="text"
                className="form-control"
                id="country"
                name="country"
                value={formData.country}
                onChange={handleChange}
                placeholder="e.g., USA"
              />
            </div>
          </div>
          <div className="col-md-6">
            <div className="mb-3">
              <div className="form-check mt-4">
                <input
                  className="form-check-input"
                  type="checkbox"
                  id="isActive"
                  name="isActive"
                  checked={formData.isActive}
                  onChange={handleChange}
                />
                <label className="form-check-label" htmlFor="isActive">
                  Active Customer
                </label>
              </div>
            </div>
          </div>
        </div>

        <div className="mb-3">
          <label htmlFor="notes" className="form-label">Notes</label>
          <textarea
            className="form-control"
            id="notes"
            name="notes"
            rows={3}
            value={formData.notes}
            onChange={handleChange}
            placeholder="Additional notes about the customer"
          />
        </div>

        <div className="modal-footer">
          {modalFooter}
        </div>
      </form>
    </BaseModal>
  );
};

export default CustomerModal;
