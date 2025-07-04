import { jwtDecode } from 'jwt-decode';

interface DecodedToken {
  exp: number;
  iat: number;
  sub: string;
  // Add other JWT claims as needed
}

export class TokenService {
  private static readonly TOKEN_KEY = 'accessToken';
  private static readonly REFRESH_TOKEN_KEY = 'refreshToken';
  private static readonly USER_KEY = 'user';
  
  // Buffer time in seconds to refresh token before it actually expires
  private static readonly REFRESH_BUFFER_TIME = 300; // 5 minutes

  /**
   * Check if a token is expired
   */
  static isTokenExpired(token: string): boolean {
    try {
      const decoded = jwtDecode<DecodedToken>(token);
      const currentTime = Date.now() / 1000;
      return decoded.exp < currentTime;
    } catch (error) {
      console.warn('Error decoding token:', error);
      return true; // Consider invalid tokens as expired
    }
  }

  /**
   * Check if a token is close to expiring (within buffer time)
   */
  static isTokenExpiringSoon(token: string): boolean {
    try {
      const decoded = jwtDecode<DecodedToken>(token);
      const currentTime = Date.now() / 1000;
      return decoded.exp < (currentTime + this.REFRESH_BUFFER_TIME);
    } catch (error) {
      console.warn('Error decoding token:', error);
      return true;
    }
  }

  /**
   * Get the expiration time of a token
   */
  static getTokenExpiration(token: string): Date | null {
    try {
      const decoded = jwtDecode<DecodedToken>(token);
      return new Date(decoded.exp * 1000);
    } catch (error) {
      console.warn('Error decoding token:', error);
      return null;
    }
  }

  /**
   * Get the current access token from localStorage
   */
  static getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  /**
   * Get the current refresh token from localStorage
   */
  static getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  /**
   * Check if user is authenticated with valid (non-expired) token
   */
  static isAuthenticated(): boolean {
    const token = this.getAccessToken();
    const user = localStorage.getItem(this.USER_KEY);
    
    if (!token || !user) {
      return false;
    }

    return !this.isTokenExpired(token);
  }

  /**
   * Check if user is authenticated and token is not expiring soon
   */
  static isAuthenticatedAndNotExpiringSoon(): boolean {
    const token = this.getAccessToken();
    const user = localStorage.getItem(this.USER_KEY);
    
    if (!token || !user) {
      return false;
    }

    return !this.isTokenExpiringSoon(token);
  }

  /**
   * Clear all authentication data
   */
  static clearAuthData(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  /**
   * Store authentication data
   */
  static storeAuthData(accessToken: string, refreshToken: string, user: any): void {
    localStorage.setItem(this.TOKEN_KEY, accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  /**
   * Get time until token expires in milliseconds
   */
  static getTimeUntilExpiry(token: string): number | null {
    try {
      const decoded = jwtDecode<DecodedToken>(token);
      const currentTime = Date.now() / 1000;
      const timeUntilExpiry = (decoded.exp - currentTime) * 1000;
      return Math.max(0, timeUntilExpiry);
    } catch (error) {
      console.warn('Error calculating time until expiry:', error);
      return null;
    }
  }
}
