import React, { useState, useEffect } from 'react';
import { useAuth } from '../auth/AuthContext';
import { authService } from '../../services/authService';
import { ChangePasswordRequest, UpdateUserProfileRequest } from '../../types/auth';
import { usePageTitle } from '../../hooks/usePageTitle';
import './UserProfilePage.scss';

interface Message {
  type: 'success' | 'error';
  text: string;
  errors?: string[];
}

const UserProfilePage: React.FC = () => {
  usePageTitle('Profile');
  const { user, updateUser } = useAuth();
  
  // Profile form state
  const [profileForm, setProfileForm] = useState({
    firstName: '',
    lastName: '',
    email: ''
  });
  
  // Password form state
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: ''
  });
  
  // UI state
  const [activeTab, setActiveTab] = useState<'profile' | 'password'>('profile');
  const [profileLoading, setProfileLoading] = useState(false);
  const [passwordLoading, setPasswordLoading] = useState(false);
  const [message, setMessage] = useState<Message | null>(null);

  // Initialize form data when user is loaded
  useEffect(() => {
    if (user) {
      setProfileForm({
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email
      });
    }
  }, [user]);

  // Clear messages after 5 seconds
  useEffect(() => {
    if (message) {
      const timer = setTimeout(() => setMessage(null), 5000);
      return () => clearTimeout(timer);
    }
  }, [message]);

  const handleProfileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setProfileForm(prev => ({ ...prev, [name]: value }));
  };

  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setPasswordForm(prev => ({ ...prev, [name]: value }));
  };

  const handleProfileSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);

    // Basic validation
    if (!profileForm.firstName.trim() || !profileForm.lastName.trim() || !profileForm.email.trim()) {
      setMessage({ type: 'error', text: 'All fields are required' });
      return;
    }

    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(profileForm.email)) {
      setMessage({ type: 'error', text: 'Please enter a valid email address' });
      return;
    }

    try {
      setProfileLoading(true);
      const response = await authService.updateProfile(profileForm as UpdateUserProfileRequest);
      
      if (response.success && response.data) {
        updateUser(response.data);
        setMessage({ type: 'success', text: 'Profile updated successfully!' });
      } else {
        setMessage({ 
          type: 'error', 
          text: response.message || 'Failed to update profile',
          errors: response.errors 
        });
      }
    } catch (error) {
      setMessage({ type: 'error', text: 'An unexpected error occurred' });
    } finally {
      setProfileLoading(false);
    }
  };

  const handlePasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);

    // Basic validation
    if (!passwordForm.currentPassword || !passwordForm.newPassword || !passwordForm.confirmNewPassword) {
      setMessage({ type: 'error', text: 'All password fields are required' });
      return;
    }

    if (passwordForm.newPassword !== passwordForm.confirmNewPassword) {
      setMessage({ type: 'error', text: 'New passwords do not match' });
      return;
    }

    if (passwordForm.newPassword.length < 8) {
      setMessage({ type: 'error', text: 'Password must be at least 8 characters long' });
      return;
    }

    try {
      setPasswordLoading(true);
      const response = await authService.changePassword(passwordForm as ChangePasswordRequest);
      
      if (response.success) {
        setMessage({ type: 'success', text: 'Password changed successfully!' });
        setPasswordForm({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
      } else {
        setMessage({ 
          type: 'error', 
          text: response.message || 'Failed to change password',
          errors: response.errors 
        });
      }
    } catch (error) {
      setMessage({ type: 'error', text: 'An unexpected error occurred' });
    } finally {
      setPasswordLoading(false);
    }
  };

  if (!user) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="user-profile-page">
      <div className="container py-4">
        {/* Page Header */}
        <div className="text-center mb-4">
          <h1 className="h2 fw-bold text-dark mb-2">
            <i className="bi bi-person-circle text-primary me-2"></i>
            User Profile
          </h1>
          <p className="text-muted fs-5 mb-0">
            Manage your account settings and personal information
          </p>
        </div>

        {/* Profile Card */}
        <div className="card shadow-sm border-0">
          {/* Profile Header */}
          <div className="profile-header text-white text-center p-4">
            <div className="user-avatar-large mb-3">
              <i className="bi bi-person-circle"></i>
            </div>
            <div className="user-info">
              <h3 className="h4 fw-semibold mb-1">{user?.fullName || 'User'}</h3>
              <p className="mb-3 opacity-75">{user?.email || ''}</p>
              <div className="user-roles">
                {user?.roles?.map((role, index) => (
                  <span key={index} className="badge bg-light bg-opacity-25 border border-light border-opacity-50 me-1">
                    {role}
                  </span>
                ))}
              </div>
            </div>
          </div>

          {/* Navigation Tabs */}
          <div className="bg-light border-bottom">
            <ul className="nav nav-tabs border-0 px-3">
              <li className="nav-item">
                <button
                  className={`nav-link border-0 fw-medium px-3 py-3 ${activeTab === 'profile' ? 'active text-primary' : 'text-muted'}`}
                  onClick={() => setActiveTab('profile')}
                >
                  <i className="bi bi-person me-2"></i>
                  Profile Information
                </button>
              </li>
              <li className="nav-item">
                <button
                  className={`nav-link border-0 fw-medium px-3 py-3 ${activeTab === 'password' ? 'active text-primary' : 'text-muted'}`}
                  onClick={() => setActiveTab('password')}
                >
                  <i className="bi bi-lock me-2"></i>
                  Change Password
                </button>
              </li>
            </ul>
          </div>

          {/* Tab Content */}
          <div className="card-body p-4">
            {message && (
              <div className={`alert ${message.type === 'success' ? 'alert-success' : 'alert-danger'} d-flex align-items-center`} role="alert">
                <i className={`bi ${message.type === 'success' ? 'bi-check-circle' : 'bi-exclamation-triangle'} me-2`}></i>
                <div className="flex-grow-1">
                  {message.text}
                  {message.errors && message.errors.length > 0 && (
                    <ul className="mb-0 mt-2">
                      {message.errors.map((error, index) => (
                        <li key={index}>{error}</li>
                      ))}
                    </ul>
                  )}
                </div>
                <button
                  type="button"
                  className="btn-close ms-auto"
                  onClick={() => setMessage(null)}
                  aria-label="Close"
                ></button>
              </div>
            )}

            {/* Profile Tab */}
            {activeTab === 'profile' && (
              <div className="tab-pane">
                <div className="row">
                  <div className="col-lg-8">
                    <div className="form-section">
                      <h4 className="h5 fw-semibold text-dark mb-2">
                        <i className="bi bi-info-circle text-primary me-2"></i>
                        Personal Information
                      </h4>
                      <p className="text-muted mb-4">
                        Update your personal details and contact information.
                      </p>

                      <form onSubmit={handleProfileSubmit}>
                        <div className="row">
                          <div className="col-md-6 mb-3">
                            <label htmlFor="firstName" className="form-label fw-medium">
                              First Name <span className="text-danger">*</span>
                            </label>
                            <input
                              type="text"
                              className="form-control"
                              id="firstName"
                              name="firstName"
                              value={profileForm.firstName}
                              onChange={handleProfileChange}
                              required
                              disabled={profileLoading}
                            />
                          </div>
                          <div className="col-md-6 mb-3">
                            <label htmlFor="lastName" className="form-label fw-medium">
                              Last Name <span className="text-danger">*</span>
                            </label>
                            <input
                              type="text"
                              className="form-control"
                              id="lastName"
                              name="lastName"
                              value={profileForm.lastName}
                              onChange={handleProfileChange}
                              required
                              disabled={profileLoading}
                            />
                          </div>
                        </div>
                        <div className="mb-3">
                          <label htmlFor="email" className="form-label fw-medium">
                            Email Address <span className="text-danger">*</span>
                          </label>
                          <input
                            type="email"
                            className="form-control"
                            id="email"
                            name="email"
                            value={profileForm.email}
                            onChange={handleProfileChange}
                            required
                            disabled={profileLoading}
                          />
                          <div className="form-text">
                            This email will be used for login and notifications.
                          </div>
                        </div>

                        <div className="border-top pt-3 mt-4">
                          <button
                            type="submit"
                            className="btn btn-primary px-4"
                            disabled={profileLoading}
                          >
                            {profileLoading ? (
                              <>
                                <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                                Updating...
                              </>
                            ) : (
                              <>
                                <i className="bi bi-check-lg me-2"></i>
                                Update Profile
                              </>
                            )}
                          </button>
                        </div>
                      </form>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Password Tab */}
            {activeTab === 'password' && (
              <div className="tab-pane">
                <div className="row">
                  <div className="col-lg-8">
                    <div className="form-section">
                      <h4 className="h5 fw-semibold text-dark mb-2">
                        <i className="bi bi-shield-lock text-primary me-2"></i>
                        Change Password
                      </h4>
                      <p className="text-muted mb-4">
                        Ensure your account stays secure by using a strong password.
                      </p>

                      <form onSubmit={handlePasswordSubmit}>
                        <div className="mb-3">
                          <label htmlFor="currentPassword" className="form-label fw-medium">
                            Current Password <span className="text-danger">*</span>
                          </label>
                          <input
                            type="password"
                            className="form-control"
                            id="currentPassword"
                            name="currentPassword"
                            value={passwordForm.currentPassword}
                            onChange={handlePasswordChange}
                            required
                            disabled={passwordLoading}
                          />
                        </div>
                        <div className="mb-3">
                          <label htmlFor="newPassword" className="form-label fw-medium">
                            New Password <span className="text-danger">*</span>
                          </label>
                          <input
                            type="password"
                            className="form-control"
                            id="newPassword"
                            name="newPassword"
                            value={passwordForm.newPassword}
                            onChange={handlePasswordChange}
                            required
                            disabled={passwordLoading}
                          />
                          <div className="form-text">
                            Password should be at least 8 characters long.
                          </div>
                        </div>
                        <div className="mb-3">
                          <label htmlFor="confirmNewPassword" className="form-label fw-medium">
                            Confirm New Password <span className="text-danger">*</span>
                          </label>
                          <input
                            type="password"
                            className="form-control"
                            id="confirmNewPassword"
                            name="confirmNewPassword"
                            value={passwordForm.confirmNewPassword}
                            onChange={handlePasswordChange}
                            required
                            disabled={passwordLoading}
                          />
                        </div>

                        <div className="border-top pt-3 mt-4">
                          <button
                            type="submit"
                            className="btn btn-primary px-4"
                            disabled={passwordLoading}
                          >
                            {passwordLoading ? (
                              <>
                                <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                                Changing...
                              </>
                            ) : (
                              <>
                                <i className="bi bi-key me-2"></i>
                                Change Password
                              </>
                            )}
                          </button>
                        </div>
                      </form>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default UserProfilePage;
