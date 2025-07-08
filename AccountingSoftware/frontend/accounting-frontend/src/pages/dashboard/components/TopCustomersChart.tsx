import React from 'react';
import { Doughnut } from 'react-chartjs-2';
import { TopCustomers } from '../../../services/dashboardService';

interface TopCustomersChartProps {
  data: TopCustomers;
  loading?: boolean;
  error?: string | null;
}

const TopCustomersChart: React.FC<TopCustomersChartProps> = ({ data, loading = false, error = null }) => {
  if (loading) {
    return (
      <div className="chart-card">
        <div className="chart-card-header">
          <h5 className="chart-title">
            <i className="bi bi-trophy me-2"></i>
            Top Customers by Revenue
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
            <i className="bi bi-trophy me-2"></i>
            Top Customers by Revenue
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
      label: 'Revenue',
      data: data.data,
      backgroundColor: data.backgroundColors || [
        '#198754',
        '#0d6efd', 
        '#ffc107',
        '#fd7e14',
        '#6f42c1'
      ],
      borderWidth: 0
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
          <i className="bi bi-trophy me-2"></i>
          Top Customers by Revenue
        </h5>
      </div>
      <div className="chart-card-body">
        <Doughnut data={chartData} options={options} />
      </div>
    </div>
  );
};

export default TopCustomersChart;
