import React, { useState } from 'react';
import { CompanyInfo, PaginationParams, SortingParams } from '../../types';
import { companyInfoService } from '../../services/companyInfoService';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData';
import AddCompanyModal from './components/AddCompanyModal';
import { Table, Button, Group, Text, TextInput, Badge, ActionIcon, Pagination, Center, Loader, Alert, Paper, Stack, Modal } from '@mantine/core';
import { IconPencil, IconStar, IconTrash, IconPlus, IconSearch, IconBuilding } from '@tabler/icons-react';
import { useDebouncedValue } from '@mantine/hooks';

const CompaniesPage: React.FC = () => {
  usePageTitle('Companies');

  interface CompanyFilteringParams { searchTerm: string; }

  const { data: companies, loading, error, pagination, setPagination, sorting, setSorting, filtering, setFiltering, totalCount, refetch } = usePagedData<CompanyInfo, PaginationParams, SortingParams, CompanyFilteringParams>({
    fetchData: companyInfoService.getCompanyInfos,
    initialPagination: { pageNumber: 1, pageSize: 10 },
    initialSorting: { orderBy: 'companyName', descending: false },
    initialFiltering: { searchTerm: '' },
  });

  const [showModal, setShowModal] = useState(false);
  const [selectedCompany, setSelectedCompany] = useState<CompanyInfo | undefined>();
  const [deleteLoading, setDeleteLoading] = useState<number | null>(null);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [companyToDelete, setCompanyToDelete] = useState<CompanyInfo | undefined>();
  const [searchValue, setSearchValue] = useState(filtering.searchTerm || '');
  const [debouncedSearch] = useDebouncedValue(searchValue, 300);

  React.useEffect(() => { setFiltering(f => ({ ...f, searchTerm: debouncedSearch })); }, [debouncedSearch]);

  const handleAdd = () => { setSelectedCompany(undefined); setShowModal(true); };
  const handleEdit = (c: CompanyInfo) => { setSelectedCompany(c); setShowModal(true); };

  const handleSaved = () => { setShowModal(false); refetch(); };

  const handleDelete = (c: CompanyInfo) => { setCompanyToDelete(c); setShowDeleteModal(true); };

  const handleConfirmDelete = async () => {
    if (!companyToDelete) return;
    try {
      setDeleteLoading(companyToDelete.id);
      await companyInfoService.deleteCompanyInfo(companyToDelete.id);
      setShowDeleteModal(false);
      setCompanyToDelete(undefined);
      refetch();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to delete company');
    } finally { setDeleteLoading(null); }
  };

  const handleSetDefault = async (companyId: number) => {
    try { await companyInfoService.setDefaultCompany(companyId); refetch(); }
    catch (err: any) { alert(err.response?.data?.message || 'Failed to set default company'); }
  };

  const totalPages = Math.ceil(totalCount / pagination.pageSize);

  if (loading) return <Center h={400}><Loader size="lg" /></Center>;
  if (error) return <Alert color="red" title="Error">{error} <Button variant="light" color="red" size="xs" ml="md" onClick={() => setPagination({ ...pagination })}>Retry</Button></Alert>;

  return (
    <Stack gap="md">
      <Text component="h1" size="xl" fw={700}>Companies</Text>

      <Group grow>
        <TextInput leftSection={<IconSearch size={16} />} placeholder="Search companies..." value={searchValue} onChange={e => setSearchValue(e.currentTarget.value)} />
        <div></div>
        <Button leftSection={<IconPlus size={18} />} onClick={handleAdd}>Add Company</Button>
      </Group>

      <Paper withBorder>
        <Table striped highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Company Name</Table.Th>
              <Table.Th>Legal Name</Table.Th>
              <Table.Th>Email</Table.Th>
              <Table.Th>Phone</Table.Th>
              <Table.Th>Currency</Table.Th>
              <Table.Th>Status</Table.Th>
              <Table.Th>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {companies.length === 0 ? (
              <Table.Tr><Table.Td colSpan={7}>
                <Center py="xl">
                  <Stack align="center" gap="sm">
                    <IconBuilding size={48} color="var(--mantine-color-gray-5)" />
                    <Text c="dimmed" size="lg">No Companies Found</Text>
                    <Text c="dimmed" size="sm">Get started by adding your first company for invoicing.</Text>
                    <Button leftSection={<IconPlus size={18} />} onClick={handleAdd}>Add Your First Company</Button>
                  </Stack>
                </Center>
              </Table.Td></Table.Tr>
            ) : (
              companies.map(company => (
                <Table.Tr key={company.id}>
                  <Table.Td fw={500}>
                    {company.companyName}
                    {company.isDefault && <Badge color="yellow" ml="xs" size="sm">Default</Badge>}
                  </Table.Td>
                  <Table.Td>{company.legalName || '-'}</Table.Td>
                  <Table.Td>{company.email || '-'}</Table.Td>
                  <Table.Td>{company.phone || '-'}</Table.Td>
                  <Table.Td>{company.currency}</Table.Td>
                  <Table.Td><Badge color="green" variant="light">Active</Badge></Table.Td>
                  <Table.Td>
                    <Group gap="xs" wrap="nowrap">
                      <ActionIcon variant="light" size="sm" onClick={() => handleEdit(company)}><IconPencil size={14} /></ActionIcon>
                      {!company.isDefault && <ActionIcon variant="light" color="yellow" size="sm" onClick={() => handleSetDefault(company.id)}><IconStar size={14} /></ActionIcon>}
                      <ActionIcon variant="light" color="red" size="sm" onClick={() => handleDelete(company)} loading={deleteLoading === company.id} disabled={company.isDefault}><IconTrash size={14} /></ActionIcon>
                    </Group>
                  </Table.Td>
                </Table.Tr>
              ))
            )}
          </Table.Tbody>
        </Table>
      </Paper>

      {totalCount > 0 && (
        <Group justify="space-between">
          <Text size="sm" c="dimmed">Showing {((pagination.pageNumber - 1) * pagination.pageSize) + 1} to {Math.min(pagination.pageNumber * pagination.pageSize, totalCount)} of {totalCount} companies</Text>
          <Pagination total={totalPages} value={pagination.pageNumber} onChange={page => setPagination({ ...pagination, pageNumber: page })} />
        </Group>
      )}

      <AddCompanyModal show={showModal} onHide={() => setShowModal(false)} onCompanySaved={handleSaved} company={selectedCompany} />

      <Modal opened={showDeleteModal} onClose={() => setShowDeleteModal(false)} title="Confirm Delete" size="sm">
        <Stack gap="md">
          <Text>Are you sure you want to delete <strong>{companyToDelete?.companyName}</strong>? This will soft delete the company. The data will be preserved but hidden from normal views.</Text>
          <Group justify="flex-end">
            <Button variant="default" onClick={() => setShowDeleteModal(false)}>Cancel</Button>
            <Button color="red" loading={deleteLoading !== null} onClick={handleConfirmDelete}>Delete Company</Button>
          </Group>
        </Stack>
      </Modal>
    </Stack>
  );
};

export default CompaniesPage;
