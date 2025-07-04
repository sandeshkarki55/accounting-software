import axios from 'axios';
import { TokenService } from './tokenService';
import { ApiResponse, LoginResponse } from '../types/auth';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7120/api';

class TokenRefreshService {
  private static instance: TokenRefreshService;
  private isRefreshing = false;
  private failedQueue: Array<{
    resolve: (value: string) => void;
    reject: (reason: any) => void;
  }> = [];

  static getInstance(): TokenRefreshService {
    if (!TokenRefreshService.instance) {
      TokenRefreshService.instance = new TokenRefreshService();
    }
    return TokenRefreshService.instance;
  }

  private processQueue(error: any, token: string | null = null): void {
    this.failedQueue.forEach(({ resolve, reject }) => {
      if (error) {
        reject(error);
      } else {
        resolve(token!);
      }
    });

    this.failedQueue = [];
  }

  async refreshToken(): Promise<string> {
    if (this.isRefreshing) {
      // If already refreshing, queue this request
      return new Promise((resolve, reject) => {
        this.failedQueue.push({ resolve, reject });
      });
    }

    this.isRefreshing = true;

    try {
      const refreshToken = TokenService.getRefreshToken();
      const accessToken = TokenService.getAccessToken();

      if (!refreshToken || !accessToken) {
        throw new Error('No refresh token available');
      }

      // Create a separate axios instance to avoid interceptor loops
      const refreshAxios = axios.create({
        baseURL: API_BASE_URL,
        headers: { 'Content-Type': 'application/json' }
      });

      const response = await refreshAxios.post<ApiResponse<LoginResponse>>('/auth/refresh-token', {
        accessToken,
        refreshToken,
      });

      if (response.data.success && response.data.data) {
        const { accessToken: newAccessToken, refreshToken: newRefreshToken, user } = response.data.data;
        TokenService.storeAuthData(newAccessToken, newRefreshToken, user);
        
        this.processQueue(null, newAccessToken);
        return newAccessToken;
      } else {
        throw new Error('Token refresh failed');
      }
    } catch (error) {
      console.error('Token refresh failed:', error);
      TokenService.clearAuthData();
      this.processQueue(error);
      
      // Redirect to login page
      window.location.href = '/login';
      throw error;
    } finally {
      this.isRefreshing = false;
    }
  }

  /**
   * Check if we should attempt to refresh the token proactively
   */
  shouldRefreshToken(): boolean {
    const token = TokenService.getAccessToken();
    const refreshToken = TokenService.getRefreshToken();
    
    if (!token || !refreshToken) {
      return false;
    }

    // Don't refresh if token is already expired (let the 401 handler deal with it)
    if (TokenService.isTokenExpired(token)) {
      return false;
    }

    // Refresh if token is expiring soon
    return TokenService.isTokenExpiringSoon(token);
  }

  /**
   * Attempt to refresh token proactively if it's expiring soon
   */
  async refreshTokenIfNeeded(): Promise<void> {
    if (this.shouldRefreshToken() && !this.isRefreshing) {
      try {
        await this.refreshToken();
        console.log('Token refreshed proactively');
      } catch (error) {
        console.warn('Proactive token refresh failed:', error);
      }
    }
  }
}

export default TokenRefreshService;
