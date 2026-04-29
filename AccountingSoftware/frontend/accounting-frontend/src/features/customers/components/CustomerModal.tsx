import React, { useEffect } from 'react';
import { Customer, CreateCustomerDto, UpdateCustomerDto } from '../../../types';
import { Modal, TextInput, Textarea, Switch, Button, Group, Stack } from '@mantine/core';
import { useForm } from '@mantine/form';

interface CustomerModalProps {
  opened: boolean;
  onClose: () => void;
  onSave: (customer: CreateCustomerDto | UpdateCustomerDto) => Promise<void>;
  customer?: Customer;
}

const CustomerModal: React.FC<CustomerModalProps> = ({ opened, onClose, onSave, customer }) => {
  const isEdit = !!customer;
  const [loading, setLoading] = React.useState(false);

  const form = useForm({
    initialValues: {
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
      notes: '',
    },
    validate: {
      companyName: (v) => !v.trim() ? 'Company name is required' : null,
      email: (v) => v && !/\S+@\S+\.\S+/.test(v) ? 'Invalid email format' : null,
    },
  });

  useEffect(() => {
    if (customer) {
      form.setValues({
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
        notes: customer.notes || '',
      });
    } else {
      form.reset();
    }
  }, [customer, opened]);

  const handleSubmit = async (values: typeof form.values) => {
    setLoading(true);
    try {
      if (isEdit) {
        const updateDto: UpdateCustomerDto = {
          companyName: values.companyName,
          contactPersonName: values.contactPersonName || undefined,
          email: values.email || undefined,
          phone: values.phone || undefined,
          address: values.address || undefined,
          city: values.city || undefined,
          state: values.state || undefined,
          postalCode: values.postalCode || undefined,
          country: values.country || undefined,
          isActive: values.isActive,
          notes: values.notes || undefined,
        };
        await onSave(updateDto);
      } else {
        const createDto: CreateCustomerDto = {
          companyName: values.companyName,
          contactPersonName: values.contactPersonName || undefined,
          email: values.email || undefined,
          phone: values.phone || undefined,
          address: values.address || undefined,
          city: values.city || undefined,
          state: values.state || undefined,
          postalCode: values.postalCode || undefined,
          country: values.country || undefined,
          notes: values.notes || undefined,
        };
        await onSave(createDto);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal opened={opened} onClose={onClose} title={isEdit ? 'Edit Customer' : 'New Customer'} size="lg" centered>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack gap="sm">
          <TextInput label="Company Name" withAsterisk {...form.getInputProps('companyName')} />
          <TextInput label="Contact Person" {...form.getInputProps('contactPersonName')} />
          <TextInput label="Email" {...form.getInputProps('email')} />
          <TextInput label="Phone" {...form.getInputProps('phone')} />
          <TextInput label="Address" {...form.getInputProps('address')} />
          <Group grow>
            <TextInput label="City" {...form.getInputProps('city')} />
            <TextInput label="State" {...form.getInputProps('state')} />
            <TextInput label="Postal Code" {...form.getInputProps('postalCode')} />
          </Group>
          <TextInput label="Country" {...form.getInputProps('country')} />
          <Textarea label="Notes" {...form.getInputProps('notes')} />
          <Switch label="Active" {...form.getInputProps('isActive', { type: 'checkbox' })} />
          <Group justify="flex-end" mt="md">
            <Button variant="default" onClick={onClose}>Cancel</Button>
            <Button type="submit" loading={loading}>{isEdit ? 'Update' : 'Create'}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  );
};

export default CustomerModal;
