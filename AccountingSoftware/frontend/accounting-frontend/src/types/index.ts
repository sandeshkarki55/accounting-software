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

// Common API response types
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface PaginatedResponse<T> {
  data: T[];
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  hasNext: boolean;
  hasPrevious: boolean;
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