import React from 'react';
import { NavLink } from 'react-router-dom';
import { LayoutDashboard, Users, CreditCard, UserPlus, UserCheck, Settings, User, X } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

export const Sidebar = ({ isOpen, onClose }) => {
  const { isAdmin } = useAuth();

  const handleLinkClick = () => {
    if (onClose) onClose();
  };

  return (
    <aside className={`erp-sidebar ${isOpen ? 'sidebar-open' : ''}`}>
      {/* Mobile drawer header with close button (only active <= 768px) */}
      <div className="sidebar-mobile-header">
        <div className="navbar-brand">
          <span className="brand-badge">ERP</span>
          <span className="brand-title" style={{ fontSize: '15px' }}>Navigation</span>
        </div>
        <button
          type="button"
          className="sidebar-close-btn"
          onClick={onClose}
          aria-label="Close navigation menu"
        >
          <X size={20} />
        </button>
      </div>

      <nav className="sidebar-nav">
        <NavLink
          to="/dashboard"
          onClick={handleLinkClick}
          className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
        >
          <LayoutDashboard size={20} />
          <span>Dashboard</span>
        </NavLink>

        <NavLink
          to="/students"
          onClick={handleLinkClick}
          className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
        >
          <Users size={20} />
          <span>Students</span>
        </NavLink>

        {isAdmin && (
          <NavLink
            to="/students/new"
            onClick={handleLinkClick}
            className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
          >
            <UserPlus size={20} />
            <span>Admission</span>
          </NavLink>
        )}

        <NavLink
          to="/fees"
          onClick={handleLinkClick}
          className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
        >
          <CreditCard size={20} />
          <span>Fee Management</span>
        </NavLink>

        <NavLink
          to="/staff"
          onClick={handleLinkClick}
          className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
        >
          <UserCheck size={20} />
          <span>Staff Members</span>
        </NavLink>

        <NavLink
          to="/profile"
          onClick={handleLinkClick}
          className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
        >
          <User size={20} />
          <span>My Profile</span>
        </NavLink>

        <NavLink
          to="/settings"
          onClick={handleLinkClick}
          className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
        >
          <Settings size={20} />
          <span>Settings</span>
        </NavLink>
      </nav>

      <div className="sidebar-footer">
        <div className="system-pill">
          <span className="status-dot"></span>
          <span>Enterprise ERP Active</span>
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;
