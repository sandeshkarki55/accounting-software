import axios from 'axios';
import { Account, CreateAccountDto, UpdateAccountDto, Customer, CreateCustomerDto, UpdateCustomerDto, CompanyInfo, CreateCompanyInfoDto, Invoice, CreateInvoiceDto, MarkInvoiceAsPaidDto } from '../types';

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

  // Get accounts in hierarchical tree structure
  getAccountsHierarchy: async (): Promise<Account[]> => {
    const response = await apiClient.get<Account[]>('/accounts/hierarchy');
    return response.data;
  },
};

// Customer service
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

// Company Info service
export const companyInfoService = {
  // Get all company infos
  getCompanyInfos: async (): Promise<CompanyInfo[]> => {
    const response = await apiClient.get<CompanyInfo[]>('/companyinfo');
    return response.data;
  },

  // Create new company info
  createCompanyInfo: async (companyInfo: CreateCompanyInfoDto): Promise<CompanyInfo> => {
    const response = await apiClient.post<CompanyInfo>('/companyinfo', companyInfo);
    return response.data;
  },

  // Update company info
  updateCompanyInfo: async (id: number, companyInfo: CreateCompanyInfoDto): Promise<CompanyInfo> => {
    const response = await apiClient.put<CompanyInfo>(`/companyinfo/${id}`, companyInfo);
    return response.data;
  },

  // Delete company info
  deleteCompanyInfo: async (id: number): Promise<void> => {
    await apiClient.delete(`/companyinfo/${id}`);
  },

  // Set default company
  setDefaultCompany: async (id: number): Promise<CompanyInfo> => {
    const response = await apiClient.put<CompanyInfo>(`/companyinfo/${id}/set-default`);
    return response.data;
  },
};

// Invoice service
export const invoiceService = {
  // Get all invoices
  getInvoices: async (): Promise<Invoice[]> => {
    const response = await apiClient.get<Invoice[]>('/invoices');
    return response.data;
  },

  // Get invoice by ID
  getInvoice: async (id: number): Promise<Invoice> => {
    const response = await apiClient.get<Invoice>(`/invoices/${id}`);
    return response.data;
  },

  // Create new invoice
  createInvoice: async (invoice: CreateInvoiceDto): Promise<Invoice> => {
    const response = await apiClient.post<Invoice>('/invoices', invoice);
    return response.data;
  },

  // Mark invoice as paid
  markInvoiceAsPaid: async (id: number, markAsPaidData: MarkInvoiceAsPaidDto): Promise<Invoice> => {
    const response = await apiClient.post<Invoice>(`/invoices/${id}/mark-as-paid`, markAsPaidData);
    return response.data;
  },

  // Delete invoice
  deleteInvoice: async (id: number): Promise<void> => {
    await apiClient.delete(`/invoices/${id}`);
  },

  // Delete invoice item
  deleteInvoiceItem: async (id: number): Promise<void> => {
    await apiClient.delete(`/invoices/items/${id}`);
  },
};

// Journal Entry service
export const journalEntryService = {
  // Delete journal entry
  deleteJournalEntry: async (id: number): Promise<void> => {
    await apiClient.delete(`/journalentries/${id}`);
  },

  // Delete journal entry line
  deleteJournalEntryLine: async (id: number): Promise<void> => {
    await apiClient.delete(`/journalentries/lines/${id}`);
  },
};

export default apiClient;