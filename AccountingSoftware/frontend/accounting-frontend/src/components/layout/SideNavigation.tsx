import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import './SideNavigation.scss';

interface SideNavigationProps {
  isOpen: boolean;
  onToggle: () => void;
}

const SideNavigation: React.FC<SideNavigationProps> = ({ isOpen, onToggle }) => {
  const location = useLocation();

  const navItems = [
    { path: '/', icon: 'bi-speedometer2', label: 'Dashboard' },
    { path: '/accounts', icon: 'bi-list-ul', label: 'Chart of Accounts' },
    { path: '/customers', icon: 'bi-people', label: 'Customers' },
    { path: '/invoices', icon: 'bi-receipt', label: 'Invoices' },
    { path: '/journal', icon: 'bi-journal-text', label: 'Journal Entries' },
    { path: '/reports', icon: 'bi-bar-chart', label: 'Reports' }
  ];

  const isActiveRoute = (path: string) => {
    return location.pathname === path;
  };

  return (
    <>
      {/* Overlay for mobile */}
      {isOpen && (
        <div 
          className="sidebar-overlay d-lg-none" 
          onClick={onToggle}
        />
      )}
      
      {/* Side Navigation */}
      <nav className={`sidebar ${isOpen ? 'sidebar-open' : 'sidebar-closed'}`}>
        <div className="sidebar-header">
          <div className="sidebar-brand">
            <i className="bi bi-calculator text-vibrant-primary me-2"></i>
            {isOpen && <span className="brand-text">Accounting Software</span>}
          </div>
        </div>
        
        <div className="sidebar-menu">
          <ul className="nav flex-column">
            {navItems.map((item) => (
              <li key={item.path} className="nav-item">
                <Link
                  to={item.path}
                  className={`nav-link ${isActiveRoute(item.path) ? 'active' : ''}`}
                  onClick={() => window.innerWidth < 992 && onToggle()}
                >
                  <i className={`bi ${item.icon} me-2`}></i>
                  {isOpen && <span className="nav-text">{item.label}</span>}
                </Link>
              </li>
            ))}
          </ul>
        </div>
      </nav>
    </>
  );
};

export default SideNavigation;