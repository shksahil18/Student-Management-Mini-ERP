import React, { useState, useEffect } from 'react';
import { staffApi } from '../../api/staffApi';
import { useAuth } from '../../context/AuthContext';
import { Badge } from '../../components/common/Badge';
import { Pagination } from '../../components/common/Pagination';
import { Modal } from '../../components/common/Modal';
import {
  UserPlus,
  Search,
  Filter,
  Eye,
  Edit,
  Power,
  AlertCircle,
} from 'lucide-react';

export const StaffList = () => {
  const { isAdmin } = useAuth();

  const [staffList, setStaffList] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [totalItems, setTotalItems] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  // Filter & Search states
  const [search, setSearch] = useState('');
  const [department, setDepartment] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 6;

  // Modals state
  const [viewStaff, setViewStaff] = useState(null);
  const [editStaff, setEditStaff] = useState(null);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [actionLoading, setActionLoading] = useState(null);
  const [error, setError] = useState('');

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

  const loadStaff = async (customSearch = search) => {
    try {
      setLoading(true);
      setError('');
      const params = {
        pageNumber: page,
        pageSize,
        search: customSearch ? customSearch.trim() : undefined,
        department: department || undefined,
        isActive: isActiveFilter === '' ? undefined : isActiveFilter === 'true',
      };
      const res = await staffApi.getStaff(params);
      if (res.success && res.data) {
        setStaffList(res.data.items || []);
        setTotalItems(res.data.totalCount || 0);
        setTotalPages(res.data.totalPages || 1);
      }
    } catch (e) {
      console.error('Failed to load staff list', e);
      setError('Failed to retrieve staff directory.');
    } finally {
      setLoading(false);
    }
  };

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    setPage(1);
    loadStaff(search);
  };

  const handleToggleStatus = async (staff) => {
    if (!isAdmin) return;
    const confirmMsg = staff.isActive
      ? 'Are you sure you want to deactivate this staff member?'
      : 'Are you sure you want to reactivate this staff member?';

    if (!window.confirm(confirmMsg)) return;

    try {
      setActionLoading(staff.id);
      const res = await staffApi.toggleStatus(staff.id);
      if (res.success) {
        loadStaff();
      }
    } catch (e) {
      alert('Failed to update staff status');
    } finally {
      setActionLoading(null);
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
      alert('Initial password and confirmation do not match.');
      return;
    }

    try {
      setActionLoading('create');
      const res = await staffApi.createStaff(createForm);
      if (res.success) {
        setIsCreateOpen(false);
        loadStaff();
        loadDepartments();
      } else {
        alert(res.message || 'Failed to create staff member.');
      }
    } catch (e) {
      alert(e.response?.data?.message || 'Error creating staff member.');
    } finally {
      setActionLoading(null);
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
      setActionLoading(editStaff.id);
      const res = await staffApi.updateStaff(editStaff.id, editForm);
      if (res.success) {
        setEditStaff(null);
        loadStaff();
      } else {
        alert(res.message || 'Failed to update staff member.');
      }
    } catch (e) {
      alert(e.response?.data?.message || 'Error updating staff member.');
    } finally {
      setActionLoading(null);
    }
  };

  const getFullPhotoUrl = (url) => {
    if (!url) return null;
    if (url.startsWith('http')) return url;
    const base = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || 'http://localhost:5000';
    return `${base}${url}`;
  };

  return (
    <div className="student-list-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Staff Members</h1>
          <p className="page-description">Directory of academic registrars, administrative staff, and ERP operators</p>
        </div>

        {isAdmin && (
          <div className="header-actions">
            <button onClick={openCreateModal} className="btn-primary">
              <UserPlus size={16} />
              <span>Add Staff Member</span>
            </button>
          </div>
        )}
      </div>

      {/* Search and Filters Bar */}
      <div className="filter-card">
        <form onSubmit={handleSearchSubmit} className="search-form">
          <div className="search-input-wrapper">
            <Search size={18} className="search-icon" />
            <input
              type="text"
              placeholder="Search by code, name, or email..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <button type="submit" className="btn-secondary">
            Search
          </button>
        </form>

        <div className="filter-controls">
          <div className="filter-group">
            <Filter size={16} className="filter-icon" />
            <select
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

          <div className="filter-group">
            <select
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

          {(search || department || isActiveFilter) && (
            <button
              className="btn-ghost"
              onClick={() => {
                setSearch('');
                setDepartment('');
                setIsActiveFilter('');
                setPage(1);
                loadStaff('');
              }}
            >
              Clear Filters
            </button>
          )}
        </div>
      </div>

      {error && (
        <div className="alert-error">
          <AlertCircle size={18} />
          <span>{error}</span>
        </div>
      )}

      {/* Staff Table */}
      <div className="panel-card table-panel">
        <div className="table-wrapper">
          <table className="erp-table">
            <thead>
              <tr>
                <th>Code</th>
                <th>Full Name</th>
                <th>Email</th>
                <th>Phone</th>
                <th>Department</th>
                <th>Status</th>
                <th className="text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan="7" className="text-center py-8">
                    <div className="inline-spinner"></div> Loading staff records...
                  </td>
                </tr>
              ) : staffList.length > 0 ? (
                staffList.map((staff) => (
                  <tr key={staff.id} className={!staff.isActive ? 'row-inactive' : ''}>
                    <td className="font-mono font-bold text-indigo">{staff.staffId}</td>
                    <td className="font-semibold">
                      <span
                        onClick={() => setViewStaff(staff)}
                        className="hover-link"
                        style={{ cursor: 'pointer' }}
                      >
                        {staff.fullName || staff.username}
                      </span>
                    </td>
                    <td>{staff.email}</td>
                    <td>{staff.phoneNumber || '—'}</td>
                    <td>
                      <span className="course-pill">{staff.department || 'General'}</span>
                    </td>
                    <td>
                      <Badge status={staff.isActive} />
                    </td>
                    <td className="text-right table-actions">
                      <button
                        onClick={() => setViewStaff(staff)}
                        className="btn-action btn-view"
                        title="View Details"
                      >
                        <Eye size={16} />
                      </button>

                      {isAdmin && (
                        <>
                          <button
                            onClick={() => openEditModal(staff)}
                            className="btn-action btn-edit"
                            title="Edit Staff Member"
                          >
                            <Edit size={16} />
                          </button>

                          <button
                            onClick={() => handleToggleStatus(staff)}
                            className={`btn-action ${staff.isActive ? 'btn-deactivate' : 'btn-activate'}`}
                            title={staff.isActive ? 'Deactivate Staff Member' : 'Activate Staff Member'}
                            disabled={actionLoading === staff.id}
                          >
                            <Power size={16} />
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan="7" className="text-center py-8 text-muted">
                    No staff members match the current filters.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        {/* Server Pagination */}
        <div className="panel-footer">
          <Pagination
            page={page}
            totalPages={totalPages}
            totalItems={totalItems}
            onPageChange={(newPage) => setPage(newPage)}
          />
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
              <p style={{ marginTop: '4px' }}>
                <Badge status={viewStaff.isActive} />
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
          <form onSubmit={handleEditSubmit} className="erp-form">
            <div className="form-group">
              <label>Full Name <span className="req">*</span></label>
              <input
                type="text"
                value={editForm.fullName}
                onChange={(e) => setEditForm({ ...editForm, fullName: e.target.value })}
                required
              />
            </div>

            <div className="form-row-2">
              <div className="form-group">
                <label>Department <span className="req">*</span></label>
                <input
                  type="text"
                  value={editForm.department}
                  onChange={(e) => setEditForm({ ...editForm, department: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label>Designation <span className="req">*</span></label>
                <input
                  type="text"
                  value={editForm.designation}
                  onChange={(e) => setEditForm({ ...editForm, designation: e.target.value })}
                  required
                />
              </div>
            </div>

            <div className="form-row-2">
              <div className="form-group">
                <label>Phone Number</label>
                <input
                  type="text"
                  value={editForm.phoneNumber}
                  onChange={(e) => setEditForm({ ...editForm, phoneNumber: e.target.value })}
                />
              </div>

              <div className="form-group">
                <label>Account Status</label>
                <select
                  value={editForm.isActive ? 'true' : 'false'}
                  onChange={(e) => setEditForm({ ...editForm, isActive: e.target.value === 'true' })}
                >
                  <option value="true">Active Account</option>
                  <option value="false">Deactivated</option>
                </select>
              </div>
            </div>

            <div className="form-group">
              <label>Office / Campus Address</label>
              <input
                type="text"
                value={editForm.address}
                onChange={(e) => setEditForm({ ...editForm, address: e.target.value })}
              />
            </div>

            <div className="form-actions">
              <button type="button" onClick={() => setEditStaff(null)} className="btn-secondary">
                Cancel
              </button>
              <button type="submit" className="btn-primary" disabled={actionLoading === editStaff.id}>
                {actionLoading === editStaff.id ? 'Saving...' : 'Save Changes'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {/* Add Staff Modal */}
      {isCreateOpen && (
        <Modal isOpen={isCreateOpen} onClose={() => setIsCreateOpen(false)} title="Register New Staff Member">
          <form onSubmit={handleCreateSubmit} className="erp-form">
            <div className="form-row-2">
              <div className="form-group">
                <label>Full Name <span className="req">*</span></label>
                <input
                  type="text"
                  placeholder="e.g. Dr. Alan Grant"
                  value={createForm.fullName}
                  onChange={(e) => setCreateForm({ ...createForm, fullName: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label>Username <span className="req">*</span></label>
                <input
                  type="text"
                  placeholder="e.g. alan.grant"
                  value={createForm.username}
                  onChange={(e) => setCreateForm({ ...createForm, username: e.target.value })}
                  required
                />
              </div>
            </div>

            <div className="form-row-2">
              <div className="form-group">
                <label>Institutional Email <span className="req">*</span></label>
                <input
                  type="email"
                  placeholder="alan@institution.edu"
                  value={createForm.email}
                  onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label>Phone Number</label>
                <input
                  type="text"
                  placeholder="+1 (555) 234-5678"
                  value={createForm.phoneNumber}
                  onChange={(e) => setCreateForm({ ...createForm, phoneNumber: e.target.value })}
                />
              </div>
            </div>

            <div className="form-row-2">
              <div className="form-group">
                <label>Department <span className="req">*</span></label>
                <input
                  type="text"
                  placeholder="e.g. Finance, Admissions"
                  value={createForm.department}
                  onChange={(e) => setCreateForm({ ...createForm, department: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label>Designation <span className="req">*</span></label>
                <input
                  type="text"
                  placeholder="e.g. Admissions Officer"
                  value={createForm.designation}
                  onChange={(e) => setCreateForm({ ...createForm, designation: e.target.value })}
                  required
                />
              </div>
            </div>

            <div className="form-row-2">
              <div className="form-group">
                <label>Initial Password <span className="req">*</span></label>
                <input
                  type="password"
                  placeholder="Min 6 characters"
                  value={createForm.password}
                  onChange={(e) => setCreateForm({ ...createForm, password: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label>Confirm Password <span className="req">*</span></label>
                <input
                  type="password"
                  placeholder="Confirm password"
                  value={createForm.confirmPassword}
                  onChange={(e) => setCreateForm({ ...createForm, confirmPassword: e.target.value })}
                  required
                />
              </div>
            </div>

            <div className="form-actions">
              <button type="button" onClick={() => setIsCreateOpen(false)} className="btn-secondary">
                Cancel
              </button>
              <button type="submit" className="btn-primary" disabled={actionLoading === 'create'}>
                {actionLoading === 'create' ? 'Creating Staff...' : 'Create Staff Member'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
};
