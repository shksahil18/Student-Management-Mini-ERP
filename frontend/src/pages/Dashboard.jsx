import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { dashboardApi } from '../api/dashboardApi';
import {
  Users,
  UserCheck,
  UserX,
  CreditCard,
  TrendingUp,
  AlertTriangle,
  ArrowRight,
  UserPlus,
  Receipt,
  Download,
} from 'lucide-react';
import { Badge } from '../components/common/Badge';
import { studentApi } from '../api/studentApi';

export const Dashboard = () => {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchDashboard();
  }, []);

  const fetchDashboard = async () => {
    try {
      setLoading(true);
      const res = await dashboardApi.getSummary();
      if (res.success) {
        setData(res.data);
      }
    } catch (err) {
      setError('Failed to load dashboard metrics. Ensure backend API is running.');
    } finally {
      setLoading(false);
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
      alert('Failed to export students CSV');
    }
  };

  if (loading) {
    return (
      <div className="page-loading">
        <div className="spinner"></div>
        <p>Calculating live ERP statistics...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="alert-error">
        <AlertTriangle size={18} />
        <span>{error}</span>
      </div>
    );
  }

  return (
    <div className="dashboard-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Executive Dashboard</h1>
          <p className="page-description">Overview of student enrollment and institutional financial collections</p>
        </div>
        <div className="header-actions">
          <button className="btn-secondary" onClick={handleExportCsv} title="Export CSV Report">
            <Download size={16} />
            <span>Export CSV</span>
          </button>
          <Link to="/students/new" className="btn-primary">
            <UserPlus size={16} />
            <span>New Admission</span>
          </Link>
        </div>
      </div>

      {/* KPI Cards Grid */}
      <div className="kpi-grid">
        <div className="kpi-card">
          <div className="kpi-icon-wrapper bg-indigo-light">
            <Users size={24} color="#4f46e5" />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Total Students</span>
            <span className="kpi-value">{data?.totalStudents || 0}</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon-wrapper bg-green-light">
            <UserCheck size={24} color="#16a34a" />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Active Students</span>
            <span className="kpi-value text-green">{data?.activeStudents || 0}</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon-wrapper bg-red-light">
            <UserX size={24} color="#dc2626" />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Inactive Students</span>
            <span className="kpi-value text-gray">{data?.inactiveStudents || 0}</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon-wrapper bg-blue-light">
            <CreditCard size={24} color="#2563eb" />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Total Fees Billed</span>
            <span className="kpi-value">₹{Number(data?.totalFeesBilled || 0).toLocaleString('en-IN')}</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon-wrapper bg-emerald-light">
            <TrendingUp size={24} color="#059669" />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Total Fees Collected</span>
            <span className="kpi-value text-emerald">₹{Number(data?.totalFeesCollected || 0).toLocaleString('en-IN')}</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon-wrapper bg-amber-light">
            <AlertTriangle size={24} color="#d97706" />
          </div>
          <div className="kpi-content">
            <span className="kpi-label">Total Outstanding</span>
            <span className="kpi-value text-amber">₹{Number(data?.totalOutstandingFees || 0).toLocaleString('en-IN')}</span>
          </div>
        </div>
      </div>

      {/* Two-Column Feeds */}
      <div className="dashboard-feeds-grid">
        {/* Recent Payments Feed */}
        <div className="panel-card">
          <div className="panel-header">
            <div className="panel-title-with-icon">
              <Receipt size={18} color="#4f46e5" />
              <h3>Recent Fee Payments</h3>
            </div>
            <Link to="/fees" className="panel-link">
              <span>View All</span>
              <ArrowRight size={14} />
            </Link>
          </div>
          <div className="panel-body">
            {data?.recentPayments?.length > 0 ? (
              <div className="feed-table-wrapper">
                <table className="feed-table">
                  <thead>
                    <tr>
                      <th>Receipt #</th>
                      <th>Amount</th>
                      <th>Method</th>
                      <th>Date</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.recentPayments.map((p) => (
                      <tr key={p.id}>
                        <td className="font-mono font-semibold text-indigo">{p.receiptNumber}</td>
                        <td className="font-semibold text-green">₹{Number(p.amount).toLocaleString('en-IN')}</td>
                        <td><span className="method-pill">{p.paymentMethod}</span></td>
                        <td className="text-muted">{new Date(p.paymentDate).toLocaleDateString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="empty-text">No payment records yet.</p>
            )}
          </div>
        </div>

        {/* Recent Admissions Feed */}
        <div className="panel-card">
          <div className="panel-header">
            <div className="panel-title-with-icon">
              <UserPlus size={18} color="#059669" />
              <h3>Recent Admissions</h3>
            </div>
            <Link to="/students" className="panel-link">
              <span>View All</span>
              <ArrowRight size={14} />
            </Link>
          </div>
          <div className="panel-body">
            {data?.recentAdmissions?.length > 0 ? (
              <div className="feed-table-wrapper">
                <table className="feed-table">
                  <thead>
                    <tr>
                      <th>Student</th>
                      <th>Course</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.recentAdmissions.map((s) => (
                      <tr key={s.id}>
                        <td>
                          <Link to={`/students/${s.id}`} className="student-link">
                            <span className="font-semibold">{s.fullName}</span>
                            <span className="sub-code">{s.studentCode}</span>
                          </Link>
                        </td>
                        <td>{s.course}</td>
                        <td><Badge status={s.isActive} /></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="empty-text">No admissions recorded yet.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
