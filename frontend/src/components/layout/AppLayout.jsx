import React from 'react';
import { Outlet } from 'react-router-dom';
import { Navbar } from './Navbar';
import { Sidebar } from './Sidebar';

export const AppLayout = () => {
  return (
    <div className="erp-app">
      <Navbar />
      <div className="erp-body">
        <Sidebar />
        <main className="erp-main-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default AppLayout;
