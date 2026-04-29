import React, { useState, useEffect } from 'react';
import { Account, AccountType, CreateAccountDto, UpdateAccountDto, PaginationParams, SortingParams, AccountFilteringParams } from '../../types';
import { accountService } from '../../services/accountService';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData';
import AccountModal from './components/AccountModal';
import ChartOfAccountsTree from './components/ChartOfAccountsTree';
import { Table, Pagination, TextInput, Group, Button, ActionIcon, Badge, Menu, Modal, Skeleton, Alert, Title, Paper, Stack, Text, Tabs, Select, Center, Loader } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { IconPlus, IconEdit, IconTrash, IconSearch, IconDotsVertical, IconListTree, IconList } from '@tabler/icons-react';
import { showNotification } from '@mantine/notifications';

const typeName = (t: AccountType) => ({ [AccountType.Asset]: 'Asset', [AccountType.Liability]: 'Liability', [AccountType.Equity]: 'Equity', [AccountType.Revenue]: 'Revenue', [AccountType.Expense]: 'Expense' }[t] || 'Unknown');
const typeColor = (t: AccountType): string => ({ [AccountType.Asset]: 'blue', [AccountType.Liability]: 'red', [AccountType.Equity]: 'grape', [AccountType.Revenue]: 'green', [AccountType.Expense]: 'orange' }[t] || 'gray');

const formatCurrency = (n: number) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n);

