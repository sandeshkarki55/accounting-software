import React, { useEffect } from 'react';
import { JournalEntry, CreateJournalEntryDto, UpdateJournalEntryDto, CreateJournalEntryLineDto, Account } from '../../../types';
import { useFormState } from '../../../hooks/useFormState';
import { useValidation } from '../../../hooks/useValidation';
import { Modal, TextInput, Select, NumberInput, Button, Group, Stack, Table, Alert, ActionIcon, Text, Badge } from '@mantine/core';
import { IconTrash, IconPlus, IconAlertTriangle, IconCheck, IconPencil } from '@tabler/icons-react';

interface JournalEntryModalProps {
  show: boolean;
  onHide: () => void;
  onSave: (journalEntry: CreateJournalEntryDto | UpdateJournalEntryDto, isUpdate?: boolean, entryId?: number) => Promise<void>;
  journalEntry?: JournalEntry;
  accounts: Account[];
}

const formatCurrency = (amount: number) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);

const JournalEntryModal: React.FC<JournalEntryModalProps> = ({ show, onHide, onSave, journalEntry, accounts }) => {
  const accountList: Account[] = Array.isArray(accounts) ? accounts : (accounts && Array.isArray((accounts as any).items)) ? (accounts as any).items : [];
  const accountOptions = accountList.filter(a => a.isActive).map(a => ({ value: String(a.id), label: `${a.accountCode} - ${a.accountName}` }));

  const initialForm = { transactionDate: new Date().toISOString().split('T')[0], description: '', reference: '' };
  const { formData, setFormData, handleChange, resetForm } = useFormState(initialForm);
  const { errors, setErrors, validate, clearErrors } = useValidation<Record<string, string>>();
  const [lines, setLines] = React.useState<CreateJournalEntryLineDto[]>([
    { accountId: 0, debitAmount: 0, creditAmount: 0, description: '' },
    { accountId: 0, debitAmount: 0, creditAmount: 0, description: '' }
  ]);
  const [loading, setLoading] = React.useState(false);
  const isViewMode = !!journalEntry && journalEntry.isPosted;
  const isEditMode = !!journalEntry && !journalEntry.isPosted;

  useEffect(() => {
    if (journalEntry) {
      setFormData({ transactionDate: journalEntry.transactionDate.split('T')[0], description: journalEntry.description, reference: journalEntry.reference });
      setLines(journalEntry.lines.map(l => ({ accountId: l.accountId, debitAmount: l.debitAmount, creditAmount: l.creditAmount, description: l.description })));
    } else {
      resetForm();
      setLines([{ accountId: 0, debitAmount: 0, creditAmount: 0, description: '' }, { accountId: 0, debitAmount: 0, creditAmount: 0, description: '' }]);
    }
    clearErrors();
  }, [journalEntry, show]);

  const validateForm = () =>
    validate(() => {
      const newErrors: Record<string, string> = {};
      if (!formData.transactionDate) newErrors.transactionDate = 'Transaction date is required';
      if (!formData.description.trim()) newErrors.description = 'Description is required';
      const validLines = lines.filter(l => l.accountId !== 0);
      if (validLines.length < 2) newErrors.lines = 'At least two account lines are required';
      const totalDebits = validLines.reduce((s, l) => s + l.debitAmount, 0);
      const totalCredits = validLines.reduce((s, l) => s + l.creditAmount, 0);
      if (Math.abs(totalDebits - totalCredits) > 0.01) newErrors.balance = 'Total debits must equal total credits';
      validLines.forEach((line, i) => {
        if (line.debitAmount === 0 && line.creditAmount === 0) newErrors[`line_${i}_amount`] = 'Either debit or credit is required';
        if (line.debitAmount > 0 && line.creditAmount > 0) newErrors[`line_${i}_amount`] = 'Cannot have both debit and credit';
      });
      return newErrors;
    });

  const handleLineChange = (index: number, field: keyof CreateJournalEntryLineDto, value: string | number) => {
    const newLines = [...lines];
    newLines[index] = { ...newLines[index], [field]: value };
    if (field === 'debitAmount' && Number(value) > 0) newLines[index].creditAmount = 0;
    else if (field === 'creditAmount' && Number(value) > 0) newLines[index].debitAmount = 0;
    setLines(newLines);
  };

  const totalDebits = lines.filter(l => l.accountId !== 0).reduce((s, l) => s + l.debitAmount, 0);
  const totalCredits = lines.filter(l => l.accountId !== 0).reduce((s, l) => s + l.creditAmount, 0);
  const isBalanced = Math.abs(totalDebits - totalCredits) < 0.01;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;
    setLoading(true);
    try {
      const validLines = lines.filter(l => l.accountId !== 0);
      if (isEditMode && journalEntry) {
        await onSave({ ...formData, lines: validLines }, true, journalEntry.id);
      } else {
        await onSave({ ...formData, lines: validLines }, false);
      }
      onHide();
      resetForm();
      clearErrors();
    } catch (error) {
      console.error('Error saving journal entry:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    resetForm();
    clearErrors();
    setLines([{ accountId: 0, debitAmount: 0, creditAmount: 0, description: '' }, { accountId: 0, debitAmount: 0, creditAmount: 0, description: '' }]);
    onHide();
  };

  const title = isViewMode ? `Journal Entry ${journalEntry?.entryNumber}` : isEditMode ? `Edit Journal Entry ${journalEntry?.entryNumber}` : 'Create New Journal Entry';

  return (
    <Modal opened={show} onClose={handleClose} title={title} size="xl" closeOnClickOutside={false}>
      <form onSubmit={handleSubmit}>
        <Stack gap="md">
          {isViewMode && (
            <Alert color="blue" icon={<IconCheck size={16} />}>
              Entry: <strong>{journalEntry?.entryNumber}</strong>{' '}<Badge color="green">Posted</Badge>
              <Text size="xs" c="dimmed" mt={4}>Posted journal entries cannot be modified.</Text>
            </Alert>
          )}
          {isEditMode && (
            <Alert color="yellow" icon={<IconPencil size={16} />}>
              Editing: <strong>{journalEntry?.entryNumber}</strong>{' '}<Badge color="yellow">Draft</Badge>
              <Text size="xs" c="dimmed" mt={4}>Only unposted entries can be edited.</Text>
            </Alert>
          )}

          <Group grow>
            <TextInput type="date" label="Transaction Date *" name="transactionDate" value={formData.transactionDate} onChange={handleChange} disabled={isViewMode} error={errors.transactionDate} />
            <TextInput label="Reference" name="reference" value={formData.reference} onChange={handleChange} placeholder="Reference number" disabled={isViewMode} />
            <Alert color={isBalanced ? 'green' : 'yellow'} p="xs" title={null}>
              <Group gap="xs">{isBalanced ? <IconCheck size={16} /> : <IconAlertTriangle size={16} />}<Text size="sm">{isBalanced ? 'Balanced' : 'Out of Balance'}</Text></Group>
            </Alert>
          </Group>

          <TextInput label="Description *" name="description" value={formData.description} onChange={handleChange} placeholder="Brief description" disabled={isViewMode} error={errors.description} />

          <Stack gap="xs">
            <Group justify="space-between">
              <Text fw={600} size="sm">Journal Entry Lines</Text>
              {!isViewMode && (
                <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} onClick={() => setLines([...lines, { accountId: 0, debitAmount: 0, creditAmount: 0, description: '' }])}>Add Line</Button>
              )}
            </Group>
            {errors.lines && <Alert color="red">{errors.lines}</Alert>}
            {errors.balance && <Alert color="red">{errors.balance}</Alert>}
            <Table striped highlightOnHover withTableBorder>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Account</Table.Th>
                  <Table.Th>Description</Table.Th>
                  <Table.Th w={120}>Debit</Table.Th>
                  <Table.Th w={120}>Credit</Table.Th>
                  {!isViewMode && <Table.Th w={50}></Table.Th>}
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {lines.map((line, i) => (
                  <Table.Tr key={i}>
                    <Table.Td>
                      {isViewMode ? (
                        <Text size="sm">{accountList.find(a => a.id === line.accountId)?.accountCode} - {accountList.find(a => a.id === line.accountId)?.accountName}</Text>
                      ) : (
                        <Select data={accountOptions} value={String(line.accountId)} onChange={v => handleLineChange(i, 'accountId', Number(v))} placeholder="Select account..." size="sm" searchable clearable={false} />
                      )}
                    </Table.Td>
                    <Table.Td>
                      <TextInput size="sm" value={line.description} onChange={e => handleLineChange(i, 'description', e.target.value)} placeholder="Line description" disabled={isViewMode} />
                    </Table.Td>
                    <Table.Td>
                      <NumberInput size="sm" value={line.debitAmount || ''} onChange={v => handleLineChange(i, 'debitAmount', Number(v) || 0)} min={0} step={0.01} disabled={isViewMode} hideControls decimalScale={2} />
                      {errors[`line_${i}_amount`] && <Text size="xs" c="red">{errors[`line_${i}_amount`]}</Text>}
                    </Table.Td>
                    <Table.Td>
                      <NumberInput size="sm" value={line.creditAmount || ''} onChange={v => handleLineChange(i, 'creditAmount', Number(v) || 0)} min={0} step={0.01} disabled={isViewMode} hideControls decimalScale={2} />
                    </Table.Td>
                    {!isViewMode && (
                      <Table.Td>
                        <ActionIcon variant="light" color="red" onClick={() => { if (lines.length > 2) setLines(lines.filter((_, j) => j !== i)); }} disabled={lines.length <= 2}><IconTrash size={14} /></ActionIcon>
                      </Table.Td>
                    )}
                  </Table.Tr>
                ))}
              </Table.Tbody>
              <Table.Tfoot>
                <Table.Tr>
                  <Table.Td colSpan={2} fw={700}>Totals:</Table.Td>
                  <Table.Td fw={700}>{formatCurrency(totalDebits)}</Table.Td>
                  <Table.Td fw={700}>{formatCurrency(totalCredits)}</Table.Td>
                  {!isViewMode && <Table.Td></Table.Td>}
                </Table.Tr>
              </Table.Tfoot>
            </Table>
          </Stack>

          <Group justify="flex-end">
            <Button variant="default" onClick={handleClose} disabled={loading}>{isViewMode ? 'Close' : 'Cancel'}</Button>
            {!isViewMode && <Button type="submit" loading={loading} disabled={!isBalanced}>{isEditMode ? 'Update Entry' : 'Create Entry'}</Button>}
          </Group>
        </Stack>
      </form>
    </Modal>
  );
};

export default JournalEntryModal;
