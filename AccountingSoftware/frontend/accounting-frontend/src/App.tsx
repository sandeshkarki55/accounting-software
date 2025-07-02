import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import './App.scss';
import AccountsPage from './pages/accounts/AccountsPage';
import InvoicesPage from './pages/invoices/InvoicesPage';
import CustomersPage from './pages/customers/CustomersPage';
import CompaniesPage from './pages/companies/CompaniesPage';
import DashboardPage from './pages/dashboard/DashboardPage';
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
              <Route path="/" element={<DashboardPage />} />
              <Route path="/accounts" element={<AccountsPage />} />
              <Route path="/customers" element={<CustomersPage />} />
              <Route path="/companies" element={<CompaniesPage />} />
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
