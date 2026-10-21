import React, { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { authApi } from '../api/authApi';
import {
  GraduationCap,
  Mail,
  AlertCircle,
  CheckCircle,
  ArrowRight,
  ShieldCheck,
  RefreshCw,
  Sparkles,
} from 'lucide-react';

export const VerifyEmail = () => {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') || '';

  const [status, setStatus] = useState('idle'); // 'idle' | 'verifying' | 'success' | 'error'
  const [message, setMessage] = useState('');
  const [resendEmail, setResendEmail] = useState('');
  const [resendLoading, setResendLoading] = useState(false);
  const [resendSuccess, setResendSuccess] = useState('');
  const [resendError, setResendError] = useState('');

  useEffect(() => {
    if (token) {
      verifyToken(token);
    } else {
      setStatus('error');
      setMessage('No verification token provided. If you need a verification link, please request one below.');
    }
  }, [token]);

  const verifyToken = async (tok) => {
    setStatus('verifying');
    setMessage('Validating your verification link with institutional directory...');
    try {
      const res = await authApi.verifyEmail(tok);
      if (res.success) {
        setStatus('success');
        setMessage(res.message || 'Your email address has been successfully verified! You have full access to your account.');
      } else {
        setStatus('error');
        setMessage(res.message || 'Verification link is invalid or has expired.');
      }
    } catch (err) {
      setStatus('error');
      setMessage(err.response?.data?.message || 'Verification token is invalid, expired, or already used.');
    }
  };

  const handleResend = async (e) => {
    e.preventDefault();
    setResendError('');
    setResendSuccess('');

    if (!resendEmail.trim()) {
      setResendError('Please enter your institutional email address.');
      return;
    }

    try {
      setResendLoading(true);
      const res = await authApi.resendVerification(resendEmail.trim());
      if (res.success) {
        setResendSuccess(res.message || 'If an unverified account exists for this email, a new verification link has been dispatched.');
        setResendEmail('');
      } else {
        setResendError(res.message || 'Unable to send verification email.');
      }
    } catch (err) {
      setResendError(err.response?.data?.message || 'Failed to send verification email. Please try again later.');
    } finally {
      setResendLoading(false);
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
            <h1 className="known-brand-title">Email Verification</h1>
            <p className="known-brand-desc">
              Verifying your email confirms your identity within the ERP network, authorizing administrative access and notification dispatch.
            </p>

            <div className="known-brand-features">
              <div className="known-brand-feature-item">
                <div className="known-feature-icon-bubble">
                  <ShieldCheck size={15} />
                </div>
                <div className="known-feature-text">
                  <h4>Identity Verification</h4>
                  <p>Ensures security notices & fee reminders reach authorized recipients.</p>
                </div>
              </div>

              <div className="known-brand-feature-item">
                <div className="known-feature-icon-bubble">
                  <Sparkles size={15} />
                </div>
                <div className="known-feature-text">
                  <h4>Uninterrupted Access</h4>
                  <p>Verified accounts experience uninterrupted session capabilities.</p>
                </div>
              </div>
            </div>
          </div>

          <div className="known-brand-footer">
            <span>© {new Date().getFullYear()} Student Management Mini-ERP • Email Verification</span>
          </div>
        </div>

        {/* Right Form Panel */}
        <div className="known-form-panel">
          <div className="known-form-header">
            <h2>Account Verification</h2>
            <p>Verification status and confirmation for your profile</p>
          </div>

          {status === 'verifying' && (
            <div style={{ textAlign: 'center', padding: '36px 12px' }}>
              <div style={{ display: 'inline-flex', alignItems: 'center', justifyContent: 'center', width: '56px', height: '56px', borderRadius: '50%', background: '#eef2ff', color: '#4f46e5', marginBottom: '16px' }}>
                <RefreshCw size={26} className="known-spin" />
              </div>
              <h3 style={{ fontSize: '18px', fontWeight: 600, color: '#111827', margin: '0 0 8px 0' }}>
                Verifying Email Token
              </h3>
              <p style={{ color: '#6b7280', fontSize: '14px', margin: 0 }}>
                Please wait a moment while we authenticate your verification link...
              </p>
            </div>
          )}

          {status === 'success' && (
            <div>
              <div className="known-alert-success" style={{ marginBottom: '24px' }}>
                <CheckCircle size={20} />
                <span style={{ fontSize: '14px', lineHeight: 1.5 }}>{message}</span>
              </div>

              <div style={{ textAlign: 'center', margin: '32px 0' }}>
                <Link to="/login" className="known-btn-submit" style={{ display: 'inline-flex', alignItems: 'center', justifyContent: 'center', textDecoration: 'none' }}>
                  <span>Proceed to Sign In</span>
                  <ArrowRight size={17} style={{ marginLeft: '8px' }} />
                </Link>
              </div>
            </div>
          )}

          {status === 'error' && (
            <div>
              <div className="known-alert-error" style={{ marginBottom: '24px' }}>
                <AlertCircle size={20} />
                <span style={{ fontSize: '14px', lineHeight: 1.5 }}>{message}</span>
              </div>

              {/* Resend Verification Form */}
              <div style={{ borderTop: '1px solid #e5e7eb', paddingTop: '20px', marginTop: '20px' }}>
                <h4 style={{ fontSize: '15px', fontWeight: 600, color: '#1f2937', marginBottom: '8px' }}>
                  Need a new verification link?
                </h4>
                <p style={{ fontSize: '13px', color: '#6b7280', marginBottom: '16px' }}>
                  Enter your registered institutional email below to receive a fresh verification link.
                </p>

                {resendSuccess && (
                  <div className="known-alert-success" style={{ marginBottom: '16px' }}>
                    <CheckCircle size={18} />
                    <span>{resendSuccess}</span>
                  </div>
                )}

                {resendError && (
                  <div className="known-alert-error" style={{ marginBottom: '16px' }}>
                    <AlertCircle size={18} />
                    <span>{resendError}</span>
                  </div>
                )}

                <form onSubmit={handleResend} className="known-form">
                  <div className="known-form-group">
                    <label htmlFor="resendEmail">Institutional Email</label>
                    <div className="known-input-wrapper">
                      <Mail size={18} className="known-input-icon" />
                      <input
                        id="resendEmail"
                        type="email"
                        className="known-input"
                        value={resendEmail}
                        onChange={(e) => setResendEmail(e.target.value)}
                        placeholder="name@institution.edu"
                        required
                      />
                    </div>
                  </div>

                  <button
                    type="submit"
                    className="known-btn-submit"
                    disabled={resendLoading}
                  >
                    {resendLoading ? 'Sending Verification Link...' : 'Resend Verification Email'}
                  </button>
                </form>
              </div>

              <div className="known-auth-footer" style={{ marginTop: '24px' }}>
                <Link to="/login" className="known-auth-link">
                  Return to Sign In
                </Link>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default VerifyEmail;

