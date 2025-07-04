import { Customer, CreateCustomerDto, UpdateCustomerDto } from '../types';
import apiClient from './apiClient';

export const customerService = {
  // Get all customers
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
