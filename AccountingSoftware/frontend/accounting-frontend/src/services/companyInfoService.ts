import { CompanyInfo, CreateCompanyInfoDto, PaginationParams, SortingParams, CompanyInfoFilteringParams, PagedResult } from '../types';
import apiClient from './apiClient';

export const companyInfoService = {
  // Get all company infos (paged)
  getCompanyInfos: async (
    pagination: PaginationParams,
    sorting: SortingParams,
    filtering: CompanyInfoFilteringParams
  ): Promise<PagedResult<CompanyInfo>> => {
    const response = await apiClient.get<PagedResult<CompanyInfo>>('/companyinfo', {
      params: {
        ...pagination,
        ...sorting,
        ...filtering,
      },
    });
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
