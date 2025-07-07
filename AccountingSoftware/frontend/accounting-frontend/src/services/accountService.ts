import { Account, CreateAccountDto, UpdateAccountDto, PaginationParams, SortingParams, AccountFilteringParams, PagedResult } from '../types';
import apiClient from './apiClient';

export const accountService = {
  // Get all accounts (paged)
  getAccountsPaged: async (
    pagination: PaginationParams,
    sorting: SortingParams,
    filtering: AccountFilteringParams
  ): Promise<PagedResult<Account>> => {
    const response = await apiClient.get<PagedResult<Account>>('/accounts', {
      params: {
        ...pagination,
        ...sorting,
        ...filtering,
      },
    });
    return response.data;
  },

  // Get all accounts (legacy, all at once)
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

  // Get accounts in hierarchical tree structure
  getAccountsHierarchy: async (): Promise<Account[]> => {
    const response = await apiClient.get<Account[]>('/accounts/hierarchy');
    return response.data;
  },
};
