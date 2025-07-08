import React from 'react';
import { Pie } from 'react-chartjs-2';
import { AccountBalanceOverview } from '../../../services/dashboardService';

interface AccountBalanceChartProps {
  data: AccountBalanceOverview;
  loading?: boolean;
  error?: string | null;
}

const AccountBalanceChart: React.FC<AccountBalanceChartProps> = ({ data, loading = false, error = null }) => {
  if (loading) {
    return (
      <div className="chart-card">
        <div className="chart-card-header">
          <h5 className="chart-title">
            <i className="bi bi-wallet2 me-2"></i>
            Financial Position
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
            <i className="bi bi-wallet2 me-2"></i>
            Financial Position
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
    datasets: [{
      data: data.data,
      backgroundColor: data.backgroundColors || [
        '#28a745', // Green for assets
        '#dc3545', // Red for liabilities  
        '#007bff'  // Blue for equity
      ],
      borderWidth: 0,
      hoverOffset: 4
    }]
  };

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom' as const
      }
    }
  };

  return (
    <div className="chart-card">
      <div className="chart-card-header">
        <h5 className="chart-title">
          <i className="bi bi-wallet2 me-2"></i>
          Financial Position
        </h5>
      </div>
      <div className="chart-card-body">
        <Pie data={chartData} options={options} />
      </div>
    </div>
  );
};

export default AccountBalanceChart;
