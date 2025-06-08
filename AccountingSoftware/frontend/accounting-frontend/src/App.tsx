import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import './App.css';
import AccountsPage from './pages/AccountsPage';

function App() {
  return (
    <Router>
      <div className="App">
        <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
          <div className="container-fluid">
            <Link className="navbar-brand text-primary fw-bold" to="/">
              Accounting Software
            </Link>
            <div className="navbar-nav ms-auto">
              <Link className="nav-link" to="/">Dashboard</Link>
              <Link className="nav-link" to="/accounts">Chart of Accounts</Link>
              <Link className="nav-link" to="/journal">Journal Entries</Link>
              <Link className="nav-link" to="/reports">Reports</Link>
            </div>
          </div>
        </nav>

        <main className="container-fluid py-4">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/accounts" element={<AccountsPage />} />
            <Route path="/journal" element={<div className="alert alert-info">Journal Entries - Coming Soon</div>} />
            <Route path="/reports" element={<div className="alert alert-info">Reports - Coming Soon</div>} />
          </Routes>
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
              <h5 className="card-title text-muted">Equity</h5>
              <p className="card-text display-6 text-info fw-bold">$0.00</p>
            </div>
          </div>
        </div>
        <div className="col-md-6 col-lg-3">
          <div className="card h-100 border-start border-success border-4">
            <div className="card-body">
              <h5 className="card-title text-muted">Monthly Revenue</h5>
              <p className="card-text display-6 text-success fw-bold">$0.00</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default App;
