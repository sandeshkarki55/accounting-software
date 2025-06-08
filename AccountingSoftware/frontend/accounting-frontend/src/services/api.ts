import axios from 'axios';
import { Account, CreateAccountDto, UpdateAccountDto } from '../types';

// Use environment variable for API base URL, with fallback for development
// Aspire automatically injects service URLs as environment variables
const getApiBaseUrl = (): string => {
  // In Aspire, the service reference creates an environment variable
  // Format: services__<service-name>__<endpoint>__0 (for HTTPS) or __1 (for HTTP)
  const aspireApiUrl = process.env.REACT_APP_services__accountingapi__https__0 || 
                      process.env.REACT_APP_services__accountingapi__http__0;
  
  // Debug logging for service discovery
  if (process.env.REACT_APP_DEBUG_SERVICE_DISCOVERY === 'true') {
    console.log('Service Discovery Debug:');
    console.log('Available environment variables:', Object.keys(process.env).filter(key => key.includes('services')));
    console.log('Aspire API URL:', aspireApiUrl);
  }
  
  if (aspireApiUrl) {
    console.log(`Using Aspire service discovery: ${aspireApiUrl}/api`);
    return `${aspireApiUrl}/api`;
  }
  
  // Fallback for local development without Aspire
  const fallbackUrl = process.env.REACT_APP_API_BASE_URL || 'https://localhost:7127/api';
  console.log(`Using fallback API URL: ${fallbackUrl}`);
  return fallbackUrl;
};

const API_BASE_URL = getApiBaseUrl();

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  // Add timeout to handle connection issues
  timeout: 10000,
});

// Add request interceptor for debugging
apiClient.interceptors.request.use(
  (config) => {
    console.log(`API Request: ${config.method?.toUpperCase()} ${config.baseURL}${config.url}`);
    return config;
  },
  (error) => {
    console.error('API Request Error:', error);
    return Promise.reject(error);
  }
);

// Add response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Response Error:', error.response?.data || error.message);
    return Promise.reject(error);
  }
);

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