import React, { useState, useEffect } from 'react';
import { Invoice, Customer, CompanyInfo, CreateInvoiceDto, PaginationParams, SortingParams } from '../../types/index';
import { InvoiceFilteringParams } from '../../types/invoices';
import { invoiceService, customerService, companyInfoService } from '../../services/api';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData';
import InvoiceModal from './components/InvoiceModal';
import InvoicePrintModal from './components/InvoicePrintModal';
import MarkAsPaidModal from './components/MarkAsPaidModal';
import { Table, Pagination, TextInput, Group, Button, ActionIcon, Badge, Menu, Modal, Skeleton, Alert, Title, Paper, Stack, Text, Select } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { IconPlus, IconEye, IconTrash, IconSearch, IconDotsVertical, IconCash, IconPrinter } from '@tabler/icons-react';
import { showNotification } from '@mantine/notifications';

const statusColor = (s: string): string => {
  switch (s) {
    case 'Paid': return 'green';
    case 'Unpaid': case 'Sent': return 'yellow';
    case 'Overdue': return 'red';
    case 'Draft': return 'gray';
    case 'Cancelled': return 'gray';
    default: return 'gray';
  }
};

const formatCurrency = (n: number) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n);
const formatDate = (d: string) => new Date(d).toLocaleDateString();

