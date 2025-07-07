import { useState, useCallback, useRef } from 'react';

/**
 * Generic hook for managing form state.
 * @param initialState The initial form state object.
 */
export function useFormState<T extends object>(initialState: T) {
  const initialRef = useRef(initialState);
  const [formData, setFormData] = useState<T>(initialState);

  // Generic change handler for input, select, textarea
  const handleChange = useCallback((e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    let newValue: any = value;
    if (type === 'checkbox' || type === 'radio') {
      newValue = (e.target as HTMLInputElement).checked;
    }
    setFormData(prev => ({
      ...prev,
      [name]: newValue
    }));
  }, []);

  // Reset form to initial state (from ref, not from possibly stale closure)
  const resetForm = useCallback(() => setFormData(initialRef.current), []);

  return { formData, setFormData, handleChange, resetForm };
}
