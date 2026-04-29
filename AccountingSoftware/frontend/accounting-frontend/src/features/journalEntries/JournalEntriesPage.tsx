import React, { useState, useEffect } from 'react';
import { JournalEntry, JournalEntryLine, Account, PaginationParams, SortingParams, JournalEntryFilteringParams } from '../../types/index';
import { journalEntryService, accountService } from '../../services/api';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData';
import JournalEntryModal from './components/JournalEntryModal';
import PostJournalEntryModal from './components/PostJournalEntryModal';
import { Table, Button, Group, Text, TextInput, Select, Badge, ActionIcon, Pagination, Center, Loader, Alert, Paper, Stack, Modal } from '@mantine/core';
import { IconEye, IconPencil, IconCheck, IconTrash, IconPlus, IconSearch, IconChevronDown, IconChevronRight, IconNotebook } from '@tabler/icons-react';
import { useDebouncedValue } from '@mantine/hooks';

const formatCurrency = (amount: number) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);
const formatDate = (dateString: string) => new Date(dateString).toLocaleDateString();

const JournalEntriesPage: React.FC = () => {
  usePageTitle('Journal Entries');

  const { data: journalEntries, loading, error, pagination, setPagination, sorting, setSorting, filtering, setFiltering, totalCount, refetch } = usePagedData<JournalEntry, PaginationParams, SortingParams, JournalEntryFilteringParams>({
    fetchData: journalEntryService.getJournalEntries,
    initialPagination: { pageNumber: 1, pageSize: 10 },
    initialSorting: { orderBy: 'transactionDate', descending: true },
    initialFiltering: { searchTerm: '', statusFilter: 'all' },
  });

  const [accounts, setAccounts] = useState<Account[]>([]);
  const [expandedRows, setExpandedRows] = useState<Set<number>>(new Set());
  const [showModal, setShowModal] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<JournalEntry | undefined>();
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [entryToDelete, setEntryToDelete] = useState<JournalEntry | undefined>();
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [showPostModal, setShowPostModal] = useState(false);
  const [entryToPost, setEntryToPost] = useState<JournalEntry | undefined>();
  const [postLoading, setPostLoading] = useState(false);
  const [searchValue, setSearchValue] = useState(filtering.searchTerm || '');
  const [debouncedSearch] = useDebouncedValue(searchValue, 300);

  useEffect(() => { setFiltering(f => ({ ...f, searchTerm: debouncedSearch })); }, [debouncedSearch]);

  useEffect(() => {
    accountService.getAccounts().then(setAccounts).catch(console.error);
  }, []);

  const toggleRow = (id: number) => {
    const next = new Set(expandedRows);
    next.has(id) ? next.delete(id) : next.add(id);
    setExpandedRows(next);
  };

  const handleSave = async (data: any, isUpdate?: boolean, entryId?: number) => {
    if (isUpdate && entryId) await journalEntryService.updateJournalEntry(entryId, data);
    else await journalEntryService.createJournalEntry(data);
    refetch();
  };

  const handleDelete = async () => {
    if (!entryToDelete) return;
    setDeleteLoading(true);
    try { await journalEntryService.deleteJournalEntry(entryToDelete.id); refetch(); setShowDeleteModal(false); setEntryToDelete(undefined); }
    catch (e) { console.error(e); }
    finally { setDeleteLoading(false); }
  };

  const handlePost = async () => {
    if (!entryToPost) return;
    setPostLoading(true);
    try { await journalEntryService.postJournalEntry(entryToPost.id); refetch(); setShowPostModal(false); setEntryToPost(undefined); }
    catch (e) { console.error(e); }
    finally { setPostLoading(false); }
  };

  const totalPages = Math.ceil(totalCount / pagination.pageSize);

  if (loading) return <Center h={400}><Loader size="lg" /></Center>;
  if (error) return <Alert color="red" title="Error">{error} <Button variant="light" color="red" size="xs" ml="md" onClick={refetch}>Retry</Button></Alert>;

  return (
    <Stack gap="md">
      <Text component="h1" size="xl" fw={700}>Journal Entries</Text>

      <Group grow>
        <TextInput leftSection={<IconSearch size={16} />} placeholder="Search entries, descriptions, or references..." value={searchValue} onChange={e => setSearchValue(e.currentTarget.value)} />
        <Select data={[
          { value: 'all', label: 'All Entries' },
          { value: 'posted', label: 'Posted Only' },
          { value: 'unposted', label: 'Unposted Only' },
        ]} value={filtering.statusFilter} onChange={v => setFiltering({ ...filtering, statusFilter: v as 'all' | 'posted' | 'unposted' })} />
        <Button leftSection={<IconPlus size={18} />} onClick={() => { setSelectedEntry(undefined); setShowModal(true); }}>Create Entry</Button>
      </Group>

      <Paper withBorder>
        <Table striped highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th w={50}></Table.Th>
              <Table.Th>Entry #</Table.Th>
              <Table.Th>Date</Table.Th>
              <Table.Th>Description</Table.Th>
              <Table.Th>Reference</Table.Th>
              <Table.Th ta="right">Total</Table.Th>
              <Table.Th>Status</Table.Th>
              <Table.Th>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {journalEntries.length === 0 ? (
              <Table.Tr><Table.Td colSpan={8}>
                <Center py="xl">
                  <Stack align="center" gap="sm">
                    <IconNotebook size={48} color="var(--mantine-color-gray-5)" />
                    <Text c="dimmed" size="lg">
                      {filtering.searchTerm || filtering.statusFilter !== 'all' ? 'No entries match your criteria' : 'No journal entries found'}
                    </Text>
                    {!filtering.searchTerm && filtering.statusFilter === 'all' && (
                      <Button leftSection={<IconPlus size={18} />} onClick={() => { setSelectedEntry(undefined); setShowModal(true); }}>Create Entry</Button>
                    )}
                  </Stack>
                </Center>
              </Table.Td></Table.Tr>
            ) : (
              journalEntries.map(entry => (
                <React.Fragment key={entry.id}>
                  <Table.Tr>
                    <Table.Td>
                      <ActionIcon variant="subtle" size="sm" onClick={() => toggleRow(entry.id)}>
                        {expandedRows.has(entry.id) ? <IconChevronDown size={16} /> : <IconChevronRight size={16} />}
                      </ActionIcon>
                    </Table.Td>
                    <Table.Td fw={500}>{entry.entryNumber}</Table.Td>
                    <Table.Td>{formatDate(entry.transactionDate)}</Table.Td>
                    <Table.Td>{entry.description}</Table.Td>
                    <Table.Td>{entry.reference}</Table.Td>
                    <Table.Td ta="right" fw={500}>{formatCurrency(entry.totalAmount)}</Table.Td>
                    <Table.Td><Badge color={entry.isPosted ? 'green' : 'yellow'} variant="light">{entry.isPosted ? 'Posted' : 'Draft'}</Badge></Table.Td>
                    <Table.Td>
                      <Group gap="xs" wrap="nowrap">
                        <ActionIcon variant="light" size="sm" onClick={() => toggleRow(entry.id)}><IconEye size={14} /></ActionIcon>
                        {!entry.isPosted && (
                          <>
                            <ActionIcon variant="light" color="blue" size="sm" onClick={() => { setSelectedEntry(entry); setShowModal(true); }}><IconPencil size={14} /></ActionIcon>
                            <ActionIcon variant="light" color="green" size="sm" onClick={() => { setEntryToPost(entry); setShowPostModal(true); }}><IconCheck size={14} /></ActionIcon>
                            <ActionIcon variant="light" color="red" size="sm" onClick={() => { setEntryToDelete(entry); setShowDeleteModal(true); }}><IconTrash size={14} /></ActionIcon>
                          </>
                        )}
                      </Group>
                    </Table.Td>
                  </Table.Tr>
                  {expandedRows.has(entry.id) && (
                    <Table.Tr key={`expanded-${entry.id}`}>
                      <Table.Td colSpan={8} p={0}>
                        <Paper p="md" bg="gray.0" style={{ borderTop: '1px solid var(--mantine-color-gray-3)' }}>
                          <Text fw={600} size="sm" mb="sm">Journal Entry Lines</Text>
                          <Table>
                            <Table.Thead>
                              <Table.Tr>
                                <Table.Th>Account Code</Table.Th>
                                <Table.Th>Account Name</Table.Th>
                                <Table.Th>Description</Table.Th>
                                <Table.Th ta="right">Debit</Table.Th>
                                <Table.Th ta="right">Credit</Table.Th>
                              </Table.Tr>
                            </Table.Thead>
                            <Table.Tbody>
                              {entry.lines.map((line: JournalEntryLine) => (
                                <Table.Tr key={line.id}>
                                  <Table.Td fw={500}>{line.accountCode}</Table.Td>
                                  <Table.Td>{line.accountName}</Table.Td>
                                  <Table.Td>{line.description}</Table.Td>
                                  <Table.Td ta="right">{line.debitAmount > 0 ? formatCurrency(line.debitAmount) : '-'}</Table.Td>
                                  <Table.Td ta="right">{line.creditAmount > 0 ? formatCurrency(line.creditAmount) : '-'}</Table.Td>
                                </Table.Tr>
                              ))}
                            </Table.Tbody>
                            <Table.Tfoot>
                              <Table.Tr>
                                <Table.Td colSpan={3} fw={700}>Totals:</Table.Td>
                                <Table.Td ta="right" fw={700}>{formatCurrency(entry.lines.reduce((s, l) => s + l.debitAmount, 0))}</Table.Td>
                                <Table.Td ta="right" fw={700}>{formatCurrency(entry.lines.reduce((s, l) => s + l.creditAmount, 0))}</Table.Td>
                              </Table.Tr>
                            </Table.Tfoot>
                          </Table>
                        </Paper>
                      </Table.Td>
                    </Table.Tr>
                  )}
                </React.Fragment>
              ))
            )}
          </Table.Tbody>
        </Table>
      </Paper>

      {totalCount > 0 && (
        <Group justify="space-between">
          <Text size="sm" c="dimmed">Showing {((pagination.pageNumber - 1) * pagination.pageSize) + 1} to {Math.min(pagination.pageNumber * pagination.pageSize, totalCount)} of {totalCount} entries</Text>
          <Pagination total={totalPages} value={pagination.pageNumber} onChange={page => setPagination({ ...pagination, pageNumber: page })} />
        </Group>
      )}

      <JournalEntryModal show={showModal} onHide={() => setShowModal(false)} onSave={handleSave} journalEntry={selectedEntry} accounts={accounts} />
      <PostJournalEntryModal show={showPostModal} onHide={() => setShowPostModal(false)} onConfirm={handlePost} journalEntry={entryToPost || null} loading={postLoading} />

      <Modal opened={showDeleteModal} onClose={() => setShowDeleteModal(false)} title="Confirm Delete" size="sm">
        <Stack gap="md">
          <Text>Are you sure you want to delete journal entry <strong>{entryToDelete?.entryNumber}</strong>? This will permanently delete the journal entry and all its lines.</Text>
          <Group justify="flex-end">
            <Button variant="default" onClick={() => setShowDeleteModal(false)}>Cancel</Button>
            <Button color="red" loading={deleteLoading} onClick={handleDelete}>Delete Entry</Button>
          </Group>
        </Stack>
      </Modal>
    </Stack>
  );
};

export default JournalEntriesPage;
