import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import './App.css';
import AccountsPage from './pages/AccountsPage';

function App() {
  return (
    <Router>
      <div className="App">
        <nav className="navbar">
          <div className="nav-brand">
            <h2>Accounting Software</h2>
          </div>
          <ul className="nav-links">
            <li><Link to="/">Dashboard</Link></li>
            <li><Link to="/accounts">Chart of Accounts</Link></li>
            <li><Link to="/journal">Journal Entries</Link></li>
            <li><Link to="/reports">Reports</Link></li>
          </ul>
        </nav>

        <main className="main-content">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/accounts" element={<AccountsPage />} />
            <Route path="/journal" element={<div>Journal Entries - Coming Soon</div>} />
            <Route path="/reports" element={<div>Reports - Coming Soon</div>} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

// Simple dashboard component
const Dashboard: React.FC = () => {
  return (
    <div className="dashboard">
      <h1>Accounting Dashboard</h1>
      <div className="dashboard-cards">
        <div className="card">
          <h3>Total Assets</h3>
          <p className="amount">$0.00</p>
        </div>
        <div className="card">
          <h3>Total Liabilities</h3>
          <p className="amount">$0.00</p>
        </div>
        <div className="card">
          <h3>Equity</h3>
          <p className="amount">$0.00</p>
        </div>
        <div className="card">
          <h3>Monthly Revenue</h3>
          <p className="amount">$0.00</p>
        </div>
      </div>
    </div>
  );
};

export default App;
