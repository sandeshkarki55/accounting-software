import React from 'react';
import { Invoice } from '../../../types';
import { Modal, Button, Group, Table, Text, Paper, Stack, Divider } from '@mantine/core';
import { IconPrinter } from '@tabler/icons-react';

interface Props {
  opened: boolean;
  onClose: () => void;
  invoice: Invoice;
}

const formatCurrency = (n: number) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n);
const formatDate = (d: string) => new Date(d).toLocaleDateString();

const InvoicePrintModal: React.FC<Props> = ({ opened, onClose, invoice }) => {
  const handlePrint = () => window.print();

  return (
    <Modal opened={opened} onClose={onClose} title={`Invoice #${invoice.invoiceNumber}`} size="lg" centered>
      <Stack gap="md">
        <Paper p="md" withBorder>
          <Group justify="space-between" mb="md">
            <Stack gap={0}>
              <Text fw={700} size="lg">INVOICE</Text>
              <Text size="sm" c="dimmed">#{invoice.invoiceNumber}</Text>
            </Stack>
            <Stack gap={0} align="flex-end">
              <Text size="sm"><strong>Date:</strong> {formatDate(invoice.invoiceDate)}</Text>
              <Text size="sm"><strong>Due:</strong> {formatDate(invoice.dueDate)}</Text>
            </Stack>
          </Group>

          <Divider mb="md" />

          <Group justify="space-between" mb="md">
            <Stack gap={2}>
              <Text size="xs" c="dimmed">BILL TO:</Text>
              <Text size="sm" fw={500}>{invoice.customerName}</Text>
            </Stack>
            <Stack gap={2} align="flex-end">
              <Text size="xs" c="dimmed">FROM:</Text>
              <Text size="sm" fw={500}>{invoice.companyName || 'Company'}</Text>
            </Stack>
          </Group>

          <Table>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Description</Table.Th>
                <Table.Th>Qty</Table.Th>
                <Table.Th>Unit Price</Table.Th>
                <Table.Th>Amount</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {invoice.items?.map((item, i) => (
                <Table.Tr key={i}>
                  <Table.Td>{item.description}</Table.Td>
                  <Table.Td>{item.quantity}</Table.Td>
                  <Table.Td>{formatCurrency(item.unitPrice)}</Table.Td>
                  <Table.Td>{formatCurrency(item.quantity * item.unitPrice)}</Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>

          <Divider my="sm" />
          <Group justify="flex-end">
            <Stack gap={2} align="flex-end">
              <Text size="sm">Subtotal: {formatCurrency((invoice as any).subtotal || invoice.totalAmount)}</Text>
              {(invoice as any).taxAmount > 0 && <Text size="sm">Tax: {formatCurrency((invoice as any).taxAmount)}</Text>}
              {(invoice as any).discountAmount > 0 && <Text size="sm">Discount: -{formatCurrency((invoice as any).discountAmount)}</Text>}
              <Text fw={700} size="md">Total: {formatCurrency(invoice.totalAmount)}</Text>
            </Stack>
          </Group>

          {invoice.notes && <Text size="xs" c="dimmed" mt="sm">Notes: {invoice.notes}</Text>}
        </Paper>

        <Group justify="flex-end">
          <Button variant="default" onClick={onClose}>Close</Button>
          <Button leftSection={<IconPrinter size="1rem" />} onClick={handlePrint}>Print</Button>
        </Group>
      </Stack>
    </Modal>
  );
};

export default InvoicePrintModal;
