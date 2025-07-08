import React from 'react';
import { Line } from 'react-chartjs-2';
import { PaymentTrend } from '../../../services/dashboardService';

interface PaymentTrendChartProps {
  data: PaymentTrend;
  loading?: boolean;
  error?: string | null;
}

const PaymentTrendChart: React.FC<PaymentTrendChartProps> = ({ data, loading = false, error = null }) => {
  if (loading) {
    return (
      <div className="chart-card">
        <div className="chart-card-header">
          <h5 className="chart-title">
            <i className="bi bi-credit-card me-2"></i>
            Payment Trend (6 Months)
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
            <i className="bi bi-credit-card me-2"></i>
            Payment Trend (6 Months)
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
      label: 'Payments Received',
      data: data.data,
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
          <i className="bi bi-credit-card me-2"></i>
          Payment Trend (6 Months)
        </h5>
      </div>
      <div className="chart-card-body">
        <Line data={chartData} options={options} />
      </div>
    </div>
  );
};

export default PaymentTrendChart;
