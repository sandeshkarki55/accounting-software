import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { User } from '../../types/auth';
import { authService } from '../../services/authService';
import { TokenService } from '../../services/tokenService';
import TokenRefreshService from '../../services/tokenRefreshService';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string, rememberMe?: boolean) => Promise<{ success: boolean; message: string; errors?: string[] }>;
  logout: () => Promise<void>;
  register: (userData: {
    email: string;
    password: string;
    confirmPassword: string;
    firstName: string;
    lastName: string;
  }) => Promise<{ success: boolean; message: string; errors?: string[] }>;
  updateUser: (user: User) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Check if user is logged in on app start
    const initializeAuth = async () => {
      try {
        // Use TokenService for better token validation
        if (TokenService.isAuthenticated()) {
          const storedUser = authService.getCurrentUserFromStorage();
          if (storedUser) {
            setUser(storedUser);
            
            // Set up automatic token refresh
            const refreshService = TokenRefreshService.getInstance();
            refreshService.refreshTokenIfNeeded().catch(error => {
              console.warn('Initial token refresh failed:', error);
            });
          } else {
            // Try to get fresh user data from server
            try {
              const response = await authService.getCurrentUser();
              if (response.success && response.data) {
                setUser(response.data);
              } else {
                authService.clearAuth();
              }
            } catch (error) {
              console.warn('Failed to get current user:', error);
              authService.clearAuth();
            }
          }
        }
      } catch (error) {
        console.error('Error initializing auth:', error);
        authService.clearAuth();
      } finally {
        setIsLoading(false);
      }
    };

    initializeAuth();

    // Set up periodic token refresh check
    const tokenRefreshInterval = setInterval(async () => {
      if (TokenService.isAuthenticated()) {
        try {
          const refreshService = TokenRefreshService.getInstance();
          await refreshService.refreshTokenIfNeeded();
        } catch (error) {
          console.warn('Periodic token refresh failed:', error);
          // If refresh fails, logout user
          setUser(null);
          authService.clearAuth();
        }
      }
    }, 60000); // Check every minute

    return () => clearInterval(tokenRefreshInterval);
  }, []);

  const login = async (email: string, password: string, rememberMe = false) => {
    try {
      setIsLoading(true);
      const response = await authService.login({ email, password, rememberMe });
      
      if (response.success && response.data) {
        setUser(response.data.user);
        return { success: true, message: response.message };
      } else {
        return { 
          success: false, 
          message: response.message,
          errors: response.errors 
        };
      }
    } catch (error) {
      console.error('Login error:', error);
      return { 
        success: false, 
        message: 'An unexpected error occurred during login.',
        errors: ['Network error'] 
      };
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (userData: {
    email: string;
    password: string;
    confirmPassword: string;
    firstName: string;
    lastName: string;
  }) => {
    try {
      setIsLoading(true);
      const response = await authService.register(userData);
      
      return {
        success: response.success,
        message: response.message,
        errors: response.errors
      };
    } catch (error) {
      console.error('Registration error:', error);
      return {
        success: false,
        message: 'An unexpected error occurred during registration.',
        errors: ['Network error']
      };
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    try {
      setIsLoading(true);
      await authService.logout();
      setUser(null);
    } catch (error) {
      console.error('Logout error:', error);
      // Clear user state even if logout request fails
      setUser(null);
      authService.clearAuth();
    } finally {
      setIsLoading(false);
    }
  };

  const updateUser = (updatedUser: User) => {
    setUser(updatedUser);
    localStorage.setItem('user', JSON.stringify(updatedUser));
  };

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user && TokenService.isAuthenticated(),
    isLoading,
    login,
    logout,
    register,
    updateUser,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