const AccountsPage: React.FC = () => {
  usePageTitle('Chart of Accounts');

  const { data: accounts, loading: loadingList, error: errorList, pagination, setPagination, sorting, setSorting, filtering, setFiltering, totalCount, refetch } =
    usePagedData<Account, PaginationParams, SortingParams, AccountFilteringParams>({
      fetchData: accountService.getAccountsPaged,
      initialPagination: { pageNumber: 1, pageSize: 10 },
      initialSorting: { orderBy: 'accountCode', descending: false },
      initialFiltering: { searchTerm: '', accountType: undefined, isActive: undefined },
    });

  const [hierarchyAccounts, setHierarchyAccounts] = useState<Account[]>([]);
  const [hLoading, setHLoading] = useState(true);
  const [hError, setHError] = useState<string | null>(null);
  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure(false);
  const [selectedAccount, setSelectedAccount] = useState<Account | undefined>();
  const [deleteOpened, { open: openDelete, close: closeDelete }] = useDisclosure(false);
  const [accountToDelete, setAccountToDelete] = useState<Account | undefined>();
  const [deleteLoading, setDeleteLoading] = useState(false);

  useEffect(() => {
    (async () => {
      try { setHLoading(true); setHError(null); const d = await accountService.getAccountsHierarchy(); setHierarchyAccounts(d); }
      catch { setHError('Failed to load accounts'); }
      finally { setHLoading(false); }
    })();
  }, []);

  const handleAdd = () => { setSelectedAccount(undefined); openModal(); };
  const handleEdit = (a: Account) => { setSelectedAccount(a); openModal(); };
  const handleDelete = (a: Account) => { setAccountToDelete(a); openDelete(); };

  const handleSave = async (dto: CreateAccountDto | UpdateAccountDto) => {
    try {
      if (selectedAccount) await accountService.updateAccount(selectedAccount.id, dto as UpdateAccountDto);
      else await accountService.createAccount(dto as CreateAccountDto);
      closeModal();
      refetch();
      showNotification({ title: 'Success', message: `Account ${selectedAccount ? 'updated' : 'created'}`, color: 'green' });
    } catch (err) {
      showNotification({ title: 'Error', message: 'Failed to save account', color: 'red' });
      throw err;
    }
  };

  const handleConfirmDelete = async () => {
    if (!accountToDelete) return;
    setDeleteLoading(true);
    try { await accountService.deleteAccount(accountToDelete.id); closeDelete(); setAccountToDelete(undefined); refetch(); showNotification({ title: 'Success', message: 'Account deleted', color: 'green' }); }
    catch { showNotification({ title: 'Error', message: 'Failed to delete', color: 'red' }); }
    finally { setDeleteLoading(false); }
  };

  const getSortIndicator = (col: string) => sorting.orderBy === col ? (sorting.descending ? ' ↓' : ' ↑') : '';
  const handleSort = (col: string) => setSorting(prev => ({ orderBy: col, descending: prev.orderBy === col ? !prev.descending : false }));
  const totalPages = Math.ceil(totalCount / pagination.pageSize);

  return (
    <Stack gap="md">
      <Group justify="space-between">
        <Title order={3}>Chart of Accounts</Title>
        <Button leftSection={<IconPlus size="1rem" />} onClick={handleAdd}>Add Account</Button>
      </Group>

      <Tabs defaultValue="tree">
        <Tabs.List>
          <Tabs.Tab value="tree" leftSection={<IconListTree size="1rem" />}>Tree View</Tabs.Tab>
          <Tabs.Tab value="list" leftSection={<IconList size="1rem" />}>List View</Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="tree" pt="md">
          {hLoading ? <Center h={200}><Loader color="navy" /></Center> :
           hError ? <Alert color="red" variant="light">{hError}</Alert> :
           <ChartOfAccountsTree accounts={hierarchyAccounts} onEditAccount={handleEdit} onDeleteAccount={handleDelete} />}
        </Tabs.Panel>

        <Tabs.Panel value="list" pt="md">
          <Paper p="sm" withBorder>
            <Group mb="md">
              <TextInput placeholder="Search accounts..." leftSection={<IconSearch size="1rem" />} value={filtering.searchTerm || ''} onChange={e => setFiltering(f => ({ ...f, searchTerm: e.currentTarget.value }))} style={{ width: 280 }} />
              <Select placeholder="All Types" clearable data={[AccountType.Asset, AccountType.Liability, AccountType.Equity, AccountType.Revenue, AccountType.Expense].map(t => ({ value: String(t), label: typeName(t) }))} value={filtering.accountType !== undefined ? String(filtering.accountType) : null} onChange={v => setFiltering(f => ({ ...f, accountType: v ? Number(v) : undefined }))} w={160} />
            </Group>

            {loadingList ? <Stack gap="sm">{Array(5).fill(0).map((_, i) => <Skeleton key={i} height={36} />)}</Stack> :
             errorList ? <Alert color="red" variant="light">{errorList}</Alert> : (
              <>
                <Table striped highlightOnHover>
                  <Table.Thead>
                    <Table.Tr>
                      <Table.Th onClick={() => handleSort('accountCode')} style={{ cursor: 'pointer' }}>Code{getSortIndicator('accountCode')}</Table.Th>
                      <Table.Th onClick={() => handleSort('accountName')} style={{ cursor: 'pointer' }}>Name{getSortIndicator('accountName')}</Table.Th>
                      <Table.Th onClick={() => handleSort('accountType')} style={{ cursor: 'pointer' }}>Type{getSortIndicator('accountType')}</Table.Th>
                      <Table.Th onClick={() => handleSort('balance')} style={{ cursor: 'pointer' }}>Balance{getSortIndicator('balance')}</Table.Th>
                      <Table.Th>Status</Table.Th>
                      <Table.Th w={60}>Actions</Table.Th>
                    </Table.Tr>
                  </Table.Thead>
                  <Table.Tbody>
                    {accounts.map(a => (
                      <Table.Tr key={a.id}>
                        <Table.Td fw={500}>{a.accountCode}</Table.Td>
                        <Table.Td>{a.accountName}</Table.Td>
                        <Table.Td><Badge color={typeColor(a.accountType)} variant="light">{typeName(a.accountType)}</Badge></Table.Td>
                        <Table.Td>{formatCurrency(a.balance)}</Table.Td>
                        <Table.Td><Badge color={a.isActive ? 'green' : 'gray'} variant="light">{a.isActive ? 'Active' : 'Inactive'}</Badge></Table.Td>
                        <Table.Td>
                          <Menu shadow="md" width={120}>
                            <Menu.Target><ActionIcon variant="subtle" color="gray"><IconDotsVertical size="1rem" /></ActionIcon></Menu.Target>
                            <Menu.Dropdown>
                              <Menu.Item leftSection={<IconEdit size="1rem" />} onClick={() => handleEdit(a)}>Edit</Menu.Item>
                              <Menu.Item color="red" leftSection={<IconTrash size="1rem" />} onClick={() => handleDelete(a)}>Delete</Menu.Item>
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
        </Tabs.Panel>
      </Tabs>

      <AccountModal opened={modalOpened} onClose={closeModal} onSave={handleSave} account={selectedAccount} accounts={accounts} />

      <Modal opened={deleteOpened} onClose={closeDelete} title="Delete Account" centered>
        <Text mb="md">Are you sure you want to delete <strong>{accountToDelete?.accountCode} - {accountToDelete?.accountName}</strong>?</Text>
        <Group justify="flex-end">
          <Button variant="default" onClick={closeDelete}>Cancel</Button>
          <Button color="red" loading={deleteLoading} onClick={handleConfirmDelete}>Delete</Button>
        </Group>
      </Modal>
    </Stack>
  );
};

export default AccountsPage;
