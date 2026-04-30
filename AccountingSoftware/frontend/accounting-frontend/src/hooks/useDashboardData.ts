import { useState, useEffect, useCallback } from 'react';
import { dashboardService, DashboardStats } from '../services/dashboardService';

interface DashboardData {
  stats: DashboardStats | null;
  invoiceStatus: any[];
  monthlyRevenue: any[];
  topCustomers: any[];
  revVsExp: any[];
  paymentTrend: any[];
  accountBalance: any[];
}

interface UseDashboardDataResult {
  data: DashboardData;
  isLoading: boolean;
  error: string | null;
  refresh: () => void;
}

function transformPie(dto: any): any[] {
  if (!dto || !dto.labels) return [];
  return dto.labels.map((label: string, i: number) => ({
    name: label,
    value: dto.data?.[i] ?? 0,
    color: dto.backgroundColors?.[i],
  }));
}

function transformLine(dto: any, xKey: string, yKey: string): any[] {
  if (!dto) return [];
  const labels = dto.labels || dto.months || [];
  const data = dto.data || dto.amounts || dto.values || [];
  return labels.map((label: string, i: number) => ({ [xKey]: label, [yKey]: data[i] ?? 0 }));
}

function transformBar(dto: any, xKey: string, yKey: string): any[] {
  if (!dto) return [];
  const labels = dto.labels || dto.names || [];
  const data = dto.data || dto.values || [];
  return labels.map((label: string, i: number) => ({ [xKey]: label, [yKey]: data[i] ?? 0 }));
}

function transformRevExp(dto: any): any[] {
  if (!dto) return [];
  const labels = dto.labels || dto.months || [];
  const revData = dto.revenue || dto.revenues || [];
  const expData = dto.expenses || [];
  return labels.map((label: string, i: number) => ({
    month: label,
    revenue: revData[i] ?? 0,
    expenses: expData[i] ?? 0,
  }));
}

export function useDashboardData(months: number = 6): UseDashboardDataResult {
  const [data, setData] = useState<DashboardData>({
    stats: null,
    invoiceStatus: [],
    monthlyRevenue: [],
    topCustomers: [],
    revVsExp: [],
    paymentTrend: [],
    accountBalance: [],
  });
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchAll = useCallback(async (m: number) => {
    setIsLoading(true);
    setError(null);
    try {
      const [stats, invStatus, mRev, topCust, rve, payTrend, acctBal] = await Promise.all([
        dashboardService.getDashboardStats(),
        dashboardService.getInvoiceStatusDistribution(),
        dashboardService.getMonthlyRevenueTrend(),
        dashboardService.getTopCustomers(5),
        dashboardService.getRevenueVsExpenses(),
        dashboardService.getPaymentTrend(m),
        dashboardService.getAccountBalanceOverview(),
      ]);

      setData({
        stats,
        invoiceStatus: transformPie(invStatus),
        monthlyRevenue: transformLine(mRev, 'month', 'amount'),
        topCustomers: transformBar(topCust, 'name', 'value'),
        revVsExp: transformRevExp(rve),
        paymentTrend: transformLine(payTrend, 'month', 'amount'),
        accountBalance: transformPie(acctBal),
      });
    } catch (err) {
      setError('Failed to load dashboard data. Please try again.');
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchAll(months);
  }, [fetchAll, months]);

  const refresh = useCallback(() => {
    fetchAll(months);
  }, [fetchAll, months]);

  return { data, isLoading, error, refresh };
}
