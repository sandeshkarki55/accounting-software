import React, { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import {
  TextInput,
  PasswordInput,
  Button,
  Checkbox,
  Paper,
  Title,
  Text,
  Alert,
  Divider,
  Container,
  Center,
  Stack,
  Anchor,
} from '@mantine/core';
import { IconExclamationCircle } from '@tabler/icons-react';
import { useAuth } from './AuthContext';
import { usePageTitle } from '../../hooks/usePageTitle';

const LoginPage: React.FC = () => {
  usePageTitle('Login');

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const from = (location.state as any)?.from?.pathname || '/';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setErrors([]);

    try {
      const result = await login(email, password, rememberMe);
      if (result.success) {
        navigate(from, { replace: true });
      } else {
        setErrors(result.errors || [result.message]);
      }
    } catch {
      setErrors(['An unexpected error occurred. Please try again.']);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Center h="100vh" bg="gray.0">
      <Container size={420} w="100%">
        <Paper shadow="md" radius="md" p="xl" withBorder>
          <Stack align="center" mb="lg">
            <Title order={2}>Welcome Back</Title>
            <Text c="dimmed" size="sm">Sign in to your account</Text>
          </Stack>

          {errors.length > 0 && (
            <Alert icon={<IconExclamationCircle size="1rem" />} color="red" mb="md" variant="light">
              {errors.map((error, i) => <Text key={i} size="sm">{error}</Text>)}
            </Alert>
          )}

          <form onSubmit={handleSubmit}>
            <TextInput
              label="Email Address"
              placeholder="Enter your email"
              value={email}
              onChange={e => { setEmail(e.currentTarget.value); setErrors([]); }}
              required
              autoComplete="email"
              mb="md"
            />

            <PasswordInput
              label="Password"
              placeholder="Enter your password"
              value={password}
              onChange={e => { setPassword(e.currentTarget.value); setErrors([]); }}
              required
              autoComplete="current-password"
              mb="md"
            />

            <Checkbox
              label="Remember me"
              checked={rememberMe}
              onChange={e => setRememberMe(e.currentTarget.checked)}
              mb="md"
            />

            <Button type="submit" fullWidth loading={isLoading} mb="md">
              Sign In
            </Button>
          </form>

          <Divider my="md" />

          <Text ta="center" size="sm">
            Don't have an account?{' '}
            <Anchor component={Link} to="/register">Sign up here</Anchor>
          </Text>
        </Paper>

        <Text ta="center" size="xs" c="dimmed" mt="md">
          Default Admin: admin@accounting.com / Admin@123
        </Text>
      </Container>
    </Center>
  );
};

export default LoginPage;
