import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/layout/ProtectedRoute';
import { AppLayout } from './components/layout/AppLayout';

import { Login } from './pages/Login';
import { Register } from './pages/Register';
import { ForgotPassword } from './pages/ForgotPassword';
import { ResetPassword } from './pages/ResetPassword';
import { VerifyEmail } from './pages/VerifyEmail';
import { Dashboard } from './pages/Dashboard';
import { StudentList } from './pages/Students/StudentList';
import { StudentCreate } from './pages/Students/StudentCreate';
import { StudentEdit } from './pages/Students/StudentEdit';
import { StudentDetail } from './pages/Students/StudentDetail';
import { FeeList } from './pages/Fees/FeeList';

export const App = () => {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/signup" element={<Register />} />
          <Route path="/register" element={<Navigate to="/signup" replace />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />
          <Route path="/reset-password" element={<ResetPassword />} />
          <Route path="/verify-email" element={<VerifyEmail />} />

          <Route
            element={
              <ProtectedRoute>
                <AppLayout />
              </ProtectedRoute>
            }
          >
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/students" element={<StudentList />} />
            <Route
              path="/students/new"
              element={
                <ProtectedRoute requireAdmin={true}>
                  <StudentCreate />
                </ProtectedRoute>
              }
            />
            <Route path="/students/:id" element={<StudentDetail />} />
            <Route
              path="/students/:id/edit"
              element={
                <ProtectedRoute requireAdmin={true}>
                  <StudentEdit />
                </ProtectedRoute>
              }
            />
            <Route path="/fees" element={<FeeList />} />
          </Route>

          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
};

export default App;
