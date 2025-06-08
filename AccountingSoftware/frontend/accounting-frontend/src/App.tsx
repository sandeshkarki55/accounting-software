import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import './App.css';
import AccountsPage from './pages/AccountsPage';
import InvoicesPage from './pages/InvoicesPage';
import CustomersPage from './pages/CustomersPage';
import SideNavigation from './components/SideNavigation';
import TopNavbar from './components/TopNavbar';

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
          <div className="container-fluid py-4">
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/accounts" element={<AccountsPage />} />
              <Route path="/customers" element={<CustomersPage />} />
              <Route path="/invoices" element={<InvoicesPage />} />
              <Route path="/journal" element={<div className="alert alert-info">Journal Entries - Coming Soon</div>} />
              <Route path="/reports" element={<div className="alert alert-info">Reports - Coming Soon</div>} />
            </Routes>
          </div>
        </main>
      </div>
    </Router>
  );
}

// Simple dashboard component
const Dashboard: React.FC = () => {
  return (
    <div className="container">
      <h1 className="mb-4 text-dark">Accounting Dashboard</h1>
      <div className="row g-4">
        <div className="col-md-6 col-lg-3">
          <div className="card h-100 border-start border-primary border-4">
            <div className="card-body">
              <h5 className="card-title text-muted">Total Assets</h5>
              <p className="card-text display-6 text-success fw-bold">$0.00</p>
            </div>
          </div>
        </div>
        <div className="col-md-6 col-lg-3">
          <div className="card h-100 border-start border-warning border-4">
            <div className="card-body">
              <h5 className="card-title text-muted">Total Liabilities</h5>
              <p className="card-text display-6 text-warning fw-bold">$0.00</p>
            </div>
          </div>
        </div>
        <div className="col-md-6 col-lg-3">
          <div className="card h-100 border-start border-info border-4">
            <div className="card-body">
              <h5 className="card-title text-muted">Total Revenue</h5>
              <p className="card-text display-6 text-info fw-bold">$0.00</p>
            </div>
          </div>
        </div>
        <div className="col-md-6 col-lg-3">
          <div className="card h-100 border-start border-danger border-4">
            <div className="card-body">
              <h5 className="card-title text-muted">Total Expenses</h5>
              <p className="card-text display-6 text-danger fw-bold">$0.00</p>
            </div>
          </div>
        </div>
      </div>
      
      <div className="row mt-5">
        <div className="col-12">
          <h3 className="mb-3">Quick Actions</h3>
          <div className="row g-3">
            <div className="col-md-4">
              <Link to="/invoices" className="btn btn-outline-primary w-100 py-3">
                <i className="bi bi-receipt display-6 d-block mb-2"></i>
                <h5>Create Invoice</h5>
                <p className="text-muted small mb-0">Generate and send invoices to customers</p>
              </Link>
            </div>
            <div className="col-md-4">
              <Link to="/customers" className="btn btn-outline-success w-100 py-3">
                <i className="bi bi-people display-6 d-block mb-2"></i>
                <h5>Manage Customers</h5>
                <p className="text-muted small mb-0">Add and manage customer information</p>
              </Link>
            </div>
            <div className="col-md-4">
              <Link to="/journal" className="btn btn-outline-info w-100 py-3">
                <i className="bi bi-journal-text display-6 d-block mb-2"></i>
                <h5>Journal Entry</h5>
                <p className="text-muted small mb-0">Record accounting transactions</p>
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default App;
