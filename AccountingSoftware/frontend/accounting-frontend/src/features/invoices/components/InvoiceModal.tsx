import React, { useEffect } from 'react';
import { Invoice, CreateInvoiceDto, CreateInvoiceItemDto, Customer, CompanyInfo } from '../../../types';
import { Modal, TextInput, Textarea, Select, NumberInput, Button, Group, Stack, Table, ActionIcon, Text, Divider, Paper } from '@mantine/core';
import { DateInput } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { IconPlus, IconTrash } from '@tabler/icons-react';

interface Props {
  opened: boolean;
  onClose: () => void;
  onSave: (invoice: CreateInvoiceDto) => Promise<void>;
  invoice?: Invoice;
  customers: Customer[];
  companyInfos: CompanyInfo[];
}

const InvoiceModal: React.FC<Props> = ({ opened, onClose, onSave, invoice, customers, companyInfos }) => {
  const customerList: Customer[] = Array.isArray(customers) ? customers : ((customers as any)?.items || []);
  const coList: CompanyInfo[] = Array.isArray(companyInfos) ? companyInfos : ((companyInfos as any)?.items || []);
  const defaultCompany = coList.find((c: CompanyInfo) => c.isDefault);
  const isEdit = !!invoice;
  const [loading, setLoading] = React.useState(false);

  const form = useForm({
    initialValues: {
      customerId: '',
      companyInfoId: defaultCompany?.id ? String(defaultCompany.id) : '',
      invoiceDate: new Date(),
      dueDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000),
      description: '',
      taxRate: 0,
      discountAmount: 0,
      notes: '',
      terms: 'Payment due within 30 days',
    },
    validate: {
      customerId: (v) => !v ? 'Customer is required' : null,
      invoiceDate: (v) => !v ? 'Invoice date is required' : null,
      dueDate: (v, values) => {
        if (!v) return 'Due date is required';
        if (values.invoiceDate && v < values.invoiceDate) return 'Due date must be on or after invoice date';
        return null;
      },
      taxRate: (v) => (v < 0 || v > 100) ? 'Rate must be between 0 and 100' : null,
      discountAmount: (v) => v < 0 ? 'Discount cannot be negative' : null,
    },
  });

  const [items, setItems] = React.useState<CreateInvoiceItemDto[]>([{ description: '', quantity: 1, unitPrice: 0, sortOrder: 0 }]);
  const [itemErrors, setItemErrors] = React.useState<Record<number, Record<string, string>>>({});

  useEffect(() => {
    if (invoice) {
      form.setValues({
        customerId: String(invoice.customerId),
        companyInfoId: invoice.companyInfoId ? String(invoice.companyInfoId) : '',
        invoiceDate: new Date(invoice.invoiceDate),
        dueDate: new Date(invoice.dueDate),
        description: invoice.description || '',
        taxRate: invoice.taxRate,
        discountAmount: invoice.discountAmount,
        notes: invoice.notes || '',
        terms: invoice.terms || '',
      });
      setItems(invoice.items.map((item, i) => ({ description: item.description, quantity: item.quantity, unitPrice: item.unitPrice, sortOrder: i })));
    } else {
      form.reset();
      setItems([{ description: '', quantity: 1, unitPrice: 0, sortOrder: 0 }]);
      setItemErrors({});
    }
  }, [invoice, opened]);

  const updateItem = (idx: number, field: string, value: any) => {
    setItems(prev => prev.map((item, i) => i === idx ? { ...item, [field]: value } : item));
    if (itemErrors[idx]) setItemErrors(prev => { const n = { ...prev }; delete n[idx]; return n; });
  };
  const addItem = () => setItems([...items, { description: '', quantity: 1, unitPrice: 0, sortOrder: items.length }]);
  const removeItem = (idx: number) => { if (items.length > 1) setItems(items.filter((_, i) => i !== idx)); };

  const subTotal = items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0);
  const taxAmount = subTotal * (form.values.taxRate / 100);
  const total = subTotal + taxAmount - form.values.discountAmount;

  const handleSubmit = async (values: typeof form.values) => {
    const validItems = items.filter(i => i.description.trim());
    // Line-item validation matching backend: CreateInvoiceItemDtoValidator
    if (validItems.length === 0) {
      setItemErrors({ 0: { description: 'At least one item is required' } });
      return;
    }
    const lineErrors: Record<number, Record<string, string>> = {};
    let hasLineErrors = false;
    validItems.forEach((item, i) => {
      const errs: Record<string, string> = {};
      if (item.quantity <= 0) { errs.quantity = 'Quantity must be greater than 0'; hasLineErrors = true; }
      if (item.unitPrice <= 0) { errs.unitPrice = 'Unit price must be greater than 0'; hasLineErrors = true; }
      if (item.description.length > 500) { errs.description = 'Max 500 characters'; hasLineErrors = true; }
      if (Object.keys(errs).length > 0) lineErrors[i] = errs;
    });
    setItemErrors(lineErrors);
    if (hasLineErrors) return;

    setLoading(true);
    try {
      await onSave({
        customerId: Number(values.customerId),
        companyInfoId: values.companyInfoId ? Number(values.companyInfoId) : undefined,
        invoiceDate: values.invoiceDate.toISOString(),
        dueDate: values.dueDate.toISOString(),
        description: values.description,
        taxRate: values.taxRate,
        discountAmount: values.discountAmount,
        notes: values.notes,
        terms: values.terms,
        items: validItems.map((item, i) => ({ ...item, sortOrder: i })),
      });
    } finally { setLoading(false); }
  };

  return (
    <Modal opened={opened} onClose={onClose} title={isEdit ? 'Invoice Details' : 'Create Invoice'} size="xl" centered>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack gap="sm">
          <Group grow>
            <Select label="Customer" withAsterisk data={customerList.map(c => ({ value: String(c.id), label: c.companyName }))} searchable {...form.getInputProps('customerId')} />
            <Select label="Company" data={coList.map(c => ({ value: String(c.id), label: c.companyName }))} searchable clearable {...form.getInputProps('companyInfoId')} />
          </Group>
          <Group grow>
            <DateInput label="Invoice Date" withAsterisk {...form.getInputProps('invoiceDate')} />
            <DateInput label="Due Date" withAsterisk {...form.getInputProps('dueDate')} />
          </Group>
          <TextInput label="Description" {...form.getInputProps('description')} />

          <Divider label="Line Items" labelPosition="center" />
          <Table>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Description</Table.Th>
                <Table.Th w={80}>Qty</Table.Th>
                <Table.Th w={120}>Unit Price</Table.Th>
                <Table.Th w={120}>Amount</Table.Th>
                <Table.Th w={40}></Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {items.map((item, idx) => (
                <Table.Tr key={idx}>
                  <Table.Td>
                    <TextInput size="sm" value={item.description} onChange={e => updateItem(idx, 'description', e.currentTarget.value)} placeholder="Item description" error={itemErrors[idx]?.description} />
                  </Table.Td>
                  <Table.Td><NumberInput size="sm" min={0} value={item.quantity} onChange={v => updateItem(idx, 'quantity', v || 0)} error={itemErrors[idx]?.quantity} /></Table.Td>
                  <Table.Td><NumberInput size="sm" min={0} decimalScale={2} value={item.unitPrice} onChange={v => updateItem(idx, 'unitPrice', v || 0)} error={itemErrors[idx]?.unitPrice} /></Table.Td>
                  <Table.Td><Text size="sm">{formatCurrency(item.quantity * item.unitPrice)}</Text></Table.Td>
                  <Table.Td>{items.length > 1 && <ActionIcon color="red" variant="subtle" onClick={() => removeItem(idx)}><IconTrash size="1rem" /></ActionIcon>}</Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
          <Button variant="light" size="xs" leftSection={<IconPlus size="1rem" />} onClick={addItem} style={{ alignSelf: 'flex-start' }}>Add Line</Button>

          <Group grow>
            <NumberInput label="Tax Rate (%)" min={0} max={100} {...form.getInputProps('taxRate')} />
            <NumberInput label="Discount ($)" min={0} decimalScale={2} {...form.getInputProps('discountAmount')} />
          </Group>
          <Textarea label="Notes" {...form.getInputProps('notes')} />
          <TextInput label="Terms" {...form.getInputProps('terms')} />

          <Paper p="sm" withBorder bg="gray.0">
            <Group justify="space-between">
              <Text size="sm">Subtotal:</Text><Text size="sm">{formatCurrency(subTotal)}</Text>
            </Group>
            <Group justify="space-between">
              <Text size="sm">Tax:</Text><Text size="sm">{formatCurrency(taxAmount)}</Text>
            </Group>
            <Group justify="space-between">
              <Text size="sm">Discount:</Text><Text size="sm">-{formatCurrency(form.values.discountAmount)}</Text>
            </Group>
            <Divider my="xs" />
            <Group justify="space-between">
              <Text fw={700}>Total:</Text><Text fw={700}>{formatCurrency(total)}</Text>
            </Group>
          </Paper>

          <Group justify="flex-end" mt="md">
            <Button variant="default" onClick={onClose}>Cancel</Button>
            <Button type="submit" loading={loading}>{isEdit ? 'Save' : 'Create'}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  );
};

const formatCurrency = (n: number) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n);

export default InvoiceModal;
