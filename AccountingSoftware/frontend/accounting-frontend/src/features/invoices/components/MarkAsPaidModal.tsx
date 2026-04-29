import React from 'react';
import { Invoice } from '../../../types';
import { Modal, TextInput, Button, Group, Stack, Alert } from '@mantine/core';
import { DateInput } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { IconAlertTriangle } from '@tabler/icons-react';

interface Props {
  opened: boolean;
  onClose: () => void;
  onConfirm: (dto: { paidDate: string; paymentReference: string }) => Promise<void>;
  invoice: Invoice | null;
}

const MarkAsPaidModal: React.FC<Props> = ({ opened, onClose, onConfirm, invoice }) => {
  const [loading, setLoading] = React.useState(false);
  const form = useForm({
    initialValues: { paymentDate: new Date(), paymentReference: '' },
  });

  const handleSubmit = async (values: typeof form.values) => {
    setLoading(true);
    try {
      await onConfirm({ paidDate: values.paymentDate.toISOString(), paymentReference: values.paymentReference });
    } finally { setLoading(false); }
  };

  if (!invoice) return null;

  return (
    <Modal opened={opened} onClose={onClose} title="Mark Invoice as Paid" centered>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack gap="sm">
          <Alert icon={<IconAlertTriangle size="1rem" />} color="yellow" variant="light">
            Mark invoice <strong>{invoice.invoiceNumber}</strong> as paid?
          </Alert>
          <DateInput label="Payment Date" {...form.getInputProps('paymentDate')} />
          <TextInput label="Reference Number" placeholder="Check #, transaction ID..." {...form.getInputProps('paymentReference')} />
          <Group justify="flex-end" mt="md">
            <Button variant="default" onClick={onClose}>Cancel</Button>
            <Button color="green" type="submit" loading={loading}>Confirm Payment</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  );
};

export default MarkAsPaidModal;
