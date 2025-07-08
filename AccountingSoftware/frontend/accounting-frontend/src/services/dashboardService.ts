import apiClient from './apiClient';

export interface DashboardStats {
  totalRevenue: number;
  outstandingInvoices: number;
  activeCustomers: number;
  averageInvoiceValue: number;
  overdueAmount: number;
  totalInvoiceCount: number;
  paidInvoicesCount: number;
  monthlyRevenue: number;
  previousMonthRevenue: number;
  revenueChange: number;
  paymentRate: number;
}

export interface ChartData {
  labels: string[];
  data: number[] | { [key: string]: number[] };
  backgroundColors?: string[];
}

export interface InvoiceStatusDistribution extends ChartData {
  data: number[];
}

export interface MonthlyRevenueTrend extends ChartData {
  data: number[];
}

export interface TopCustomers extends ChartData {
  data: number[];
}

export interface RevenueVsExpenses {
  labels: string[];
  revenueData: number[];
  expensesData: number[];
}

export interface PaymentTrend extends ChartData {
  data: number[];
}

export interface AccountBalanceOverview extends ChartData {
  data: number[];
}

class DashboardService {
  private readonly baseUrl = '/dashboard';

  async getDashboardStats(): Promise<DashboardStats> {
    const response = await apiClient.get(`${this.baseUrl}/stats`);
    return response.data;
  }

  async getInvoiceStatusDistribution(): Promise<InvoiceStatusDistribution> {
    const response = await apiClient.get(`${this.baseUrl}/invoice-status-distribution`);
    return response.data;
  }

  async getMonthlyRevenueTrend(): Promise<MonthlyRevenueTrend> {
    const response = await apiClient.get(`${this.baseUrl}/monthly-revenue-trend`);
    return response.data;
  }

  async getTopCustomers(limit: number = 5): Promise<TopCustomers> {
    const response = await apiClient.get(`${this.baseUrl}/top-customers?limit=${limit}`);
    return response.data;
  }

  async getRevenueVsExpenses(): Promise<RevenueVsExpenses> {
    const response = await apiClient.get(`${this.baseUrl}/revenue-vs-expenses`);
    return response.data;
  }

  async getPaymentTrend(months: number = 6): Promise<PaymentTrend> {
    const response = await apiClient.get(`${this.baseUrl}/payment-trend?months=${months}`);
    return response.data;
  }

  async getAccountBalanceOverview(): Promise<AccountBalanceOverview> {
    const response = await apiClient.get(`${this.baseUrl}/account-balance-overview`);
    return response.data;
  }
}

export const dashboardService = new DashboardService();
