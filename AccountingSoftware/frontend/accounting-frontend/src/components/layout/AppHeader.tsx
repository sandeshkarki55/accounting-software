import React from 'react';
import { Link } from 'react-router-dom';
import {
  AppShell,
  Group,
  Burger,
  Title,
  Menu,
  UnstyledButton,
  Avatar,
  Stack,
  Text,
  ActionIcon,
} from '@mantine/core';
import { useMantineColorScheme } from '@mantine/core';
import {
  IconCalculator,
  IconUserCircle,
  IconLogout,
  IconSun,
  IconMoon,
} from '@tabler/icons-react';
import { useAuth } from '../../features/auth/AuthContext';

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

interface AppHeaderProps {
  opened: boolean;
  toggle: () => void;
}

const AppHeader: React.FC<AppHeaderProps> = ({ opened, toggle }) => {
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
              <Menu.Item component={Link} to="/profile" leftSection={<IconUserCircle size="1rem" />}>
                Profile Settings
              </Menu.Item>
              <Menu.Divider />
              <Menu.Item color="red" leftSection={<IconLogout size="1rem" />} onClick={handleLogout}>
                Sign Out
              </Menu.Item>
            </Menu.Dropdown>
          </Menu>
        </Group>
      </Group>
    </AppShell.Header>
  );
};

export default AppHeader;
