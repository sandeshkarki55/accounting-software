import { Customer, CreateCustomerDto, UpdateCustomerDto, PaginationParams, SortingParams, CustomerFilteringParams, PagedResult } from '../types';
import apiClient from './apiClient';

export const customerService = {
  // Get all customers (paged)
  getCustomersPaged: async (
    pagination: PaginationParams,
    sorting: SortingParams,
    filtering: CustomerFilteringParams
  ): Promise<PagedResult<Customer>> => {
    const response = await apiClient.get<PagedResult<Customer>>('/customers', {
      params: {
        ...pagination,
        ...sorting,
        ...filtering,
      },
    });
    return response.data;
  },

  // Get all customers (legacy, all at once)
  getCustomers: async (): Promise<Customer[]> => {
    const response = await apiClient.get<Customer[]>('/customers');
    return response.data;
  },

  // Get customer by ID
  getCustomer: async (id: number): Promise<Customer> => {
    const response = await apiClient.get<Customer>(`/customers/${id}`);
    return response.data;
  },

  // Create new customer
  createCustomer: async (customer: CreateCustomerDto): Promise<Customer> => {
    const response = await apiClient.post<Customer>('/customers', customer);
    return response.data;
  },

  // Update customer
  updateCustomer: async (id: number, customer: UpdateCustomerDto): Promise<Customer> => {
    const response = await apiClient.put<Customer>(`/customers/${id}`, customer);
    return response.data;
  },

  // Delete customer
  deleteCustomer: async (id: number): Promise<void> => {
    await apiClient.delete(`/customers/${id}`);
  },
};
