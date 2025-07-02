import React from 'react';
import { useAuth } from '../auth/AuthContext';
import './TopNavbar.scss';

interface TopNavbarProps {
  sidebarOpen: boolean;
  onToggleSidebar: () => void;
}

const TopNavbar: React.FC<TopNavbarProps> = ({ sidebarOpen, onToggleSidebar }) => {
  const { user, logout } = useAuth();

  const handleLogout = async (e: React.MouseEvent) => {
    e.preventDefault();
    try {
      await logout();
      // Navigation to login will be handled by the auth context
    } catch (error) {
      console.error('Logout error:', error);
    }
  };

  return (
    <nav className={`top-navbar ${sidebarOpen ? 'top-navbar-expanded' : 'top-navbar-collapsed'}`}>
      <div className="top-navbar-content">
        {/* Left side - Sidebar toggle for desktop */}
        <div className="top-navbar-left">
          <button 
            className="sidebar-toggle-desktop d-none d-lg-block"
            onClick={onToggleSidebar}
            title={sidebarOpen ? 'Collapse sidebar' : 'Expand sidebar'}
          >
            <i className={`bi ${sidebarOpen ? 'bi-list' : 'bi-list'}`}></i>
          </button>
          
          {/* Mobile hamburger menu */}
          <button 
            className="mobile-menu-btn d-lg-none"
            onClick={onToggleSidebar}
          >
            <i className="bi bi-list"></i>
          </button>
          
          <span className="app-title d-lg-none">Accounting Software</span>
        </div>

        {/* Right side - User information */}
        <div className="top-navbar-right">
          <div className="user-info-section">
            {/* Notifications */}
            <div className="notification-btn">
              <button className="btn btn-link text-muted position-relative">
                <i className="bi bi-bell"></i>
                <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
                  3
                  <span className="visually-hidden">unread notifications</span>
                </span>
              </button>
            </div>

            {/* User dropdown */}
            <div className="dropdown">
              <button 
                className="btn btn-link dropdown-toggle user-dropdown-btn" 
                type="button" 
                data-bs-toggle="dropdown" 
                aria-expanded="false"
              >
                <div className="user-avatar">
                  <i className="bi bi-person-circle"></i>
                </div>
                <div className="user-details d-none d-sm-block">
                  <span className="user-name">{user?.fullName || 'User'}</span>
                  <small className="user-role text-muted">
                    {user?.roles?.join(', ') || 'User'}
                  </small>
                </div>
              </button>
              <ul className="dropdown-menu dropdown-menu-end">
                <li>
                  <h6 className="dropdown-header">
                    <div className="d-flex align-items-center">
                      <i className="bi bi-person-circle me-2"></i>
                      <div>
                        <div>{user?.fullName || 'User'}</div>
                        <small className="text-muted">{user?.email || ''}</small>
                      </div>
                    </div>
                  </h6>
                </li>
                <li><hr className="dropdown-divider" /></li>
                <li>
                  <a className="dropdown-item" href="#profile">
                    <i className="bi bi-person me-2"></i>
                    Profile Settings
                  </a>
                </li>
                <li>
                  <a className="dropdown-item" href="#preferences">
                    <i className="bi bi-gear me-2"></i>
                    Preferences
                  </a>
                </li>
                <li>
                  <a className="dropdown-item" href="#help">
                    <i className="bi bi-question-circle me-2"></i>
                    Help & Support
                  </a>
                </li>
                <li><hr className="dropdown-divider" /></li>
                <li>
                  <button 
                    className="dropdown-item text-danger" 
                    onClick={handleLogout}
                    style={{ border: 'none', background: 'none', width: '100%', textAlign: 'left' }}
                  >
                    <i className="bi bi-box-arrow-right me-2"></i>
                    Sign Out
                  </button>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </nav>
  );
};

export default TopNavbar;