import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { authApi } from '../api/authApi';
import {
  GraduationCap,
  Mail,
  AlertCircle,
  CheckCircle,
  ArrowRight,
  ArrowLeft,
  KeyRound,
  ShieldCheck,
  CreditCard,
  Users,
} from 'lucide-react';

export const ForgotPassword = () => {
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccessMsg('');

    if (!email.trim()) {
      setError('Please provide your registered email address.');
      return;
    }

    try {
      setIsSubmitting(true);
      const res = await authApi.forgotPassword(email.trim());
      if (res.success) {
        setSuccessMsg(res.message || 'If an account exists for this email, password reset instructions have been sent.');
        setEmail('');
      } else {
        setError(res.message || 'Request failed. Please try again.');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Unable to process your request. Please try again later.');
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
            <h1 className="known-brand-title">Secure Credential Recovery</h1>
            <p className="known-brand-desc">
              Institutional security safeguards your account with single-use, time-limited cryptographic reset tokens sent directly to your verified inbox.
            </p>

            <div className="known-brand-features">
              <div className="known-brand-feature-item">
                <div className="known-feature-icon-bubble">
                  <KeyRound size={15} />
                </div>
                <div className="known-feature-text">
                  <h4>Cryptographic Tokens</h4>
                  <p>SHA-256 protected, single-use, 60-minute expiration.</p>
                </div>
              </div>

              <div className="known-brand-feature-item">
                <div className="known-feature-icon-bubble">
                  <ShieldCheck size={15} />
                </div>
                <div className="known-feature-text">
                  <h4>Privacy Preserving</h4>
                  <p>Generic status responses prevent email enumeration attacks.</p>
                </div>
              </div>
            </div>
          </div>

          <div className="known-brand-footer">
            <span>© {new Date().getFullYear()} Student Management Mini-ERP • Secure Recovery</span>
          </div>
        </div>

        {/* Right Form Panel */}
        <div className="known-form-panel">
          <div className="known-form-header">
            <h2>Forgot Password</h2>
            <p>Enter your institutional email address to receive reset instructions</p>
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
              <label htmlFor="email">Registered Email</label>
              <div className="known-input-wrapper">
                <Mail size={18} className="known-input-icon" />
                <input
                  id="email"
                  type="email"
                  className="known-input"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="name@institution.edu"
                  required
                  autoFocus
                />
              </div>
            </div>

            <button
              type="submit"
              className="known-btn-submit"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <span>Sending Reset Link...</span>
              ) : (
                <>
                  <span>Send Reset Instructions</span>
                  <ArrowRight size={17} />
                </>
              )}
            </button>
          </form>

          <div className="known-auth-footer" style={{ marginTop: '24px' }}>
            <Link to="/login" className="known-auth-link" style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
              <ArrowLeft size={16} />
              <span>Return to Sign In</span>
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ForgotPassword;

