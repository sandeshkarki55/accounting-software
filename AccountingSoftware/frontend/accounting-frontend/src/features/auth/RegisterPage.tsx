import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import {
  TextInput,
  PasswordInput,
  Button,
  Paper,
  Title,
  Text,
  Alert,
  Divider,
  Container,
  Center,
  Stack,
  Anchor,
  SimpleGrid,
} from '@mantine/core';
import { IconExclamationCircle, IconCircleCheck } from '@tabler/icons-react';
import { useAuth } from './AuthContext';
import { usePageTitle } from '../../hooks/usePageTitle';

const RegisterPage: React.FC = () => {
  usePageTitle('Register');

  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });
  const [errors, setErrors] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');

  const { register } = useAuth();
  const navigate = useNavigate();

  const updateField = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    if (errors.length > 0) setErrors([]);
    if (successMessage) setSuccessMessage('');
  };

  const validateForm = (): boolean => {
    const newErrors: string[] = [];
    if (!formData.firstName.trim()) newErrors.push('First name is required.');
    else if (formData.firstName.length > 50) newErrors.push('First name cannot exceed 50 characters.');
    if (!formData.lastName.trim()) newErrors.push('Last name is required.');
    else if (formData.lastName.length > 50) newErrors.push('Last name cannot exceed 50 characters.');
    if (!formData.email.trim()) newErrors.push('Email is required.');
    else if (!/\S+@\S+\.\S+/.test(formData.email)) newErrors.push('Email format is invalid.');
    if (!formData.password) newErrors.push('Password is required.');
    else if (formData.password.length < 8) newErrors.push('Password must be at least 8 characters.');
    else if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/.test(formData.password))
      newErrors.push('Password must contain uppercase, lowercase, digit, and special character.');
    if (formData.password !== formData.confirmPassword) newErrors.push('Passwords do not match.');
    setErrors(newErrors);
    return newErrors.length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;
    setIsLoading(true);
    setErrors([]);

    try {
      const result = await register(formData);
      if (result.success) {
        setSuccessMessage('Registration successful! Redirecting to login...');
        setTimeout(() => navigate('/login'), 2000);
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
      <Container size={520} w="100%">
        <Paper shadow="md" radius="md" p="xl" withBorder>
          <Stack align="center" mb="lg">
            <Title order={2}>Create Account</Title>
            <Text c="dimmed" size="sm">Sign up for a new account</Text>
          </Stack>

          {errors.length > 0 && (
            <Alert icon={<IconExclamationCircle size="1rem" />} color="red" mb="md" variant="light">
              {errors.map((err, i) => <Text key={i} size="sm">{err}</Text>)}
            </Alert>
          )}

          {successMessage && (
            <Alert icon={<IconCircleCheck size="1rem" />} color="green" mb="md" variant="light">
              {successMessage}
            </Alert>
          )}

          <form onSubmit={handleSubmit}>
            <SimpleGrid cols={2} mb="md">
              <TextInput
                label="First Name"
                placeholder="First name"
                value={formData.firstName}
                onChange={e => updateField('firstName', e.currentTarget.value)}
                required
              />
              <TextInput
                label="Last Name"
                placeholder="Last name"
                value={formData.lastName}
                onChange={e => updateField('lastName', e.currentTarget.value)}
                required
              />
            </SimpleGrid>

            <TextInput
              label="Email Address"
              placeholder="Enter your email"
              value={formData.email}
              onChange={e => updateField('email', e.currentTarget.value)}
              required
              autoComplete="email"
              mb="md"
            />

            <PasswordInput
              label="Password"
              placeholder="Enter password"
              value={formData.password}
              onChange={e => updateField('password', e.currentTarget.value)}
              required
              autoComplete="new-password"
              description="At least 8 chars with uppercase, lowercase, number, and special character."
              mb="md"
            />

            <PasswordInput
              label="Confirm Password"
              placeholder="Confirm password"
              value={formData.confirmPassword}
              onChange={e => updateField('confirmPassword', e.currentTarget.value)}
              required
              autoComplete="new-password"
              mb="lg"
            />

            <Button type="submit" fullWidth loading={isLoading} mb="md">
              Create Account
            </Button>
          </form>

          <Divider my="md" />

          <Text ta="center" size="sm">
            Already have an account?{' '}
            <Anchor component={Link} to="/login">Sign in</Anchor>
          </Text>
        </Paper>
      </Container>
    </Center>
  );
};

export default RegisterPage;
