import apiClient from './apiClient';

export const feeApi = {
  getFeesByStudent: async (studentId) => {
    const response = await apiClient.get(`/fees/student/${studentId}`);
    return response.data;
  },

  getAllFees: async () => {
    const response = await apiClient.get('/fees');
    return response.data;
  },

  createFee: async (feeData) => {
    const response = await apiClient.post('/fees', feeData);
    return response.data;
  },

  recordPayment: async (feeId, paymentData) => {
    const response = await apiClient.post(`/fees/${feeId}/payments`, paymentData);
    return response.data;
  },

  getReceipt: async (receiptNumber) => {
    const response = await apiClient.get(`/fees/receipts/${receiptNumber}`);
    return response.data;
  },

  triggerReminders: async () => {
    const response = await apiClient.post('/fees/trigger-reminders');
    return response.data;
  },
};
