import React from 'react';
import { Account, AccountType } from '../../../types';
import { Group, Text, Badge, ActionIcon, Menu, Paper, Stack, Collapse, ThemeIcon } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { IconEdit, IconTrash, IconChevronRight, IconDotsVertical } from '@tabler/icons-react';

interface Props {
  accounts: Account[];
  onEditAccount: (a: Account) => void;
  onDeleteAccount: (a: Account) => void;
}

const typeLabel = (t: AccountType) => ({ [AccountType.Asset]: 'Asset', [AccountType.Liability]: 'Liability', [AccountType.Equity]: 'Equity', [AccountType.Revenue]: 'Revenue', [AccountType.Expense]: 'Expense' }[t] || 'Unknown');
const typeColors: Record<number, string> = { [AccountType.Asset]: 'blue', [AccountType.Liability]: 'red', [AccountType.Equity]: 'grape', [AccountType.Revenue]: 'green', [AccountType.Expense]: 'orange' };

const formatCurrency = (n: number) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n);

const AccountRow: React.FC<{ account: Account; allAccounts: Account[]; level: number; onEdit: (a: Account) => void; onDelete: (a: Account) => void }> =
  ({ account, allAccounts, level, onEdit, onDelete }) => {
    const children = allAccounts.filter(a => a.parentAccountId === account.id);
    const [opened, { toggle }] = useDisclosure(level < 1);
    const hasChildren = children.length > 0;
    const color = typeColors[account.accountType] || 'gray';

    return (
      <>
        <Paper p="xs" withBorder={false} style={{ paddingLeft: `${12 + level * 24}px` }}>
          <Group justify="space-between" wrap="nowrap">
            <Group gap="xs" wrap="nowrap">
              {hasChildren ? (
                <ActionIcon variant="subtle" size="sm" onClick={toggle}>
                  <IconChevronRight size="1rem" style={{ transform: opened ? 'rotate(90deg)' : 'none', transition: 'transform 0.2s' }} />
                </ActionIcon>
              ) : <div style={{ width: 28 }} />}
              <Text size="sm" fw={600} style={{ fontFamily: 'monospace' }}>{account.accountCode}</Text>
              <Text size="sm">{account.accountName}</Text>
              <Badge color={color} variant="light" size="sm">{typeLabel(account.accountType)}</Badge>
            </Group>
            <Group gap="xs" wrap="nowrap">
              <Text size="sm" fw={500}>{formatCurrency(account.balance)}</Text>
              <Menu shadow="md" width={120}>
                <Menu.Target><ActionIcon variant="subtle" color="gray"><IconDotsVertical size="1rem" /></ActionIcon></Menu.Target>
                <Menu.Dropdown>
                  <Menu.Item leftSection={<IconEdit size="1rem" />} onClick={() => onEdit(account)}>Edit</Menu.Item>
                  <Menu.Item color="red" leftSection={<IconTrash size="1rem" />} onClick={() => onDelete(account)}>Delete</Menu.Item>
                </Menu.Dropdown>
              </Menu>
            </Group>
          </Group>
        </Paper>
          {hasChildren && (
            <Collapse expanded={opened}>
              {children.map(child => <AccountRow key={child.id} account={child} allAccounts={allAccounts} level={level + 1} onEdit={onEdit} onDelete={onDelete} />)}
            </Collapse>
          )}
      </>
    );
  };

const ChartOfAccountsTree: React.FC<Props> = ({ accounts, onEditAccount, onDeleteAccount }) => {
  const typeOrder = [AccountType.Asset, AccountType.Liability, AccountType.Equity, AccountType.Revenue, AccountType.Expense];

  const grouped = typeOrder.map(type => {
    const typeAccounts = accounts.filter(a => a.accountType === type && !a.parentAccountId);
    return { type, typeAccounts };
  }).filter(g => g.typeAccounts.length > 0);

  if (accounts.length === 0) {
    return <Text c="dimmed" ta="center" py="xl">No accounts found. Create your first account to get started.</Text>;
  }

  return (
    <Stack gap="md">
      {grouped.map(({ type, typeAccounts }) => (
        <Paper key={type} p="sm" withBorder>
          <Group mb="xs">
            <ThemeIcon variant="light" size="sm" radius="sm" color={typeColors[type] || 'gray'}>
              <Text size="xs" fw={700}>{typeLabel(type).charAt(0)}</Text>
            </ThemeIcon>
            <Text fw={700} size="sm">{typeLabel(type)}</Text>
          </Group>
          {typeAccounts.map(a => <AccountRow key={a.id} account={a} allAccounts={accounts} level={0} onEdit={onEditAccount} onDelete={onDeleteAccount} />)}
        </Paper>
      ))}
    </Stack>
  );
};

export default ChartOfAccountsTree;
