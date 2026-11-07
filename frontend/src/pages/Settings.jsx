import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { profileApi } from '../api/profileApi';
import {
  Moon,
  Sun,
  Monitor,
  Bell,
  Mail,
  KeyRound,
  CheckCircle,
  AlertCircle,
  User,
  ExternalLink,
} from 'lucide-react';
import { Link } from 'react-router-dom';

export const Settings = () => {
  const { user } = useAuth();
  const { themePreference, setTheme } = useTheme();

  const [emailNotifications, setEmailNotifications] = useState(true);
  const [loading, setLoading] = useState(false);
  const [successMsg, setSuccessMsg] = useState('');
  const [errorMsg, setErrorMsg] = useState('');

  // Password change state
  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });
  const [pwdSubmitting, setPwdSubmitting] = useState(false);
  const [pwdSuccess, setPwdSuccess] = useState('');
  const [pwdError, setPwdError] = useState('');

  useEffect(() => {
    fetchProfileSettings();
  }, []);

  const fetchProfileSettings = async () => {
    try {
      const res = await profileApi.getProfile();
      if (res.success && res.data) {
        setEmailNotifications(res.data.emailNotificationsEnabled ?? true);
        if (res.data.themePreference) {
          setTheme(res.data.themePreference);
        }
      }
    } catch (e) {
      console.error('Failed to load profile settings', e);
    }
  };

  const handleThemeChange = async (newTheme) => {
    setTheme(newTheme);
    try {
      await profileApi.updateProfile({ themePreference: newTheme });
    } catch (e) {
      console.error('Failed to persist theme to backend', e);
    }
  };

  const handleToggleEmailNotifications = async () => {
    const nextVal = !emailNotifications;
    setEmailNotifications(nextVal);
    try {
      setLoading(true);
      const res = await profileApi.updateProfile({ emailNotificationsEnabled: nextVal });
      if (res.success) {
        setSuccessMsg(`Email notifications ${nextVal ? 'enabled' : 'disabled'} successfully.`);
        setTimeout(() => setSuccessMsg(''), 3000);
      }
    } catch (err) {
      setErrorMsg('Failed to update notification preferences.');
      setEmailNotifications(!nextVal);
      setTimeout(() => setErrorMsg(''), 3000);
    } finally {
      setLoading(false);
    }
  };

  const handlePasswordChange = async (e) => {
    e.preventDefault();
    setPwdError('');
    setPwdSuccess('');

    if (!passwordData.currentPassword) {
      setPwdError('Current password is required.');
      return;
    }

    if (passwordData.newPassword.length < 6) {
      setPwdError('New password must be at least 6 characters.');
      return;
    }

    if (passwordData.newPassword !== passwordData.confirmPassword) {
      setPwdError('New password and confirmation do not match.');
      return;
    }

    try {
      setPwdSubmitting(true);
      const res = await profileApi.changePassword(passwordData);
      if (res.success) {
        setPwdSuccess('Your password has been changed successfully.');
        setPasswordData({ currentPassword: '', newPassword: '', confirmPassword: '' });
      } else {
        setPwdError(res.message || 'Failed to change password.');
      }
    } catch (err) {
      setPwdError(err.response?.data?.message || 'Failed to change password. Please check your current password.');
    } finally {
      setPwdSubmitting(false);
    }
  };

  return (
    <div className="student-list-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Application Settings</h1>
          <p className="page-description">Configure theme preferences, institutional notifications, and account credentials</p>
        </div>
      </div>

      {successMsg && (
        <div className="alert-success" style={{ marginBottom: '20px', display: 'flex', alignItems: 'center', gap: '8px', padding: '12px 16px', borderRadius: 'var(--radius-sm)', backgroundColor: '#dcfce7', color: '#15803d' }}>
          <CheckCircle size={18} />
          <span>{successMsg}</span>
        </div>
      )}

      {errorMsg && (
        <div className="alert-error" style={{ marginBottom: '20px' }}>
          <AlertCircle size={18} />
          <span>{errorMsg}</span>
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '24px', alignItems: 'start' }}>
        {/* Card 1: Theme & Appearance */}
        <div className="panel-card" style={{ padding: '24px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '16px' }}>
            <div className="btn-action btn-view" style={{ width: '36px', height: '36px' }}>
              <Sun size={18} />
            </div>
            <div>
              <h3 style={{ fontSize: '17px', fontWeight: '700' }}>Theme & Appearance</h3>
              <p style={{ fontSize: '13px', color: 'var(--text-muted)' }}>Customize the visual interface of the Mini-ERP</p>
            </div>
          </div>

          <p style={{ fontSize: '14px', marginBottom: '16px', color: 'var(--text-muted)' }}>
            Choose your preferred color theme. The theme is saved locally and synchronized across your institutional session.
          </p>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
            <label
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                padding: '12px 16px',
                borderRadius: 'var(--radius-sm)',
                border: '1px solid var(--border)',
                cursor: 'pointer',
                backgroundColor: themePreference === 'light' ? 'var(--primary-light)' : 'transparent',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <Sun size={18} color={themePreference === 'light' ? 'var(--primary)' : 'var(--text-muted)'} />
                <div>
                  <div style={{ fontWeight: '600', fontSize: '14px' }}>Light Mode</div>
                  <div style={{ fontSize: '12px', color: 'var(--text-dim)' }}>Standard bright view for well-lit environments</div>
                </div>
              </div>
              <input
                type="radio"
                name="theme"
                value="light"
                checked={themePreference === 'light'}
                onChange={() => handleThemeChange('light')}
              />
            </label>

            <label
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                padding: '12px 16px',
                borderRadius: 'var(--radius-sm)',
                border: '1px solid var(--border)',
                cursor: 'pointer',
                backgroundColor: themePreference === 'dark' ? 'var(--primary-light)' : 'transparent',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <Moon size={18} color={themePreference === 'dark' ? 'var(--primary)' : 'var(--text-muted)'} />
                <div>
                  <div style={{ fontWeight: '600', fontSize: '14px' }}>Dark Mode</div>
                  <div style={{ fontSize: '12px', color: 'var(--text-dim)' }}>High-contrast dark palette to reduce eye strain</div>
                </div>
              </div>
              <input
                type="radio"
                name="theme"
                value="dark"
                checked={themePreference === 'dark'}
                onChange={() => handleThemeChange('dark')}
              />
            </label>

            <label
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                padding: '12px 16px',
                borderRadius: 'var(--radius-sm)',
                border: '1px solid var(--border)',
                cursor: 'pointer',
                backgroundColor: themePreference === 'system' ? 'var(--primary-light)' : 'transparent',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                <Monitor size={18} color={themePreference === 'system' ? 'var(--primary)' : 'var(--text-muted)'} />
                <div>
                  <div style={{ fontWeight: '600', fontSize: '14px' }}>System Default</div>
                  <div style={{ fontSize: '12px', color: 'var(--text-dim)' }}>Automatically match your operating system theme</div>
                </div>
              </div>
              <input
                type="radio"
                name="theme"
                value="system"
                checked={themePreference === 'system'}
                onChange={() => handleThemeChange('system')}
              />
            </label>
          </div>
        </div>

        {/* Card 2: Notifications & Profile Shortcut */}
        <div className="panel-card" style={{ padding: '24px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '16px' }}>
            <div className="btn-action btn-view" style={{ width: '36px', height: '36px' }}>
              <Bell size={18} />
            </div>
            <div>
              <h3 style={{ fontSize: '17px', fontWeight: '700' }}>Notifications & Alerts</h3>
              <p style={{ fontSize: '13px', color: 'var(--text-muted)' }}>Institutional emails & reminder dispatches</p>
            </div>
          </div>

          <div style={{ marginBottom: '24px' }}>
            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                padding: '14px',
                borderRadius: 'var(--radius-sm)',
                border: '1px solid var(--border)',
                marginBottom: '16px',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                <Mail size={20} color="var(--primary)" />
                <div>
                  <div style={{ fontWeight: '600', fontSize: '14px' }}>Email Notifications</div>
                  <div style={{ fontSize: '12px', color: 'var(--text-muted)' }}>
                    Receive fee receipt confirmations, admission notifications, and system alerts
                  </div>
                </div>
              </div>
              <input
                type="checkbox"
                checked={emailNotifications}
                onChange={handleToggleEmailNotifications}
                disabled={loading}
                style={{ width: '18px', height: '18px', cursor: 'pointer' }}
              />
            </div>

            <div
              style={{
                padding: '16px',
                borderRadius: 'var(--radius-sm)',
                backgroundColor: 'var(--bg-main)',
                border: '1px solid var(--border)',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '8px' }}>
                <User size={18} color="var(--primary)" />
                <span style={{ fontWeight: '600', fontSize: '14px' }}>User Profile & Photo</span>
              </div>
              <p style={{ fontSize: '13px', color: 'var(--text-muted)', marginBottom: '12px' }}>
                Manage your institutional designation, phone number, physical address, and avatar image.
              </p>
              <Link to="/profile" className="btn-secondary" style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
                <span>Edit Profile</span>
                <ExternalLink size={14} />
              </Link>
            </div>
          </div>
        </div>

        {/* Card 3: Security & Password */}
        <div className="panel-card" style={{ gridColumn: '1 / -1', padding: '24px 28px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '16px' }}>
            <div className="btn-action btn-view" style={{ width: '36px', height: '36px' }}>
              <KeyRound size={18} />
            </div>
            <div>
              <h3 style={{ fontSize: '17px', fontWeight: '700' }}>Account Security & Password</h3>
              <p style={{ fontSize: '13px', color: 'var(--text-muted)' }}>Update your secret authentication credentials</p>
            </div>
          </div>

          {pwdSuccess && (
            <div className="alert-success" style={{ marginBottom: '16px', display: 'flex', alignItems: 'center', gap: '8px', padding: '12px 16px', borderRadius: 'var(--radius-sm)', backgroundColor: '#dcfce7', color: '#15803d' }}>
              <CheckCircle size={18} />
              <span>{pwdSuccess}</span>
            </div>
          )}

          {pwdError && (
            <div className="alert-error" style={{ marginBottom: '16px' }}>
              <AlertCircle size={18} />
              <span>{pwdError}</span>
            </div>
          )}

          <form onSubmit={handlePasswordChange} className="erp-form" style={{ maxWidth: '540px' }}>
            <div className="form-group">
              <label>Current Password</label>
              <input
                type="password"
                placeholder="Enter current password"
                value={passwordData.currentPassword}
                onChange={(e) => setPasswordData({ ...passwordData, currentPassword: e.target.value })}
                required
              />
            </div>

            <div className="form-row-2">
              <div className="form-group">
                <label>New Password</label>
                <input
                  type="password"
                  placeholder="Min 6 characters"
                  value={passwordData.newPassword}
                  onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label>Confirm New Password</label>
                <input
                  type="password"
                  placeholder="Repeat new password"
                  value={passwordData.confirmPassword}
                  onChange={(e) => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
                  required
                />
              </div>
            </div>

            <div className="form-actions" style={{ justifyContent: 'flex-start', borderTop: 'none', paddingLeft: 0 }}>
              <button type="submit" className="btn-primary" disabled={pwdSubmitting}>
                {pwdSubmitting ? 'Updating Password...' : 'Update Password'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};
