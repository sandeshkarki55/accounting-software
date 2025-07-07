import { useState, useCallback } from 'react';

/**
 * Hook for managing form validation errors.
 */
export function useValidation<T extends object>() {
  const [errors, setErrors] = useState<{ [K in keyof T]?: string }>({});

  // Set errors from a validation function
  const validate = useCallback((validateFn: () => Partial<{ [K in keyof T]: string }>) => {
    const newErrors = validateFn();
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }, []);

  // Clear all errors
  const clearErrors = useCallback(() => setErrors({}), []);

  // Clear a specific error
  const clearError = useCallback((field: keyof T) => {
    setErrors(prev => {
      const updated = { ...prev };
      delete updated[field];
      return updated;
    });
  }, []);

  return { errors, setErrors, validate, clearErrors, clearError };
}
