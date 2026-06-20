import React from 'react';
import { Modal, Text, Group, Button, Stack } from '@mantine/core';

interface DeleteConfirmModalProps {
  opened: boolean;
  onClose: () => void;
  onConfirm: () => void;
  loading?: boolean;
  title?: string;
  message: React.ReactNode;
  confirmLabel?: string;
}

const DeleteConfirmModal: React.FC<DeleteConfirmModalProps> = ({
  opened,
  onClose,
  onConfirm,
  loading,
  title = 'Confirm Delete',
  message,
  confirmLabel = 'Delete',
}) => (
  <Modal opened={opened} onClose={onClose} title={title} size="sm" centered>
    <Stack gap="md">
      <Text>{message}</Text>
      <Group justify="flex-end">
        <Button variant="default" onClick={onClose}>Cancel</Button>
        <Button color="red" loading={loading} onClick={onConfirm}>{confirmLabel}</Button>
      </Group>
    </Stack>
  </Modal>
);

export default DeleteConfirmModal;
