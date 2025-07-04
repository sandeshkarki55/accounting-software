import { Invoice, CreateInvoiceDto, MarkInvoiceAsPaidDto } from '../types';
import apiClient from './apiClient';

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
