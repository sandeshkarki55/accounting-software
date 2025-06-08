// Customer-related types
export interface Customer {
  id: number;
  customerCode: string;
  companyName: string;
  contactPersonName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isActive: boolean;
  notes?: string;
}

export interface CreateCustomerDto {
  customerCode: string;
  companyName: string;
  contactPersonName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  notes?: string;
}

export interface UpdateCustomerDto {
  companyName: string;
  contactPersonName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isActive: boolean;
  notes?: string;
}

// Company Info types
export interface CompanyInfo {
  id: number;
  companyName: string;
  legalName?: string;
  taxNumber?: string;
  registrationNumber?: string;
  email?: string;
  phone?: string;
  website?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  logoUrl?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankRoutingNumber?: string;
  currency: string;
  isDefault: boolean;
}

export interface CreateCompanyInfoDto {
  companyName: string;
  legalName?: string;
  taxNumber?: string;
  registrationNumber?: string;
  email?: string;
  phone?: string;
  website?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  logoUrl?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankRoutingNumber?: string;
  currency: string;
  isDefault: boolean;
}