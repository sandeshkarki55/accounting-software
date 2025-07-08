
import React, { useEffect } from 'react';
import { CompanyInfo, CreateCompanyInfoDto } from '../../types/customers';
import { companyInfoService } from '../../services/api';
import BaseModal from './BaseModal';
import { useFormState } from '../../hooks/useFormState';
import { useValidation } from '../../hooks/useValidation';

interface AddCompanyModalProps {
  show: boolean;
  onHide: () => void;
  onCompanySaved: (company: CompanyInfo) => void;
  company?: CompanyInfo;
}

const AddCompanyModal: React.FC<AddCompanyModalProps> = ({ show, onHide, onCompanySaved, company }) => {
  const initialForm: CreateCompanyInfoDto = {
    companyName: '',
    legalName: '',
    taxNumber: '',
    registrationNumber: '',
    email: '',
    phone: '',
    website: '',
    address: '',
    city: '',
    state: '',
    postalCode: '',
    country: '',
    logoUrl: '',
    bankName: '',
    bankAccountNumber: '',
    bankRoutingNumber: '',
    currency: 'USD',
    isDefault: false
  };
  const { formData, setFormData, handleChange, resetForm } = useFormState(initialForm);
  const { errors, setErrors, validate, clearErrors } = useValidation<typeof initialForm>();
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const isEditMode = !!company;

  useEffect(() => {
    if (company) {
      setFormData({
        companyName: company.companyName,
        legalName: company.legalName || '',
        taxNumber: company.taxNumber || '',
        registrationNumber: company.registrationNumber || '',
        email: company.email || '',
        phone: company.phone || '',
        website: company.website || '',
        address: company.address || '',
        city: company.city || '',
        state: company.state || '',
        postalCode: company.postalCode || '',
        country: company.country || '',
        logoUrl: company.logoUrl || '',
        bankName: company.bankName || '',
        bankAccountNumber: company.bankAccountNumber || '',
        bankRoutingNumber: company.bankRoutingNumber || '',
        currency: company.currency,
        isDefault: company.isDefault
      });
    } else {
      resetForm();
    }
    clearErrors();
    setError(null);
  }, [company, setFormData, resetForm, clearErrors]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    handleChange(e);
    setError(null);
    if ((errors as any)[e.target.name]) {
      setErrors((prev: any) => ({ ...prev, [e.target.name]: '' }));
    }
  };

  const validateForm = () =>
    validate(() => {
      const newErrors: { [key: string]: string } = {};
      if (!formData.companyName.trim()) {
        newErrors.companyName = 'Company name is required';
      }
      if (!formData.currency) {
        newErrors.currency = 'Currency is required';
      }
      if (formData.email && !/\S+@\S+\.\S+/.test(formData.email)) {
        newErrors.email = 'Please enter a valid email address';
      }
      if (formData.website && !/^https?:\/\/.+/.test(formData.website) && !/^www\..+/.test(formData.website) && !/\..+/.test(formData.website)) {
        newErrors.website = 'Please enter a valid website URL';
      }
      return newErrors;
    });

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!validateForm()) return;
    setLoading(true);
    setError(null);
    try {
      let savedCompany: CompanyInfo;
      if (isEditMode && company) {
        savedCompany = await companyInfoService.updateCompanyInfo(company.id, formData);
      } else {
        savedCompany = await companyInfoService.createCompanyInfo(formData);
      }
      onCompanySaved(savedCompany);
      handleClose();
    } catch (err: any) {
      setError(err.response?.data?.message || `Failed to ${isEditMode ? 'update' : 'create'} company`);
      console.error(`Error ${isEditMode ? 'updating' : 'creating'} company:`, err);
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    resetForm();
    clearErrors();
    setError(null);
    onHide();
  };

  if (!show) return null;

  const modalTitle = (
    <>
      <i className="bi bi-building me-2"></i>
      {isEditMode ? 'Edit Company' : 'Add New Company'}
    </>
  );

  const modalFooter = (
    <>
      <button type="button" className="btn btn-secondary" onClick={handleClose} disabled={loading}>
        <i className="bi bi-x-circle me-2"></i>
        Cancel
      </button>
      <button type="submit" className="btn btn-primary save-btn" disabled={loading}>
        {loading ? (
          <>
            <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
            {isEditMode ? 'Updating...' : 'Creating...'}
          </>
        ) : (
          <>
            <i className="bi bi-check-circle me-2"></i>
            {isEditMode ? 'Update Company' : 'Create Company'}
          </>
        )}
      </button>
    </>
  );

  return (
    <BaseModal
      show={show}
      onHide={handleClose}
      title={modalTitle}
      footer={null}
      size="xl"
      className="add-company-modal"
      ariaLabel={isEditMode ? 'Edit Company Modal' : 'Add Company Modal'}
    >
      <form onSubmit={handleSubmit}>
        <div className="modal-body">
          {error && (
            <div className="alert alert-danger mb-3" role="alert">
              <i className="bi bi-exclamation-circle me-2"></i>
              {error}
            </div>
          )}
          {/* Basic Information */}
          <div className="section-header mb-3">
            <h6 className="text-primary mb-2">
              <i className="bi bi-info-circle me-2"></i>
              Basic Information
            </h6>
          </div>
          <div className="row mb-3">
            <div className="col-md-6">
              <label htmlFor="companyName" className="form-label">Company Name *</label>
              <input
                type="text"
                className={`form-control ${errors.companyName ? 'is-invalid' : ''}`}
                id="companyName"
                name="companyName"
                value={formData.companyName}
                onChange={handleInputChange}
                placeholder="Enter company name"
              />
              {errors.companyName && <div className="invalid-feedback">{errors.companyName}</div>}
            </div>
            <div className="col-md-6">
              <label htmlFor="legalName" className="form-label">Legal Name</label>
              <input
                type="text"
                className="form-control"
                id="legalName"
                name="legalName"
                value={formData.legalName}
                onChange={handleInputChange}
                placeholder="Enter legal name (if different)"
              />
            </div>
          </div>
          <div className="row mb-3">
            <div className="col-md-6">
              <label htmlFor="taxNumber" className="form-label">Tax Number</label>
              <input
                type="text"
                className="form-control"
                id="taxNumber"
                name="taxNumber"
                value={formData.taxNumber}
                onChange={handleInputChange}
                placeholder="Enter tax number"
              />
            </div>
            <div className="col-md-6">
              <label htmlFor="registrationNumber" className="form-label">Registration Number</label>
              <input
                type="text"
                className="form-control"
                id="registrationNumber"
                name="registrationNumber"
                value={formData.registrationNumber}
                onChange={handleInputChange}
                placeholder="Enter registration number"
              />
            </div>
          </div>
          <div className="row mb-4">
            <div className="col-md-6">
              <label htmlFor="currency" className="form-label">Currency *</label>
              <select
                className={`form-select ${errors.currency ? 'is-invalid' : ''}`}
                id="currency"
                name="currency"
                value={formData.currency}
                onChange={handleInputChange}
              >
                <option value="USD">USD - US Dollar</option>
                <option value="EUR">EUR - Euro</option>
                <option value="GBP">GBP - British Pound</option>
                <option value="CAD">CAD - Canadian Dollar</option>
                <option value="AUD">AUD - Australian Dollar</option>
                <option value="JPY">JPY - Japanese Yen</option>
              </select>
              {errors.currency && <div className="invalid-feedback">{errors.currency}</div>}
            </div>
            <div className="col-md-6 d-flex align-items-end">
              <div className="form-check mb-2">
                <input
                  type="checkbox"
                  className="form-check-input"
                  id="isDefault"
                  name="isDefault"
                  checked={formData.isDefault}
                  onChange={handleInputChange}
                />
                <label className="form-check-label" htmlFor="isDefault">
                  Set as default company
                </label>
                <div className="form-text">
                  Default company will be automatically selected when creating invoices
                </div>
              </div>
            </div>
          </div>
          {/* Contact Information */}
          <div className="section-header mb-3">
            <h6 className="text-primary mb-2">
              <i className="bi bi-telephone me-2"></i>
              Contact Information
            </h6>
          </div>
          <div className="row mb-3">
            <div className="col-md-6">
              <label htmlFor="email" className="form-label">Email</label>
              <input
                type="email"
                className={`form-control ${errors.email ? 'is-invalid' : ''}`}
                id="email"
                name="email"
                value={formData.email}
                onChange={handleInputChange}
                placeholder="Enter email address"
              />
              {errors.email && <div className="invalid-feedback">{errors.email}</div>}
            </div>
            <div className="col-md-6">
              <label htmlFor="phone" className="form-label">Phone</label>
              <input
                type="tel"
                className="form-control"
                id="phone"
                name="phone"
                value={formData.phone}
                onChange={handleInputChange}
                placeholder="Enter phone number"
              />
            </div>
          </div>
          <div className="row mb-4">
            <div className="col-md-12">
              <label htmlFor="website" className="form-label">Website</label>
              <input
                type="url"
                className={`form-control ${errors.website ? 'is-invalid' : ''}`}
                id="website"
                name="website"
                value={formData.website}
                onChange={handleInputChange}
                placeholder="Enter website URL"
              />
              {errors.website && <div className="invalid-feedback">{errors.website}</div>}
            </div>
          </div>
          {/* Address Information */}
          <div className="section-header mb-3">
            <h6 className="text-primary mb-2">
              <i className="bi bi-geo-alt me-2"></i>
              Address Information
            </h6>
          </div>
          <div className="row mb-3">
            <div className="col-md-12">
              <label htmlFor="address" className="form-label">Address</label>
              <input
                type="text"
                className="form-control"
                id="address"
                name="address"
                value={formData.address}
                onChange={handleInputChange}
                placeholder="Enter street address"
              />
            </div>
          </div>
          <div className="row mb-4">
            <div className="col-md-6">
              <label htmlFor="city" className="form-label">City</label>
              <input
                type="text"
                className="form-control"
                id="city"
                name="city"
                value={formData.city}
                onChange={handleInputChange}
                placeholder="Enter city"
              />
            </div>
            <div className="col-md-3">
              <label htmlFor="state" className="form-label">State</label>
              <input
                type="text"
                className="form-control"
                id="state"
                name="state"
                value={formData.state}
                onChange={handleInputChange}
                placeholder="Enter state"
              />
            </div>
            <div className="col-md-3">
              <label htmlFor="postalCode" className="form-label">Postal Code</label>
              <input
                type="text"
                className="form-control"
                id="postalCode"
                name="postalCode"
                value={formData.postalCode}
                onChange={handleInputChange}
                placeholder="Enter postal code"
              />
            </div>
          </div>
          <div className="row mb-4">
            <div className="col-md-12">
              <label htmlFor="country" className="form-label">Country</label>
              <input
                type="text"
                className="form-control"
                id="country"
                name="country"
                value={formData.country}
                onChange={handleInputChange}
                placeholder="Enter country"
              />
            </div>
          </div>
          {/* Banking Information */}
          <div className="section-header mb-3">
            <h6 className="text-primary mb-2">
              <i className="bi bi-bank me-2"></i>
              Banking Information (Optional)
            </h6>
          </div>
          <div className="row mb-3">
            <div className="col-md-12">
              <label htmlFor="bankName" className="form-label">Bank Name</label>
              <input
                type="text"
                className="form-control"
                id="bankName"
                name="bankName"
                value={formData.bankName}
                onChange={handleInputChange}
                placeholder="Enter bank name"
              />
            </div>
          </div>
          <div className="row mb-3">
            <div className="col-md-6">
              <label htmlFor="bankAccountNumber" className="form-label">Account Number</label>
              <input
                type="text"
                className="form-control"
                id="bankAccountNumber"
                name="bankAccountNumber"
                value={formData.bankAccountNumber}
                onChange={handleInputChange}
                placeholder="Enter account number"
              />
            </div>
            <div className="col-md-6">
              <label htmlFor="bankRoutingNumber" className="form-label">Routing Number</label>
              <input
                type="text"
                className="form-control"
                id="bankRoutingNumber"
                name="bankRoutingNumber"
                value={formData.bankRoutingNumber}
                onChange={handleInputChange}
                placeholder="Enter routing number"
              />
            </div>
          </div>
        </div>
        <div className="modal-footer">{modalFooter}</div>
      </form>

    </BaseModal>
  );
}

export default AddCompanyModal;
