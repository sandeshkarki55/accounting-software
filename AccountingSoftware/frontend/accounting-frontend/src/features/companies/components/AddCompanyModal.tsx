import React, { useEffect } from 'react';
import { CompanyInfo, CreateCompanyInfoDto } from '../../../types/customers';
import { companyInfoService } from '../../../services/companyInfoService';
import { useFormState } from '../../../hooks/useFormState';
import { useValidation } from '../../../hooks/useValidation';
import { Modal, TextInput, Select, Button, Group, Stack, Alert, Checkbox, Divider, Text } from '@mantine/core';
import { IconBuilding, IconPhone, IconMapPin, IconCreditCard, IconInfoCircle } from '@tabler/icons-react';

interface AddCompanyModalProps {
  show: boolean;
  onHide: () => void;
  onCompanySaved: (company: CompanyInfo) => void;
  company?: CompanyInfo;
}

const currencyOptions = [
  { value: 'USD', label: 'USD - US Dollar' },
  { value: 'EUR', label: 'EUR - Euro' },
  { value: 'GBP', label: 'GBP - British Pound' },
  { value: 'CAD', label: 'CAD - Canadian Dollar' },
  { value: 'AUD', label: 'AUD - Australian Dollar' },
  { value: 'JPY', label: 'JPY - Japanese Yen' },
];

