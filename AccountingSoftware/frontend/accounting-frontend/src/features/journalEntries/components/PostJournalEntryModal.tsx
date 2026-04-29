import React from 'react';
import { JournalEntry } from '../../../types';
import { Modal, Text, Group, Button, Table, Alert, Stack, Paper } from '@mantine/core';
import { IconCheck, IconAlertTriangle, IconInfoCircle } from '@tabler/icons-react';

interface PostJournalEntryModalProps {
  show: boolean;
  onHide: () => void;
  onConfirm: () => Promise<void>;
  journalEntry: JournalEntry | null;
  loading: boolean;
}

const formatCurrency = (amount: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);

const formatDate = (dateString: string) =>
  new Date(dateString).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });

const PostJournalEntryModal: React.FC<PostJournalEntryModalProps> = ({ show, onHide, onConfirm, journalEntry, loading }) => {
  if (!journalEntry) return null;

  const handleConfirm = async () => {
    await onConfirm();
    onHide();
  };

  return (
    <Modal
      opened={show}
      onClose={onHide}
      title={
        <Group gap="xs">
          <IconCheck size={20} color="var(--mantine-color-green-6)" />
          <Text fw={700}>Post Journal Entry</Text>
        </Group>
      }
      size="lg"
    >
      <Stack gap="md">
        <Alert color="yellow" icon={<IconAlertTriangle size={16} />} title="Important">
          Once posted, this journal entry cannot be modified or deleted.
        </Alert>

        <Stack gap="xs">
          <Text fw={600} size="sm">Journal Entry Details:</Text>
          <Paper p="md" withBorder>
            <Table variant="verticalLayout" withRowBorders={false} data={{
              body: [
                [{ element: <Text size="sm" fw={600}>Entry Number:</Text> }, { element: <Text size="sm">{journalEntry.entryNumber}</Text> }],
                [{ element: <Text size="sm" fw={600}>Transaction Date:</Text> }, { element: <Text size="sm">{formatDate(journalEntry.transactionDate)}</Text> }],
                [{ element: <Text size="sm" fw={600}>Description:</Text> }, { element: <Text size="sm">{journalEntry.description}</Text> }],
                ...(journalEntry.reference ? [[{ element: <Text size="sm" fw={600}>Reference:</Text> }, { element: <Text size="sm">{journalEntry.reference}</Text> }]] as any : []),
                [{ element: <Text size="sm" fw={600}>Total Amount:</Text> }, { element: <Text size="sm" fw={700}>{formatCurrency(journalEntry.totalAmount)}</Text> }],
              ]
            }} />
          </Paper>
        </Stack>

        <Stack gap="xs">
          <Text fw={600} size="sm">Journal Entry Lines:</Text>
          <Table striped highlightOnHover withTableBorder>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Account</Table.Th>
                <Table.Th>Description</Table.Th>
                <Table.Th ta="right">Debit</Table.Th>
                <Table.Th ta="right">Credit</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {journalEntry.lines.map((line, i) => (
                <Table.Tr key={i}>
                  <Table.Td>
                    <Text size="xs" c="dimmed">{line.accountCode}</Text>
                    <Text size="sm">{line.accountName}</Text>
                  </Table.Td>
                  <Table.Td>{line.description}</Table.Td>
                  <Table.Td ta="right">{line.debitAmount > 0 ? formatCurrency(line.debitAmount) : '-'}</Table.Td>
                  <Table.Td ta="right">{line.creditAmount > 0 ? formatCurrency(line.creditAmount) : '-'}</Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
            <Table.Tfoot>
              <Table.Tr>
                <Table.Td colSpan={2} fw={700}>Totals:</Table.Td>
                <Table.Td ta="right" fw={700}>{formatCurrency(journalEntry.lines.reduce((s, l) => s + l.debitAmount, 0))}</Table.Td>
                <Table.Td ta="right" fw={700}>{formatCurrency(journalEntry.lines.reduce((s, l) => s + l.creditAmount, 0))}</Table.Td>
              </Table.Tr>
            </Table.Tfoot>
          </Table>
        </Stack>

        <Alert color="blue" icon={<IconInfoCircle size={16} />}>
          Posting this entry will update the account balances and make the entry permanent.
        </Alert>

        <Group justify="flex-end" mt="md">
          <Button variant="default" onClick={onHide}>Cancel</Button>
          <Button color="green" onClick={handleConfirm} loading={loading}>
            Post Entry
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
};

export default PostJournalEntryModal;
