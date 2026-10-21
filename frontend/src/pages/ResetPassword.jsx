import React, { useState } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import { authApi } from '../api/authApi';
import {
  GraduationCap,
  Lock,
  AlertCircle,
  CheckCircle,
  Eye,
  EyeOff,
  ArrowRight,
  ArrowLeft,
  KeyRound,
  ShieldCheck,
} from 'lucide-react';

export const ResetPassword = () => {
  const [searchParams] = useSearchParams();
  const tokenFromUrl = searchParams.get('token') || '';
  const navigate = useNavigate();

  const [token, setToken] = useState(tokenFromUrl);
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccessMsg('');

    if (!token.trim()) {
      setError('A valid reset token is required. Please check your reset link or request a new one.');
      return;
    }

    if (newPassword.length < 6) {
      setError('Password must be at least 6 characters long.');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    try {
      setIsSubmitting(true);
      const res = await authApi.resetPassword({
        token: token.trim(),
        newPassword,
        confirmPassword,
      });

      if (res.success) {
        setSuccessMsg(res.message || 'Your password has been reset successfully. You may now sign in with your new credentials.');
        setTimeout(() => {
          navigate('/login', {
            state: { successMessage: 'Password reset successful. Please sign in with your new password.' },
          });
        }, 2500);
      } else {
        setError(res.message || 'Failed to reset password. The link may have expired or already been used.');
      }
    } catch (err) {
      setError(
        err.response?.data?.message ||
          'Failed to reset password. The reset link is either invalid, already used, or expired.'
      );
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
            <h1 className="known-brand-title">Create New Password</h1>
            <p className="known-brand-desc">
              Ensure your new password complies with institutional security guidelines to protect administrative records and tuition data.
            </p>

            <div className="known-brand-features">
              <div className="known-brand-feature-item">
                <div className="known-feature-icon-bubble">
                  <KeyRound size={15} />
                </div>
                <div className="known-feature-text">
                  <h4>Cryptographic Security</h4>
                  <p>Single-use security token verification with SHA-256 integrity.</p>
                </div>
              </div>

              <div className="known-brand-feature-item">
                <div className="known-feature-icon-bubble">
                  <ShieldCheck size={15} />
                </div>
                <div className="known-feature-text">
                  <h4>Account Protection</h4>
                  <p>Immediate invalidation of prior sessions and active tokens.</p>
                </div>
              </div>
            </div>
          </div>

          <div className="known-brand-footer">
            <span>© {new Date().getFullYear()} Student Management Mini-ERP • Secure Authentication</span>
          </div>
        </div>

        {/* Right Form Panel */}
        <div className="known-form-panel">
          <div className="known-form-header">
            <h2>Reset Your Password</h2>
            <p>Set a strong, secure password for your account</p>
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

          {!tokenFromUrl && (
            <div className="known-form-group">
              <label htmlFor="token">Security Token</label>
              <div className="known-input-wrapper">
                <KeyRound size={18} className="known-input-icon" />
                <input
                  id="token"
                  type="text"
                  className="known-input"
                  value={token}
                  onChange={(e) => setToken(e.target.value)}
                  placeholder="Paste reset token here"
                  required
                />
              </div>
            </div>
          )}

          <form onSubmit={handleSubmit} className="known-form">
            <div className="known-form-group">
              <label htmlFor="newPassword">New Password</label>
              <div className="known-input-wrapper">
                <Lock size={18} className="known-input-icon" />
                <input
                  id="newPassword"
                  type={showPassword ? 'text' : 'password'}
                  className="known-input"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  placeholder="Enter new password (min. 6 characters)"
                  required
                  autoFocus
                />
                <button
                  type="button"
                  className="known-password-toggle"
                  onClick={() => setShowPassword((prev) => !prev)}
                  tabIndex={-1}
                >
                  {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>

            <div className="known-form-group">
              <label htmlFor="confirmPassword">Confirm New Password</label>
              <div className="known-input-wrapper">
                <Lock size={18} className="known-input-icon" />
                <input
                  id="confirmPassword"
                  type={showConfirmPassword ? 'text' : 'password'}
                  className="known-input"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  placeholder="Re-enter new password"
                  required
                />
                <button
                  type="button"
                  className="known-password-toggle"
                  onClick={() => setShowConfirmPassword((prev) => !prev)}
                  tabIndex={-1}
                >
                  {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>

            <button
              type="submit"
              className="known-btn-submit"
              disabled={isSubmitting || !!successMsg}
            >
              {isSubmitting ? (
                <span>Resetting Password...</span>
              ) : (
                <>
                  <span>Save New Password</span>
                  <ArrowRight size={17} />
                </>
              )}
            </button>
          </form>

          <div className="known-auth-footer" style={{ marginTop: '24px' }}>
            <Link to="/login" className="known-auth-link" style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
              <ArrowLeft size={16} />
              <span>Back to Sign In</span>
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ResetPassword;

