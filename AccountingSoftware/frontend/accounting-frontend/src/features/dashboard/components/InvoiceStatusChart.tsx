import React from 'react';
import { Pie } from 'react-chartjs-2';
import { InvoiceStatusDistribution } from '../../../services/dashboardService';

interface InvoiceStatusChartProps {
  data: InvoiceStatusDistribution;
  loading?: boolean;
  error?: string | null;
}

const InvoiceStatusChart: React.FC<InvoiceStatusChartProps> = ({ data, loading = false, error = null }) => {
  if (loading) {
    return (
      <div className="chart-card">
        <div className="chart-card-header">
          <h5 className="chart-title">
            <i className="bi bi-pie-chart me-2"></i>
            Invoice Status Distribution
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
            <i className="bi bi-pie-chart me-2"></i>
            Invoice Status Distribution
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
        '#6c757d', // Draft - gray
        '#0d6efd', // Sent - blue  
        '#198754', // Paid - green
        '#dc3545', // Overdue - red
        '#343a40'  // Cancelled - dark
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
          <i className="bi bi-pie-chart me-2"></i>
          Invoice Status Distribution
        </h5>
      </div>
      <div className="chart-card-body">
        <Pie data={chartData} options={options} />
      </div>
    </div>
  );
};

export default InvoiceStatusChart;
