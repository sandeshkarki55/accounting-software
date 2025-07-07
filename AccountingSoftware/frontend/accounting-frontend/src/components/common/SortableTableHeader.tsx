import React from 'react';

export interface SortableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  className?: string;
  style?: React.CSSProperties;
}

export interface SortableTableHeaderProps {
  columns: SortableColumn[];
  sorting: { orderBy: string; descending: boolean };
  onSort: (column: string) => void;
}

const SortableTableHeader: React.FC<SortableTableHeaderProps> = ({ columns, sorting, onSort }) => {
  return (
    <thead className="table-dark">
      <tr>
        {columns.map((col) => (
          <th
            key={col.key}
            style={col.sortable ? { ...col.style, cursor: 'pointer' } : col.style}
            className={col.className}
            onClick={col.sortable ? () => onSort(col.key) : undefined}
            scope="col"
          >
            {col.label}
            {col.sortable && sorting.orderBy === col.key && (
              <i className={`bi ms-1 bi-sort-${sorting.descending ? 'down' : 'up'}-alt`}></i>
            )}
          </th>
        ))}
      </tr>
    </thead>
  );
};

export default SortableTableHeader;