const AddCompanyModal: React.FC<AddCompanyModalProps> = ({ show, onHide, onCompanySaved, company }) => {
  const initialForm: CreateCompanyInfoDto = {
    companyName: '', legalName: '', taxNumber: '', registrationNumber: '', email: '', phone: '', website: '',
    address: '', city: '', state: '', postalCode: '', country: '', logoUrl: '', bankName: '', bankAccountNumber: '', bankRoutingNumber: '', currency: 'USD', isDefault: false
  };
  const { formData, setFormData, handleChange, resetForm } = useFormState(initialForm);
  const { errors, setErrors, validate, clearErrors } = useValidation<typeof initialForm>();
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const isEditMode = !!company;

  useEffect(() => {
    if (company) {
      setFormData({
        companyName: company.companyName, legalName: company.legalName || '', taxNumber: company.taxNumber || '',
        registrationNumber: company.registrationNumber || '', email: company.email || '', phone: company.phone || '',
        website: company.website || '', address: company.address || '', city: company.city || '', state: company.state || '',
        postalCode: company.postalCode || '', country: company.country || '', logoUrl: company.logoUrl || '',
        bankName: company.bankName || '', bankAccountNumber: company.bankAccountNumber || '', bankRoutingNumber: company.bankRoutingNumber || '',
        currency: company.currency, isDefault: company.isDefault
      });
    } else {
      resetForm();
    }
    clearErrors();
    setError(null);
  }, [company]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    handleChange(e);
    setError(null);
    if ((errors as any)[e.target.name]) setErrors((prev: any) => ({ ...prev, [e.target.name]: '' }));
  };

  const validateForm = () =>
    validate(() => {
      const newErrors: Record<string, string> = {};
      if (!formData.companyName.trim()) newErrors.companyName = 'Company name is required';
      if (!formData.currency) newErrors.currency = 'Currency is required';
      if (formData.email && !/\S+@\S+\.\S+/.test(formData.email)) newErrors.email = 'Valid email is required';
      if (formData.website && !/^https?:\/\/.+/.test(formData.website) && !/^www\..+/.test(formData.website) && !/\..+/.test(formData.website)) newErrors.website = 'Valid URL is required';
      return newErrors;
    });

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!validateForm()) return;
    setLoading(true);
    setError(null);
    try {
      const saved = isEditMode && company ? await companyInfoService.updateCompanyInfo(company.id, formData) : await companyInfoService.createCompanyInfo(formData);
      onCompanySaved(saved);
      handleClose();
    } catch (err: any) {
      setError(err.response?.data?.message || `Failed to ${isEditMode ? 'update' : 'create'} company`);
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => { resetForm(); clearErrors(); setError(null); onHide(); };

  return (
    <Modal opened={show} onClose={handleClose} title={<Group gap="xs"><IconBuilding size={20} /><Text fw={700}>{isEditMode ? 'Edit Company' : 'Add New Company'}</Text></Group>} size="xl" closeOnClickOutside={false}>
      <form onSubmit={handleSubmit}>
        <Stack gap="md">
          {error && <Alert color="red" icon={<IconInfoCircle size={16} />}>{error}</Alert>}

          <Divider label={<Group gap="xs"><IconInfoCircle size={14} /><Text size="sm" fw={600}>Basic Information</Text></Group>} />
          <Group grow>
            <TextInput label="Company Name *" name="companyName" value={formData.companyName} onChange={handleInputChange} error={errors.companyName} placeholder="Enter company name" />
            <TextInput label="Legal Name" name="legalName" value={formData.legalName} onChange={handleInputChange} placeholder="Legal name (if different)" />
          </Group>
          <Group grow>
            <TextInput label="Tax Number" name="taxNumber" value={formData.taxNumber} onChange={handleInputChange} placeholder="Enter tax number" />
            <TextInput label="Registration Number" name="registrationNumber" value={formData.registrationNumber} onChange={handleInputChange} placeholder="Registration number" />
          </Group>
          <Group grow>
            <Select label="Currency *" data={currencyOptions} value={formData.currency} onChange={v => handleInputChange({ target: { name: 'currency', value: v } } as any)} error={errors.currency} />
            <div>
              <Checkbox label="Set as default company" name="isDefault" checked={formData.isDefault} onChange={e => handleInputChange({ target: { name: 'isDefault', value: e.target.checked, type: 'checkbox' } } as any)} />
              <Text size="xs" c="dimmed" mt={4}>Default company auto-selected when creating invoices</Text>
            </div>
          </Group>

          <Divider label={<Group gap="xs"><IconPhone size={14} /><Text size="sm" fw={600}>Contact Information</Text></Group>} />
          <Group grow>
            <TextInput label="Email" name="email" type="email" value={formData.email} onChange={handleInputChange} error={errors.email} placeholder="Enter email" />
            <TextInput label="Phone" name="phone" type="tel" value={formData.phone} onChange={handleInputChange} placeholder="Enter phone" />
          </Group>
          <TextInput label="Website" name="website" type="url" value={formData.website} onChange={handleInputChange} error={errors.website} placeholder="Enter website URL" />

          <Divider label={<Group gap="xs"><IconMapPin size={14} /><Text size="sm" fw={600}>Address Information</Text></Group>} />
          <TextInput label="Address" name="address" value={formData.address} onChange={handleInputChange} placeholder="Street address" />
          <Group grow>
            <TextInput label="City" name="city" value={formData.city} onChange={handleInputChange} placeholder="City" />
            <TextInput label="State" name="state" value={formData.state} onChange={handleInputChange} placeholder="State" />
            <TextInput label="Postal Code" name="postalCode" value={formData.postalCode} onChange={handleInputChange} placeholder="Postal code" />
          </Group>
          <TextInput label="Country" name="country" value={formData.country} onChange={handleInputChange} placeholder="Country" />

          <Divider label={<Group gap="xs"><IconCreditCard size={14} /><Text size="sm" fw={600}>Banking Information (Optional)</Text></Group>} />
          <Group grow>
            <TextInput label="Bank Name" name="bankName" value={formData.bankName} onChange={handleInputChange} placeholder="Bank name" />
            <TextInput label="Account Number" name="bankAccountNumber" value={formData.bankAccountNumber} onChange={handleInputChange} placeholder="Account number" />
            <TextInput label="Routing Number" name="bankRoutingNumber" value={formData.bankRoutingNumber} onChange={handleInputChange} placeholder="Routing number" />
          </Group>

          <Group justify="flex-end">
            <Button variant="default" onClick={handleClose} disabled={loading}>Cancel</Button>
            <Button type="submit" loading={loading}>{isEditMode ? 'Update Company' : 'Create Company'}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  );
};

export default AddCompanyModal;
