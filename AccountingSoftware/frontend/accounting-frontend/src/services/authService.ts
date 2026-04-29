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
import apiClient from './apiClient';

export const authService = {
  async login(credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> {
    try {
      const response = await apiClient.post<ApiResponse<LoginResponse>>('/auth/login', credentials);
      
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
      const response = await apiClient.post<ApiResponse<User>>('/auth/register', userData);
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
      const response = await apiClient.get<ApiResponse<User>>('/auth/me');
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
      const response = await apiClient.post<ApiResponse<LoginResponse>>('/auth/refresh-token', request);
      
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
      const response = await apiClient.post<ApiResponse<string>>('/auth/change-password', request);
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
      const response = await apiClient.put<ApiResponse<User>>('/auth/profile', request);
      
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
      const response = await apiClient.post<ApiResponse<string>>('/auth/logout');
      
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
