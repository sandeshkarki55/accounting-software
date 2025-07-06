import { JournalEntry, CreateJournalEntryDto, UpdateJournalEntryDto, PaginationParams, SortingParams, FilteringParams, PagedResult } from '../types/index';
import apiClient from './apiClient';

export const journalEntryService = {
  // Get all journal entries with pagination, sorting, and filtering
  getJournalEntries: async (pagination: PaginationParams, sorting: SortingParams, filtering: FilteringParams): Promise<PagedResult<JournalEntry>> => {
    const response = await apiClient.get<PagedResult<JournalEntry>>('/journalentries', {
      params: { ...pagination, ...sorting, ...filtering }
    });
    return response.data;
  },

  // Get single journal entry
  getJournalEntry: async (id: number): Promise<JournalEntry> => {
    const response = await apiClient.get<JournalEntry>(`/journalentries/${id}`);
    return response.data;
  },

  // Create new journal entry
  createJournalEntry: async (journalEntry: CreateJournalEntryDto): Promise<JournalEntry> => {
    const response = await apiClient.post<JournalEntry>('/journalentries', journalEntry);
    return response.data;
  },

  // Update journal entry
  updateJournalEntry: async (id: number, journalEntry: UpdateJournalEntryDto): Promise<JournalEntry> => {
    const response = await apiClient.put<JournalEntry>(`/journalentries/${id}`, journalEntry);
    return response.data;
  },

  // Delete journal entry
  deleteJournalEntry: async (id: number): Promise<void> => {
    await apiClient.delete(`/journalentries/${id}`);
  },

  // Delete journal entry line
  deleteJournalEntryLine: async (id: number): Promise<void> => {
    await apiClient.delete(`/journalentries/lines/${id}`);
  },

  // Post journal entry
  postJournalEntry: async (id: number): Promise<void> => {
    await apiClient.post(`/journalentries/${id}/post`);
  },
};
