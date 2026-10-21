import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { feeApi } from '../../api/feeApi';
import { studentApi } from '../../api/studentApi';
import {
  CreditCard,
  Filter,
  DollarSign,
  Receipt,
  Eye,
  AlertCircle,
  PlusCircle,
} from 'lucide-react';
import { Badge } from '../../components/common/Badge';
import { PaymentModal } from './PaymentModal';
import { ReceiptModal } from '../../components/common/ReceiptModal';
import { Modal } from '../../components/common/Modal';

export const FeeList = () => {
  const [fees, setFees] = useState([]);
  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  // Modals state
  const [selectedFee, setSelectedFee] = useState(null);
  const [selectedStudent, setSelectedStudent] = useState(null);
  const [isPaymentModalOpen, setIsPaymentModalOpen] = useState(false);

  const [selectedReceipt, setSelectedReceipt] = useState(null);
  const [isReceiptModalOpen, setIsReceiptModalOpen] = useState(false);

  // New Fee modal state
  const [isCreateFeeOpen, setIsCreateFeeOpen] = useState(false);
  const [feeStudentId, setFeeStudentId] = useState('');
  const [feeAmount, setFeeAmount] = useState('');
  const [feeDueDate, setFeeDueDate] = useState('');
  const [feeError, setFeeError] = useState('');
  const [isSubmittingFee, setIsSubmittingFee] = useState(false);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [feesRes, studentsRes] = await Promise.all([
        feeApi.getAllFees(),
        studentApi.getStudents({ pageSize: 100 }),
      ]);

      if (feesRes.success) setFees(feesRes.data);
      if (studentsRes.success) setStudents(studentsRes.data.items);
    } catch (err) {
      setError('Failed to fetch fees. Please check server.');
    } finally {
      setLoading(false);
    }
  };

  const filteredFees = fees.filter((f) => {
    if (!statusFilter) return true;
    return f.status.toLowerCase() === statusFilter.toLowerCase();
  });

  const handleOpenPayment = (fee) => {
    const matchedStudent = students.find((s) => s.id === fee.studentId);
    setSelectedFee(fee);
    setSelectedStudent(matchedStudent);
    setIsPaymentModalOpen(true);
  };

  const handleViewReceipt = async (receiptNumber) => {
    try {
      const res = await feeApi.getReceipt(receiptNumber);
      if (res.success && res.data) {
        setSelectedReceipt(res.data);
        setIsReceiptModalOpen(true);
      }
    } catch (e) {
      alert('Failed to retrieve receipt.');
    }
  };

  const handlePaymentSuccess = (receiptData) => {
    setIsPaymentModalOpen(false);
    setSelectedReceipt(receiptData);
    setIsReceiptModalOpen(true);
    fetchData();
  };

  const handleCreateFeeSubmit = async (e) => {
    e.preventDefault();
    setFeeError('');
    if (!feeStudentId || !feeAmount || !feeDueDate) {
      setFeeError('All fields are required.');
      return;
    }

    try {
      setIsSubmittingFee(true);
      const res = await feeApi.createFee({
        studentId: Number(feeStudentId),
        totalAmount: Number(feeAmount),
        dueDate: feeDueDate,
      });

      if (res.success) {
        setIsCreateFeeOpen(false);
        setFeeStudentId('');
        setFeeAmount('');
        setFeeDueDate('');
        setFeeError('');
        fetchData();
      }
    } catch (err) {
      setFeeError(err.response?.data?.message || 'Failed to assign fee.');
    } finally {
      setIsSubmittingFee(false);
    }
  };

  const openAssignFeeModal = () => {
    setFeeError('');
    setIsCreateFeeOpen(true);
  };

  return (
    <div className="fee-list-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Institutional Fee Management</h1>
          <p className="page-description">Track tuition receivables, balances, and collection vouchers</p>
        </div>
        <div className="header-actions">
          <button className="btn-primary" onClick={openAssignFeeModal}>
            <PlusCircle size={16} />
            <span>Assign Fee Structure</span>
          </button>
        </div>
      </div>

      {/* Filter Bar */}
      <div className="filter-card">
        <div className="filter-controls">
          <div className="filter-group">
            <Filter size={16} className="filter-icon" />
            <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
              <option value="">All Payment Statuses</option>
              <option value="Paid">Fully Paid</option>
              <option value="Partial">Partially Paid</option>
              <option value="Pending">Pending Payment</option>
            </select>
          </div>
        </div>
      </div>

      {error && (
        <div className="alert-error">
          <AlertCircle size={18} />
          <span>{error}</span>
        </div>
      )}

      {/* Fee Table */}
      <div className="panel-card table-panel">
        <div className="table-wrapper">
          <table className="erp-table">
            <thead>
              <tr>
                <th>Student</th>
                <th>Total Fee</th>
                <th>Paid to Date</th>
                <th>Outstanding</th>
                <th>Due Date</th>
                <th>Status</th>
                <th className="text-right">Action</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan="7" className="text-center py-8">
                    <div className="inline-spinner"></div> Loading fees...
                  </td>
                </tr>
              ) : filteredFees.length > 0 ? (
                filteredFees.map((f) => {
                  const s = students.find((st) => st.id === f.studentId);
                  return (
                    <tr key={f.id}>
                      <td>
                        <Link to={`/students/${f.studentId}`} className="student-link">
                          <span className="font-semibold">{s ? s.fullName : `Student #${f.studentId}`}</span>
                          <span className="sub-code">{s?.studentCode} • {s?.course}</span>
                        </Link>
                      </td>
                      <td className="font-bold">₹{Number(f.totalAmount).toLocaleString('en-IN')}</td>
                      <td className="font-semibold text-green">₹{Number(f.paidAmount).toLocaleString('en-IN')}</td>
                      <td className={`font-bold ${f.outstandingAmount > 0 ? 'text-red' : 'text-green'}`}>
                        ₹{Number(f.outstandingAmount).toLocaleString('en-IN')}
                      </td>
                      <td>{new Date(f.dueDate).toLocaleDateString()}</td>
                      <td>
                        <Badge status={f.status} />
                      </td>
                      <td className="text-right table-actions">
                        {f.outstandingAmount > 0 ? (
                          <button
                            className="btn-primary btn-sm"
                            onClick={() => handleOpenPayment(f)}
                          >
                            <CreditCard size={14} />
                            <span>Collect</span>
                          </button>
                        ) : (
                          <span className="settled-pill">Settled</span>
                        )}

                        {f.payments && f.payments.length > 0 && (
                          <button
                            className="btn-ghost btn-sm"
                            title="View Last Receipt"
                            onClick={() => handleViewReceipt(f.payments[0].receiptNumber)}
                          >
                            <Receipt size={14} />
                          </button>
                        )}
                      </td>
                    </tr>
                  );
                })
              ) : (
                <tr>
                  <td colSpan="7" className="text-center py-8 text-muted">
                    No fee records found for the selected filter.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal: Assign Fee */}
      <Modal isOpen={isCreateFeeOpen} onClose={() => setIsCreateFeeOpen(false)} title="Assign Fee to Student">
        {feeError && (
          <div className="alert-error" style={{ marginBottom: '16px' }}>
            <AlertCircle size={16} />
            <span>{feeError}</span>
          </div>
        )}
        <form onSubmit={handleCreateFeeSubmit} className="erp-form">
          <div className="form-group">
            <label>Select Student <span className="req">*</span></label>
            <select
              value={feeStudentId}
              onChange={(e) => setFeeStudentId(e.target.value)}
              required
            >
              <option value="">-- Choose Student --</option>
              {students.map((st) => (
                <option key={st.id} value={st.id}>
                  {st.studentCode} - {st.fullName} ({st.course})
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Fee Amount (INR) <span className="req">*</span></label>
            <input
              type="number"
              min="1"
              step="0.01"
              placeholder="e.g. 75000"
              value={feeAmount}
              onChange={(e) => setFeeAmount(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label>Due Date <span className="req">*</span></label>
            <input
              type="date"
              value={feeDueDate}
              onChange={(e) => setFeeDueDate(e.target.value)}
              required
            />
          </div>

          <div className="form-actions">
            <button type="button" className="btn-secondary" onClick={() => setIsCreateFeeOpen(false)}>
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={isSubmittingFee}>
              {isSubmittingFee ? 'Saving...' : 'Assign Fee'}
            </button>
          </div>
        </form>
      </Modal>

      {/* Reusable Payment & Receipt Modals */}
      {selectedFee && (
        <PaymentModal
          isOpen={isPaymentModalOpen}
          onClose={() => setIsPaymentModalOpen(false)}
          fee={selectedFee}
          student={selectedStudent}
          onPaymentSuccess={handlePaymentSuccess}
        />
      )}

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
