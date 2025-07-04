import apiClient from './apiClient';

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
