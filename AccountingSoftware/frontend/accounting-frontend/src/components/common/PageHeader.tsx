import React from 'react';
import { Group, Title, Button } from '@mantine/core';
import { IconPlus } from '@tabler/icons-react';

interface PageHeaderProps {
  title: string;
  actionLabel?: string;
  onAction?: () => void;
  actionIcon?: React.ReactNode;
}

const PageHeader: React.FC<PageHeaderProps> = ({
  title,
  actionLabel,
  onAction,
  actionIcon = <IconPlus size="1rem" />,
}) => (
  <Group justify="space-between">
    <Title order={3}>{title}</Title>
    {actionLabel && onAction && (
      <Button leftSection={actionIcon} onClick={onAction}>
        {actionLabel}
      </Button>
    )}
  </Group>
);

export default PageHeader;
