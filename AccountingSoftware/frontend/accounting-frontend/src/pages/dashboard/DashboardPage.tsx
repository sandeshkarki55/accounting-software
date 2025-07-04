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
import { Bar, Line, Pie, Doughnut } from 'react-chartjs-2';
import { Invoice, InvoiceStatus, Customer, Account } from '../../types';
import { invoiceService, customerService, accountService } from '../../services/api';
import { usePageTitle } from '../../hooks/usePageTitle';
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

interface DashboardStats {
  totalRevenue: number;
  outstandingInvoices: number;
  activeCustomers: number;
  averageInvoiceValue: number;
  overdueAmount: number;
  totalInvoiceCount: number;
  paidInvoicesCount: number;
  monthlyRevenue: number;
  previousMonthRevenue: number;
}

const DashboardPage: React.FC = () => {
  usePageTitle('Dashboard');
  const navigate = useNavigate();
  const [stats, setStats] = useState<DashboardStats>({
    totalRevenue: 0,
    outstandingInvoices: 0,
    activeCustomers: 0,
    averageInvoiceValue: 0,
    overdueAmount: 0,
    totalInvoiceCount: 0,
    paidInvoicesCount: 0,
    monthlyRevenue: 0,
    previousMonthRevenue: 0
  });
  
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadDashboardData = useCallback(async () => {
    try {
      setLoading(true);
      const [invoicesData, customersData, accountsData] = await Promise.all([
        invoiceService.getInvoices(),
        customerService.getCustomers(),
        accountService.getAccounts()
      ]);

      setInvoices(invoicesData);
      setCustomers(customersData);
      setAccounts(accountsData);
      
      calculateStats(invoicesData, customersData);
      setError(null);
    } catch (err) {
      setError('Failed to load dashboard data');
      console.error('Error loading dashboard data:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadDashboardData();
  }, [loadDashboardData]);

  const calculateStats = (invoicesData: Invoice[], customersData: Customer[]) => {
    const currentMonth = new Date().getMonth();
    const currentYear = new Date().getFullYear();
    const previousMonth = currentMonth === 0 ? 11 : currentMonth - 1;
    const previousMonthYear = currentMonth === 0 ? currentYear - 1 : currentYear;

    // Calculate totals
    const totalRevenue = invoicesData
      .filter(inv => inv.status === InvoiceStatus.Paid)
      .reduce((sum, inv) => sum + inv.totalAmount, 0);

    const outstandingInvoices = invoicesData
      .filter(inv => inv.status !== InvoiceStatus.Paid && inv.status !== InvoiceStatus.Cancelled)
      .reduce((sum, inv) => sum + inv.totalAmount, 0);

    const overdueAmount = invoicesData
      .filter(inv => {
        const dueDate = new Date(inv.dueDate);
        const today = new Date();
        return inv.status !== InvoiceStatus.Paid && 
               inv.status !== InvoiceStatus.Cancelled && 
               dueDate < today;
      })
      .reduce((sum, inv) => sum + inv.totalAmount, 0);

    const activeCustomers = customersData.filter(customer => customer.isActive).length;
    
    const averageInvoiceValue = invoicesData.length > 0 
      ? invoicesData.reduce((sum, inv) => sum + inv.totalAmount, 0) / invoicesData.length 
      : 0;

    // Monthly revenue calculations
    const monthlyRevenue = invoicesData
      .filter(inv => {
        const invoiceDate = new Date(inv.invoiceDate);
        return inv.status === InvoiceStatus.Paid &&
               invoiceDate.getMonth() === currentMonth &&
               invoiceDate.getFullYear() === currentYear;
      })
      .reduce((sum, inv) => sum + inv.totalAmount, 0);

    const previousMonthRevenue = invoicesData
      .filter(inv => {
        const invoiceDate = new Date(inv.invoiceDate);
        return inv.status === InvoiceStatus.Paid &&
               invoiceDate.getMonth() === previousMonth &&
               invoiceDate.getFullYear() === previousMonthYear;
      })
      .reduce((sum, inv) => sum + inv.totalAmount, 0);

    const paidInvoicesCount = invoicesData.filter(inv => inv.status === InvoiceStatus.Paid).length;

    setStats({
      totalRevenue,
      outstandingInvoices,
      activeCustomers,
      averageInvoiceValue,
      overdueAmount,
      totalInvoiceCount: invoicesData.length,
      paidInvoicesCount,
      monthlyRevenue,
      previousMonthRevenue
    });
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', { 
      style: 'currency', 
      currency: 'USD' 
    }).format(amount);
  };

  const calculatePercentageChange = (current: number, previous: number) => {
    if (previous === 0) return current > 0 ? 100 : 0;
    return ((current - previous) / previous) * 100;
  };

  // Chart data preparation
  const getInvoiceStatusData = () => {
    const statusCounts = {
      [InvoiceStatus.Draft]: 0,
      [InvoiceStatus.Sent]: 0,
      [InvoiceStatus.Paid]: 0,
      [InvoiceStatus.Overdue]: 0,
      [InvoiceStatus.Cancelled]: 0
    };

    invoices.forEach(invoice => {
      statusCounts[invoice.status]++;
    });

    return {
      labels: ['Draft', 'Sent', 'Paid', 'Overdue', 'Cancelled'],
      datasets: [{
        data: Object.values(statusCounts),
        backgroundColor: [
          '#6c757d', // Draft - gray
          '#0d6efd', // Sent - blue  
          '#198754', // Paid - green
          '#dc3545', // Overdue - red
          '#343a40'  // Cancelled - dark
        ],
        borderWidth: 0
      }]
    };
  };

  const getMonthlyRevenueData = () => {
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const currentYear = new Date().getFullYear();
    const monthlyData = new Array(12).fill(0);

    invoices
      .filter(inv => inv.status === InvoiceStatus.Paid)
      .forEach(invoice => {
        const invoiceDate = new Date(invoice.invoiceDate);
        if (invoiceDate.getFullYear() === currentYear) {
          monthlyData[invoiceDate.getMonth()] += invoice.totalAmount;
        }
      });

    return {
      labels: months,
      datasets: [{
        label: 'Monthly Revenue',
        data: monthlyData,
        backgroundColor: 'rgba(13, 110, 253, 0.1)',
        borderColor: '#0d6efd',
        borderWidth: 2,
        fill: true,
        tension: 0.4
      }]
    };
  };

  const getTopCustomersData = () => {
    const customerRevenue = new Map<string, number>();

    invoices
      .filter(inv => inv.status === InvoiceStatus.Paid)
      .forEach(invoice => {
        const current = customerRevenue.get(invoice.customerName) || 0;
        customerRevenue.set(invoice.customerName, current + invoice.totalAmount);
      });

    const sortedCustomers = Array.from(customerRevenue.entries())
      .sort((a, b) => b[1] - a[1])
      .slice(0, 5);

    return {
      labels: sortedCustomers.map(([name]) => name),
      datasets: [{
        label: 'Revenue',
        data: sortedCustomers.map(([, revenue]) => revenue),
        backgroundColor: [
          '#198754',
          '#0d6efd', 
          '#ffc107',
          '#fd7e14',
          '#6f42c1'
        ],
        borderWidth: 0
      }]
    };
  };

  const getRevenueVsExpensesData = () => {
    const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const currentYear = new Date().getFullYear();
    const revenueData = new Array(12).fill(0);
    const expenseData = new Array(12).fill(0);

    // Calculate revenue from paid invoices
    invoices
      .filter(inv => inv.status === InvoiceStatus.Paid)
      .forEach(invoice => {
        const invoiceDate = new Date(invoice.invoiceDate);
        if (invoiceDate.getFullYear() === currentYear) {
          revenueData[invoiceDate.getMonth()] += invoice.totalAmount;
        }
      });

    // Calculate expenses from expense accounts (AccountType.Expense = 4)
    accounts
      .filter(account => account.accountType === 4 && account.balance > 0) // Expense accounts with positive balance
      .forEach(account => {
        // For demo purposes, distribute the balance across the year
        // In a real system, you'd have transaction dates to properly allocate
        const monthlyExpense = account.balance / 12;
        for (let i = 0; i < 12; i++) {
          expenseData[i] += monthlyExpense;
        }
      });

    return {
      labels: months,
      datasets: [
        {
          label: 'Revenue',
          data: revenueData,
          backgroundColor: 'rgba(25, 135, 84, 0.8)',
          borderColor: '#198754',
          borderWidth: 1
        },
        {
          label: 'Expenses',
          data: expenseData,
          backgroundColor: 'rgba(220, 53, 69, 0.8)',
          borderColor: '#dc3545',
          borderWidth: 1
        }
      ]
    };
  };

  const getPaymentTrendData = () => {
    const last6Months = [];
    const paymentData = [];
    const currentDate = new Date();

    // Generate last 6 months
    for (let i = 5; i >= 0; i--) {
      const date = new Date(currentDate.getFullYear(), currentDate.getMonth() - i, 1);
      last6Months.push(date.toLocaleDateString('en-US', { month: 'short', year: '2-digit' }));
      
      // Calculate payments for this month
      const monthPayments = invoices
        .filter(inv => {
          const invoiceDate = new Date(inv.invoiceDate);
          return inv.status === InvoiceStatus.Paid &&
                 invoiceDate.getMonth() === date.getMonth() &&
                 invoiceDate.getFullYear() === date.getFullYear();
        })
        .reduce((sum, inv) => sum + inv.totalAmount, 0);
      
      paymentData.push(monthPayments);
    }

    return {
      labels: last6Months,
      datasets: [{
        label: 'Payments Received',
        data: paymentData,
        backgroundColor: 'rgba(32, 201, 151, 0.1)',
        borderColor: '#20c997',
        borderWidth: 3,
        fill: true,
        tension: 0.4,
        pointBackgroundColor: '#20c997',
        pointBorderColor: '#fff',
        pointBorderWidth: 2,
        pointRadius: 6
      }]
    };
  };

  const getAccountBalanceData = () => {
    const assetAccounts = accounts.filter(acc => acc.accountType === 0 && acc.isActive); // Assets
    const liabilityAccounts = accounts.filter(acc => acc.accountType === 1 && acc.isActive); // Liabilities
    
    const totalAssets = assetAccounts.reduce((sum, acc) => sum + acc.balance, 0);
    const totalLiabilities = liabilityAccounts.reduce((sum, acc) => sum + acc.balance, 0);
    const equity = totalAssets - totalLiabilities;

    return {
      labels: ['Assets', 'Liabilities', 'Equity'],
      datasets: [{
        data: [totalAssets, totalLiabilities, equity],
        backgroundColor: [
          '#28a745', // Green for assets
          '#dc3545', // Red for liabilities  
          '#007bff'  // Blue for equity
        ],
        borderWidth: 0,
        hoverOffset: 4
      }]
    };
  };

  if (loading) return (
    <div className="d-flex justify-content-center align-items-center" style={{minHeight: '400px'}}>
      <div className="spinner-border text-primary" role="status">
        <span className="visually-hidden">Loading dashboard...</span>
      </div>
    </div>
  );

  if (error) return (
    <div className="alert alert-danger" role="alert">
      <strong>Error:</strong> {error}
      <button className="btn btn-sm btn-outline-danger ms-2" onClick={loadDashboardData}>
        Try Again
      </button>
    </div>
  );

  const revenueChange = calculatePercentageChange(stats.monthlyRevenue, stats.previousMonthRevenue);

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
                      <div className={`kpi-change ${revenueChange >= 0 ? 'positive' : 'negative'}`}>
                        <i className={`bi bi-arrow-${revenueChange >= 0 ? 'up' : 'down'}`}></i>
                        <span>{Math.abs(revenueChange).toFixed(1)}% from last month</span>
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
                        {invoices.filter(i => i.status !== InvoiceStatus.Paid && i.status !== InvoiceStatus.Cancelled).length} invoices pending
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
                        {customers.length} total customers
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
                      <h3 className="kpi-value">
                        {stats.totalInvoiceCount > 0 
                          ? ((stats.paidInvoicesCount / stats.totalInvoiceCount) * 100).toFixed(1)
                          : 0}%
                      </h3>
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
                <div className="chart-card">
                  <div className="chart-card-header">
                    <h5 className="chart-title">
                      <i className="bi bi-pie-chart me-2"></i>
                      Invoice Status Distribution
                    </h5>
                  </div>
                  <div className="chart-card-body">
                    <Pie data={getInvoiceStatusData()} options={{
                      responsive: true,
                      maintainAspectRatio: false,
                      plugins: {
                        legend: {
                          position: 'bottom'
                        }
                      }
                    }} />
                  </div>
                </div>
              </div>

              {/* Monthly Revenue Trend */}
              <div className="col-lg-6">
                <div className="chart-card">
                  <div className="chart-card-header">
                    <h5 className="chart-title">
                      <i className="bi bi-graph-up me-2"></i>
                      Monthly Revenue Trend
                    </h5>
                  </div>
                  <div className="chart-card-body">
                    <Line data={getMonthlyRevenueData()} options={{
                      responsive: true,
                      maintainAspectRatio: false,
                      plugins: {
                        legend: {
                          display: false
                        }
                      },
                      scales: {
                        y: {
                          beginAtZero: true,
                          ticks: {
                            callback: function(value) {
                              return '$' + Number(value).toLocaleString();
                            }
                          }
                        }
                      }
                    }} />
                  </div>
                </div>
              </div>

              {/* Top Customers by Revenue */}
              <div className="col-lg-6">
                <div className="chart-card">
                  <div className="chart-card-header">
                    <h5 className="chart-title">
                      <i className="bi bi-trophy me-2"></i>
                      Top Customers by Revenue
                    </h5>
                  </div>
                  <div className="chart-card-body">
                    <Doughnut data={getTopCustomersData()} options={{
                      responsive: true,
                      maintainAspectRatio: false,
                      plugins: {
                        legend: {
                          position: 'bottom'
                        }
                      }
                    }} />
                  </div>
                </div>
              </div>

              {/* Revenue vs Expenses */}
              <div className="col-lg-6">
                <div className="chart-card">
                  <div className="chart-card-header">
                    <h5 className="chart-title">
                      <i className="bi bi-bar-chart me-2"></i>
                      Revenue vs Expenses
                    </h5>
                  </div>
                  <div className="chart-card-body">
                    <Bar data={getRevenueVsExpensesData()} options={{
                      responsive: true,
                      maintainAspectRatio: false,
                      plugins: {
                        legend: {
                          position: 'top'
                        }
                      },
                      scales: {
                        y: {
                          beginAtZero: true,
                          ticks: {
                            callback: function(value) {
                              return '$' + Number(value).toLocaleString();
                            }
                          }
                        }
                      }
                    }} />
                  </div>
                </div>
              </div>

              {/* Payment Trend */}
              <div className="col-lg-6">
                <div className="chart-card">
                  <div className="chart-card-header">
                    <h5 className="chart-title">
                      <i className="bi bi-credit-card me-2"></i>
                      Payment Trend (6 Months)
                    </h5>
                  </div>
                  <div className="chart-card-body">
                    <Line data={getPaymentTrendData()} options={{
                      responsive: true,
                      maintainAspectRatio: false,
                      plugins: {
                        legend: {
                          display: false
                        }
                      },
                      scales: {
                        y: {
                          beginAtZero: true,
                          ticks: {
                            callback: function(value) {
                              return '$' + Number(value).toLocaleString();
                            }
                          }
                        }
                      }
                    }} />
                  </div>
                </div>
              </div>

              {/* Account Balance Overview */}
              <div className="col-lg-6">
                <div className="chart-card">
                  <div className="chart-card-header">
                    <h5 className="chart-title">
                      <i className="bi bi-wallet2 me-2"></i>
                      Financial Position
                    </h5>
                  </div>
                  <div className="chart-card-body">
                    <Pie data={getAccountBalanceData()} options={{
                      responsive: true,
                      maintainAspectRatio: false,
                      plugins: {
                        legend: {
                          position: 'bottom'
                        }
                      }
                    }} />
                  </div>
                </div>
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
                          onClick={() => window.location.reload()}
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
