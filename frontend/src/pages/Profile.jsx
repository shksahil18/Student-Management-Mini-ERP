import React, { useState, useEffect, useRef } from 'react';
import { useAuth } from '../context/AuthContext';
import { profileApi } from '../api/profileApi';
import {
  User,
  Mail,
  Phone,
  Briefcase,
  Building2,
  Calendar,
  MapPin,
  Camera,
  Trash2,
  ShieldCheck,
  CheckCircle,
  AlertCircle,
  Upload,
} from 'lucide-react';

export const Profile = () => {
  const { user } = useAuth();
  const fileInputRef = useRef(null);

  const [profile, setProfile] = useState(null);
  const [formData, setFormData] = useState({
    fullName: '',
    phoneNumber: '',
    designation: '',
    department: '',
    address: '',
  });

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [uploadingPhoto, setUploadingPhoto] = useState(false);
  const [successMsg, setSuccessMsg] = useState('');
  const [errorMsg, setErrorMsg] = useState('');

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      setLoading(true);
      const res = await profileApi.getProfile();
      if (res.success && res.data) {
        setProfile(res.data);
        setFormData({
          fullName: res.data.fullName || '',
          phoneNumber: res.data.phoneNumber || '',
          designation: res.data.designation || '',
          department: res.data.department || '',
          address: res.data.address || '',
        });
      }
    } catch (err) {
      setErrorMsg('Failed to load user profile.');
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSuccessMsg('');
    setErrorMsg('');

    try {
      setSaving(true);
      const res = await profileApi.updateProfile(formData);
      if (res.success) {
        setProfile(res.data);
        // Also update stored user in localStorage
        const stored = localStorage.getItem('user');
        if (stored) {
          const parsed = JSON.parse(stored);
          const updatedUser = { ...parsed, fullName: res.data.fullName, profilePhotoUrl: res.data.profilePhotoUrl };
          localStorage.setItem('user', JSON.stringify(updatedUser));
        }
        setSuccessMsg('Profile details updated successfully.');
        setTimeout(() => setSuccessMsg(''), 3500);
      } else {
        setErrorMsg(res.message || 'Failed to update profile.');
      }
    } catch (err) {
      setErrorMsg(err.response?.data?.message || 'Error occurred while saving profile changes.');
    } finally {
      setSaving(false);
    }
  };

  const handlePhotoSelect = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (file.size > 2 * 1024 * 1024) {
      setErrorMsg('Profile photo must be less than 2MB.');
      return;
    }

    try {
      setUploadingPhoto(true);
      setErrorMsg('');
      const res = await profileApi.uploadPhoto(file);
      if (res.success) {
        setProfile((prev) => ({ ...prev, profilePhotoUrl: res.data }));
        const stored = localStorage.getItem('user');
        if (stored) {
          const parsed = JSON.parse(stored);
          localStorage.setItem('user', JSON.stringify({ ...parsed, profilePhotoUrl: res.data }));
        }
        setSuccessMsg('Profile photo uploaded successfully.');
        setTimeout(() => setSuccessMsg(''), 3500);
      } else {
        setErrorMsg(res.message || 'Photo upload failed.');
      }
    } catch (err) {
      setErrorMsg(err.response?.data?.message || 'Failed to upload photo.');
    } finally {
      setUploadingPhoto(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const handleDeletePhoto = async () => {
    if (!window.confirm('Are you sure you want to remove your profile photo?')) return;

    try {
      setUploadingPhoto(true);
      setErrorMsg('');
      const res = await profileApi.deletePhoto();
      if (res.success) {
        setProfile((prev) => ({ ...prev, profilePhotoUrl: null }));
        const stored = localStorage.getItem('user');
        if (stored) {
          const parsed = JSON.parse(stored);
          localStorage.setItem('user', JSON.stringify({ ...parsed, profilePhotoUrl: null }));
        }
        setSuccessMsg('Profile photo removed.');
        setTimeout(() => setSuccessMsg(''), 3500);
      }
    } catch (err) {
      setErrorMsg('Failed to remove profile photo.');
    } finally {
      setUploadingPhoto(false);
    }
  };

  const getFullPhotoUrl = (url) => {
    if (!url) return null;
    if (url.startsWith('http')) return url;
    const base = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || 'http://localhost:5000';
    return `${base}${url}`;
  };

  if (loading) {
    return (
      <div className="erp-page">
        <div className="text-center" style={{ padding: '60px 0' }}>
          <div className="spinner"></div>
          <p style={{ marginTop: '12px', color: 'var(--text-muted)' }}>Loading profile information...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="erp-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">User Profile</h1>
          <p className="page-subtitle">Manage your personal credentials, identity photo, and departmental details</p>
        </div>
      </div>

      {successMsg && (
        <div className="alert alert-success" style={{ marginBottom: '20px', display: 'flex', alignItems: 'center', gap: '8px' }}>
          <CheckCircle size={18} />
          <span>{successMsg}</span>
        </div>
      )}

      {errorMsg && (
        <div className="alert alert-danger" style={{ marginBottom: '20px', display: 'flex', alignItems: 'center', gap: '8px' }}>
          <AlertCircle size={18} />
          <span>{errorMsg}</span>
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '320px 1fr', gap: '24px' }}>
        {/* Left Column: Photo & Identity Summary */}
        <div className="panel-card" style={{ textAlign: 'center', height: 'fit-content' }}>
          <div style={{ position: 'relative', display: 'inline-block', margin: '0 auto 16px auto' }}>
            <div className="profile-avatar-large">
              {profile?.profilePhotoUrl ? (
                <img src={getFullPhotoUrl(profile.profilePhotoUrl)} alt={profile.fullName || profile.username} />
              ) : (
                <span>{(profile?.fullName || profile?.username || 'U').charAt(0).toUpperCase()}</span>
              )}
            </div>

            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={uploadingPhoto}
              title="Upload New Photo"
              style={{
                position: 'absolute',
                bottom: '0',
                right: '0',
                backgroundColor: 'var(--primary)',
                color: 'white',
                border: '2px solid var(--bg-surface)',
                borderRadius: '50%',
                width: '32px',
                height: '32px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                cursor: 'pointer',
              }}
            >
              <Camera size={16} />
            </button>

            <input
              type="file"
              ref={fileInputRef}
              onChange={handlePhotoSelect}
              accept="image/png,image/jpeg,image/webp"
              style={{ display: 'none' }}
            />
          </div>

          <h2 style={{ fontSize: '18px', fontWeight: '700', marginBottom: '4px' }}>
            {profile?.fullName || profile?.username}
          </h2>
          <div style={{ display: 'inline-flex', alignItems: 'center', gap: '6px', marginBottom: '16px' }}>
            <span className={`user-role-badge ${profile?.role === 'Admin' ? 'role-admin' : 'role-staff'}`}>
              <ShieldCheck size={13} />
              {profile?.role || 'Staff Member'}
            </span>
          </div>

          {profile?.profilePhotoUrl && (
            <div style={{ marginBottom: '16px' }}>
              <button
                type="button"
                onClick={handleDeletePhoto}
                disabled={uploadingPhoto}
                className="btn btn-secondary"
                style={{ fontSize: '12px', padding: '6px 12px', color: '#ef4444', borderColor: '#fca5a5' }}
              >
                <Trash2 size={13} style={{ marginRight: '4px' }} />
                <span>Remove Photo</span>
              </button>
            </div>
          )}

          <div style={{ textAlign: 'left', borderTop: '1px solid var(--border)', paddingTop: '16px', fontSize: '13px' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '10px', color: 'var(--text-muted)' }}>
              <Mail size={15} />
              <span style={{ color: 'var(--text-main)', wordBreak: 'break-all' }}>{profile?.email}</span>
            </div>
            {profile?.department && (
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '10px', color: 'var(--text-muted)' }}>
                <Building2 size={15} />
                <span style={{ color: 'var(--text-main)' }}>{profile.department}</span>
              </div>
            )}
            {profile?.designation && (
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '10px', color: 'var(--text-muted)' }}>
                <Briefcase size={15} />
                <span style={{ color: 'var(--text-main)' }}>{profile.designation}</span>
              </div>
            )}
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px', color: 'var(--text-muted)' }}>
              <Calendar size={15} />
              <span style={{ color: 'var(--text-main)' }}>
                Member since {new Date(profile?.createdAt || Date.now()).toLocaleDateString()}
              </span>
            </div>
          </div>
        </div>

        {/* Right Column: Edit Profile Form */}
        <div className="panel-card">
          <h3 style={{ fontSize: '18px', fontWeight: '700', marginBottom: '6px' }}>Personal Details</h3>
          <p style={{ fontSize: '13px', color: 'var(--text-muted)', marginBottom: '20px' }}>
            Ensure your professional details are accurate for institution directories and automated notifications.
          </p>

          <form onSubmit={handleSubmit}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
              <div className="form-group">
                <label className="form-label">Full Name</label>
                <input
                  type="text"
                  name="fullName"
                  className="form-control"
                  placeholder="e.g. Dr. Robert Vance"
                  value={formData.fullName}
                  onChange={handleInputChange}
                />
              </div>

              <div className="form-group">
                <label className="form-label">Username (Immutable)</label>
                <input
                  type="text"
                  className="form-control"
                  value={profile?.username || ''}
                  disabled
                  style={{ opacity: 0.7, cursor: 'not-allowed' }}
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
              <div className="form-group">
                <label className="form-label">Email Address (Immutable)</label>
                <input
                  type="email"
                  className="form-control"
                  value={profile?.email || ''}
                  disabled
                  style={{ opacity: 0.7, cursor: 'not-allowed' }}
                />
              </div>

              <div className="form-group">
                <label className="form-label">Phone Number</label>
                <input
                  type="text"
                  name="phoneNumber"
                  className="form-control"
                  placeholder="+1 (555) 019-2834"
                  value={formData.phoneNumber}
                  onChange={handleInputChange}
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
              <div className="form-group">
                <label className="form-label">Department</label>
                <input
                  type="text"
                  name="department"
                  className="form-control"
                  placeholder="e.g. Academic Affairs / Admissions"
                  value={formData.department}
                  onChange={handleInputChange}
                />
              </div>

              <div className="form-group">
                <label className="form-label">Designation</label>
                <input
                  type="text"
                  name="designation"
                  className="form-control"
                  placeholder="e.g. Senior Academic Registrar"
                  value={formData.designation}
                  onChange={handleInputChange}
                />
              </div>
            </div>

            <div className="form-group" style={{ marginBottom: '24px' }}>
              <label className="form-label">Campus Address / Office Location</label>
              <textarea
                name="address"
                className="form-control"
                rows="3"
                placeholder="Building B, Room 304, Campus Lane"
                value={formData.address}
                onChange={handleInputChange}
              />
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
              <button type="submit" className="btn btn-primary" disabled={saving}>
                {saving ? 'Saving Changes...' : 'Save Profile Changes'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};
