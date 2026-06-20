import React from 'react';
import { Stack, Skeleton } from '@mantine/core';

interface TableSkeletonProps {
  rows?: number;
  height?: number;
}

const TableSkeleton: React.FC<TableSkeletonProps> = ({ rows = 5, height = 40 }) => (
  <Stack gap="sm">
    {Array(rows).fill(0).map((_, i) => <Skeleton key={i} height={height} />)}
  </Stack>
);

export default TableSkeleton;
