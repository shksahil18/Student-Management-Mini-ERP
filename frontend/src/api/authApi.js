import apiClient from './apiClient';

export const authApi = {
  login: async (credentials) => {
    const response = await apiClient.post('/auth/login', credentials);
    return response.data;
  },

  register: async (registrationData) => {
    const response = await apiClient.post('/auth/register', registrationData);
    return response.data;
  },

  getCurrentUser: async () => {
    const response = await apiClient.get('/auth/me');
    return response.data;
  },

  forgotPassword: async (email) => {
    const response = await apiClient.post('/auth/forgot-password', { email });
    return response.data;
  },

  resetPassword: async ({ token, newPassword, confirmPassword }) => {
    const response = await apiClient.post('/auth/reset-password', {
      token,
      newPassword,
      confirmPassword,
    });
    return response.data;
  },

  verifyEmail: async (token) => {
    const response = await apiClient.post('/auth/verify-email', { token });
    return response.data;
  },

  resendVerification: async (email) => {
    const response = await apiClient.post('/auth/resend-verification', { email });
    return response.data;
  },
};
