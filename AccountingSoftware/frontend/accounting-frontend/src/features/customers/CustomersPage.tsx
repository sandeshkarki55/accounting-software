import React, { useState } from 'react';
import { Customer, CreateCustomerDto, UpdateCustomerDto, PaginationParams, SortingParams, CustomerFilteringParams } from '../../types';
import { customerService } from '../../services/customerService';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData';
import CustomerModal from './components/CustomerModal';
import { Table, Pagination, TextInput, Group, Button, ActionIcon, Badge, Menu, Modal, Skeleton, Alert, Title, Paper, Stack, Text } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { IconPlus, IconEdit, IconTrash, IconSearch, IconDotsVertical } from '@tabler/icons-react';
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
    } catch { showNotification({ title: 'Error', message: 'Failed to delete', color: 'red' }); }
    finally { setDeleteLoading(false); }
  };

  const handleSort = (column: string) => {
    setSorting(prev => ({ orderBy: column, descending: prev.orderBy === column ? !prev.descending : false }));
  };

  const getSortIndicator = (column: string) => {
    if (sorting.orderBy !== column) return '';
    return sorting.descending ? ' ↓' : ' ↑';
  };

  const totalPages = Math.ceil(totalCount / pagination.pageSize);

  return (
    <Stack gap="md">
      <Group justify="space-between">
        <Title order={3}>Customers</Title>
        <Button leftSection={<IconPlus size="1rem" />} onClick={handleAdd}>Add Customer</Button>
      </Group>

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

        {loading ? <Stack gap="sm">{Array(5).fill(0).map((_, i) => <Skeleton key={i} height={40} />)}</Stack> :
          error ? <Alert color="red" variant="light">{error}</Alert> : (
            <>
              <Table striped highlightOnHover>
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th onClick={() => handleSort('companyName')} style={{ cursor: 'pointer' }}>Company Name{getSortIndicator('companyName')}</Table.Th>
                    <Table.Th onClick={() => handleSort('contactPersonName')} style={{ cursor: 'pointer' }}>Contact{getSortIndicator('contactPersonName')}</Table.Th>
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
                      <Table.Td><Badge color={c.isActive ? 'green' : 'gray'} variant="light">{c.isActive ? 'Active' : 'Inactive'}</Badge></Table.Td>
                      <Table.Td>
                        <Menu shadow="md" width={120}>
                          <Menu.Target>
                            <ActionIcon variant="subtle" color="gray"><IconDotsVertical size="1rem" /></ActionIcon>
                          </Menu.Target>
                          <Menu.Dropdown>
                            <Menu.Item leftSection={<IconEdit size="1rem" />} onClick={() => handleEdit(c)}>Edit</Menu.Item>
                            <Menu.Item color="red" leftSection={<IconTrash size="1rem" />} onClick={() => handleDelete(c)}>Delete</Menu.Item>
                          </Menu.Dropdown>
                        </Menu>
                      </Table.Td>
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
              {totalPages > 1 && (
                <Group justify="center" mt="md">
                  <Pagination total={totalPages} value={pagination.pageNumber} onChange={p => setPagination(prev => ({ ...prev, pageNumber: p }))} />
                </Group>
              )}
            </>
          )}
      </Paper>

      <CustomerModal opened={modalOpened} onClose={closeModal} onSave={handleSave} customer={selectedCustomer} />

      <Modal opened={deleteOpened} onClose={closeDelete} title="Delete Customer" centered>
        <Text mb="md">Are you sure you want to delete <strong>{customerToDelete?.companyName}</strong>?</Text>
        <Group justify="flex-end">
          <Button variant="default" onClick={closeDelete}>Cancel</Button>
          <Button color="red" loading={deleteLoading} onClick={handleConfirmDelete}>Delete</Button>
        </Group>
      </Modal>
    </Stack>
  );
};

export default CustomersPage;
