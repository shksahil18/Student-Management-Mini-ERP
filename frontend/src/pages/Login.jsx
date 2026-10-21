import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import {
  GraduationCap,
  Lock,
  User,
  AlertCircle,
  CheckCircle,
  Eye,
  EyeOff,
  ArrowRight,
  ShieldCheck,
  CreditCard,
  Users,
} from 'lucide-react';

export const Login = () => {
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('Admin@123');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const from = location.state?.from?.pathname || '/dashboard';

  useEffect(() => {
    if (location.state?.successMessage) {
      setSuccessMsg(location.state.successMessage);
    }
    if (location.state?.prefilledUsername) {
      setUsername(location.state.prefilledUsername);
      setPassword('');
    }
  }, [location.state]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccessMsg('');

    if (!username.trim() || !password) {
      setError('Please provide both username and password.');
      return;
    }

    setIsSubmitting(true);
    const result = await login(username, password);
    setIsSubmitting(false);

    if (result.success) {
      navigate(from, { replace: true });
    } else {
      setError(result.message || 'Login failed. Please check credentials.');
    }
  };

  const setCredentials = (u, p) => {
    setUsername(u);
    setPassword(p);
    setError('');
    setSuccessMsg('');
  };

  return (
    <div className="known-auth-page">
      <div className="known-auth-split-card">
        {/* Left Brand Panel (Known Education Showcase) */}
        <div className="known-brand-panel">
          <div>
            <div className="known-brand-badge">
              <GraduationCap size={16} />
              <span>Known Education ERP</span>
            </div>
            <h1 className="known-brand-title">Welcome to Student Management Portal</h1>
            <p className="known-brand-desc">
              A comprehensive administrative solution for managing admissions, tuition ledgers, payment installments, and real-time executive analytics.
            </p>

            <div className="known-brand-features">
              <div className="known-brand-feature-item">
                <div className="known-feature-icon-bubble">
                  <Users size={15} />
                </div>
                <div className="known-feature-text">
                  <h4>Student Lifecycle</h4>
                  <p>Admissions, directory search, and demographic profiles.</p>
                </div>
              </div>

              <div className="known-brand-feature-item">
                <div className="known-feature-icon-bubble">
                  <CreditCard size={15} />
                </div>
                <div className="known-feature-text">
                  <h4>Fee & Installment Ledger</h4>
                  <p>Real-time balances, overpayment guards, and receipts.</p>
                </div>
              </div>

              <div className="known-brand-feature-item">
                <div className="known-feature-icon-bubble">
                  <ShieldCheck size={15} />
                </div>
                <div className="known-feature-text">
                  <h4>Role-Based Access</h4>
                  <p>Dedicated Admin and Staff operational permissions.</p>
                </div>
              </div>
            </div>
          </div>

          <div className="known-brand-footer">
            <span>© 2026 Student Management Mini-ERP • Known Reference Design</span>
          </div>
        </div>

        {/* Right Form Panel */}
        <div className="known-form-panel">
          <div className="known-form-header">
            <h2>Sign In to ERP</h2>
            <p>Enter your authorized credentials to access your dashboard</p>
          </div>

          {successMsg && (
            <div className="known-alert-success">
              <CheckCircle size={18} />
              <span>{successMsg}</span>
            </div>
          )}

          {error && (
            <div className="known-alert-error">
              <AlertCircle size={18} />
              <span>{error}</span>
            </div>
          )}

          <form onSubmit={handleSubmit} className="known-form">
            <div className="known-form-group">
              <label htmlFor="username">Username</label>
              <div className="known-input-wrapper">
                <User size={18} className="known-input-icon" />
                <input
                  id="username"
                  type="text"
                  className="known-input"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  placeholder="Enter username"
                  required
                  autoFocus
                />
              </div>
            </div>

            <div className="known-form-group">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
                <label htmlFor="password" style={{ margin: 0 }}>Password</label>
                <Link to="/forgot-password" className="known-auth-link" style={{ fontSize: '13px', fontWeight: 500 }}>
                  Forgot password?
                </Link>
              </div>
              <div className="known-input-wrapper">
                <Lock size={18} className="known-input-icon" />
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  className="known-input"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Enter password"
                  required
                />
                <button
                  type="button"
                  className="known-password-toggle"
                  onClick={() => setShowPassword((prev) => !prev)}
                  title={showPassword ? 'Hide password' : 'Show password'}
                  tabIndex={-1}
                >
                  {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>

            <button
              type="submit"
              className="known-btn-submit"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <span>Authenticating...</span>
              ) : (
                <>
                  <span>Sign In</span>
                  <ArrowRight size={17} />
                </>
              )}
            </button>
          </form>

          {/* Quick Demo Credentials Box - Strictly Preserved */}
          <div className="known-demo-box">
            <p className="known-demo-title">Quick Demo Logins (Click to autofill):</p>
            <div className="known-demo-buttons">
              <button
                type="button"
                className={`known-demo-pill ${username === 'admin' ? 'active' : ''}`}
                onClick={() => setCredentials('admin', 'Admin@123')}
              >
                👑 Admin (Full Access)
              </button>
              <button
                type="button"
                className={`known-demo-pill ${username === 'staff' ? 'active' : ''}`}
                onClick={() => setCredentials('staff', 'Staff@123')}
              >
                👤 Staff (View & Pay)
              </button>
            </div>
          </div>

          <div className="known-auth-footer">
            <span>Don't have an account?</span>
            <Link to="/signup" className="known-auth-link">
              Create Staff Account
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
