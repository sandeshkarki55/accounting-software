import React from 'react';
import { Table } from '@mantine/core';

interface SortableThProps {
  column: string;
  currentSortBy: string | undefined;
  descending: boolean | undefined;
  onSort: (column: string) => void;
  children: React.ReactNode;
  style?: React.CSSProperties;
}

const SortableTh: React.FC<SortableThProps> = ({
  column,
  currentSortBy,
  descending,
  onSort,
  children,
  style,
}) => (
  <Table.Th onClick={() => onSort(column)} style={{ cursor: 'pointer', ...style }}>
    {children}
    {currentSortBy === column ? (descending ? ' ↓' : ' ↑') : ''}
  </Table.Th>
);

export default SortableTh;
