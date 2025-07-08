
import React, { useEffect } from 'react';
import { Invoice, CreateInvoiceDto, CreateInvoiceItemDto, Customer, CompanyInfo } from '../../types';
import BaseModal from './BaseModal';
import { useFormState } from '../../hooks/useFormState';
import { useValidation } from '../../hooks/useValidation';

interface InvoiceModalProps {
  show: boolean;
  onHide: () => void;
  onSave: (invoice: CreateInvoiceDto) => Promise<void>;
  invoice?: Invoice;
  customers: Customer[];
  companyInfos: CompanyInfo[];
}

const InvoiceModal: React.FC<InvoiceModalProps> = ({ show, onHide, onSave, invoice, customers, companyInfos }) => {
  // Support both paged and non-paged data
  const customerList: Customer[] = Array.isArray(customers)
    ? customers
    : (customers && Array.isArray((customers as any).items))
      ? (customers as any).items
      : [];
  const companyInfoList: CompanyInfo[] = Array.isArray(companyInfos)
    ? companyInfos
    : (companyInfos && Array.isArray((companyInfos as any).items))
      ? (companyInfos as any).items
      : [];
  const initialForm = {
    invoiceDate: new Date().toISOString().split('T')[0],
    dueDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    customerId: 0,
    companyInfoId: companyInfoList.find(c => c.isDefault)?.id as number | undefined,
    description: '',
    taxRate: 0,
    discountAmount: 0,
    notes: '',
    terms: 'Payment due within 30 days'
  };
  const { formData, setFormData, handleChange, resetForm } = useFormState(initialForm);
  const { errors, setErrors, validate, clearErrors } = useValidation<typeof initialForm & { items?: string }>();
  const [items, setItems] = React.useState<CreateInvoiceItemDto[]>([{ description: '', quantity: 1, unitPrice: 0, sortOrder: 0 }]);
  const [loading, setLoading] = React.useState(false);
  const isEditMode = !invoice ? false : true;

  useEffect(() => {
    if (invoice) {
      setFormData({
        invoiceDate: invoice.invoiceDate.split('T')[0],
        dueDate: invoice.dueDate.split('T')[0],
        customerId: invoice.customerId,
        companyInfoId: invoice.companyInfoId,
        description: invoice.description || '',
        taxRate: invoice.taxRate,
        discountAmount: invoice.discountAmount,
        notes: invoice.notes || '',
        terms: invoice.terms || ''
      });
      setItems(invoice.items.map((item, index) => ({
        description: item.description,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        sortOrder: index
      })));
    } else {
      resetForm();
      setItems([{ description: '', quantity: 1, unitPrice: 0, sortOrder: 0 }]);
    }
    clearErrors();
  }, [invoice, companyInfos, setFormData, resetForm, clearErrors]);

  const validateForm = () =>
    validate(() => {
      const newErrors: { [key: string]: string } = {};
      if (!formData.invoiceDate) {
        newErrors.invoiceDate = 'Invoice date is required';
      }
      if (!formData.dueDate) {
        newErrors.dueDate = 'Due date is required';
      }
      if (formData.customerId === 0) {
        newErrors.customerId = 'Customer is required';
      }
      if (items.length === 0 || items.every(item => !item.description.trim())) {
        newErrors.items = 'At least one item is required';
      }
      return newErrors;
    });

  const handleItemChange = (index: number, field: keyof CreateInvoiceItemDto, value: string | number) => {
    const newItems = [...items];
    newItems[index] = { ...newItems[index], [field]: value };
    setItems(newItems);
  };

  const addItem = () => {
    setItems([...items, { description: '', quantity: 1, unitPrice: 0, sortOrder: items.length }]);
  };

  const removeItem = (index: number) => {
    if (items.length > 1) {
      const newItems = items.filter((_, i) => i !== index);
      newItems.forEach((item, i) => item.sortOrder = i);
      setItems(newItems);
    }
  };

  const calculateItemAmount = (quantity: number, unitPrice: number) => quantity * unitPrice;
  const calculateSubTotal = () => items.reduce((sum, item) => sum + calculateItemAmount(item.quantity, item.unitPrice), 0);
  const calculateTaxAmount = () => calculateSubTotal() * (formData.taxRate / 100);
  const calculateTotal = () => calculateSubTotal() + calculateTaxAmount() - formData.discountAmount;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;
    setLoading(true);
    try {
      const invoiceData: CreateInvoiceDto = {
        ...formData,
        items: items.filter(item => item.description.trim())
      };
      await onSave(invoiceData);
      onHide();
    } catch (error) {
      console.error('Error saving invoice:', error);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount: number) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);

  if (!show) return null;

  const modalTitle = (
    <>
      <i className="bi bi-receipt me-2"></i>
      {isEditMode ? (invoice ? `Invoice ${invoice.invoiceNumber}` : 'Edit Invoice') : 'Create New Invoice'}
    </>
  );

  const modalFooter = (
    <>
      <button type="button" className="btn btn-secondary" onClick={onHide}>
        <i className="bi bi-x-circle me-2"></i>
        {isEditMode ? 'Close' : 'Cancel'}
      </button>
      {!isEditMode && (
        <button type="submit" className="btn btn-primary save-btn" disabled={loading}>
          {loading ? (
            <>
              <span className="spinner-border spinner-border-sm me-2" role="status"></span>
              Creating Invoice...
            </>
          ) : (
            <>
              <i className="bi bi-receipt me-2"></i>
              Create Invoice
            </>
          )}
        </button>
      )}
    </>
  );

  return (
    <BaseModal
      show={show}
      onHide={onHide}
      title={modalTitle}
      footer={null}
      size="xl"
      className="invoice-modal"
      ariaLabel={isEditMode ? 'Edit Invoice Modal' : 'Add Invoice Modal'}
    >
      <form onSubmit={handleSubmit}>
        <div className="modal-body">
          {isEditMode && invoice && (
            <div className="alert alert-info mb-3">
              <i className="bi bi-info-circle me-2"></i>
              Invoice Number: <strong>{invoice.invoiceNumber}</strong>
            </div>
          )}
          {/* Customer Selection */}
          <div className="row mb-3">
            <div className="col-md-6">
              <label htmlFor="customerId" className="form-label">Customer *</label>
              <select
                className={`form-select ${errors.customerId ? 'is-invalid' : ''}`}
                id="customerId"
                name="customerId"
                value={formData.customerId}
                onChange={handleChange}
                disabled={isEditMode}
              >
                <option value={0}>Select a customer...</option>
                {customerList.map((customer) => (
                  <option key={customer.id} value={customer.id}>
                    {customer.companyName} ({customer.customerCode})
                  </option>
                ))}
              </select>
              {errors.customerId && <div className="invalid-feedback">{errors.customerId}</div>}
            </div>
          </div>

          {/* Invoice Dates and Company */}
          <div className="row mb-3">
            <div className="col-md-4">
              <label htmlFor="invoiceDate" className="form-label">Invoice Date *</label>
              <input
                type="date"
                className={`form-control ${errors.invoiceDate ? 'is-invalid' : ''}`}
                id="invoiceDate"
                name="invoiceDate"
                value={formData.invoiceDate}
                onChange={handleChange}
                disabled={isEditMode}
              />
              {errors.invoiceDate && <div className="invalid-feedback">{errors.invoiceDate}</div>}
            </div>
            <div className="col-md-4">
              <label htmlFor="dueDate" className="form-label">Due Date *</label>
              <input
                type="date"
                className={`form-control ${errors.dueDate ? 'is-invalid' : ''}`}
                id="dueDate"
                name="dueDate"
                value={formData.dueDate}
                onChange={handleChange}
                disabled={isEditMode}
              />
              {errors.dueDate && <div className="invalid-feedback">{errors.dueDate}</div>}
            </div>
            <div className="col-md-4">
              <label htmlFor="companyInfoId" className="form-label">Company</label>
              <select
                className="form-select"
                id="companyInfoId"
                name="companyInfoId"
                value={formData.companyInfoId || ''}
                onChange={handleChange}
                disabled={isEditMode}
              >
                <option value="">Use default company...</option>
                {companyInfoList.map((company) => (
                  <option key={company.id} value={company.id}>
                    {company.companyName}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Description */}
          <div className="mb-3">
            <label htmlFor="description" className="form-label">Description</label>
            <input
              type="text"
              className="form-control"
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              placeholder="Brief description of the invoice"
              disabled={isEditMode}
            />
          </div>

          {/* Invoice Items */}
          <div className="mb-4">
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h6 className="mb-0">Invoice Items</h6>
              {!isEditMode && (
                <button type="button" className="btn btn-sm btn-outline-primary" onClick={addItem}>
                  <i className="bi bi-plus-circle me-1"></i>
                  Add Item
                </button>
              )}
            </div>
            {errors.items && <div className="alert alert-danger">{errors.items}</div>}
            <div className="table-responsive">
              <table className="table table-sm">
                <thead>
                  <tr>
                    <th>Description</th>
                    <th style={{width: '100px'}}>Quantity</th>
                    <th style={{width: '120px'}}>Unit Price</th>
                    <th style={{width: '120px'}}>Amount</th>
                    {!isEditMode && <th style={{width: '50px'}}>Actions</th>}
                  </tr>
                </thead>
                <tbody>
                  {items.map((item, index) => (
                    <tr key={index}>
                      <td>
                        <input
                          type="text"
                          className="form-control form-control-sm"
                          value={item.description}
                          onChange={e => handleItemChange(index, 'description', e.target.value)}
                          placeholder="Item description"
                          disabled={isEditMode}
                        />
                      </td>
                      <td>
                        <input
                          type="number"
                          className="form-control form-control-sm"
                          value={item.quantity}
                          onChange={e => handleItemChange(index, 'quantity', parseInt(e.target.value) || 0)}
                          min="1"
                          disabled={isEditMode}
                        />
                      </td>
                      <td>
                        <input
                          type="number"
                          className="form-control form-control-sm"
                          value={item.unitPrice}
                          onChange={e => handleItemChange(index, 'unitPrice', parseFloat(e.target.value) || 0)}
                          min="0"
                          step="0.01"
                          disabled={isEditMode}
                        />
                      </td>
                      <td>
                        <span className="fw-bold">
                          {formatCurrency(calculateItemAmount(item.quantity, item.unitPrice))}
                        </span>
                      </td>
                      {!isEditMode && (
                        <td>
                          <button
                            type="button"
                            className="btn btn-sm btn-outline-danger"
                            onClick={() => removeItem(index)}
                            disabled={items.length === 1}
                          >
                            <i className="bi bi-trash"></i>
                          </button>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Invoice Totals */}
          <div className="row mb-4">
            <div className="col-md-8">
              <div className="row">
                <div className="col-md-6">
                  <label htmlFor="taxRate" className="form-label">Tax Rate (%)</label>
                  <input
                    type="number"
                    className="form-control"
                    id="taxRate"
                    name="taxRate"
                    value={formData.taxRate}
                    onChange={handleChange}
                    min="0"
                    max="100"
                    step="0.01"
                    disabled={isEditMode}
                  />
                </div>
                <div className="col-md-6">
                  <label htmlFor="discountAmount" className="form-label">Discount Amount</label>
                  <input
                    type="number"
                    className="form-control"
                    id="discountAmount"
                    name="discountAmount"
                    value={formData.discountAmount}
                    onChange={handleChange}
                    min="0"
                    step="0.01"
                    disabled={isEditMode}
                  />
                </div>
              </div>
            </div>
            <div className="col-md-4">
              <div className="card bg-light">
                <div className="card-body">
                  <div className="d-flex justify-content-between">
                    <span>Subtotal:</span>
                    <span>{formatCurrency(calculateSubTotal())}</span>
                  </div>
                  <div className="d-flex justify-content-between">
                    <span>Tax ({formData.taxRate}%):</span>
                    <span>{formatCurrency(calculateTaxAmount())}</span>
                  </div>
                  <div className="d-flex justify-content-between">
                    <span>Discount:</span>
                    <span>-{formatCurrency(formData.discountAmount)}</span>
                  </div>
                  <hr />
                  <div className="d-flex justify-content-between fw-bold">
                    <span>Total:</span>
                    <span>{formatCurrency(calculateTotal())}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Notes and Terms */}
          <div className="row mb-4">
            <div className="col-md-6">
              <label htmlFor="notes" className="form-label">Notes</label>
              <textarea
                className="form-control"
                id="notes"
                name="notes"
                rows={3}
                value={formData.notes}
                onChange={handleChange}
                placeholder="Additional notes for the invoice"
                disabled={isEditMode}
              />
            </div>
            <div className="col-md-6">
              <label htmlFor="terms" className="form-label">Terms & Conditions</label>
              <textarea
                className="form-control"
                id="terms"
                name="terms"
                rows={3}
                value={formData.terms}
                onChange={handleChange}
                placeholder="Payment terms and conditions"
                disabled={isEditMode}
              />
            </div>
          </div>
        </div>
        <div className="modal-footer">{modalFooter}</div>
      </form>
    </BaseModal>
  );
};

export default InvoiceModal;