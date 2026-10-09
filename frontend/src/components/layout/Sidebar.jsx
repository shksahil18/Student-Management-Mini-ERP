import React from 'react';
import { NavLink } from 'react-router-dom';
import { LayoutDashboard, Users, CreditCard, UserPlus } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

export const Sidebar = () => {
  const { isAdmin } = useAuth();

  return (
    <aside className="erp-sidebar">
      <nav className="sidebar-nav">
        <NavLink
          to="/dashboard"
          className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
        >
          <LayoutDashboard size={20} />
          <span>Dashboard</span>
        </NavLink>

        <NavLink
          to="/students"
          className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
        >
          <Users size={20} />
          <span>Students</span>
        </NavLink>

        {isAdmin && (
          <NavLink
            to="/students/new"
            className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
          >
            <UserPlus size={20} />
            <span>Admission</span>
          </NavLink>
        )}

        <NavLink
          to="/fees"
          className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
        >
          <CreditCard size={20} />
          <span>Fee Management</span>
        </NavLink>
      </nav>

      <div className="sidebar-footer">
        <div className="system-pill">
          <span className="status-dot"></span>
          <span>SQL Server Active</span>
        </div>
      </div>
    </aside>
  );
};
