import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { studentApi } from '../../api/studentApi';
import { useAuth } from '../../context/AuthContext';
import {
  Search,
  Filter,
  UserPlus,
  Eye,
  Edit,
  Power,
  Download,
  AlertCircle,
  RefreshCw,
} from 'lucide-react';
import { Badge } from '../../components/common/Badge';
import { Pagination } from '../../components/common/Pagination';

export const StudentList = () => {
  const [students, setStudents] = useState([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(6);
  const [totalItems, setTotalItems] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  const [search, setSearch] = useState('');
  const [course, setCourse] = useState('');
  const [isActive, setIsActive] = useState('');

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [actionLoading, setActionLoading] = useState(null);

  const { isAdmin } = useAuth();

  useEffect(() => {
    fetchStudents();
  }, [page, course, isActive]);

  const fetchStudents = async (customSearch = search) => {
    try {
      setLoading(true);
      setError('');
      const params = {
        page,
        pageSize,
        search: customSearch || undefined,
        course: course || undefined,
        isActive: isActive !== '' ? isActive === 'true' : undefined,
      };

      const res = await studentApi.getStudents(params);
      if (res.success && res.data) {
        setStudents(res.data.items);
        setTotalItems(res.data.totalItems);
        setTotalPages(res.data.totalPages);
      }
    } catch (err) {
      setError('Failed to fetch students. Please check API server.');
    } finally {
      setLoading(false);
    }
  };

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    setPage(1);
    fetchStudents(search);
  };

  const handleToggleStatus = async (id, currentStatus) => {
    if (!isAdmin) return;
    const confirmMsg = currentStatus
      ? 'Are you sure you want to deactivate this student?'
      : 'Are you sure you want to reactivate this student?';

    if (!window.confirm(confirmMsg)) return;

    try {
      setActionLoading(id);
      const res = await studentApi.toggleStatus(id);
      if (res.success) {
        // Refresh current page
        fetchStudents();
      }
    } catch (err) {
      alert('Failed to update student status');
    } finally {
      setActionLoading(null);
    }
  };

  const handleExportCsv = async () => {
    try {
      const blob = await studentApi.exportCsv();
      const url = window.URL.createObjectURL(new Blob([blob]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `students_${new Date().toISOString().slice(0, 10)}.csv`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (e) {
      alert('Failed to export students to CSV.');
    }
  };

  return (
    <div className="student-list-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Student Directory</h1>
          <p className="page-description">Search, filter, manage enrollments, and track academic status</p>
        </div>
        <div className="header-actions">
          <button className="btn-secondary" onClick={handleExportCsv} title="Export directory to CSV">
            <Download size={16} />
            <span>Export CSV</span>
          </button>
          {isAdmin && (
            <Link to="/students/new" className="btn-primary">
              <UserPlus size={16} />
              <span>Admit Student</span>
            </Link>
          )}
        </div>
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
              value={course}
              onChange={(e) => {
                setCourse(e.target.value);
                setPage(1);
              }}
            >
              <option value="">All Courses</option>
              <option value="MCA">MCA</option>
              <option value="BCA">BCA</option>
              <option value="B.Tech Computer Science">B.Tech Computer Science</option>
              <option value="MBA">MBA</option>
            </select>
          </div>

          <div className="filter-group">
            <select
              value={isActive}
              onChange={(e) => {
                setIsActive(e.target.value);
                setPage(1);
              }}
            >
              <option value="">All Statuses</option>
              <option value="true">Active Only</option>
              <option value="false">Inactive Only</option>
            </select>
          </div>

          {(search || course || isActive) && (
            <button
              className="btn-ghost"
              onClick={() => {
                setSearch('');
                setCourse('');
                setIsActive('');
                setPage(1);
                fetchStudents('');
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

      {/* Data Table */}
      <div className="panel-card table-panel">
        <div className="table-wrapper">
          <table className="erp-table">
            <thead>
              <tr>
                <th>Code</th>
                <th>Full Name</th>
                <th>Email</th>
                <th>Phone</th>
                <th>Course</th>
                <th>Status</th>
                <th className="text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan="7" className="text-center py-8">
                    <div className="inline-spinner"></div> Loading students...
                  </td>
                </tr>
              ) : students.length > 0 ? (
                students.map((s) => (
                  <tr key={s.id} className={!s.isActive ? 'row-inactive' : ''}>
                    <td className="font-mono font-bold text-indigo">{s.studentCode}</td>
                    <td className="font-semibold">
                      <Link to={`/students/${s.id}`} className="hover-link">
                        {s.fullName}
                      </Link>
                    </td>
                    <td>{s.email}</td>
                    <td>{s.phone}</td>
                    <td>
                      <span className="course-pill">{s.course}</span>
                    </td>
                    <td>
                      <Badge status={s.isActive} />
                    </td>
                    <td className="text-right table-actions">
                      <Link
                        to={`/students/${s.id}`}
                        className="btn-action btn-view"
                        title="View Profile & Fees"
                      >
                        <Eye size={16} />
                      </Link>

                      {isAdmin && (
                        <>
                          <Link
                            to={`/students/${s.id}/edit`}
                            className="btn-action btn-edit"
                            title="Edit Student"
                          >
                            <Edit size={16} />
                          </Link>

                          <button
                            onClick={() => handleToggleStatus(s.id, s.isActive)}
                            className={`btn-action ${s.isActive ? 'btn-deactivate' : 'btn-activate'}`}
                            title={s.isActive ? 'Deactivate Student' : 'Activate Student'}
                            disabled={actionLoading === s.id}
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
                    No students match the current filters.
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
    </div>
  );
};
