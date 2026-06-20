import React from 'react';
import {
  IconLayoutDashboard,
  IconList,
  IconUsers,
  IconBuilding,
  IconReceipt,
  IconFileText,
  IconChartBar,
} from '@tabler/icons-react';

export interface NavItemConfig {
  to: string;
  label: string;
  icon: React.ReactNode;
}

export const navItems: NavItemConfig[] = [
  { to: '/', label: 'Dashboard', icon: <IconLayoutDashboard size="1.25rem" /> },
  { to: '/companies', label: 'Companies', icon: <IconBuilding size="1.25rem" /> },
  { to: '/accounts', label: 'Chart of Accounts', icon: <IconList size="1.25rem" /> },
  { to: '/invoices', label: 'Invoices', icon: <IconReceipt size="1.25rem" /> },
  { to: '/customers', label: 'Customers', icon: <IconUsers size="1.25rem" /> },
  { to: '/journal', label: 'Journal Entries', icon: <IconFileText size="1.25rem" /> },
  { to: '/reports', label: 'Reports', icon: <IconChartBar size="1.25rem" /> },
];
