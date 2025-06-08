import React from 'react';
import './TopNavbar.css';

interface TopNavbarProps {
  sidebarOpen: boolean;
  onToggleSidebar: () => void;
}

const TopNavbar: React.FC<TopNavbarProps> = ({ sidebarOpen, onToggleSidebar }) => {
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
                  <span className="user-name">John Doe</span>
                  <small className="user-role text-muted">Administrator</small>
                </div>
              </button>
              <ul className="dropdown-menu dropdown-menu-end">
                <li>
                  <h6 className="dropdown-header">
                    <div className="d-flex align-items-center">
                      <i className="bi bi-person-circle me-2"></i>
                      <div>
                        <div>John Doe</div>
                        <small className="text-muted">john.doe@company.com</small>
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
                  <a className="dropdown-item text-danger" href="#logout">
                    <i className="bi bi-box-arrow-right me-2"></i>
                    Sign Out
                  </a>
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