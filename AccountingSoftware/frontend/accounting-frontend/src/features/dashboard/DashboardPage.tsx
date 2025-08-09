import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
} from 'chart.js';
import { 
  dashboardService, 
  DashboardStats,
  InvoiceStatusDistribution,
  MonthlyRevenueTrend,
  TopCustomers,
  RevenueVsExpenses,
  PaymentTrend,
  AccountBalanceOverview
} from '../../services/dashboardService';
import { usePageTitle } from '../../hooks/usePageTitle';
import {
  InvoiceStatusChart,
  MonthlyRevenueChart,
  TopCustomersChart,
  RevenueVsExpensesChart,
  PaymentTrendChart,
  AccountBalanceChart
} from './components';
import './DashboardPage.scss';

// Register Chart.js components
ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

const DashboardPage: React.FC = () => {
  usePageTitle('Dashboard');
  const navigate = useNavigate();
  
  // State for KPI stats
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [statsLoading, setStatsLoading] = useState(true);
  const [statsError, setStatsError] = useState<string | null>(null);

  // State for chart data
  const [invoiceStatusData, setInvoiceStatusData] = useState<InvoiceStatusDistribution | null>(null);
  const [invoiceStatusLoading, setInvoiceStatusLoading] = useState(true);
  const [invoiceStatusError, setInvoiceStatusError] = useState<string | null>(null);

  const [monthlyRevenueData, setMonthlyRevenueData] = useState<MonthlyRevenueTrend | null>(null);
  const [monthlyRevenueLoading, setMonthlyRevenueLoading] = useState(true);
  const [monthlyRevenueError, setMonthlyRevenueError] = useState<string | null>(null);

  const [topCustomersData, setTopCustomersData] = useState<TopCustomers | null>(null);
  const [topCustomersLoading, setTopCustomersLoading] = useState(true);
  const [topCustomersError, setTopCustomersError] = useState<string | null>(null);

  const [revenueVsExpensesData, setRevenueVsExpensesData] = useState<RevenueVsExpenses | null>(null);
  const [revenueVsExpensesLoading, setRevenueVsExpensesLoading] = useState(true);
  const [revenueVsExpensesError, setRevenueVsExpensesError] = useState<string | null>(null);

  const [paymentTrendData, setPaymentTrendData] = useState<PaymentTrend | null>(null);
  const [paymentTrendLoading, setPaymentTrendLoading] = useState(true);
  const [paymentTrendError, setPaymentTrendError] = useState<string | null>(null);

  const [accountBalanceData, setAccountBalanceData] = useState<AccountBalanceOverview | null>(null);
  const [accountBalanceLoading, setAccountBalanceLoading] = useState(true);
  const [accountBalanceError, setAccountBalanceError] = useState<string | null>(null);

  // Load dashboard stats
  const loadDashboardStats = useCallback(async () => {
    try {
      setStatsLoading(true);
      const data = await dashboardService.getDashboardStats();
      setStats(data);
      setStatsError(null);
    } catch (err) {
      setStatsError('Failed to load dashboard statistics');
      console.error('Error loading dashboard stats:', err);
    } finally {
      setStatsLoading(false);
    }
  }, []);

  // Load invoice status distribution
  const loadInvoiceStatusData = useCallback(async () => {
    try {
      setInvoiceStatusLoading(true);
      const data = await dashboardService.getInvoiceStatusDistribution();
      setInvoiceStatusData(data);
      setInvoiceStatusError(null);
    } catch (err) {
      setInvoiceStatusError('Failed to load invoice status data');
      console.error('Error loading invoice status data:', err);
    } finally {
      setInvoiceStatusLoading(false);
    }
  }, []);

  // Load monthly revenue trend
  const loadMonthlyRevenueData = useCallback(async () => {
    try {
      setMonthlyRevenueLoading(true);
      const data = await dashboardService.getMonthlyRevenueTrend();
      setMonthlyRevenueData(data);
      setMonthlyRevenueError(null);
    } catch (err) {
      setMonthlyRevenueError('Failed to load monthly revenue data');
      console.error('Error loading monthly revenue data:', err);
    } finally {
      setMonthlyRevenueLoading(false);
    }
  }, []);

  // Load top customers data
  const loadTopCustomersData = useCallback(async () => {
    try {
      setTopCustomersLoading(true);
      const data = await dashboardService.getTopCustomers(5);
      setTopCustomersData(data);
      setTopCustomersError(null);
    } catch (err) {
      setTopCustomersError('Failed to load top customers data');
      console.error('Error loading top customers data:', err);
    } finally {
      setTopCustomersLoading(false);
    }
  }, []);

  // Load revenue vs expenses data
  const loadRevenueVsExpensesData = useCallback(async () => {
    try {
      setRevenueVsExpensesLoading(true);
      const data = await dashboardService.getRevenueVsExpenses();
      setRevenueVsExpensesData(data);
      setRevenueVsExpensesError(null);
    } catch (err) {
      setRevenueVsExpensesError('Failed to load revenue vs expenses data');
      console.error('Error loading revenue vs expenses data:', err);
    } finally {
      setRevenueVsExpensesLoading(false);
    }
  }, []);

  // Load payment trend data
  const loadPaymentTrendData = useCallback(async () => {
    try {
      setPaymentTrendLoading(true);
      const data = await dashboardService.getPaymentTrend(6);
      setPaymentTrendData(data);
      setPaymentTrendError(null);
    } catch (err) {
      setPaymentTrendError('Failed to load payment trend data');
      console.error('Error loading payment trend data:', err);
    } finally {
      setPaymentTrendLoading(false);
    }
  }, []);

  // Load account balance data
  const loadAccountBalanceData = useCallback(async () => {
    try {
      setAccountBalanceLoading(true);
      const data = await dashboardService.getAccountBalanceOverview();
      setAccountBalanceData(data);
      setAccountBalanceError(null);
    } catch (err) {
      setAccountBalanceError('Failed to load account balance data');
      console.error('Error loading account balance data:', err);
    } finally {
      setAccountBalanceLoading(false);
    }
  }, []);

  // Load all dashboard data
  const loadAllDashboardData = useCallback(async () => {
    await Promise.all([
      loadDashboardStats(),
      loadInvoiceStatusData(),
      loadMonthlyRevenueData(),
      loadTopCustomersData(),
      loadRevenueVsExpensesData(),
      loadPaymentTrendData(),
      loadAccountBalanceData()
    ]);
  }, [
    loadDashboardStats,
    loadInvoiceStatusData,
    loadMonthlyRevenueData,
    loadTopCustomersData,
    loadRevenueVsExpensesData,
    loadPaymentTrendData,
    loadAccountBalanceData
  ]);

  useEffect(() => {
    loadAllDashboardData();
  }, [loadAllDashboardData]);

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', { 
      style: 'currency', 
      currency: 'USD' 
    }).format(amount);
  };

  // Show main loading if stats are still loading
  if (statsLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{minHeight: '400px'}}>
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading dashboard...</span>
        </div>
      </div>
    );
  }

  // Show main error if stats failed to load
  if (statsError || !stats) {
    return (
      <div className="alert alert-danger" role="alert">
        <strong>Error:</strong> {statsError || 'Failed to load dashboard data'}
        <button className="btn btn-sm btn-outline-danger ms-2" onClick={loadAllDashboardData}>
          Try Again
        </button>
      </div>
    );
  }

  return (
    <div className="dashboard-page">
      <div className="container-fluid">
        <div className="row">
          <div className="col-12">
            <div className="page-header mb-4">
              <h1 className="page-title">
                <i className="bi bi-speedometer2 me-3"></i>
                Accounting Dashboard
              </h1>
              <p className="page-subtitle text-muted">Welcome back! Here's your business overview</p>
            </div>

            {/* KPI Tiles */}
            <div className="row g-4 mb-5">
              <div className="col-md-6 col-xl-3">
                <div className="kpi-card kpi-revenue">
                  <div className="kpi-card-body">
                    <div className="kpi-icon">
                      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" style={{transform: 'rotate(3deg)'}}>
                        <path d="M12 1v22M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>
                      </svg>
                    </div>
                    <div className="kpi-content">
                      <h6 className="kpi-title">Total Revenue</h6>
                      <h3 className="kpi-value">{formatCurrency(stats.totalRevenue)}</h3>
                      <div className={`kpi-change ${stats.revenueChange >= 0 ? 'positive' : 'negative'}`}>
                        <i className={`bi bi-arrow-${stats.revenueChange >= 0 ? 'up' : 'down'}`}></i>
                        <span>{Math.abs(stats.revenueChange).toFixed(1)}% from last month</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div className="col-md-6 col-xl-3">
                <div className="kpi-card kpi-outstanding">
                  <div className="kpi-card-body">
                    <div className="kpi-icon">
                      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" style={{transform: 'rotate(-2deg)'}}>
                        <circle cx="12" cy="12" r="10"/>
                        <polyline points="12,6 12,12 16,14"/>
                      </svg>
                    </div>
                    <div className="kpi-content">
                      <h6 className="kpi-title">Outstanding Invoices</h6>
                      <h3 className="kpi-value">{formatCurrency(stats.outstandingInvoices)}</h3>
                      <div className="kpi-subtitle">
                        {stats.totalInvoiceCount - stats.paidInvoicesCount} invoices pending
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div className="col-md-6 col-xl-3">
                <div className="kpi-card kpi-customers">
                  <div className="kpi-card-body">
                    <div className="kpi-icon">
                      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" style={{transform: 'rotate(1deg)'}}>
                        <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
                        <circle cx="12" cy="7" r="4"/>
                      </svg>
                    </div>
                    <div className="kpi-content">
                      <h6 className="kpi-title">Active Customers</h6>
                      <h3 className="kpi-value">{stats.activeCustomers}</h3>
                      <div className="kpi-subtitle">
                        Active customers
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div className="col-md-6 col-xl-3">
                <div className="kpi-card kpi-average">
                  <div className="kpi-card-body">
                    <div className="kpi-icon">
                      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" style={{transform: 'rotate(-1deg)'}}>
                        <path d="M3 3v18h18"/>
                        <path d="M18.7 8l-5.1 5.2-2.8-2.7L7 14.3"/>
                      </svg>
                    </div>
                    <div className="kpi-content">
                      <h6 className="kpi-title">Average Invoice Value</h6>
                      <h3 className="kpi-value">{formatCurrency(stats.averageInvoiceValue)}</h3>
                      <div className="kpi-subtitle">
                        {stats.totalInvoiceCount} total invoices
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Secondary KPI Row */}
            <div className="row g-4 mb-5">
              <div className="col-md-4">
                <div className="kpi-card kpi-overdue">
                  <div className="kpi-card-body">
                    <div className="kpi-icon">
                      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" style={{transform: 'rotate(2deg)'}}>
                        <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/>
                        <line x1="12" y1="9" x2="12" y2="13"/>
                        <line x1="12" y1="17" x2="12.01" y2="17"/>
                      </svg>
                    </div>
                    <div className="kpi-content">
                      <h6 className="kpi-title">Overdue Amount</h6>
                      <h3 className="kpi-value">{formatCurrency(stats.overdueAmount)}</h3>
                      <div className="kpi-subtitle text-danger">
                        Requires immediate attention
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div className="col-md-4">
                <div className="kpi-card kpi-payment-rate">
                  <div className="kpi-card-body">
                    <div className="kpi-icon">
                      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" style={{transform: 'rotate(-1deg)'}}>
                        <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                        <polyline points="22,4 12,14.01 9,11.01"/>
                      </svg>
                    </div>
                    <div className="kpi-content">
                      <h6 className="kpi-title">Payment Rate</h6>
                      <h3 className="kpi-value">{stats.paymentRate.toFixed(1)}%</h3>
                      <div className="kpi-subtitle">
                        {stats.paidInvoicesCount} of {stats.totalInvoiceCount} paid
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div className="col-md-4">
                <div className="kpi-card kpi-monthly">
                  <div className="kpi-card-body">
                    <div className="kpi-icon">
                      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" style={{transform: 'rotate(1deg)'}}>
                        <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
                        <line x1="16" y1="2" x2="16" y2="6"/>
                        <line x1="8" y1="2" x2="8" y2="6"/>
                        <line x1="3" y1="10" x2="21" y2="10"/>
                      </svg>
                    </div>
                    <div className="kpi-content">
                      <h6 className="kpi-title">This Month's Revenue</h6>
                      <h3 className="kpi-value">{formatCurrency(stats.monthlyRevenue)}</h3>
                      <div className="kpi-subtitle">
                        vs {formatCurrency(stats.previousMonthRevenue)} last month
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Charts Section */}
            <div className="row g-4">
              {/* Invoice Status Distribution */}
              <div className="col-lg-6">
                <InvoiceStatusChart 
                  data={invoiceStatusData!} 
                  loading={invoiceStatusLoading} 
                  error={invoiceStatusError} 
                />
              </div>

              {/* Monthly Revenue Trend */}
              <div className="col-lg-6">
                <MonthlyRevenueChart 
                  data={monthlyRevenueData!} 
                  loading={monthlyRevenueLoading} 
                  error={monthlyRevenueError} 
                />
              </div>

              {/* Top Customers by Revenue */}
              <div className="col-lg-6">
                <TopCustomersChart 
                  data={topCustomersData!} 
                  loading={topCustomersLoading} 
                  error={topCustomersError} 
                />
              </div>

              {/* Revenue vs Expenses */}
              <div className="col-lg-6">
                <RevenueVsExpensesChart 
                  data={revenueVsExpensesData!} 
                  loading={revenueVsExpensesLoading} 
                  error={revenueVsExpensesError} 
                />
              </div>

              {/* Payment Trend */}
              <div className="col-lg-6">
                <PaymentTrendChart 
                  data={paymentTrendData!} 
                  loading={paymentTrendLoading} 
                  error={paymentTrendError} 
                />
              </div>

              {/* Account Balance Overview */}
              <div className="col-lg-6">
                <AccountBalanceChart 
                  data={accountBalanceData!} 
                  loading={accountBalanceLoading} 
                  error={accountBalanceError} 
                />
              </div>
            </div>

            {/* Quick Actions */}
            <div className="row g-4 mt-4">
              <div className="col-12">
                <div className="quick-actions-card">
                  <div className="quick-actions-header">
                    <h5 className="section-title">
                      <i className="bi bi-lightning me-2"></i>
                      Quick Actions
                    </h5>
                  </div>
                  <div className="quick-actions-body">
                    <div className="row g-3">
                      <div className="col-md-3">
                        <button 
                          className="btn btn-outline-primary w-100 quick-action-btn"
                          onClick={() => navigate('/invoices')}
                        >
                          <i className="bi bi-plus-circle me-2"></i>
                          Create Invoice
                        </button>
                      </div>
                      <div className="col-md-3">
                        <button 
                          className="btn btn-outline-success w-100 quick-action-btn"
                          onClick={() => navigate('/customers')}
                        >
                          <i className="bi bi-person-plus me-2"></i>
                          Add Customer
                        </button>
                      </div>
                      <div className="col-md-3">
                        <button 
                          className="btn btn-outline-info w-100 quick-action-btn"
                          onClick={() => navigate('/accounts')}
                        >
                          <i className="bi bi-file-earmark-text me-2"></i>
                          View Accounts
                        </button>
                      </div>
                      <div className="col-md-3">
                        <button 
                          className="btn btn-outline-warning w-100 quick-action-btn"
                          onClick={loadAllDashboardData}
                        >
                          <i className="bi bi-arrow-clockwise me-2"></i>
                          Refresh Data
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
