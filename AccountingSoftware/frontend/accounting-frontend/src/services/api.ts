// Re-export all services from the new modular structure
// This maintains backward compatibility while moving to a better organized structure
export {
  accountService,
  customerService,
  companyInfoService,
  invoiceService,
  journalEntryService,
  apiClient as default
} from './index';