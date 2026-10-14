import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { authApi } from '../api/authApi';
import {
  GraduationCap,
  User,
  Mail,
  Lock,
  Eye,
  EyeOff,
  CheckCircle,
  AlertCircle,
  ArrowRight,
  ShieldCheck,
  CreditCard,
  Users,
} from 'lucide-react';

export const Register = () => {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
  });

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  const navigate = useNavigate();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (error) setError('');
  };

  const validateForm = () => {
    const { username, email, password, confirmPassword } = formData;

    if (!username.trim() || !email.trim() || !password || !confirmPassword) {
      setError('All fields are required.');
      return false;
    }

    if (username.trim().length < 3) {
      setError('Username must be at least 3 characters long.');
      return false;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email.trim())) {
      setError('Please provide a valid email address.');
      return false;
    }

    if (password.length < 6) {
      setError('Password must be at least 6 characters long.');
      return false;
    }

    if (password !== confirmPassword) {
      setError('Password and password confirmation do not match.');
      return false;
    }

    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!validateForm()) return;

    setIsSubmitting(true);
    try {
      const response = await authApi.register({
        username: formData.username.trim(),
        email: formData.email.trim(),
        password: formData.password,
        confirmPassword: formData.confirmPassword,
      });

      if (response.success) {
        navigate('/login', {
          replace: true,
          state: {
            successMessage: 'Account created successfully! You can now sign in with your credentials.',
            prefilledUsername: formData.username.trim(),
          },
        });
      } else {
        setError(response.message || 'Registration failed. Please try again.');
      }
    } catch (err) {
      const msg = err.response?.data?.message || err.response?.data?.errors?.[0] || 'Registration failed. Username or email may already be in use.';
      setError(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="known-auth-page">
      <div className="known-auth-split-card">
        {/* Left Brand Panel */}
        <div className="known-brand-panel">
          <div>
            <div className="known-brand-badge">
              <GraduationCap size={16} />
              <span>Known Education ERP</span>
            </div>
            <h1 className="known-brand-title">Join the Student Administration Portal</h1>
            <p className="known-brand-desc">
              Empowering educators and administrators with reliable student records, transparent fee collections, and instant digital receipts.
            </p>

            <div className="known-brand-features">
              <div className="known-brand-feature-item">
                <div className="known-feature-icon-bubble">
                  <Users size={15} />
                </div>
                <div className="known-feature-text">
                  <h4>Full Student Lifecycle</h4>
                  <p>Admissions, directory search, and demographic profiles.</p>
                </div>
              </div>

              <div className="known-brand-feature-item">
                <div className="known-feature-icon-bubble">
                  <CreditCard size={15} />
                </div>
                <div className="known-feature-text">
                  <h4>ACID-Safe Fee Ledger</h4>
                  <p>Partial payments, balance tracking, and overpayment guards.</p>
                </div>
              </div>

              <div className="known-brand-feature-item">
                <div className="known-feature-icon-bubble">
                  <ShieldCheck size={15} />
                </div>
                <div className="known-feature-text">
                  <h4>Verified Institutional Access</h4>
                  <p>Role-protected routes and secure BCrypt authentication.</p>
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
            <h2>Create Staff Account</h2>
            <p>Register below to access student directory and fee collections</p>
          </div>

          {error && (
            <div className="known-alert-error">
              <AlertCircle size={18} />
              <span>{error}</span>
            </div>
          )}

          <form onSubmit={handleSubmit} className="known-form" noValidate>
            <div className="known-form-group">
              <label htmlFor="reg-username">Username</label>
              <div className="known-input-wrapper">
                <User size={18} className="known-input-icon" />
                <input
                  id="reg-username"
                  name="username"
                  type="text"
                  className="known-input"
                  placeholder="e.g. jsmith"
                  value={formData.username}
                  onChange={handleChange}
                  required
                  autoFocus
                />
              </div>
            </div>

            <div className="known-form-group">
              <label htmlFor="reg-email">Email Address</label>
              <div className="known-input-wrapper">
                <Mail size={18} className="known-input-icon" />
                <input
                  id="reg-email"
                  name="email"
                  type="email"
                  className="known-input"
                  placeholder="e.g. jsmith@institution.edu"
                  value={formData.email}
                  onChange={handleChange}
                  required
                />
              </div>
            </div>

            <div className="known-form-group">
              <label htmlFor="reg-password">Password</label>
              <div className="known-input-wrapper">
                <Lock size={18} className="known-input-icon" />
                <input
                  id="reg-password"
                  name="password"
                  type={showPassword ? 'text' : 'password'}
                  className="known-input"
                  placeholder="At least 6 characters"
                  value={formData.password}
                  onChange={handleChange}
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

            <div className="known-form-group">
              <label htmlFor="reg-confirm-password">Confirm Password</label>
              <div className="known-input-wrapper">
                <ShieldCheck size={18} className="known-input-icon" />
                <input
                  id="reg-confirm-password"
                  name="confirmPassword"
                  type={showConfirmPassword ? 'text' : 'password'}
                  className="known-input"
                  placeholder="Re-enter your password"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  required
                />
                <button
                  type="button"
                  className="known-password-toggle"
                  onClick={() => setShowConfirmPassword((prev) => !prev)}
                  title={showConfirmPassword ? 'Hide password' : 'Show password'}
                  tabIndex={-1}
                >
                  {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>

            <button
              type="submit"
              className="known-btn-submit"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <span>Creating Account...</span>
              ) : (
                <>
                  <span>Create Account</span>
                  <ArrowRight size={17} />
                </>
              )}
            </button>
          </form>

          <div className="known-auth-footer">
            <span>Already have an account?</span>
            <Link to="/login" className="known-auth-link">
              Sign In
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Register;
