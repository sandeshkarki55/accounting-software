import { JournalEntry, CreateJournalEntryDto } from '../types';
import apiClient from './apiClient';

export const journalEntryService = {
  // Get all journal entries
  getJournalEntries: async (): Promise<JournalEntry[]> => {
    const response = await apiClient.get<JournalEntry[]>('/journalentries');
    return response.data;
  },

  // Create new journal entry
  createJournalEntry: async (journalEntry: CreateJournalEntryDto): Promise<JournalEntry> => {
    const response = await apiClient.post<JournalEntry>('/journalentries', journalEntry);
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
};
