import React from 'react';
import { Group, Pagination, Text } from '@mantine/core';

interface PaginationRowProps {
  page: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  showCount?: boolean;
}

const PaginationRow: React.FC<PaginationRowProps> = ({
  page,
  totalPages,
  totalCount,
  pageSize,
  onPageChange,
  showCount = false,
}) => {
  if (totalCount === 0) return null;
  if (!showCount && totalPages <= 1) return null;

  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalCount);

  return (
    <Group justify={showCount ? 'space-between' : 'center'} mt="md">
      {showCount && (
        <Text size="sm" c="dimmed">
          Showing {from} to {to} of {totalCount}
        </Text>
      )}
      {totalPages > 1 && (
        <Pagination total={totalPages} value={page} onChange={onPageChange} />
      )}
    </Group>
  );
};

export default PaginationRow;
