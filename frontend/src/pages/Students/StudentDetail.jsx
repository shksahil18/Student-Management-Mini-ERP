import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { studentApi } from '../../api/studentApi';
import { feeApi } from '../../api/feeApi';
import { useAuth } from '../../context/AuthContext';
import {
  ArrowLeft,
  Edit,
  Power,
  CreditCard,
  PlusCircle,
  Receipt,
  AlertCircle,
  Calendar,
  Mail,
  Phone,
  MapPin,
  BookOpen,
} from 'lucide-react';
import { Badge } from '../../components/common/Badge';
import { PaymentModal } from '../Fees/PaymentModal';
import { ReceiptModal } from '../../components/common/ReceiptModal';
import { Modal } from '../../components/common/Modal';

export const StudentDetail = () => {
  const { id } = useParams();
  const { isAdmin } = useAuth();

  const [student, setStudent] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Modals state
  const [selectedFee, setSelectedFee] = useState(null);
  const [isPaymentModalOpen, setIsPaymentModalOpen] = useState(false);
  const [selectedReceipt, setSelectedReceipt] = useState(null);
  const [isReceiptModalOpen, setIsReceiptModalOpen] = useState(false);

  // New Fee modal state
  const [isFeeModalOpen, setIsFeeModalOpen] = useState(false);
  const [newFeeAmount, setNewFeeAmount] = useState('');
  const [newFeeDueDate, setNewFeeDueDate] = useState('');
  const [feeSubmitting, setFeeSubmitting] = useState(false);

  useEffect(() => {
    fetchStudentDetails();
  }, [id]);

  const fetchStudentDetails = async () => {
    try {
      setLoading(true);
      setError('');
      const res = await studentApi.getStudentById(id);
      if (res.success && res.data) {
        setStudent(res.data);
      }
    } catch (err) {
      setError('Failed to load student profile.');
    } finally {
      setLoading(false);
    }
  };

  const handleToggleStatus = async () => {
    if (!isAdmin) return;
    const confirmMsg = student.isActive
      ? 'Deactivate this student?'
      : 'Reactivate this student?';

    if (!window.confirm(confirmMsg)) return;

    try {
      const res = await studentApi.toggleStatus(student.id);
      if (res.success) {
        fetchStudentDetails();
      }
    } catch (e) {
      alert('Failed to update status');
    }
  };

  const handleCreateFee = async (e) => {
    e.preventDefault();
    if (!newFeeAmount || Number(newFeeAmount) <= 0 || !newFeeDueDate) {
      alert('Please provide a valid fee amount and due date.');
      return;
    }

    try {
      setFeeSubmitting(true);
      const res = await feeApi.createFee({
        studentId: Number(id),
        totalAmount: Number(newFeeAmount),
        dueDate: newFeeDueDate,
      });

      if (res.success) {
        setIsFeeModalOpen(false);
        setNewFeeAmount('');
        setNewFeeDueDate('');
        fetchStudentDetails();
      }
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to assign fee structure.');
    } finally {
      setFeeSubmitting(false);
    }
  };

  const handleOpenPayment = (fee) => {
    setSelectedFee(fee);
    setIsPaymentModalOpen(true);
  };

  const handleViewReceipt = async (receiptNumber) => {
    try {
      const res = await feeApi.getReceipt(receiptNumber);
      if (res.success && res.data) {
        setSelectedReceipt(res.data);
        setIsReceiptModalOpen(true);
      }
    } catch (err) {
      alert('Failed to fetch receipt details');
    }
  };

  const handlePaymentSuccess = (receiptData) => {
    setIsPaymentModalOpen(false);
    setSelectedReceipt(receiptData);
    setIsReceiptModalOpen(true);
    fetchStudentDetails();
  };

  if (loading) {
    return (
      <div className="page-loading">
        <div className="spinner"></div>
        <p>Loading student profile & ledger...</p>
      </div>
    );
  }

  if (error || !student) {
    return (
      <div className="alert-error">
        <AlertCircle size={18} />
        <span>{error || 'Student not found.'}</span>
      </div>
    );
  }

  return (
    <div className="student-detail-page">
      <div className="form-header-bar">
        <Link to="/students" className="btn-back">
          <ArrowLeft size={18} />
          <span>Back to Students</span>
        </Link>
        <div className="header-actions">
          {isAdmin && (
            <>
              <Link to={`/students/${student.id}/edit`} className="btn-secondary">
                <Edit size={16} />
                <span>Edit Profile</span>
              </Link>
              <button
                onClick={handleToggleStatus}
                className={`btn-action-wide ${student.isActive ? 'btn-deactivate' : 'btn-activate'}`}
              >
                <Power size={16} />
                <span>{student.isActive ? 'Deactivate Student' : 'Activate Student'}</span>
              </button>
            </>
          )}
        </div>
      </div>

      {/* Profile Overview Card */}
      <div className="profile-hero-card">
        <div className="hero-avatar">
          {student.firstName[0]}{student.lastName[0]}
        </div>
        <div className="hero-details">
          <div className="hero-title-row">
            <h1 className="hero-name">{student.fullName}</h1>
            <span className="hero-code">{student.studentCode}</span>
            <Badge status={student.isActive} />
          </div>
          <div className="hero-badges">
            <span className="hero-pill"><BookOpen size={14} /> {student.course}</span>
            <span className="hero-pill"><Mail size={14} /> {student.email}</span>
            <span className="hero-pill"><Phone size={14} /> {student.phone}</span>
          </div>
        </div>
      </div>

      {/* Financial Ledger Highlights */}
      <div className="kpi-grid">
        <div className="kpi-card">
          <div className="kpi-content">
            <span className="kpi-label">Total Billed</span>
            <span className="kpi-value">₹{Number(student.totalBilled).toLocaleString('en-IN')}</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-content">
            <span className="kpi-label">Total Paid</span>
            <span className="kpi-value text-green">₹{Number(student.totalPaid).toLocaleString('en-IN')}</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-content">
            <span className="kpi-label">Remaining Balance</span>
            <span className={`kpi-value ${student.totalOutstanding > 0 ? 'text-red' : 'text-green'}`}>
              ₹{Number(student.totalOutstanding).toLocaleString('en-IN')}
            </span>
          </div>
        </div>
      </div>

      {/* Detailed Info Grid */}
      <div className="detail-sections-grid">
        {/* Left: Demographics */}
        <div className="panel-card">
          <div className="panel-header">
            <h3>Demographic Information</h3>
          </div>
          <div className="panel-body info-list">
            <div className="info-item">
              <span className="info-label">Date of Birth:</span>
              <span className="info-value">{new Date(student.dateOfBirth).toLocaleDateString()}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Gender:</span>
              <span className="info-value">{student.gender}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Admission Date:</span>
              <span className="info-value">{new Date(student.admissionDate).toLocaleDateString()}</span>
            </div>
            <div className="info-item">
              <span className="info-label">Address:</span>
              <span className="info-value">{student.address || 'Not Provided'}</span>
            </div>
            <div className="info-item">
              <span className="info-label">System Enrolled On:</span>
              <span className="info-value">{new Date(student.createdAt).toLocaleString()}</span>
            </div>
          </div>
        </div>

        {/* Right: Assigned Fees & Payments */}
        <div className="panel-card">
          <div className="panel-header">
            <div className="panel-title-with-icon">
              <CreditCard size={18} color="#4f46e5" />
              <h3>Academic Fee Schedules</h3>
            </div>
            {isAdmin && (
              <button className="btn-secondary btn-sm" onClick={() => setIsFeeModalOpen(true)}>
                <PlusCircle size={14} />
                <span>Assign Fee</span>
              </button>
            )}
          </div>
          <div className="panel-body">
            {student.fees && student.fees.length > 0 ? (
              <div className="fees-accordion-list">
                {student.fees.map((f) => (
                  <div key={f.id} className="fee-box">
                    <div className="fee-box-header">
                      <div>
                        <span className="fee-amount-main">₹{Number(f.totalAmount).toLocaleString('en-IN')}</span>
                        <span className="fee-due-date">Due: {new Date(f.dueDate).toLocaleDateString()}</span>
                      </div>
                      <div className="fee-box-actions">
                        <Badge status={f.status} />
                        {f.outstandingAmount > 0 && (
                          <button
                            className="btn-primary btn-sm"
                            onClick={() => handleOpenPayment(f)}
                          >
                            Pay ₹{Number(f.outstandingAmount).toLocaleString('en-IN')}
                          </button>
                        )}
                      </div>
                    </div>

                    <div className="fee-progress-bar">
                      <div
                        className="fee-progress-fill"
                        style={{
                          width: `${Math.min(100, (Number(f.paidAmount) / Number(f.totalAmount)) * 100)}%`,
                        }}
                      ></div>
                    </div>

                    <div className="fee-split-summary">
                      <span>Paid: <strong>₹{Number(f.paidAmount).toLocaleString('en-IN')}</strong></span>
                      <span>Outstanding: <strong className="text-red">₹{Number(f.outstandingAmount).toLocaleString('en-IN')}</strong></span>
                    </div>

                    {/* Associated Payments for this fee */}
                    {f.payments && f.payments.length > 0 && (
                      <div className="sub-payments-table">
                        <span className="sub-title">Payment Receipts:</span>
                        <table>
                          <thead>
                            <tr>
                              <th>Receipt</th>
                              <th>Amount</th>
                              <th>Date</th>
                              <th>Method</th>
                              <th>Action</th>
                            </tr>
                          </thead>
                          <tbody>
                            {f.payments.map((p) => (
                              <tr key={p.id}>
                                <td className="font-mono text-indigo font-bold">{p.receiptNumber}</td>
                                <td className="font-semibold text-green">₹{Number(p.amount).toLocaleString('en-IN')}</td>
                                <td>{new Date(p.paymentDate).toLocaleDateString()}</td>
                                <td>{p.paymentMethod}</td>
                                <td>
                                  <button
                                    className="btn-ghost btn-sm"
                                    onClick={() => handleViewReceipt(p.receiptNumber)}
                                  >
                                    <Receipt size={14} />
                                    <span>Receipt</span>
                                  </button>
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            ) : (
              <p className="empty-text">No fees assigned to this student yet.</p>
            )}
          </div>
        </div>
      </div>

      {/* Modal: Assign New Fee Structure */}
      <Modal isOpen={isFeeModalOpen} onClose={() => setIsFeeModalOpen(false)} title="Assign New Academic Fee">
        <form onSubmit={handleCreateFee} className="erp-form">
          <div className="form-group">
            <label>Fee Amount (INR) <span className="req">*</span></label>
            <input
              type="number"
              min="1"
              step="0.01"
              placeholder="e.g. 50000"
              value={newFeeAmount}
              onChange={(e) => setNewFeeAmount(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label>Due Date <span className="req">*</span></label>
            <input
              type="date"
              value={newFeeDueDate}
              onChange={(e) => setNewFeeDueDate(e.target.value)}
              required
            />
          </div>

          <div className="form-actions">
            <button type="button" className="btn-secondary" onClick={() => setIsFeeModalOpen(false)}>
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={feeSubmitting}>
              {feeSubmitting ? 'Assigning...' : 'Assign Fee'}
            </button>
          </div>
        </form>
      </Modal>

      {/* Modal: Process Payment */}
      {selectedFee && (
        <PaymentModal
          isOpen={isPaymentModalOpen}
          onClose={() => setIsPaymentModalOpen(false)}
          fee={selectedFee}
          student={student}
          onPaymentSuccess={handlePaymentSuccess}
        />
      )}

      {/* Modal: Printable Receipt */}
      {selectedReceipt && (
        <ReceiptModal
          isOpen={isReceiptModalOpen}
          onClose={() => setIsReceiptModalOpen(false)}
          receipt={selectedReceipt}
        />
      )}
    </div>
  );
};
