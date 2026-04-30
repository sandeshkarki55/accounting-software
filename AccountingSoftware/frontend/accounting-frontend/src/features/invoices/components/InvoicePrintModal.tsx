import React from 'react';
import { Invoice } from '../../../types';
import { Modal, Button, Group, Table, Text, Paper, Stack, Divider, Badge, Box } from '@mantine/core';
import { IconPrinter } from '@tabler/icons-react';

interface Props {
  opened: boolean;
  onClose: () => void;
  invoice: Invoice;
}

const formatCurrency = (n: number) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n);
const formatDate = (d: string) => new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });

const statusBadge = (statusName: string) => {
  const color = statusName === 'Paid' ? 'green' : statusName === 'Overdue' ? 'red' : statusName === 'Draft' ? 'gray' : 'yellow';
  return <Badge color={color} variant="light">{statusName}</Badge>;
};

const InvoicePrintModal: React.FC<Props> = ({ opened, onClose, invoice }) => {
  const handlePrint = () => window.print();

  const subtotal = invoice.items?.reduce((s, i) => s + i.amount, 0) ?? invoice.subTotal ?? 0;

  return (
    <Modal opened={opened} onClose={onClose} title={`Invoice #${invoice.invoiceNumber}`} size="xl" centered>
      <Stack gap="md" id="printable-invoice">
        {/* ─── Professional Invoice Layout ──────────────────────────── */}
        <Paper p={0} style={{ overflow: 'hidden' }}>

          {/* Header Strip */}
          <Box bg="navy.9" c="white" p="xl">
            <Group justify="space-between" wrap="nowrap">
              <Stack gap={0}>
                <Text size="28px" fw={800} lh={1.1}>INVOICE</Text>
                <Text size="sm" opacity={0.7}>#{invoice.invoiceNumber}</Text>
              </Stack>
              <Stack gap={0} align="flex-end">
                {statusBadge(invoice.statusName)}
                {invoice.paidDate && (
                  <Text size="xs" opacity={0.7} mt={4}>Paid on {formatDate(invoice.paidDate)}</Text>
                )}
              </Stack>
            </Group>
          </Box>

          <Box p="xl">
            {/* Company & Customer Info */}
            <Group justify="space-between" mb="xl" wrap="wrap">
              <Stack gap={2}>
                <Text size="xs" c="dimmed" tt="uppercase" fw={600}>Your Company</Text>
                <Text fw={600}>{invoice.companyName || 'Company Name'}</Text>
              </Stack>
              <Stack gap={4} align="flex-end">
                <Group gap="xl">
                  <Stack gap={0}>
                    <Text size="xs" c="dimmed">Invoice Date</Text>
                    <Text size="sm" fw={500}>{formatDate(invoice.invoiceDate)}</Text>
                  </Stack>
                  <Stack gap={0}>
                    <Text size="xs" c="dimmed">Due Date</Text>
                    <Text size="sm" fw={500}>{formatDate(invoice.dueDate)}</Text>
                  </Stack>
                </Group>
              </Stack>
            </Group>

            <Divider mb="md" />

            {/* Bill To */}
            <Stack gap={2} mb="xl">
              <Text size="xs" c="dimmed" tt="uppercase" fw={600}>Bill To</Text>
              <Text fw={600}>{invoice.customerName}</Text>
            </Stack>

            {/* Line Items Table */}
            <Table striped highlightOnHover mb="md">
              <Table.Thead bg="gray.0">
                <Table.Tr>
                  <Table.Th>#</Table.Th>
                  <Table.Th>Description</Table.Th>
                  <Table.Th ta="center">Qty</Table.Th>
                  <Table.Th ta="right">Unit Price</Table.Th>
                  <Table.Th ta="right">Amount</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {invoice.items?.map((item, i) => (
                  <Table.Tr key={i}>
                    <Table.Td c="dimmed" fz="xs">{i + 1}</Table.Td>
                    <Table.Td>{item.description}</Table.Td>
                    <Table.Td ta="center">{item.quantity}</Table.Td>
                    <Table.Td ta="right">{formatCurrency(item.unitPrice)}</Table.Td>
                    <Table.Td ta="right" fw={500}>{formatCurrency(item.amount)}</Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>

            {/* Totals Section */}
            <Divider mb="sm" />
            <Group justify="flex-end">
              <Stack gap={4} align="flex-end" w={260}>
                <Group justify="space-between" w="100%">
                  <Text size="sm" c="dimmed">Subtotal</Text>
                  <Text size="sm">{formatCurrency(subtotal)}</Text>
                </Group>
                {invoice.taxRate > 0 && (
                  <Group justify="space-between" w="100%">
                    <Text size="sm" c="dimmed">Tax ({invoice.taxRate}%)</Text>
                    <Text size="sm">{formatCurrency(invoice.taxAmount)}</Text>
                  </Group>
                )}
                {invoice.discountAmount > 0 && (
                  <Group justify="space-between" w="100%">
                    <Text size="sm" c="dimmed">Discount</Text>
                    <Text size="sm" c="red">-{formatCurrency(invoice.discountAmount)}</Text>
                  </Group>
                )}
                <Divider w="100%" />
                <Group justify="space-between" w="100%">
                  <Text fw={700} size="md">Total</Text>
                  <Text fw={700} size="md">{formatCurrency(invoice.totalAmount)}</Text>
                </Group>
                {invoice.paymentReference && (
                  <Group justify="space-between" w="100%">
                    <Text size="xs" c="dimmed">Payment Ref</Text>
                    <Text size="xs">{invoice.paymentReference}</Text>
                  </Group>
                )}
              </Stack>
            </Group>

            {/* Notes & Terms */}
            {(invoice.notes || invoice.terms) && (
              <>
                <Divider mt="xl" mb="md" />
                <Group wrap="wrap" gap="xl">
                  {invoice.notes && (
                    <Stack gap={4} style={{ flex: 1, minWidth: 200 }}>
                      <Text size="xs" c="dimmed" tt="uppercase" fw={600}>Notes</Text>
                      <Text size="sm">{invoice.notes}</Text>
                    </Stack>
                  )}
                  {invoice.terms && (
                    <Stack gap={4} style={{ flex: 1, minWidth: 200 }}>
                      <Text size="xs" c="dimmed" tt="uppercase" fw={600}>Terms</Text>
                      <Text size="sm">{invoice.terms}</Text>
                    </Stack>
                  )}
                </Group>
              </>
            )}

            {/* Footer */}
            <Divider mt="xl" mb="sm" />
            <Text size="xs" c="dimmed" ta="center">
              Thank you for your business. Payment is appreciated by the due date.
            </Text>
          </Box>
        </Paper>

        {/* Actions */}
        <Group justify="flex-end">
          <Button variant="default" onClick={onClose}>Close</Button>
          <Button leftSection={<IconPrinter size="1rem" />} onClick={handlePrint} color="navy">Print Invoice</Button>
        </Group>
      </Stack>
    </Modal>
  );
};

export default InvoicePrintModal;
