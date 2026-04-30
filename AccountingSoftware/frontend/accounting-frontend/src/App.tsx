import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link, useLocation } from 'react-router-dom';
import {
  AppShell,
  Burger,
  Group,
  NavLink,
  Title,
  Text,
  Menu,
  UnstyledButton,
  Avatar,
  ActionIcon,
  Stack,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import {
  IconLayoutDashboard,
  IconList,
  IconUsers,
  IconBuilding,
  IconReceipt,
  IconFileText,
  IconChartBar,
  IconUserCircle,
  IconLogout,
  IconSun,
  IconMoon,
  IconCalculator,
} from '@tabler/icons-react';
import { ThemeProvider } from './theme/ThemeProvider';
import { AuthProvider, useAuth } from './features/auth/AuthContext';
import ProtectedRoute from './features/auth/ProtectedRoute';
import LoginPage from './features/auth/LoginPage';
import RegisterPage from './features/auth/RegisterPage';
import DashboardPage from './features/dashboard/DashboardPage';
import AccountsPage from './features/accounts/AccountsPage';
import InvoicesPage from './features/invoices/InvoicesPage';
import CustomersPage from './features/customers/CustomersPage';
import CompaniesPage from './features/companies/CompaniesPage';
import JournalEntriesPage from './features/journalEntries/JournalEntriesPage';
import ReportsPage from './features/reports/ReportsPage';
import UserProfilePage from './features/profile/UserProfilePage';
import { useMantineColorScheme } from '@mantine/core';

// ─── Navigation Items ───────────────────────────────────────────────────────

interface NavItemData {
  to: string;
  label: string;
  icon: React.ReactNode;
}

const navItems: NavItemData[] = [
  { to: '/', label: 'Dashboard', icon: <IconLayoutDashboard size="1.25rem" /> },
  { to: '/companies', label: 'Companies', icon: <IconBuilding size="1.25rem" /> },
  { to: '/accounts', label: 'Chart of Accounts', icon: <IconList size="1.25rem" /> },
  { to: '/invoices', label: 'Invoices', icon: <IconReceipt size="1.25rem" /> },
  { to: '/customers', label: 'Customers', icon: <IconUsers size="1.25rem" /> },
  { to: '/journal', label: 'Journal Entries', icon: <IconFileText size="1.25rem" /> },
  { to: '/reports', label: 'Reports', icon: <IconChartBar size="1.25rem" /> },
];

// ─── Color Scheme Toggle ────────────────────────────────────────────────────

const ColorSchemeToggle: React.FC = () => {
  const { colorScheme, toggleColorScheme } = useMantineColorScheme();
  return (
    <ActionIcon
      variant="subtle"
      color="gray"
      onClick={toggleColorScheme}
      title={colorScheme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'}
      size="lg"
    >
      {colorScheme === 'dark' ? <IconSun size="1.25rem" /> : <IconMoon size="1.25rem" />}
    </ActionIcon>
  );
};

// ─── Side Navigation ────────────────────────────────────────────────────────

const SideNavigation: React.FC<{ onNavigate?: () => void }> = ({ onNavigate }) => {
  const location = useLocation();

  return (
    <AppShell.Navbar p="xs">
      <AppShell.Section grow mt="xs">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            component={Link}
            to={item.to}
            label={item.label}
            leftSection={<span style={{ display: 'flex', alignItems: 'center' }}>{item.icon}</span>}
            active={location.pathname === item.to}
            variant="filled"
            mb={4}
            onClick={onNavigate}
          />
        ))}
      </AppShell.Section>
    </AppShell.Navbar>
  );
};

// ─── Header ─────────────────────────────────────────────────────────────────

const AppHeader: React.FC<{ opened: boolean; toggle: () => void }> = ({ opened, toggle }) => {
  const { user, logout } = useAuth();

  const handleLogout = async () => {
    try {
      await logout();
    } catch (error) {
      console.error('Logout error:', error);
    }
  };

  return (
    <AppShell.Header p="sm">
      <Group h="100%" justify="space-between">
        <Group>
          <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
          <IconCalculator size={28} color="var(--mantine-color-navy-7)" />
          <Title order={4} visibleFrom="sm">Accounting Software</Title>
        </Group>

        <Group gap="xs">
          <ColorSchemeToggle />
          <Menu shadow="md" width={200} position="bottom-end">
            <Menu.Target>
              <UnstyledButton>
                <Group gap="xs">
                  <Avatar color="navy" radius="xl" size="sm">
                    {user?.firstName?.charAt(0)}{user?.lastName?.charAt(0)}
                  </Avatar>
                  <Stack gap={0} visibleFrom="sm">
                    <Text size="sm" fw={500}>{user?.fullName || 'User'}</Text>
                    <Text size="xs" c="dimmed">{user?.roles?.join(', ') || 'User'}</Text>
                  </Stack>
                </Group>
              </UnstyledButton>
            </Menu.Target>
            <Menu.Dropdown>
              <Menu.Label>
                <Text size="xs" c="dimmed">{user?.email}</Text>
              </Menu.Label>
              <Menu.Divider />
              <Menu.Item
                component={Link}
                to="/profile"
                leftSection={<IconUserCircle size="1rem" />}
              >
                Profile Settings
              </Menu.Item>
              <Menu.Divider />
              <Menu.Item
                color="red"
                leftSection={<IconLogout size="1rem" />}
                onClick={handleLogout}
              >
                Sign Out
              </Menu.Item>
            </Menu.Dropdown>
          </Menu>
        </Group>
      </Group>
    </AppShell.Header>
  );
};

// ─── App Shell ──────────────────────────────────────────────────────────────

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

// ─── App ────────────────────────────────────────────────────────────────────

function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <Router>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/*" element={<AppShellLayout />} />
          </Routes>
        </Router>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
