import React from 'react';
import { useAuth } from '../../context/AuthContext';
import { LogOut, User, ShieldCheck } from 'lucide-react';

export const Navbar = () => {
  const { user, logout, isAdmin } = useAuth();

  return (
    <header className="erp-navbar">
      <div className="navbar-brand">
        <span className="brand-badge">ERP</span>
        <span className="brand-title">Student Management Mini-ERP</span>
      </div>

      <div className="navbar-actions">
        <div className="user-profile">
          <div className="user-avatar">
            <User size={18} />
          </div>
          <div className="user-info">
            <span className="user-name">{user?.username || 'User'}</span>
            <span className={`user-role-badge ${isAdmin ? 'role-admin' : 'role-staff'}`}>
              <ShieldCheck size={12} />
              {user?.role || 'Staff'}
            </span>
          </div>
        </div>

        <button onClick={logout} className="btn-logout" title="Sign Out">
          <LogOut size={16} />
          <span>Logout</span>
        </button>
      </div>
    </header>
  );
};
