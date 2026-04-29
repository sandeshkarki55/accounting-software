import { useState, useEffect, useCallback, useRef } from 'react';
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
  const [refreshKey, setRefreshKey] = useState(0);

  // Track if filter changed so we can reset pagination
  const prevFilteringRef = useRef<F>(filtering);

  useEffect(() => {
    // Reset to page 1 when filtering changes (but not on initial mount)
    if (prevFilteringRef.current !== filtering) {
      setPagination(prev => ({ ...prev, pageNumber: 1 } as P));
    }
    prevFilteringRef.current = filtering;
  }, [filtering]);

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
  }, [pagination, sorting, filtering, refreshKey, fetchData]);

  // Explicit refetch function that callers can use after mutations (create/update/delete)
  const refetch = useCallback(() => {
    setRefreshKey(k => k + 1);
  }, []);

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
    refetch,
  };
};

export default usePagedData;
