import React, { useState, useCallback } from 'react';
import { Outlet } from 'react-router-dom';
import { Navbar } from './Navbar';
import { Sidebar } from './Sidebar';

export const AppLayout = () => {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const toggleSidebar = useCallback(() => setSidebarOpen((prev) => !prev), []);
  const closeSidebar  = useCallback(() => setSidebarOpen(false), []);

  return (
    <div className="erp-app">
      <Navbar onHamburgerClick={toggleSidebar} />
      <div className="erp-body">
        {/* Backdrop — only rendered on mobile when sidebar is open */}
        {sidebarOpen && (
          <div
            className="sidebar-backdrop"
            onClick={closeSidebar}
            aria-hidden="true"
          />
        )}
        <Sidebar isOpen={sidebarOpen} onClose={closeSidebar} />
        <main className="erp-main-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default AppLayout;
