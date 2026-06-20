import React, { useState, useEffect } from 'react';
import { Invoice, Customer, CompanyInfo, CreateInvoiceDto, PaginationParams, SortingParams } from '../../types/index';
import { InvoiceFilteringParams } from '../../types/invoices';
import { invoiceService, customerService, companyInfoService } from '../../services/api';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData';
import InvoiceModal from './components/InvoiceModal';
import InvoicePrintModal from './components/InvoicePrintModal';
import MarkAsPaidModal from './components/MarkAsPaidModal';
import { PageHeader, DeleteConfirmModal, ActionMenu, SortableTh, TableSkeleton, PaginationRow } from '../../components/common';
import { formatCurrency, formatDate, invoiceStatusColor } from '../../utils';
import { Table, TextInput, Group, Badge, Alert, Paper, Stack, Select } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { IconSearch, IconEye, IconPrinter, IconCash, IconTrash } from '@tabler/icons-react';
import { showNotification } from '@mantine/notifications';

const statusOptions = [
  { value: 'all', label: 'All Statuses' },
  { value: 'Unpaid', label: 'Unpaid' },
  { value: 'Paid', label: 'Paid' },
  { value: 'Overdue', label: 'Overdue' },
  { value: 'Draft', label: 'Draft' },
  { value: 'Cancelled', label: 'Cancelled' },
];

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
        const [custPaged, coPaged] = await Promise.all([
          customerService.getCustomersPaged({ pageNumber: 1, pageSize: 50 }, { orderBy: 'companyName', descending: false }, { searchTerm: '' }),
          companyInfoService.getCompanyInfos({ pageNumber: 1, pageSize: 50 }, { orderBy: 'companyName', descending: false }, { searchTerm: '' }),
        ]);
        setCustomers(custPaged.items || []);
        setCompanyInfos(coPaged.items || []);
      } catch { /* ignore */ }
    })();
  }, []);

  const handleAdd = () => { setSelectedInvoice(undefined); openModal(); };
  const handleEdit = (inv: Invoice) => { setSelectedInvoice(inv); openModal(); };
  const handlePrint = (inv: Invoice) => { setInvoiceToPrint(inv); openPrint(); };
  const handlePay = (inv: Invoice) => { setInvoiceToPay(inv); openPaid(); };
  const handleDelete = (inv: Invoice) => { setInvoiceToDelete(inv); openDelete(); };

  const handleSave = async (dto: CreateInvoiceDto) => {
    await invoiceService.createInvoice(dto);
    closeModal();
    refetch();
    showNotification({ title: 'Success', message: 'Invoice created', color: 'green' });
  };

  const handleMarkPaid = async (dto: { paidDate: string; paymentReference: string }) => {
    if (!invoiceToPay) return;
    try {
      await invoiceService.markInvoiceAsPaid(invoiceToPay.id, dto);
      closePaid();
      refetch();
      showNotification({ title: 'Success', message: 'Invoice marked as paid', color: 'green' });
    } catch {
      showNotification({ title: 'Error', message: 'Failed to mark as paid', color: 'red' });
    }
  };

  const handleConfirmDelete = async () => {
    if (!invoiceToDelete) return;
    setDeleteLoading(true);
    try {
      await invoiceService.deleteInvoice(invoiceToDelete.id);
      closeDelete();
      setInvoiceToDelete(undefined);
      refetch();
      showNotification({ title: 'Success', message: 'Invoice deleted', color: 'green' });
    } catch {
      showNotification({ title: 'Error', message: 'Failed to delete', color: 'red' });
    } finally {
      setDeleteLoading(false);
    }
  };

  const handleSort = (col: string) =>
    setSorting(prev => ({ orderBy: col, descending: prev.orderBy === col ? !prev.descending : false }));

  const totalPages = Math.ceil(totalCount / pagination.pageSize);

  return (
    <Stack gap="md">
      <PageHeader title="Invoices" actionLabel="Create Invoice" onAction={handleAdd} />

      <Paper p="sm" withBorder>
        <Group mb="md">
          <TextInput
            placeholder="Search invoices..."
            leftSection={<IconSearch size="1rem" />}
            value={filtering.searchTerm || ''}
            onChange={e => setFiltering(f => ({ ...f, searchTerm: e.currentTarget.value }))}
            style={{ width: 280 }}
          />
          <Select
            data={statusOptions}
            value={filtering.statusFilter || 'all'}
            onChange={v => setFiltering(f => ({ ...f, statusFilter: (v || 'all') as typeof filtering.statusFilter }))}
            w={160}
          />
        </Group>

        {loading ? <TableSkeleton /> :
         error ? <Alert color="red" variant="light">{error}</Alert> : (
          <>
            <Table striped highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <SortableTh column="invoiceNumber" currentSortBy={sorting.orderBy} descending={sorting.descending} onSort={handleSort}>Invoice #</SortableTh>
                  <Table.Th>Customer</Table.Th>
                  <SortableTh column="invoiceDate" currentSortBy={sorting.orderBy} descending={sorting.descending} onSort={handleSort}>Date</SortableTh>
                  <SortableTh column="dueDate" currentSortBy={sorting.orderBy} descending={sorting.descending} onSort={handleSort}>Due</SortableTh>
                  <SortableTh column="totalAmount" currentSortBy={sorting.orderBy} descending={sorting.descending} onSort={handleSort}>Total</SortableTh>
                  <Table.Th>Status</Table.Th>
                  <Table.Th w={100}>Actions</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {invoices.map(inv => {
                  const status = inv.status as unknown as string;
                  return (
                    <Table.Tr key={inv.id}>
                      <Table.Td fw={500}>{inv.invoiceNumber}</Table.Td>
                      <Table.Td>{inv.customerName || '-'}</Table.Td>
                      <Table.Td>{formatDate(inv.invoiceDate)}</Table.Td>
                      <Table.Td>{formatDate(inv.dueDate)}</Table.Td>
                      <Table.Td>{formatCurrency(inv.totalAmount)}</Table.Td>
                      <Table.Td><Badge color={invoiceStatusColor(status)} variant="light">{inv.status}</Badge></Table.Td>
                      <Table.Td>
                        <ActionMenu
                          menuWidth={160}
                          items={[
                            { label: 'View/Edit', icon: <IconEye size="1rem" />, onClick: () => handleEdit(inv) },
                            { label: 'Print', icon: <IconPrinter size="1rem" />, onClick: () => handlePrint(inv) },
                            ...(status !== 'Paid' && status !== 'Cancelled'
                              ? [{ label: 'Mark Paid', icon: <IconCash size="1rem" />, color: 'green', onClick: () => handlePay(inv) }]
                              : []),
                            { label: 'Delete', icon: <IconTrash size="1rem" />, color: 'red', onClick: () => handleDelete(inv) },
                          ]}
                        />
                      </Table.Td>
                    </Table.Tr>
                  );
                })}
              </Table.Tbody>
            </Table>
            <PaginationRow
              page={pagination.pageNumber}
              totalPages={totalPages}
              totalCount={totalCount}
              pageSize={pagination.pageSize}
              onPageChange={p => setPagination(prev => ({ ...prev, pageNumber: p }))}
            />
          </>
        )}
      </Paper>

      <InvoiceModal opened={modalOpened} onClose={closeModal} onSave={handleSave} invoice={selectedInvoice} customers={customers} companyInfos={companyInfos} />
      {invoiceToPrint && <InvoicePrintModal opened={printOpened} onClose={closePrint} invoice={invoiceToPrint} />}
      {invoiceToPay && <MarkAsPaidModal opened={paidOpened} onClose={closePaid} onConfirm={handleMarkPaid} invoice={invoiceToPay} />}

      <DeleteConfirmModal
        opened={deleteOpened}
        onClose={closeDelete}
        onConfirm={handleConfirmDelete}
        loading={deleteLoading}
        title="Delete Invoice"
        message={<>Are you sure you want to delete invoice <strong>{invoiceToDelete?.invoiceNumber}</strong>?</>}
      />
    </Stack>
  );
};

export default InvoicesPage;
