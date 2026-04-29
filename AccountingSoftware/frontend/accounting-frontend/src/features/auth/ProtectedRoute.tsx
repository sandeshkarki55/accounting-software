import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { Center, Loader, Container, Paper, Title, Text, Button, ThemeIcon, Stack } from '@mantine/core';
import { IconShieldExclamation, IconArrowLeft } from '@tabler/icons-react';
import { useAuth } from './AuthContext';

interface ProtectedRouteProps {
  children: React.ReactNode;
  roles?: string[];
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, roles }) => {
  const { isAuthenticated, user, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <Center h="100vh">
        <Stack align="center" gap="xs">
          <Loader size="lg" color="navy" />
          <Text c="dimmed" size="sm">Loading...</Text>
        </Stack>
      </Center>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (roles && roles.length > 0 && user) {
    const hasRequiredRole = roles.some(role => user.roles.includes(role));
    if (!hasRequiredRole) {
      return (
        <Center h="100vh">
          <Container size={420}>
            <Paper shadow="md" radius="md" p="xl" withBorder>
              <Stack align="center">
                <ThemeIcon variant="light" size={60} radius="xl" color="yellow">
                  <IconShieldExclamation size={30} />
                </ThemeIcon>
                <Title order={3}>Access Denied</Title>
                <Text c="dimmed" ta="center">
                  You don't have permission to access this page.
                </Text>
                <Text c="dimmed" size="sm">
                  Required roles: {roles.join(', ')}
                </Text>
                <Button
                  variant="light"
                  leftSection={<IconArrowLeft size="1rem" />}
                  onClick={() => window.history.back()}
                >
                  Go Back
                </Button>
              </Stack>
            </Paper>
          </Container>
        </Center>
      );
    }
  }

  return <>{children}</>;
};

export default ProtectedRoute;
