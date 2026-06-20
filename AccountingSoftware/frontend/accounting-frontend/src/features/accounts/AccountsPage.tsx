import React, { useState, useEffect } from 'react';
import { Account, AccountType, CreateAccountDto, UpdateAccountDto, PaginationParams, SortingParams, AccountFilteringParams } from '../../types';
import { accountService } from '../../services/accountService';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData';
import AccountModal from './components/AccountModal';
import ChartOfAccountsTree from './components/ChartOfAccountsTree';
import { PageHeader, DeleteConfirmModal, ActionMenu, SortableTh, TableSkeleton, PaginationRow } from '../../components/common';
import { formatCurrency } from '../../utils';
import { accountTypeColor, accountTypeName, activeStatusColor } from '../../utils';
import { Table, TextInput, Group, Badge, Alert, Paper, Stack, Tabs, Select, Center, Loader } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { IconSearch, IconListTree, IconList } from '@tabler/icons-react';
import { showNotification } from '@mantine/notifications';

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
      try {
        setHLoading(true);
        setHError(null);
        const d = await accountService.getAccountsHierarchy();
        setHierarchyAccounts(d);
      } catch {
        setHError('Failed to load accounts');
      } finally {
        setHLoading(false);
      }
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
    try {
      await accountService.deleteAccount(accountToDelete.id);
      closeDelete();
      setAccountToDelete(undefined);
      refetch();
      showNotification({ title: 'Success', message: 'Account deleted', color: 'green' });
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
      <PageHeader title="Chart of Accounts" actionLabel="Add Account" onAction={handleAdd} />

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
              <TextInput
                placeholder="Search accounts..."
                leftSection={<IconSearch size="1rem" />}
                value={filtering.searchTerm || ''}
                onChange={e => setFiltering(f => ({ ...f, searchTerm: e.currentTarget.value }))}
                style={{ width: 280 }}
              />
              <Select
                placeholder="All Types"
                clearable
                data={[AccountType.Asset, AccountType.Liability, AccountType.Equity, AccountType.Revenue, AccountType.Expense]
                  .map(t => ({ value: String(t), label: accountTypeName(t) }))}
                value={filtering.accountType !== undefined ? String(filtering.accountType) : null}
                onChange={v => setFiltering(f => ({ ...f, accountType: v ? Number(v) : undefined }))}
                w={160}
              />
            </Group>

            {loadingList ? <TableSkeleton /> :
             errorList ? <Alert color="red" variant="light">{errorList}</Alert> : (
              <>
                <Table striped highlightOnHover>
                  <Table.Thead>
                    <Table.Tr>
                      <SortableTh column="accountCode" currentSortBy={sorting.orderBy} descending={sorting.descending} onSort={handleSort}>Code</SortableTh>
                      <SortableTh column="accountName" currentSortBy={sorting.orderBy} descending={sorting.descending} onSort={handleSort}>Name</SortableTh>
                      <SortableTh column="accountType" currentSortBy={sorting.orderBy} descending={sorting.descending} onSort={handleSort}>Type</SortableTh>
                      <SortableTh column="balance" currentSortBy={sorting.orderBy} descending={sorting.descending} onSort={handleSort}>Balance</SortableTh>
                      <Table.Th>Status</Table.Th>
                      <Table.Th w={60}>Actions</Table.Th>
                    </Table.Tr>
                  </Table.Thead>
                  <Table.Tbody>
                    {accounts.map(a => (
                      <Table.Tr key={a.id}>
                        <Table.Td fw={500}>{a.accountCode}</Table.Td>
                        <Table.Td>{a.accountName}</Table.Td>
                        <Table.Td><Badge color={accountTypeColor(a.accountType)} variant="light">{accountTypeName(a.accountType)}</Badge></Table.Td>
                        <Table.Td>{formatCurrency(a.balance)}</Table.Td>
                        <Table.Td><Badge color={activeStatusColor(a.isActive)} variant="light">{a.isActive ? 'Active' : 'Inactive'}</Badge></Table.Td>
                        <Table.Td>
                          <ActionMenu onEdit={() => handleEdit(a)} onDelete={() => handleDelete(a)} />
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
        </Tabs.Panel>
      </Tabs>

      <AccountModal opened={modalOpened} onClose={closeModal} onSave={handleSave} account={selectedAccount} accounts={accounts} />

      <DeleteConfirmModal
        opened={deleteOpened}
        onClose={closeDelete}
        onConfirm={handleConfirmDelete}
        loading={deleteLoading}
        title="Delete Account"
        message={<>Are you sure you want to delete <strong>{accountToDelete?.accountCode} - {accountToDelete?.accountName}</strong>?</>}
      />
    </Stack>
  );
};

export default AccountsPage;
