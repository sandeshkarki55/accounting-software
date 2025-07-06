// Re-export all types from organized structure
export * from './accounts';
export * from './customers';
export * from './invoices';

// Component-related types (if any specific component types are needed)
export interface ModalProps {
  show: boolean;
  onHide: () => void;
}

export interface LoadingState {
  loading: boolean;
  error: string | null;
}

export interface TableAction {
  icon: string;
  label: string;
  variant: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info';
  onClick: () => void;
}

// Pagination, sorting, and filtering types
export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
}

export interface SortingParams {
  orderBy?: string;
  descending?: boolean;
}

export interface FilteringParams {
  searchTerm?: string;
  statusFilter?: 'all' | 'posted' | 'unposted';
}

// Common API response types
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Form validation types
export interface ValidationError {
  field: string;
  message: string;
}

export interface FormState<T> {
  values: T;
  errors: ValidationError[];
  isSubmitting: boolean;
  isValid: boolean;
}