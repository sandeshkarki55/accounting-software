import { useState, useEffect } from 'react';
import { PaginationParams, SortingParams, FilteringParams, PagedResult } from '../types/index';

interface UsePagedDataOptions<T, P, S, F> {
  fetchData: (pagination: P, sorting: S, filtering: F) => Promise<PagedResult<T>>;
  initialPagination?: P;
  initialSorting?: S;
  initialFiltering?: F;
}

const usePagedData = <T, P extends PaginationParams, S extends SortingParams, F extends FilteringParams>(
  options: UsePagedDataOptions<T, P, S, F>
) => {
  const { fetchData, initialPagination, initialSorting, initialFiltering } = options;

  const [data, setData] = useState<T[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pagination, setPagination] = useState<P>(initialPagination || { pageNumber: 1, pageSize: 10 } as P);
  const [sorting, setSorting] = useState<S>(initialSorting || { orderBy: undefined, descending: false } as S);
  const [filtering, setFiltering] = useState<F>(initialFiltering || {} as F);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        const result = await fetchData(pagination, sorting, filtering);
        setData(result.items);
        setTotalCount(result.totalCount);
        setError(null);
      } catch (err) {
        setError('Failed to load data');
        console.error('Error loading data:', err);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [pagination, sorting, filtering, fetchData]);

  return {
    data,
    loading,
    error,
    pagination,
    setPagination,
    sorting,
    setSorting,
    filtering,
    setFiltering,
    totalCount,
  };
};

export default usePagedData;
