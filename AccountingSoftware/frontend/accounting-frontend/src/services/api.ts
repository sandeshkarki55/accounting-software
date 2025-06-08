import axios from 'axios';
import { Account, CreateAccountDto, UpdateAccountDto } from '../types';

const API_BASE_URL = 'https://localhost:7042/api'; // Update this to match your backend port

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const accountService = {
  // Get all accounts
  getAccounts: async (): Promise<Account[]> => {
    const response = await apiClient.get<Account[]>('/accounts');
    return response.data;
  },

  // Get account by ID
  getAccount: async (id: number): Promise<Account> => {
    const response = await apiClient.get<Account>(`/accounts/${id}`);
    return response.data;
  },

  // Create new account
  createAccount: async (account: CreateAccountDto): Promise<Account> => {
    const response = await apiClient.post<Account>('/accounts', account);
    return response.data;
  },

  // Update account
  updateAccount: async (id: number, account: UpdateAccountDto): Promise<void> => {
    await apiClient.put(`/accounts/${id}`, account);
  },

  // Delete account
  deleteAccount: async (id: number): Promise<void> => {
    await apiClient.delete(`/accounts/${id}`);
  },
};

export default apiClient;