import React, { useState } from 'react';
import { Customer, CreateCustomerDto, UpdateCustomerDto, PaginationParams, SortingParams, CustomerFilteringParams } from '../../types';
import { customerService } from '../../services/customerService';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData';
import CustomerModal from './components/CustomerModal';
import { PageHeader, DeleteConfirmModal, ActionMenu, SortableTh, TableSkeleton, PaginationRow } from '../../components/common';
import { activeStatusColor } from '../../utils';
import { Table, TextInput, Group, Badge, Alert, Paper, Stack } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { IconSearch } from '@tabler/icons-react';
import { showNotification } from '@mantine/notifications';

const CustomersPage: React.FC = () => {
  usePageTitle('Customers');

  const { data: customers, loading, error, pagination, setPagination, sorting, setSorting, filtering, setFiltering, totalCount, refetch } =
    usePagedData<Customer, PaginationParams, SortingParams, CustomerFilteringParams>({
      fetchData: customerService.getCustomersPaged,
      initialPagination: { pageNumber: 1, pageSize: 10 },
      initialSorting: { orderBy: 'companyName', descending: false },
      initialFiltering: { searchTerm: '', isActive: undefined },
    });

  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | undefined>();
  const [deleteOpened, { open: openDelete, close: closeDelete }] = useDisclosure(false);
  const [customerToDelete, setCustomerToDelete] = useState<Customer | undefined>();
  const [deleteLoading, setDeleteLoading] = useState(false);

  const handleAdd = () => { setSelectedCustomer(undefined); openModal(); };
  const handleEdit = (c: Customer) => { setSelectedCustomer(c); openModal(); };
  const handleDelete = (c: Customer) => { setCustomerToDelete(c); openDelete(); };

  const handleSave = async (dto: CreateCustomerDto | UpdateCustomerDto) => {
    try {
      if (selectedCustomer) await customerService.updateCustomer(selectedCustomer.id, dto as UpdateCustomerDto);
      else await customerService.createCustomer(dto as CreateCustomerDto);
      closeModal();
      refetch();
      showNotification({ title: 'Success', message: `Customer ${selectedCustomer ? 'updated' : 'created'} successfully`, color: 'green' });
    } catch (err) {
      showNotification({ title: 'Error', message: 'Failed to save customer', color: 'red' });
      throw err;
    }
  };

  const handleConfirmDelete = async () => {
    if (!customerToDelete) return;
    setDeleteLoading(true);
    try {
      await customerService.deleteCustomer(customerToDelete.id);
      closeDelete();
      setCustomerToDelete(undefined);
      refetch();
      showNotification({ title: 'Success', message: 'Customer deleted', color: 'green' });
    } catch {
      showNotification({ title: 'Error', message: 'Failed to delete', color: 'red' });
    } finally {
      setDeleteLoading(false);
    }
  };

  const handleSort = (column: string) =>
    setSorting(prev => ({ orderBy: column, descending: prev.orderBy === column ? !prev.descending : false }));

  const totalPages = Math.ceil(totalCount / pagination.pageSize);

  return (
    <Stack gap="md">
      <PageHeader title="Customers" actionLabel="Add Customer" onAction={handleAdd} />

      <Paper p="sm" withBorder>
        <Group justify="space-between" mb="md">
          <TextInput
            placeholder="Search customers..."
            leftSection={<IconSearch size="1rem" />}
            value={filtering.searchTerm || ''}
            onChange={e => setFiltering(prev => ({ ...prev, searchTerm: e.currentTarget.value }))}
            style={{ width: 300 }}
          />
        </Group>

        {loading ? <TableSkeleton /> :
          error ? <Alert color="red" variant="light">{error}</Alert> : (
            <>
              <Table striped highlightOnHover>
                <Table.Thead>
                  <Table.Tr>
                    <SortableTh column="companyName" currentSortBy={sorting.orderBy} descending={sorting.descending} onSort={handleSort}>Company Name</SortableTh>
                    <SortableTh column="contactPersonName" currentSortBy={sorting.orderBy} descending={sorting.descending} onSort={handleSort}>Contact</SortableTh>
                    <Table.Th>Email</Table.Th>
                    <Table.Th>Phone</Table.Th>
                    <Table.Th>City</Table.Th>
                    <Table.Th>Status</Table.Th>
                    <Table.Th w={60}>Actions</Table.Th>
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {customers.map(c => (
                    <Table.Tr key={c.id}>
                      <Table.Td fw={500}>{c.companyName}</Table.Td>
                      <Table.Td>{c.contactPersonName || '-'}</Table.Td>
                      <Table.Td>{c.email || '-'}</Table.Td>
                      <Table.Td>{c.phone || '-'}</Table.Td>
                      <Table.Td>{c.city || '-'}</Table.Td>
                      <Table.Td><Badge color={activeStatusColor(c.isActive)} variant="light">{c.isActive ? 'Active' : 'Inactive'}</Badge></Table.Td>
                      <Table.Td>
                        <ActionMenu onEdit={() => handleEdit(c)} onDelete={() => handleDelete(c)} />
                      </Table.Td>
                    </Table.Tr>
                  ))}
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

      <CustomerModal opened={modalOpened} onClose={closeModal} onSave={handleSave} customer={selectedCustomer} />

      <DeleteConfirmModal
        opened={deleteOpened}
        onClose={closeDelete}
        onConfirm={handleConfirmDelete}
        loading={deleteLoading}
        title="Delete Customer"
        message={<>Are you sure you want to delete <strong>{customerToDelete?.companyName}</strong>?</>}
      />
    </Stack>
  );
};

export default CustomersPage;
