import React from 'react';
import { Bar } from 'react-chartjs-2';
import { RevenueVsExpenses } from '../../../services/dashboardService';

interface RevenueVsExpensesChartProps {
  data: RevenueVsExpenses;
  loading?: boolean;
  error?: string | null;
}

const RevenueVsExpensesChart: React.FC<RevenueVsExpensesChartProps> = ({ data, loading = false, error = null }) => {
  if (loading) {
    return (
      <div className="chart-card">
        <div className="chart-card-header">
          <h5 className="chart-title">
            <i className="bi bi-bar-chart me-2"></i>
            Revenue vs Expenses
          </h5>
        </div>
        <div className="chart-card-body d-flex justify-content-center align-items-center" style={{ minHeight: '300px' }}>
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading chart...</span>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="chart-card">
        <div className="chart-card-header">
          <h5 className="chart-title">
            <i className="bi bi-bar-chart me-2"></i>
            Revenue vs Expenses
          </h5>
        </div>
        <div className="chart-card-body d-flex justify-content-center align-items-center" style={{ minHeight: '300px' }}>
          <div className="text-danger">
            <i className="bi bi-exclamation-triangle me-2"></i>
            {error}
          </div>
        </div>
      </div>
    );
  }

  const chartData = {
    labels: data.labels,
    datasets: [
      {
        label: 'Revenue',
        data: data.revenueData,
        backgroundColor: 'rgba(25, 135, 84, 0.8)',
        borderColor: '#198754',
        borderWidth: 1
      },
      {
        label: 'Expenses',
        data: data.expensesData,
        backgroundColor: 'rgba(220, 53, 69, 0.8)',
        borderColor: '#dc3545',
        borderWidth: 1
      }
    ]
  };

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top' as const
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: function(value: any) {
            return '$' + Number(value).toLocaleString();
          }
        }
      }
    }
  };

  return (
    <div className="chart-card">
      <div className="chart-card-header">
        <h5 className="chart-title">
          <i className="bi bi-bar-chart me-2"></i>
          Revenue vs Expenses
        </h5>
      </div>
      <div className="chart-card-body">
        <Bar data={chartData} options={options} />
      </div>
    </div>
  );
};

export default RevenueVsExpensesChart;
