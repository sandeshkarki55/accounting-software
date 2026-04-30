import React, { useState, useEffect } from 'react';
import { useAuth } from '../auth/AuthContext';
import { authService } from '../../services/authService';
import { ChangePasswordRequest, UpdateUserProfileRequest } from '../../types/auth';
import { usePageTitle } from '../../hooks/usePageTitle';
import {
  Paper,
  Text,
  Title,
  Stack,
  Group,
  Avatar,
  Badge,
  Tabs,
  TextInput,
  Button,
  Alert,
  Loader,
  Center,
  Box,
} from '@mantine/core';
import { IconUserCircle, IconLock, IconCheck, IconX } from '@tabler/icons-react';

interface Message {
  type: 'success' | 'error';
  text: string;
  errors?: string[];
}

const UserProfilePage: React.FC = () => {
  usePageTitle('Profile');
  const { user, updateUser } = useAuth();

  const [profileForm, setProfileForm] = useState({ firstName: '', lastName: '', email: '' });
  const [passwordForm, setPasswordForm] = useState({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
  const [profileLoading, setProfileLoading] = useState(false);
  const [passwordLoading, setPasswordLoading] = useState(false);
  const [message, setMessage] = useState<Message | null>(null);
  const [profileErrors, setProfileErrors] = useState<Record<string, string>>({});
  const [passwordErrors, setPasswordErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (user) {
      setProfileForm({ firstName: user.firstName, lastName: user.lastName, email: user.email });
    }
  }, [user]);

  useEffect(() => {
    if (message) {
      const timer = setTimeout(() => setMessage(null), 5000);
      return () => clearTimeout(timer);
    }
  }, [message]);

  const validateProfile = (): boolean => {
    const errors: Record<string, string> = {};
    if (!profileForm.firstName.trim()) errors.firstName = 'First name is required';
    if (!profileForm.lastName.trim()) errors.lastName = 'Last name is required';
    if (!profileForm.email.trim()) errors.email = 'Email is required';
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(profileForm.email)) errors.email = 'Invalid email format';
    setProfileErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const validatePassword = (): boolean => {
    const errors: Record<string, string> = {};
    if (!passwordForm.currentPassword) errors.currentPassword = 'Current password is required';
    if (!passwordForm.newPassword) errors.newPassword = 'New password is required';
    else if (passwordForm.newPassword.length < 8) errors.newPassword = 'At least 8 characters required';
    else if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/.test(passwordForm.newPassword))
      errors.newPassword = 'Must contain uppercase, lowercase, digit, and special character';
    if (!passwordForm.confirmNewPassword) errors.confirmNewPassword = 'Please confirm your new password';
    else if (passwordForm.newPassword !== passwordForm.confirmNewPassword) errors.confirmNewPassword = 'Passwords do not match';
    setPasswordErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleProfileSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    if (!validateProfile()) return;

    try {
      setProfileLoading(true);
      const response = await authService.updateProfile(profileForm as UpdateUserProfileRequest);
      if (response.success && response.data) {
        updateUser(response.data);
        setMessage({ type: 'success', text: 'Profile updated successfully!' });
      } else {
        setMessage({ type: 'error', text: response.message || 'Failed to update profile', errors: response.errors });
      }
    } catch {
      setMessage({ type: 'error', text: 'An unexpected error occurred' });
    } finally {
      setProfileLoading(false);
    }
  };

  const handlePasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    if (!validatePassword()) return;

    try {
      setPasswordLoading(true);
      const response = await authService.changePassword(passwordForm as ChangePasswordRequest);
      if (response.success) {
        setMessage({ type: 'success', text: 'Password changed successfully!' });
        setPasswordForm({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
        setPasswordErrors({});
      } else {
        setMessage({ type: 'error', text: response.message || 'Failed to change password', errors: response.errors });
      }
    } catch {
      setMessage({ type: 'error', text: 'An unexpected error occurred' });
    } finally {
      setPasswordLoading(false);
    }
  };

  if (!user) {
    return (
      <Center h={400}>
        <Loader size="lg" color="navy" />
      </Center>
    );
  }

  const initials = `${user.firstName?.charAt(0) ?? ''}${user.lastName?.charAt(0) ?? ''}`.toUpperCase();

  return (
    <Stack gap="lg">
      {/* Page Header */}
      <Stack gap={4}>
        <Title order={3}>User Profile</Title>
        <Text c="dimmed" size="sm">Manage your account settings and personal information</Text>
      </Stack>

      {/* Profile Header Card */}
      <Paper p="xl" radius="md" withBorder>
        <Group wrap="nowrap">
          <Avatar color="navy" radius="xl" size="xl" fw={700}>
            {initials || 'U'}
          </Avatar>
          <Stack gap={4}>
            <Text size="lg" fw={600}>{user.fullName || 'User'}</Text>
            <Text size="sm" c="dimmed">{user.email}</Text>
            <Group gap="xs" mt={2}>
              {user.roles?.map((role) => (
                <Badge key={role} variant="light" color="navy" size="sm">{role}</Badge>
              ))}
            </Group>
          </Stack>
        </Group>
      </Paper>

      {/* Message Alert */}
      {message && (
        <Alert
          color={message.type === 'success' ? 'teal' : 'red'}
          icon={message.type === 'success' ? <IconCheck size="1rem" /> : <IconX size="1rem" />}
          withCloseButton
          onClose={() => setMessage(null)}
          title={message.type === 'success' ? 'Success' : 'Error'}
        >
          <Stack gap={4}>
            <Text size="sm">{message.text}</Text>
            {message.errors && message.errors.length > 0 && (
              <Stack gap={0} component="ul" ml="md">
                {message.errors.map((err, i) => (
                  <Text key={i} size="sm" component="li">{err}</Text>
                ))}
              </Stack>
            )}
          </Stack>
        </Alert>
      )}

      {/* Tabs */}
      <Paper p="md" radius="md" withBorder>
        <Tabs defaultValue="profile">
          <Tabs.List mb="lg">
            <Tabs.Tab value="profile" leftSection={<IconUserCircle size="1rem" />}>
              Profile Information
            </Tabs.Tab>
            <Tabs.Tab value="password" leftSection={<IconLock size="1rem" />}>
              Change Password
            </Tabs.Tab>
          </Tabs.List>

          {/* Profile Tab */}
          <Tabs.Panel value="profile">
            <Box maw={600}>
              <Text size="sm" c="dimmed" mb="lg">
                Update your personal details and contact information.
              </Text>

              <form onSubmit={handleProfileSubmit}>
                <Group grow mb="md">
                  <TextInput
                    label="First Name"
                    required
                    placeholder="First name"
                    value={profileForm.firstName}
                    onChange={(e) => setProfileForm(p => ({ ...p, firstName: e.target.value }))}
                    error={profileErrors.firstName}
                    disabled={profileLoading}
                    radius="md"
                  />
                  <TextInput
                    label="Last Name"
                    required
                    placeholder="Last name"
                    value={profileForm.lastName}
                    onChange={(e) => setProfileForm(p => ({ ...p, lastName: e.target.value }))}
                    error={profileErrors.lastName}
                    disabled={profileLoading}
                    radius="md"
                  />
                </Group>

                <TextInput
                  label="Email Address"
                  required
                  type="email"
                  placeholder="you@example.com"
                  value={profileForm.email}
                  onChange={(e) => setProfileForm(p => ({ ...p, email: e.target.value }))}
                  error={profileErrors.email}
                  description="This email will be used for login and notifications."
                  disabled={profileLoading}
                  radius="md"
                  mb="lg"
                />

                <Button
                  type="submit"
                  color="navy"
                  loading={profileLoading}
                  leftSection={<IconCheck size="1rem" />}
                >
                  Update Profile
                </Button>
              </form>
            </Box>
          </Tabs.Panel>

          {/* Password Tab */}
          <Tabs.Panel value="password">
            <Box maw={600}>
              <Text size="sm" c="dimmed" mb="lg">
                Ensure your account stays secure by using a strong password.
              </Text>

              <form onSubmit={handlePasswordSubmit}>
                <TextInput
                  type="password"
                  label="Current Password"
                  required
                  placeholder="Enter current password"
                  value={passwordForm.currentPassword}
                  onChange={(e) => setPasswordForm(p => ({ ...p, currentPassword: e.target.value }))}
                  error={passwordErrors.currentPassword}
                  disabled={passwordLoading}
                  radius="md"
                  mb="md"
                />

                <TextInput
                  type="password"
                  label="New Password"
                  required
                  placeholder="Enter new password"
                  value={passwordForm.newPassword}
                  onChange={(e) => setPasswordForm(p => ({ ...p, newPassword: e.target.value }))}
                  error={passwordErrors.newPassword}
                  description="Password should be at least 8 characters long."
                  disabled={passwordLoading}
                  radius="md"
                  mb="md"
                />

                <TextInput
                  type="password"
                  label="Confirm New Password"
                  required
                  placeholder="Confirm new password"
                  value={passwordForm.confirmNewPassword}
                  onChange={(e) => setPasswordForm(p => ({ ...p, confirmNewPassword: e.target.value }))}
                  error={passwordErrors.confirmNewPassword}
                  disabled={passwordLoading}
                  radius="md"
                  mb="lg"
                />

                <Button
                  type="submit"
                  color="navy"
                  loading={passwordLoading}
                  leftSection={<IconLock size="1rem" />}
                >
                  Change Password
                </Button>
              </form>
            </Box>
          </Tabs.Panel>
        </Tabs>
      </Paper>
    </Stack>
  );
};

export default UserProfilePage;
