import React, { useEffect } from 'react';
import { Account, CreateAccountDto, UpdateAccountDto, AccountType } from '../../../types';
import { Modal, TextInput, Textarea, Switch, Button, Group, Stack, Select } from '@mantine/core';
import { useForm } from '@mantine/form';

interface AccountModalProps {
  opened: boolean;
  onClose: () => void;
  onSave: (account: CreateAccountDto | UpdateAccountDto) => Promise<void>;
  account?: Account;
  accounts: Account[];
}

const typeOptions = [
  { value: String(AccountType.Asset), label: 'Asset' },
  { value: String(AccountType.Liability), label: 'Liability' },
  { value: String(AccountType.Equity), label: 'Equity' },
  { value: String(AccountType.Revenue), label: 'Revenue' },
  { value: String(AccountType.Expense), label: 'Expense' },
];

const AccountModal: React.FC<AccountModalProps> = ({ opened, onClose, onSave, account, accounts }) => {
  const isEdit = !!account;
  const [loading, setLoading] = React.useState(false);

  const form = useForm({
    initialValues: {
      accountCode: '',
      accountName: '',
      accountType: String(AccountType.Asset),
      description: '',
      parentAccountId: '',
      isActive: true,
    },
    validate: {
      accountCode: (v, values) => {
        if (!v.trim()) return 'Account code is required';
        if (v.length < 3) return 'Must be at least 3 characters';
        if (!isEdit || (isEdit && v !== account?.accountCode)) {
          if (accounts.some(a => a.accountCode === v)) return 'Account code already exists';
        }
        return null;
      },
      accountName: (v) => !v.trim() ? 'Account name is required' : null,
      description: (v) => !v.trim() ? 'Description is required' : null,
    },
  });

  useEffect(() => {
    if (account) {
      form.setValues({
        accountCode: account.accountCode,
        accountName: account.accountName,
        accountType: String(account.accountType),
        description: account.description,
        parentAccountId: account.parentAccountId ? String(account.parentAccountId) : '',
        isActive: account.isActive,
      });
    } else {
      form.reset();
    }
  }, [account, opened]);

  const handleSubmit = async (values: typeof form.values) => {
    setLoading(true);
    try {
      if (isEdit) {
        await onSave({ accountName: values.accountName, description: values.description, isActive: values.isActive } as UpdateAccountDto);
      } else {
        await onSave({
          accountCode: values.accountCode,
          accountName: values.accountName,
          accountType: Number(values.accountType) as AccountType,
          description: values.description,
          parentAccountId: values.parentAccountId ? Number(values.parentAccountId) : undefined,
        } as CreateAccountDto);
      }
    } finally { setLoading(false); }
  };

  const parentOptions = [
    { value: '', label: 'None (Root Account)' },
    ...accounts.filter(a => a.id !== account?.id).map(a => ({ value: String(a.id), label: `${a.accountCode} - ${a.accountName}` })),
  ];

  return (
    <Modal opened={opened} onClose={onClose} title={isEdit ? 'Edit Account' : 'New Account'} size="lg" centered>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack gap="sm">
          <TextInput label="Account Code" withAsterisk disabled={isEdit} {...form.getInputProps('accountCode')} />
          <TextInput label="Account Name" withAsterisk {...form.getInputProps('accountName')} />
          <Select label="Account Type" withAsterisk disabled={isEdit} data={typeOptions} {...form.getInputProps('accountType')} />
          <Textarea label="Description" withAsterisk {...form.getInputProps('description')} />
          <Select label="Parent Account" data={parentOptions} searchable clearable {...form.getInputProps('parentAccountId')} />
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

export default AccountModal;
