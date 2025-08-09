import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import './App.scss';

// Import pages
import AccountsPage from './features/accounts/AccountsPage';
import InvoicesPage from './features/invoices/InvoicesPage';
import CustomersPage from './features/customers/CustomersPage';
import CompaniesPage from './features/companies/CompaniesPage';
import DashboardPage from './features/dashboard/DashboardPage';
import UserProfilePage from './features/profile/UserProfilePage';
import JournalEntriesPage from './features/journalEntries/JournalEntriesPage';

// Import layout components
import SideNavigation from './components/layout/SideNavigation';
import TopNavbar from './components/layout/TopNavbar';

// Import auth components
import { AuthProvider } from './features/auth/AuthContext';
import ProtectedRoute from './features/auth/ProtectedRoute';
import LoginPage from './features/auth/LoginPage';
import RegisterPage from './features/auth/RegisterPage';

// Import hooks
import { usePageTitle } from './hooks/usePageTitle';

// Coming Soon component for incomplete pages
const ComingSoonPage: React.FC<{ title: string }> = ({ title }) => {
  usePageTitle(title);
  
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

// Main app layout component
const AppLayout: React.FC = () => {
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
    <div className="App">
      <SideNavigation isOpen={sidebarOpen} onToggle={toggleSidebar} />
      <TopNavbar sidebarOpen={sidebarOpen} onToggleSidebar={toggleSidebar} />

      <main className={`main-content ${sidebarOpen ? 'main-content-expanded' : 'main-content-collapsed'}`}>
        <div className="main-content-inner">
          <Routes>
            <Route path="/" element={
              <ProtectedRoute>
                <DashboardPage />
              </ProtectedRoute>
            } />
            <Route path="/accounts" element={
              <ProtectedRoute>
                <AccountsPage />
              </ProtectedRoute>
            } />
            <Route path="/customers" element={
              <ProtectedRoute>
                <CustomersPage />
              </ProtectedRoute>
            } />
            <Route path="/companies" element={
              <ProtectedRoute>
                <CompaniesPage />
              </ProtectedRoute>
            } />
            <Route path="/invoices" element={
              <ProtectedRoute>
                <InvoicesPage />
              </ProtectedRoute>
            } />
            <Route path="/journal" element={
              <ProtectedRoute>
                <JournalEntriesPage />
              </ProtectedRoute>
            } />
            <Route path="/reports" element={
              <ProtectedRoute>
                <ComingSoonPage title="Reports" />
              </ProtectedRoute>
            } />
            <Route path="/profile" element={
              <ProtectedRoute>
                <UserProfilePage />
              </ProtectedRoute>
            } />
          </Routes>
        </div>
      </main>
    </div>
  );
};

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          
          {/* Protected routes */}
          <Route path="/*" element={<AppLayout />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;
