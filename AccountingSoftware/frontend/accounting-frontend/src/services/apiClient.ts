import axios from 'axios';
import { TokenService } from './tokenService';
import TokenRefreshService from './tokenRefreshService';

// Use environment variable for API base URL, with fallback for development
// Aspire automatically injects service URLs as environment variables
const getApiBaseUrl = (): string => {
  // In Aspire, the service reference creates an environment variable
  // Format: services__<service-name>__<endpoint>__0 (for HTTPS) or __1 (for HTTP)
  const aspireApiUrl = process.env.REACT_APP_services__accountingapi__https__0 || 
                      process.env.REACT_APP_services__accountingapi__http__0;
  
  // Debug logging for service discovery
  if (process.env.REACT_APP_DEBUG_SERVICE_DISCOVERY === 'true') {
    console.log('Service Discovery Debug:');
    console.log('Available environment variables:', Object.keys(process.env).filter(key => key.includes('services')));
    console.log('Aspire API URL:', aspireApiUrl);
  }
  
  if (aspireApiUrl) {
    console.log(`Using Aspire service discovery: ${aspireApiUrl}/api`);
    return `${aspireApiUrl}/api`;
  }
  
  // Fallback for local development without Aspire
  const fallbackUrl = process.env.REACT_APP_API_BASE_URL || 'https://localhost:7127/api';
  console.log(`Using fallback API URL: ${fallbackUrl}`);
  return fallbackUrl;
};

const API_BASE_URL = getApiBaseUrl();

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  // Add timeout to handle connection issues
  timeout: 10000,
});

// Add request interceptor for authentication and debugging
apiClient.interceptors.request.use(
  async (config) => {
    // Add Bearer token to requests
    const token = TokenService.getAccessToken();
    if (token && !TokenService.isTokenExpired(token)) {
      config.headers.Authorization = `Bearer ${token}`;
      
      // Try to refresh token proactively if it's expiring soon
      const refreshService = TokenRefreshService.getInstance();
      try {
        await refreshService.refreshTokenIfNeeded();
        // Get the potentially updated token after refresh
        const updatedToken = TokenService.getAccessToken();
        if (updatedToken && updatedToken !== token) {
          config.headers.Authorization = `Bearer ${updatedToken}`;
        }
      } catch (error) {
        console.warn('Proactive token refresh failed in request interceptor:', error);
      }
    }
    
    console.log(`API Request: ${config.method?.toUpperCase()} ${config.baseURL}${config.url}`);
    return config;
  },
  (error) => {
    console.error('API Request Error:', error);
    return Promise.reject(error);
  }
);

// Add response interceptor for error handling and token refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Handle 401 errors (unauthorized) with token refresh
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshService = TokenRefreshService.getInstance();
        const newAccessToken = await refreshService.refreshToken();

        // Retry the original request with new token
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        // Refresh failed, user will be redirected to login by the refresh service
        console.error('Token refresh failed in response interceptor:', refreshError);
        return Promise.reject(refreshError);
      }
    }

    console.error('API Response Error:', error.response?.data || error.message);
    return Promise.reject(error);
  }
);

export default apiClient;
