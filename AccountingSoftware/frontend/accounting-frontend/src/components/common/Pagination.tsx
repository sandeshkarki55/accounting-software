import React from 'react';

interface PaginationProps {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
  ariaLabel?: string;
}

const Pagination: React.FC<PaginationProps> = ({ pageNumber, pageSize, totalCount, onPageChange, ariaLabel = 'Pagination' }) => {
  if (totalCount <= pageSize) return null;
  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <nav aria-label={ariaLabel} className="mt-3">
      <ul className="pagination justify-content-center">
        <li className={`page-item ${pageNumber === 1 ? 'disabled' : ''}`}>
          <button className="page-link" onClick={() => onPageChange(pageNumber - 1)} disabled={pageNumber === 1}>
            Previous
          </button>
        </li>
        {/* Page numbers */}
        {Array.from({ length: totalPages }, (_, i) => i + 1).map(page => (
          <li key={page} className={`page-item ${pageNumber === page ? 'active' : ''}`}>
            <button className="page-link" onClick={() => onPageChange(page)}>{page}</button>
          </li>
        ))}
        <li className={`page-item ${pageNumber * pageSize >= totalCount ? 'disabled' : ''}`}>
          <button className="page-link" onClick={() => onPageChange(pageNumber + 1)} disabled={pageNumber * pageSize >= totalCount}>
            Next
          </button>
        </li>
      </ul>
    </nav>
  );
};

export default Pagination;
