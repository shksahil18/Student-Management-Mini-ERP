import React, { useState, useEffect } from 'react';
import { staffApi } from '../../api/staffApi';
import { useAuth } from '../../context/AuthContext';
import { Badge } from '../../components/common/Badge';
import { Modal } from '../../components/common/Modal';
import {
  UserCheck,
  UserPlus,
  Search,
  Filter,
  Eye,
  Edit2,
  Power,
  Mail,
  Phone,
  Building2,
  Calendar,
  ShieldCheck,
  AlertCircle,
  CheckCircle,
} from 'lucide-react';

export const StaffList = () => {
  const { isAdmin } = useAuth();

  const [staffList, setStaffList] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalCount, setTotalCount] = useState(0);

  // Filter & Search states
  const [search, setSearch] = useState('');
  const [department, setDepartment] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 10;

  // Modals state
  const [viewStaff, setViewStaff] = useState(null);
  const [editStaff, setEditStaff] = useState(null);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [notification, setNotification] = useState({ type: '', message: '' });

  // Form state for Create
  const [createForm, setCreateForm] = useState({
    fullName: '',
    username: '',
    email: '',
    phoneNumber: '',
    designation: '',
    department: '',
    joiningDate: new Date().toISOString().split('T')[0],
    address: '',
    role: 'Staff',
    password: '',
    confirmPassword: '',
  });

  // Form state for Edit
  const [editForm, setEditForm] = useState({
    fullName: '',
    phoneNumber: '',
    designation: '',
    department: '',
    address: '',
    isActive: true,
  });

  useEffect(() => {
    loadDepartments();
  }, []);

  useEffect(() => {
    loadStaff();
  }, [page, department, isActiveFilter]);

  const loadDepartments = async () => {
    try {
      const res = await staffApi.getDepartments();
      if (res.success && res.data) {
        setDepartments(res.data);
      }
    } catch (e) {
      console.error('Failed to load departments', e);
    }
  };

  const loadStaff = async () => {
    try {
      setLoading(true);
      const params = {
        pageNumber: page,
        pageSize,
        search: search.trim() || undefined,
        department: department || undefined,
        isActive: isActiveFilter === '' ? undefined : isActiveFilter === 'true',
      };
      const res = await staffApi.getStaff(params);
      if (res.success && res.data) {
        setStaffList(res.data.items || []);
        setTotalCount(res.data.totalCount || 0);
      }
    } catch (e) {
      console.error('Failed to load staff list', e);
      setNotification({ type: 'error', message: 'Failed to retrieve staff directory.' });
    } finally {
      setLoading(false);
    }
  };

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    setPage(1);
    loadStaff();
  };

  const handleToggleStatus = async (staff) => {
    if (!isAdmin) return;
    try {
      const res = await staffApi.toggleStatus(staff.id);
      if (res.success) {
        setNotification({
          type: 'success',
          message: `Staff member ${staff.fullName || staff.username} has been ${res.data ? 'activated' : 'deactivated'}.`,
        });
        loadStaff();
      }
    } catch (e) {
      setNotification({ type: 'error', message: 'Failed to toggle account status.' });
    }
  };

  const openCreateModal = () => {
    setCreateForm({
      fullName: '',
      username: '',
      email: '',
      phoneNumber: '',
      designation: '',
      department: '',
      joiningDate: new Date().toISOString().split('T')[0],
      address: '',
      role: 'Staff',
      password: '',
      confirmPassword: '',
    });
    setIsCreateOpen(true);
  };

  const handleCreateSubmit = async (e) => {
    e.preventDefault();
    if (createForm.password !== createForm.confirmPassword) {
      setNotification({ type: 'error', message: 'Initial password and confirmation do not match.' });
      return;
    }

    try {
      setActionLoading(true);
      const res = await staffApi.createStaff(createForm);
      if (res.success) {
        setIsCreateOpen(false);
        setNotification({ type: 'success', message: 'New staff member registered successfully!' });
        loadStaff();
        loadDepartments();
      } else {
        setNotification({ type: 'error', message: res.message || 'Failed to create staff member.' });
      }
    } catch (e) {
      setNotification({ type: 'error', message: e.response?.data?.message || 'Error creating staff member.' });
    } finally {
      setActionLoading(false);
    }
  };

  const openEditModal = (staff) => {
    setEditStaff(staff);
    setEditForm({
      fullName: staff.fullName || '',
      phoneNumber: staff.phoneNumber || '',
      designation: staff.designation || '',
      department: staff.department || '',
      address: staff.address || '',
      isActive: staff.isActive,
    });
  };

  const handleEditSubmit = async (e) => {
    e.preventDefault();
    try {
      setActionLoading(true);
      const res = await staffApi.updateStaff(editStaff.id, editForm);
      if (res.success) {
        setEditStaff(null);
        setNotification({ type: 'success', message: 'Staff member updated successfully!' });
        loadStaff();
      } else {
        setNotification({ type: 'error', message: res.message || 'Failed to update staff member.' });
      }
    } catch (e) {
      setNotification({ type: 'error', message: e.response?.data?.message || 'Error updating staff member.' });
    } finally {
      setActionLoading(false);
    }
  };

  const getFullPhotoUrl = (url) => {
    if (!url) return null;
    if (url.startsWith('http')) return url;
    const base = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || 'http://localhost:5000';
    return `${base}${url}`;
  };

  return (
    <div className="erp-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Staff Members</h1>
          <p className="page-subtitle">Directory of academic registrars, administrative staff, and ERP operators</p>
        </div>

        {isAdmin && (
          <button onClick={openCreateModal} className="btn btn-primary">
            <UserPlus size={18} />
            <span>Add Staff Member</span>
          </button>
        )}
      </div>

      {notification.message && (
        <div
          className={`alert ${notification.type === 'error' ? 'alert-danger' : 'alert-success'}`}
          style={{ marginBottom: '20px', display: 'flex', alignItems: 'center', gap: '8px' }}
        >
          {notification.type === 'error' ? <AlertCircle size={18} /> : <CheckCircle size={18} />}
          <span>{notification.message}</span>
        </div>
      )}

      {/* Filter and Search Bar */}
      <div className="panel-card" style={{ marginBottom: '24px' }}>
        <form onSubmit={handleSearchSubmit} style={{ display: 'flex', flexWrap: 'wrap', gap: '16px', alignItems: 'flex-end' }}>
          <div style={{ flex: '1 1 240px' }}>
            <label className="form-label">Search Staff</label>
            <div style={{ position: 'relative' }}>
              <input
                type="text"
                className="form-control"
                placeholder="Search by name, email, or designation..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                style={{ paddingLeft: '36px' }}
              />
              <Search size={16} style={{ position: 'absolute', left: '12px', top: '12px', color: 'var(--text-dim)' }} />
            </div>
          </div>

          <div style={{ width: '180px' }}>
            <label className="form-label">Department</label>
            <select
              className="form-select"
              value={department}
              onChange={(e) => {
                setDepartment(e.target.value);
                setPage(1);
              }}
            >
              <option value="">All Departments</option>
              {departments.map((dept) => (
                <option key={dept} value={dept}>
                  {dept}
                </option>
              ))}
            </select>
          </div>

          <div style={{ width: '150px' }}>
            <label className="form-label">Status</label>
            <select
              className="form-select"
              value={isActiveFilter}
              onChange={(e) => {
                setIsActiveFilter(e.target.value);
                setPage(1);
              }}
            >
              <option value="">All Statuses</option>
              <option value="true">Active Only</option>
              <option value="false">Inactive Only</option>
            </select>
          </div>

          <button type="submit" className="btn btn-secondary" style={{ height: '42px' }}>
            <Filter size={16} />
            <span>Filter</span>
          </button>
        </form>
      </div>

      {/* Staff Table */}
      <div className="panel-card" style={{ padding: 0, overflow: 'hidden' }}>
        <div style={{ overflowX: 'auto' }}>
          <table className="data-table">
            <thead>
              <tr>
                <th>Staff Member</th>
                <th>Staff ID</th>
                <th>Department</th>
                <th>Designation</th>
                <th>Joining Date</th>
                <th>Status</th>
                <th style={{ textAlign: 'right' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan="7" className="text-center" style={{ padding: '40px' }}>
                    <div className="spinner"></div>
                    <p style={{ marginTop: '10px', color: 'var(--text-muted)' }}>Loading staff records...</p>
                  </td>
                </tr>
              ) : staffList.length === 0 ? (
                <tr>
                  <td colSpan="7" className="text-center" style={{ padding: '40px', color: 'var(--text-muted)' }}>
                    No staff members match the current filter criteria.
                  </td>
                </tr>
              ) : (
                staffList.map((staff) => (
                  <tr key={staff.id}>
                    <td>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                        <div className="staff-avatar-circle">
                          {staff.profilePhotoUrl ? (
                            <img src={getFullPhotoUrl(staff.profilePhotoUrl)} alt={staff.fullName || staff.username} />
                          ) : (
                            <span>{(staff.fullName || staff.username).charAt(0).toUpperCase()}</span>
                          )}
                        </div>
                        <div>
                          <div style={{ fontWeight: '600', color: 'var(--text-main)' }}>
                            {staff.fullName || staff.username}
                          </div>
                          <div style={{ fontSize: '12px', color: 'var(--text-muted)' }}>{staff.email}</div>
                        </div>
                      </div>
                    </td>
                    <td>
                      <span className="font-mono" style={{ fontWeight: '600' }}>
                        {staff.staffId}
                      </span>
                    </td>
                    <td>{staff.department || '—'}</td>
                    <td>{staff.designation || 'Staff'}</td>
                    <td>
                      {staff.joiningDate ? new Date(staff.joiningDate).toLocaleDateString() : '—'}
                    </td>
                    <td>
                      <Badge variant={staff.isActive ? 'success' : 'danger'}>
                        {staff.isActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </td>
                    <td style={{ textAlign: 'right' }}>
                      <div style={{ display: 'inline-flex', gap: '8px' }}>
                        <button
                          onClick={() => setViewStaff(staff)}
                          className="btn-icon"
                          title="View Details"
                        >
                          <Eye size={16} />
                        </button>

                        {isAdmin && (
                          <>
                            <button
                              onClick={() => openEditModal(staff)}
                              className="btn-icon"
                              title="Edit Staff Member"
                            >
                              <Edit2 size={16} />
                            </button>

                            <button
                              onClick={() => handleToggleStatus(staff)}
                              className="btn-icon"
                              title={staff.isActive ? 'Deactivate' : 'Activate'}
                              style={{ color: staff.isActive ? '#ef4444' : '#10b981' }}
                            >
                              <Power size={16} />
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* View Staff Modal */}
      {viewStaff && (
        <Modal isOpen={!!viewStaff} onClose={() => setViewStaff(null)} title="Staff Member Details">
          <div style={{ textAlign: 'center', marginBottom: '20px' }}>
            <div className="profile-avatar-large" style={{ margin: '0 auto 12px auto' }}>
              {viewStaff.profilePhotoUrl ? (
                <img src={getFullPhotoUrl(viewStaff.profilePhotoUrl)} alt={viewStaff.fullName || viewStaff.username} />
              ) : (
                <span>{(viewStaff.fullName || viewStaff.username).charAt(0).toUpperCase()}</span>
              )}
            </div>
            <h3 style={{ fontSize: '18px', fontWeight: '700' }}>{viewStaff.fullName || viewStaff.username}</h3>
            <p style={{ color: 'var(--text-muted)', fontSize: '13px' }}>{viewStaff.staffId}</p>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', fontSize: '14px' }}>
            <div>
              <span style={{ color: 'var(--text-muted)', fontSize: '12px' }}>Email Address</span>
              <p style={{ fontWeight: '500' }}>{viewStaff.email}</p>
            </div>
            <div>
              <span style={{ color: 'var(--text-muted)', fontSize: '12px' }}>Phone Number</span>
              <p style={{ fontWeight: '500' }}>{viewStaff.phoneNumber || 'Not provided'}</p>
            </div>
            <div>
              <span style={{ color: 'var(--text-muted)', fontSize: '12px' }}>Department</span>
              <p style={{ fontWeight: '500' }}>{viewStaff.department || 'Not specified'}</p>
            </div>
            <div>
              <span style={{ color: 'var(--text-muted)', fontSize: '12px' }}>Designation</span>
              <p style={{ fontWeight: '500' }}>{viewStaff.designation || 'Staff Member'}</p>
            </div>
            <div>
              <span style={{ color: 'var(--text-muted)', fontSize: '12px' }}>Joining Date</span>
              <p style={{ fontWeight: '500' }}>
                {viewStaff.joiningDate ? new Date(viewStaff.joiningDate).toLocaleDateString() : 'N/A'}
              </p>
            </div>
            <div>
              <span style={{ color: 'var(--text-muted)', fontSize: '12px' }}>Account Status</span>
              <p>
                <Badge variant={viewStaff.isActive ? 'success' : 'danger'}>
                  {viewStaff.isActive ? 'Active' : 'Inactive'}
                </Badge>
              </p>
            </div>
          </div>
          {viewStaff.address && (
            <div style={{ marginTop: '16px' }}>
              <span style={{ color: 'var(--text-muted)', fontSize: '12px' }}>Campus Address</span>
              <p style={{ fontWeight: '500', fontSize: '14px' }}>{viewStaff.address}</p>
            </div>
          )}
        </Modal>
      )}

      {/* Edit Staff Modal */}
      {editStaff && (
        <Modal isOpen={!!editStaff} onClose={() => setEditStaff(null)} title={`Edit Staff: ${editStaff.fullName || editStaff.username}`}>
          <form onSubmit={handleEditSubmit}>
            <div className="form-group" style={{ marginBottom: '14px' }}>
              <label className="form-label">Full Name</label>
              <input
                type="text"
                className="form-control"
                value={editForm.fullName}
                onChange={(e) => setEditForm({ ...editForm, fullName: e.target.value })}
                required
              />
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '14px', marginBottom: '14px' }}>
              <div className="form-group">
                <label className="form-label">Department</label>
                <input
                  type="text"
                  className="form-control"
                  value={editForm.department}
                  onChange={(e) => setEditForm({ ...editForm, department: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Designation</label>
                <input
                  type="text"
                  className="form-control"
                  value={editForm.designation}
                  onChange={(e) => setEditForm({ ...editForm, designation: e.target.value })}
                  required
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '14px', marginBottom: '14px' }}>
              <div className="form-group">
                <label className="form-label">Phone Number</label>
                <input
                  type="text"
                  className="form-control"
                  value={editForm.phoneNumber}
                  onChange={(e) => setEditForm({ ...editForm, phoneNumber: e.target.value })}
                />
              </div>

              <div className="form-group">
                <label className="form-label">Active Status</label>
                <select
                  className="form-select"
                  value={editForm.isActive ? 'true' : 'false'}
                  onChange={(e) => setEditForm({ ...editForm, isActive: e.target.value === 'true' })}
                >
                  <option value="true">Active Account</option>
                  <option value="false">Deactivated</option>
                </select>
              </div>
            </div>

            <div className="form-group" style={{ marginBottom: '20px' }}>
              <label className="form-label">Office / Address</label>
              <textarea
                className="form-control"
                rows="2"
                value={editForm.address}
                onChange={(e) => setEditForm({ ...editForm, address: e.target.value })}
              />
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
              <button type="button" onClick={() => setEditStaff(null)} className="btn btn-secondary">
                Cancel
              </button>
              <button type="submit" className="btn btn-primary" disabled={actionLoading}>
                {actionLoading ? 'Saving...' : 'Save Changes'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {/* Add Staff Modal */}
      {isCreateOpen && (
        <Modal isOpen={isCreateOpen} onClose={() => setIsCreateOpen(false)} title="Register New Staff Member">
          <form onSubmit={handleCreateSubmit}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '14px', marginBottom: '14px' }}>
              <div className="form-group">
                <label className="form-label">Full Name</label>
                <input
                  type="text"
                  className="form-control"
                  placeholder="e.g. Dr. Alan Grant"
                  value={createForm.fullName}
                  onChange={(e) => setCreateForm({ ...createForm, fullName: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Username</label>
                <input
                  type="text"
                  className="form-control"
                  placeholder="e.g. alan.grant"
                  value={createForm.username}
                  onChange={(e) => setCreateForm({ ...createForm, username: e.target.value })}
                  required
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '14px', marginBottom: '14px' }}>
              <div className="form-group">
                <label className="form-label">Institutional Email</label>
                <input
                  type="email"
                  className="form-control"
                  placeholder="alan@institution.edu"
                  value={createForm.email}
                  onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Phone Number</label>
                <input
                  type="text"
                  className="form-control"
                  placeholder="+1 (555) 234-5678"
                  value={createForm.phoneNumber}
                  onChange={(e) => setCreateForm({ ...createForm, phoneNumber: e.target.value })}
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '14px', marginBottom: '14px' }}>
              <div className="form-group">
                <label className="form-label">Department</label>
                <input
                  type="text"
                  className="form-control"
                  placeholder="e.g. Finance, Admissions"
                  value={createForm.department}
                  onChange={(e) => setCreateForm({ ...createForm, department: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Designation</label>
                <input
                  type="text"
                  className="form-control"
                  placeholder="e.g. Admissions Officer"
                  value={createForm.designation}
                  onChange={(e) => setCreateForm({ ...createForm, designation: e.target.value })}
                  required
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '14px', marginBottom: '14px' }}>
              <div className="form-group">
                <label className="form-label">Initial Password</label>
                <input
                  type="password"
                  className="form-control"
                  placeholder="Min 6 characters"
                  value={createForm.password}
                  onChange={(e) => setCreateForm({ ...createForm, password: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Confirm Password</label>
                <input
                  type="password"
                  className="form-control"
                  placeholder="Confirm password"
                  value={createForm.confirmPassword}
                  onChange={(e) => setCreateForm({ ...createForm, confirmPassword: e.target.value })}
                  required
                />
              </div>
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px', marginTop: '20px' }}>
              <button type="button" onClick={() => setIsCreateOpen(false)} className="btn btn-secondary">
                Cancel
              </button>
              <button type="submit" className="btn btn-primary" disabled={actionLoading}>
                {actionLoading ? 'Creating Staff...' : 'Create Staff Member'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
};
