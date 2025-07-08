import React from 'react';
import { Line } from 'react-chartjs-2';
import { MonthlyRevenueTrend } from '../../../services/dashboardService';

interface MonthlyRevenueChartProps {
  data: MonthlyRevenueTrend;
  loading?: boolean;
  error?: string | null;
}

const MonthlyRevenueChart: React.FC<MonthlyRevenueChartProps> = ({ data, loading = false, error = null }) => {
  if (loading) {
    return (
      <div className="chart-card">
        <div className="chart-card-header">
          <h5 className="chart-title">
            <i className="bi bi-graph-up me-2"></i>
            Monthly Revenue Trend
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
            <i className="bi bi-graph-up me-2"></i>
            Monthly Revenue Trend
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
      label: 'Monthly Revenue',
      data: data.data,
      backgroundColor: 'rgba(13, 110, 253, 0.1)',
      borderColor: '#0d6efd',
      borderWidth: 2,
      fill: true,
      tension: 0.4
    }]
  };

  const options = {
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
          <i className="bi bi-graph-up me-2"></i>
          Monthly Revenue Trend
        </h5>
      </div>
      <div className="chart-card-body">
        <Line data={chartData} options={options} />
      </div>
    </div>
  );
};

export default MonthlyRevenueChart;
