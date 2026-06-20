import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { AppShell, NavLink } from '@mantine/core';
import { navItems } from '../../config/navigation';

interface SideNavigationProps {
  onNavigate?: () => void;
}

const SideNavigation: React.FC<SideNavigationProps> = ({ onNavigate }) => {
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

export default SideNavigation;
