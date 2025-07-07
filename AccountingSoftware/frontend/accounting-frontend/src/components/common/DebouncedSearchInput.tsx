import React, { useRef, useCallback } from 'react';

interface DebouncedSearchInputProps {
  placeholder?: string;
  defaultValue?: string;
  debounceMs?: number;
  onSearch: (value: string) => void;
  className?: string;
}

const DebouncedSearchInput: React.FC<DebouncedSearchInputProps> = ({
  placeholder = 'Search...',
  defaultValue = '',
  debounceMs = 400,
  onSearch,
  className = '',
}) => {
  const debounceTimeout = useRef<NodeJS.Timeout | null>(null);

  const handleChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    if (debounceTimeout.current) {
      clearTimeout(debounceTimeout.current);
    }
    debounceTimeout.current = setTimeout(() => {
      onSearch(value);
    }, debounceMs);
  }, [onSearch, debounceMs]);

  return (
    <input
      type="text"
      className={`form-control ${className}`}
      placeholder={placeholder}
      defaultValue={defaultValue}
      onChange={handleChange}
    />
  );
};

export default DebouncedSearchInput;
