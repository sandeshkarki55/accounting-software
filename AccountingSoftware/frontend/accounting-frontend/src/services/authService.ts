import axios, { AxiosResponse } from 'axios';
import {
  LoginRequest,
  RegisterRequest,
  LoginResponse,
  User,
  ApiResponse,
  RefreshTokenRequest,
  ChangePasswordRequest,
  UpdateUserProfileRequest
} from '../types/auth';
import { TokenService } from './tokenService';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7120/api';

// Create axios instance
const authApi = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
authApi.interceptors.request.use(
  (config) => {
    const token = TokenService.getAccessToken();
    if (token && !TokenService.isTokenExpired(token)) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export const authService = {
  async login(credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> {
    try {
      const response: AxiosResponse<ApiResponse<LoginResponse>> = await authApi.post('/auth/login', credentials);
      
      if (response.data.success && response.data.data) {
        const { accessToken, refreshToken, user } = response.data.data;
        TokenService.storeAuthData(accessToken, refreshToken, user);
      }
      
      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Login failed',
        errors: error.response?.data?.errors || ['Network error']
      };
    }
  },

  async register(userData: RegisterRequest): Promise<ApiResponse<User>> {
    try {
      const response: AxiosResponse<ApiResponse<User>> = await authApi.post('/auth/register', userData);
      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Registration failed',
        errors: error.response?.data?.errors || ['Network error']
      };
    }
  },

  async getCurrentUser(): Promise<ApiResponse<User>> {
    try {
      const response: AxiosResponse<ApiResponse<User>> = await authApi.get('/auth/me');
      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Failed to get user info',
        errors: error.response?.data?.errors || ['Network error']
      };
    }
  },

  async refreshToken(request: RefreshTokenRequest): Promise<ApiResponse<LoginResponse>> {
    try {
      const response: AxiosResponse<ApiResponse<LoginResponse>> = await authApi.post('/auth/refresh-token', request);
      
      if (response.data.success && response.data.data) {
        const { accessToken, refreshToken, user } = response.data.data;
        TokenService.storeAuthData(accessToken, refreshToken, user);
      }
      
      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Token refresh failed',
        errors: error.response?.data?.errors || ['Network error']
      };
    }
  },

  async changePassword(request: ChangePasswordRequest): Promise<ApiResponse<string>> {
    try {
      const response: AxiosResponse<ApiResponse<string>> = await authApi.post('/auth/change-password', request);
      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Password change failed',
        errors: error.response?.data?.errors || ['Network error']
      };
    }
  },

  async updateProfile(request: UpdateUserProfileRequest): Promise<ApiResponse<User>> {
    try {
      const response: AxiosResponse<ApiResponse<User>> = await authApi.put('/auth/profile', request);
      
      if (response.data.success && response.data.data) {
        // Update stored user data
        localStorage.setItem('user', JSON.stringify(response.data.data));
      }
      
      return response.data;
    } catch (error: any) {
      return {
        success: false,
        message: error.response?.data?.message || 'Profile update failed',
        errors: error.response?.data?.errors || ['Network error']
      };
    }
  },

  async logout(): Promise<ApiResponse<string>> {
    try {
      const response: AxiosResponse<ApiResponse<string>> = await authApi.post('/auth/logout');
      
      // Clear local storage regardless of response
      TokenService.clearAuthData();
      
      return response.data;
    } catch (error: any) {
      // Clear local storage even if the request fails
      TokenService.clearAuthData();
      
      return {
        success: false,
        message: error.response?.data?.message || 'Logout failed',
        errors: error.response?.data?.errors || ['Network error']
      };
    }
  },

  isAuthenticated(): boolean {
    return TokenService.isAuthenticated();
  },

  isAuthenticatedAndNotExpiringSoon(): boolean {
    return TokenService.isAuthenticatedAndNotExpiringSoon();
  },

  getCurrentUserFromStorage(): User | null {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch {
        return null;
      }
    }
    return null;
  },

  clearAuth(): void {
    TokenService.clearAuthData();
  }
};
