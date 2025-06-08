import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import './App.scss';
import AccountsPage from './pages/accounts/AccountsPage';
import InvoicesPage from './pages/InvoicesPage';
import CustomersPage from './pages/CustomersPage';
import SideNavigation from './components/layout/SideNavigation';
import TopNavbar from './components/layout/TopNavbar';

function App() {
  const [sidebarOpen, setSidebarOpen] = useState(true);

  // Handle responsive behavior
  useEffect(() => {
    const handleResize = () => {
      if (window.innerWidth < 992) {
        setSidebarOpen(false);
      } else {
        setSidebarOpen(true);
      }
    };

    // Set initial state
    handleResize();

    // Add event listener
    window.addEventListener('resize', handleResize);

    // Cleanup
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  const toggleSidebar = () => {
    setSidebarOpen(!sidebarOpen);
  };

  return (
    <Router>
      <div className="App">
        <SideNavigation isOpen={sidebarOpen} onToggle={toggleSidebar} />
        <TopNavbar sidebarOpen={sidebarOpen} onToggleSidebar={toggleSidebar} />

        <main className={`main-content ${sidebarOpen ? 'main-content-expanded' : 'main-content-collapsed'}`}>
          <div className="main-content-inner">
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/accounts" element={<AccountsPage />} />
              <Route path="/customers" element={<CustomersPage />} />
              <Route path="/invoices" element={<InvoicesPage />} />
              <Route path="/journal" element={<ComingSoonPage title="Journal Entries" />} />
              <Route path="/reports" element={<ComingSoonPage title="Reports" />} />
            </Routes>
          </div>
        </main>
      </div>
    </Router>
  );
}

// Enhanced Dashboard component with vibrant styling
const Dashboard: React.FC = () => {
  return (
    <div className="dashboard-page">
      <div className="page-header">
        <div className="page-title-section">
          <h1 className="page-title">
            <i className="bi bi-speedometer2 me-3"></i>
            Accounting Dashboard
          </h1>
          <p className="page-subtitle">Welcome back! Here's your business overview</p>
        </div>
      </div>

      <div className="dashboard-content">
        {/* Financial Overview Cards */}
        <div className="financial-overview">
          <div className="row g-4">
            <div className="col-md-6 col-lg-3">
              <div className="stat-card stat-card-assets">
                <div className="stat-card-icon">
                  <i className="bi bi-piggy-bank"></i>
                </div>
                <div className="stat-card-content">
                  <h5 className="stat-card-title">Total Assets</h5>
                  <p className="stat-card-value">$0.00</p>
                  <div className="stat-card-trend">
                    <i className="bi bi-arrow-up"></i>
                    <span>0% from last month</span>
                  </div>
                </div>
              </div>
            </div>
            <div className="col-md-6 col-lg-3">
              <div className="stat-card stat-card-liabilities">
                <div className="stat-card-icon">
                  <i className="bi bi-credit-card"></i>
                </div>
                <div className="stat-card-content">
                  <h5 className="stat-card-title">Total Liabilities</h5>
                  <p className="stat-card-value">$0.00</p>
                  <div className="stat-card-trend">
                    <i className="bi bi-dash"></i>
                    <span>0% from last month</span>
                  </div>
                </div>
              </div>
            </div>
            <div className="col-md-6 col-lg-3">
              <div className="stat-card stat-card-revenue">
                <div className="stat-card-icon">
                  <i className="bi bi-graph-up"></i>
                </div>
                <div className="stat-card-content">
                  <h5 className="stat-card-title">Total Revenue</h5>
                  <p className="stat-card-value">$0.00</p>
                  <div className="stat-card-trend">
                    <i className="bi bi-arrow-up"></i>
                    <span>0% from last month</span>
                  </div>
                </div>
              </div>
            </div>
            <div className="col-md-6 col-lg-3">
              <div className="stat-card stat-card-expenses">
                <div className="stat-card-icon">
                  <i className="bi bi-graph-down"></i>
                </div>
                <div className="stat-card-content">
                  <h5 className="stat-card-title">Total Expenses</h5>
                  <p className="stat-card-value">$0.00</p>
                  <div className="stat-card-trend">
                    <i className="bi bi-arrow-down"></i>
                    <span>0% from last month</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        {/* Quick Actions Section */}
        <div className="quick-actions-section">
          <h3 className="section-title">
            <i className="bi bi-lightning me-2"></i>
            Quick Actions
          </h3>
          <div className="row g-4">
            <div className="col-md-4">
              <Link to="/invoices" className="quick-action-card quick-action-primary">
                <div className="quick-action-icon">
                  <i className="bi bi-receipt"></i>
                </div>
                <div className="quick-action-content">
                  <h5>Create Invoice</h5>
                  <p>Generate and send invoices to customers</p>
                </div>
                <div className="quick-action-arrow">
                  <i className="bi bi-arrow-right"></i>
                </div>
              </Link>
            </div>
            <div className="col-md-4">
              <Link to="/customers" className="quick-action-card quick-action-success">
                <div className="quick-action-icon">
                  <i className="bi bi-people"></i>
                </div>
                <div className="quick-action-content">
                  <h5>Manage Customers</h5>
                  <p>Add and manage customer information</p>
                </div>
                <div className="quick-action-arrow">
                  <i className="bi bi-arrow-right"></i>
                </div>
              </Link>
            </div>
            <div className="col-md-4">
              <Link to="/journal" className="quick-action-card quick-action-info">
                <div className="quick-action-icon">
                  <i className="bi bi-journal-text"></i>
                </div>
                <div className="quick-action-content">
                  <h5>Journal Entry</h5>
                  <p>Record accounting transactions</p>
                </div>
                <div className="quick-action-arrow">
                  <i className="bi bi-arrow-right"></i>
                </div>
              </Link>
            </div>
          </div>
        </div>

        {/* Recent Activity Section */}
        <div className="recent-activity-section">
          <h3 className="section-title">
            <i className="bi bi-clock-history me-2"></i>
            Recent Activity
          </h3>
          <div className="activity-card">
            <div className="empty-state">
              <div className="empty-state-icon">
                <i className="bi bi-activity"></i>
              </div>
              <h5 className="empty-state-title">No recent activity</h5>
              <p className="empty-state-description">Start using the system to see your recent transactions and activities here.</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

// Coming Soon component for incomplete pages
const ComingSoonPage: React.FC<{ title: string }> = ({ title }) => {
  return (
    <div className="coming-soon-page">
      <div className="coming-soon-content">
        <div className="coming-soon-icon">
          <i className="bi bi-tools"></i>
        </div>
        <h2 className="coming-soon-title">{title}</h2>
        <p className="coming-soon-description">
          This feature is currently under development and will be available soon.
        </p>
        <div className="coming-soon-progress">
          <div className="progress">
            <div className="progress-bar bg-vibrant-primary" role="progressbar" style={{ width: '75%' }}>
              75% Complete
            </div>
          </div>
        </div>
        <Link to="/" className="btn btn-primary mt-3">
          <i className="bi bi-house me-2"></i>
          Back to Dashboard
        </Link>
      </div>
    </div>
  );
};

export default App;
