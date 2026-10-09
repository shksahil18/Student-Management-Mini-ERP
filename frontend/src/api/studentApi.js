import apiClient from './apiClient';

export const studentApi = {
  getStudents: async (params = {}) => {
    const response = await apiClient.get('/students', { params });
    return response.data;
  },

  getStudentById: async (id) => {
    const response = await apiClient.get(`/students/${id}`);
    return response.data;
  },

  createStudent: async (studentData) => {
    const response = await apiClient.post('/students', studentData);
    return response.data;
  },

  updateStudent: async (id, studentData) => {
    const response = await apiClient.put(`/students/${id}`, studentData);
    return response.data;
  },

  toggleStatus: async (id) => {
    const response = await apiClient.patch(`/students/${id}/toggle-status`);
    return response.data;
  },

  exportCsv: async () => {
    const response = await apiClient.get('/students/export', {
      responseType: 'blob',
    });
    return response.data;
  },
};
