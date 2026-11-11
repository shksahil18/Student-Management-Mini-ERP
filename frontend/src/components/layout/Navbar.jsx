import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useTheme } from '../../context/ThemeContext';
import { LogOut, User, ShieldCheck, Sun, Moon, Menu } from 'lucide-react';

export const Navbar = ({ onHamburgerClick }) => {
  const { user, logout, isAdmin } = useAuth();
  const { isDark, toggleTheme } = useTheme();

  return (
    <header className="erp-navbar">
      <div className="navbar-brand">
        {/* Hamburger — only visible on ≤768 px */}
        <button
          className="hamburger-btn"
          onClick={onHamburgerClick}
          aria-label="Open navigation menu"
          type="button"
        >
          <Menu size={22} />
        </button>

        <span className="brand-badge">ERP</span>
        <span className="brand-title">Student Management Mini-ERP</span>
      </div>

      <div className="navbar-actions">
        {/* Quick Theme Toggle Button */}
        <button
          onClick={toggleTheme}
          className="quick-theme-btn"
          title={isDark ? "Switch to Light Mode" : "Switch to Dark Mode"}
          type="button"
        >
          {isDark ? <Sun size={18} /> : <Moon size={18} />}
        </button>

        <Link to="/profile" className="user-profile" title="View Profile">
          <div className="user-avatar">
            {user?.profilePhotoUrl ? (
              <img
                src={user.profilePhotoUrl.startsWith('http') ? user.profilePhotoUrl : `${import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || 'http://localhost:5000'}${user.profilePhotoUrl}`}
                alt="Avatar"
                style={{ width: '100%', height: '100%', borderRadius: '50%', objectFit: 'cover' }}
              />
            ) : (
              <User size={18} />
            )}
          </div>
          <div className="user-info">
            <span className="user-name">{user?.fullName || user?.username || 'User'}</span>
            <span className={`user-role-badge ${isAdmin ? 'role-admin' : 'role-staff'}`}>
              <ShieldCheck size={12} />
              {user?.role || 'Staff'}
            </span>
          </div>
        </Link>

        <button onClick={logout} className="btn-logout" title="Sign Out">
          <LogOut size={16} />
          <span className="btn-logout-text">Logout</span>
        </button>
      </div>
    </header>
  );
};