const InvoicesPage: React.FC = () => {
  usePageTitle('Invoices');

  const { data: invoices, loading, error, pagination, setPagination, sorting, setSorting, filtering, setFiltering, totalCount, refetch } =
    usePagedData<Invoice, PaginationParams, SortingParams, InvoiceFilteringParams>({
      fetchData: invoiceService.getInvoices,
      initialPagination: { pageNumber: 1, pageSize: 10 },
      initialSorting: { orderBy: 'invoiceDate', descending: true },
      initialFiltering: { searchTerm: '', statusFilter: 'all' },
    });

  const [customers, setCustomers] = useState<Customer[]>([]);
  const [companyInfos, setCompanyInfos] = useState<CompanyInfo[]>([]);
  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | undefined>();
  const [printOpened, { open: openPrint, close: closePrint }] = useDisclosure(false);
  const [invoiceToPrint, setInvoiceToPrint] = useState<Invoice | undefined>();
  const [paidOpened, { open: openPaid, close: closePaid }] = useDisclosure(false);
  const [invoiceToPay, setInvoiceToPay] = useState<Invoice | undefined>();
  const [deleteOpened, { open: openDelete, close: closeDelete }] = useDisclosure(false);
  const [invoiceToDelete, setInvoiceToDelete] = useState<Invoice | undefined>();
  const [deleteLoading, setDeleteLoading] = useState(false);

  useEffect(() => {
    (async () => {
      try {
        const [cust, coPaged] = await Promise.all([
          customerService.getCustomers(),
          companyInfoService.getCompanyInfos({ pageNumber: 1, pageSize: 1000 }, { orderBy: 'companyName', descending: false }, { searchTerm: '' }),
        ]);
        setCustomers(Array.isArray(cust) ? cust : (cust as any)?.items || []);
        setCompanyInfos(Array.isArray(coPaged) ? coPaged as any : (coPaged as any)?.items || []);
      } catch { /* ignore */ }
    })();
  }, []);

  const handleAdd = () => { setSelectedInvoice(undefined); openModal(); };
  const handleEdit = (inv: Invoice) => { setSelectedInvoice(inv); openModal(); };
  const handlePrint = (inv: Invoice) => { setInvoiceToPrint(inv); openPrint(); };
  const handlePay = (inv: Invoice) => { setInvoiceToPay(inv); openPaid(); };
  const handleDelete = (inv: Invoice) => { setInvoiceToDelete(inv); openDelete(); };
  const handleSave = async (dto: CreateInvoiceDto) => { await invoiceService.createInvoice(dto); closeModal(); refetch(); showNotification({ title: 'Success', message: 'Invoice created', color: 'green' }); };

  const handleMarkPaid = async (dto: { paidDate: string; paymentReference: string }) => {
    if (!invoiceToPay) return;
    try { await invoiceService.markInvoiceAsPaid(invoiceToPay.id, dto); closePaid(); refetch(); showNotification({ title: 'Success', message: 'Invoice marked as paid', color: 'green' }); }
    catch { showNotification({ title: 'Error', message: 'Failed to mark as paid', color: 'red' }); }
  };

  const handleConfirmDelete = async () => {
    if (!invoiceToDelete) return;
    setDeleteLoading(true);
    try { await invoiceService.deleteInvoice(invoiceToDelete.id); closeDelete(); setInvoiceToDelete(undefined); refetch(); showNotification({ title: 'Success', message: 'Invoice deleted', color: 'green' }); }
    catch { showNotification({ title: 'Error', message: 'Failed to delete', color: 'red' }); }
    finally { setDeleteLoading(false); }
  };

  const getSortIndicator = (col: string) => sorting.orderBy === col ? (sorting.descending ? ' ↓' : ' ↑') : '';
  const handleSort = (col: string) => setSorting(prev => ({ orderBy: col, descending: prev.orderBy === col ? !prev.descending : false }));
  const totalPages = Math.ceil(totalCount / pagination.pageSize);

  const statusOptions = [
    { value: 'all', label: 'All Statuses' },
    { value: 'Unpaid', label: 'Unpaid' },
    { value: 'Paid', label: 'Paid' },
    { value: 'Overdue', label: 'Overdue' },
    { value: 'Draft', label: 'Draft' },
    { value: 'Cancelled', label: 'Cancelled' },
  ];

  return (
    <Stack gap="md">
      <Group justify="space-between">
        <Title order={3}>Invoices</Title>
        <Button leftSection={<IconPlus size="1rem" />} onClick={handleAdd}>Create Invoice</Button>
      </Group>

      <Paper p="sm" withBorder>
        <Group mb="md">
          <TextInput placeholder="Search invoices..." leftSection={<IconSearch size="1rem" />} value={filtering.searchTerm || ''} onChange={e => setFiltering(f => ({ ...f, searchTerm: e.currentTarget.value }))} style={{ width: 280 }} />
          <Select data={statusOptions} value={filtering.statusFilter || 'all'} onChange={v => setFiltering(f => ({ ...f, statusFilter: (v || 'all') as typeof filtering.statusFilter }))} w={160} />
        </Group>

        {loading ? <Stack gap="sm">{Array(5).fill(0).map((_, i) => <Skeleton key={i} height={40} />)}</Stack> :
         error ? <Alert color="red" variant="light">{error}</Alert> : (
          <>
            <Table striped highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th onClick={() => handleSort('invoiceNumber')} style={{ cursor: 'pointer' }}>Invoice #{getSortIndicator('invoiceNumber')}</Table.Th>
                  <Table.Th>Customer</Table.Th>
                  <Table.Th onClick={() => handleSort('invoiceDate')} style={{ cursor: 'pointer' }}>Date{getSortIndicator('invoiceDate')}</Table.Th>
                  <Table.Th onClick={() => handleSort('dueDate')} style={{ cursor: 'pointer' }}>Due{getSortIndicator('dueDate')}</Table.Th>
                  <Table.Th onClick={() => handleSort('totalAmount')} style={{ cursor: 'pointer' }}>Total{getSortIndicator('totalAmount')}</Table.Th>
                  <Table.Th>Status</Table.Th>
                  <Table.Th w={100}>Actions</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {invoices.map(inv => (
                  <Table.Tr key={inv.id}>
                    <Table.Td fw={500}>{inv.invoiceNumber}</Table.Td>
                    <Table.Td>{inv.customerName || '-'}</Table.Td>
                    <Table.Td>{formatDate(inv.invoiceDate)}</Table.Td>
                    <Table.Td>{formatDate(inv.dueDate)}</Table.Td>
                    <Table.Td>{formatCurrency(inv.totalAmount)}</Table.Td>
                    <Table.Td><Badge color={statusColor(inv.status as unknown as string)} variant="light">{inv.status}</Badge></Table.Td>
                    <Table.Td>
                      <Menu shadow="md" width={160}>
                        <Menu.Target><ActionIcon variant="subtle" color="gray"><IconDotsVertical size="1rem" /></ActionIcon></Menu.Target>
                        <Menu.Dropdown>
                          <Menu.Item leftSection={<IconEye size="1rem" />} onClick={() => handleEdit(inv)}>View/Edit</Menu.Item>
                          <Menu.Item leftSection={<IconPrinter size="1rem" />} onClick={() => handlePrint(inv)}>Print</Menu.Item>
                          {(inv.status as unknown as string) !== 'Paid' && (inv.status as unknown as string) !== 'Cancelled' && (
                            <Menu.Item leftSection={<IconCash size="1rem" />} color="green" onClick={() => handlePay(inv)}>Mark Paid</Menu.Item>
                          )}
                          <Menu.Item color="red" leftSection={<IconTrash size="1rem" />} onClick={() => handleDelete(inv)}>Delete</Menu.Item>
                        </Menu.Dropdown>
                      </Menu>
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
            {totalPages > 1 && <Group justify="center" mt="md"><Pagination total={totalPages} value={pagination.pageNumber} onChange={p => setPagination(prev => ({ ...prev, pageNumber: p }))} /></Group>}
          </>
        )}
      </Paper>

      <InvoiceModal opened={modalOpened} onClose={closeModal} onSave={handleSave} invoice={selectedInvoice} customers={customers} companyInfos={companyInfos} />
      {invoiceToPrint && <InvoicePrintModal opened={printOpened} onClose={closePrint} invoice={invoiceToPrint} />}
      {invoiceToPay && <MarkAsPaidModal opened={paidOpened} onClose={closePaid} onConfirm={handleMarkPaid} invoice={invoiceToPay} />}
      <Modal opened={deleteOpened} onClose={closeDelete} title="Delete Invoice" centered>
        <Text mb="md">Are you sure you want to delete invoice <strong>{invoiceToDelete?.invoiceNumber}</strong>?</Text>
        <Group justify="flex-end">
          <Button variant="default" onClick={closeDelete}>Cancel</Button>
          <Button color="red" loading={deleteLoading} onClick={handleConfirmDelete}>Delete</Button>
        </Group>
      </Modal>
    </Stack>
  );
};

export default InvoicesPage;
