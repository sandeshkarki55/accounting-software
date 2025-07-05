import { JournalEntry, CreateJournalEntryDto, UpdateJournalEntryDto } from '../types';
import apiClient from './apiClient';

export const journalEntryService = {
  // Get all journal entries
  getJournalEntries: async (): Promise<JournalEntry[]> => {
    const response = await apiClient.get<JournalEntry[]>('/journalentries');
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
};
