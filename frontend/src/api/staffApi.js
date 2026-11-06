import apiClient from './apiClient';

export const staffApi = {
  getStaff: async (params = {}) => {
    const response = await apiClient.get('/staff', { params });
    return response.data;
  },

  getDepartments: async () => {
    const response = await apiClient.get('/staff/departments');
    return response.data;
  },

  getStaffById: async (id) => {
    const response = await apiClient.get(`/staff/${id}`);
    return response.data;
  },

  createStaff: async (staffData) => {
    const response = await apiClient.post('/staff', staffData);
    return response.data;
  },

  updateStaff: async (id, staffData) => {
    const response = await apiClient.put(`/staff/${id}`, staffData);
    return response.data;
  },

  toggleStatus: async (id) => {
    const response = await apiClient.patch(`/staff/${id}/toggle-status`);
    return response.data;
  },
};
