import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from './AuthContext';
import { usePageTitle } from '../../hooks/usePageTitle';

const RegisterPage: React.FC = () => {
  usePageTitle('Register');

  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: ''
  });
  const [errors, setErrors] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');

  const { register } = useAuth();
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Clear errors and success message when user starts typing
    if (errors.length > 0) {
      setErrors([]);
    }
    if (successMessage) {
      setSuccessMessage('');
    }
  };

  const validateForm = (): boolean => {
    const newErrors: string[] = [];

    if (!formData.firstName.trim()) {
      newErrors.push('First name is required.');
    }

    if (!formData.lastName.trim()) {
      newErrors.push('Last name is required.');
    }

    if (!formData.email.trim()) {
      newErrors.push('Email is required.');
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.push('Email format is invalid.');
    }

    if (!formData.password) {
      newErrors.push('Password is required.');
    } else if (formData.password.length < 8) {
      newErrors.push('Password must be at least 8 characters long.');
    } else if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/.test(formData.password)) {
      newErrors.push('Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.');
    }

    if (formData.password !== formData.confirmPassword) {
      newErrors.push('Passwords do not match.');
    }

    setErrors(newErrors);
    return newErrors.length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setIsLoading(true);
    setErrors([]);

    try {
      const result = await register(formData);
      
      if (result.success) {
        setSuccessMessage('Registration successful! You can now sign in with your credentials.');
        // Optionally redirect to login after a delay
        setTimeout(() => {
          navigate('/login');
        }, 2000);
      } else {
        setErrors(result.errors || [result.message]);
      }
    } catch (error) {
      setErrors(['An unexpected error occurred. Please try again.']);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div className="container">
        <div className="row justify-content-center">
          <div className="col-md-6 col-lg-5">
            <div className="card shadow-sm">
              <div className="card-body p-4">
                <div className="text-center mb-4">
                  <h2 className="h4 mb-1">Create Account</h2>
                  <p className="text-muted">Sign up for a new account</p>
                </div>

                {errors.length > 0 && (
                  <div className="alert alert-danger" role="alert">
                    <div className="d-flex align-items-start">
                      <i className="bi bi-exclamation-triangle-fill me-2 mt-1"></i>
                      <div>
                        {errors.map((error, index) => (
                          <div key={index} className="small">{error}</div>
                        ))}
                      </div>
                    </div>
                  </div>
                )}

                {successMessage && (
                  <div className="alert alert-success" role="alert">
                    <div className="d-flex align-items-center">
                      <i className="bi bi-check-circle-fill me-2"></i>
                      <div>{successMessage}</div>
                    </div>
                  </div>
                )}

                <form onSubmit={handleSubmit}>
                  <div className="row">
                    <div className="col-md-6 mb-3">
                      <label htmlFor="firstName" className="form-label">First Name</label>
                      <input
                        type="text"
                        className="form-control"
                        id="firstName"
                        name="firstName"
                        value={formData.firstName}
                        onChange={handleChange}
                        required
                        placeholder="Enter first name"
                      />
                    </div>
                    <div className="col-md-6 mb-3">
                      <label htmlFor="lastName" className="form-label">Last Name</label>
                      <input
                        type="text"
                        className="form-control"
                        id="lastName"
                        name="lastName"
                        value={formData.lastName}
                        onChange={handleChange}
                        required
                        placeholder="Enter last name"
                      />
                    </div>
                  </div>

                  <div className="mb-3">
                    <label htmlFor="email" className="form-label">Email Address</label>
                    <input
                      type="email"
                      className="form-control"
                      id="email"
                      name="email"
                      value={formData.email}
                      onChange={handleChange}
                      required
                      autoComplete="email"
                      placeholder="Enter your email"
                    />
                  </div>

                  <div className="mb-3">
                    <label htmlFor="password" className="form-label">Password</label>
                    <input
                      type="password"
                      className="form-control"
                      id="password"
                      name="password"
                      value={formData.password}
                      onChange={handleChange}
                      required
                      autoComplete="new-password"
                      placeholder="Enter password"
                    />
                    <div className="form-text">
                      <small>
                        Password must be at least 8 characters and contain uppercase, lowercase, number, and special character.
                      </small>
                    </div>
                  </div>

                  <div className="mb-4">
                    <label htmlFor="confirmPassword" className="form-label">Confirm Password</label>
                    <input
                      type="password"
                      className="form-control"
                      id="confirmPassword"
                      name="confirmPassword"
                      value={formData.confirmPassword}
                      onChange={handleChange}
                      required
                      autoComplete="new-password"
                      placeholder="Confirm password"
                    />
                  </div>

                  <button
                    type="submit"
                    className="btn btn-primary w-100"
                    disabled={isLoading}
                  >
                    {isLoading ? (
                      <>
                        <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                        Creating Account...
                      </>
                    ) : (
                      <>
                        <i className="bi bi-person-plus me-2"></i>
                        Create Account
                      </>
                    )}
                  </button>
                </form>

                <hr className="my-4" />

                <div className="text-center">
                  <p className="mb-0">
                    Already have an account?{' '}
                    <Link to="/login" className="text-decoration-none">
                      Sign in here
                    </Link>
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default RegisterPage;
