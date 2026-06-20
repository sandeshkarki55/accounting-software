import React from 'react';
import { Routes, Route } from 'react-router-dom';
import { AppShell } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import AppHeader from './AppHeader';
import SideNavigation from './SideNavigation';
import ProtectedRoute from '../../features/auth/ProtectedRoute';
import DashboardPage from '../../features/dashboard/DashboardPage';
import AccountsPage from '../../features/accounts/AccountsPage';
import InvoicesPage from '../../features/invoices/InvoicesPage';
import CustomersPage from '../../features/customers/CustomersPage';
import CompaniesPage from '../../features/companies/CompaniesPage';
import JournalEntriesPage from '../../features/journalEntries/JournalEntriesPage';
import ReportsPage from '../../features/reports/ReportsPage';
import UserProfilePage from '../../features/profile/UserProfilePage';

const AppShellLayout: React.FC = () => {
  const [opened, { toggle, close }] = useDisclosure(false);

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{ width: 240, breakpoint: 'sm', collapsed: { mobile: !opened } }}
      padding="md"
    >
      <AppHeader opened={opened} toggle={toggle} />
      <SideNavigation onNavigate={close} />
      <AppShell.Main>
        <Routes>
          <Route path="/" element={<ProtectedRoute><DashboardPage /></ProtectedRoute>} />
          <Route path="/accounts" element={<ProtectedRoute><AccountsPage /></ProtectedRoute>} />
          <Route path="/invoices" element={<ProtectedRoute><InvoicesPage /></ProtectedRoute>} />
          <Route path="/customers" element={<ProtectedRoute><CustomersPage /></ProtectedRoute>} />
          <Route path="/companies" element={<ProtectedRoute><CompaniesPage /></ProtectedRoute>} />
          <Route path="/journal" element={<ProtectedRoute><JournalEntriesPage /></ProtectedRoute>} />
          <Route path="/reports" element={<ProtectedRoute><ReportsPage /></ProtectedRoute>} />
          <Route path="/profile" element={<ProtectedRoute><UserProfilePage /></ProtectedRoute>} />
        </Routes>
      </AppShell.Main>
    </AppShell>
  );
};

export default AppShellLayout;
