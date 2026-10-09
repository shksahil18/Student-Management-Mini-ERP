import React, { useState } from 'react';
import { Modal } from '../../components/common/Modal';
import { feeApi } from '../../api/feeApi';
import { CreditCard, AlertCircle } from 'lucide-react';

export const PaymentModal = ({ isOpen, onClose, fee, student, onPaymentSuccess }) => {
  const [amount, setAmount] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('UPI');
  const [notes, setNotes] = useState('');
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (!fee) return null;

  const outstanding = Number(fee.outstandingAmount);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    const numAmount = Number(amount);
    if (!numAmount || numAmount <= 0) {
      setError('Payment amount must be greater than 0.');
      return;
    }

    if (numAmount > outstanding) {
      setError(`Payment cannot exceed the outstanding balance of ₹${outstanding.toLocaleString('en-IN')}`);
      return;
    }

    try {
      setIsSubmitting(true);
      const res = await feeApi.recordPayment(fee.id, {
        amount: numAmount,
        paymentMethod,
        notes,
      });

      if (res.success && res.data) {
        onPaymentSuccess(res.data);
      }
    } catch (err) {
      const msg = err.response?.data?.message || 'Payment processing failed.';
      setError(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handlePayFull = () => {
    setAmount(outstanding.toString());
    setError('');
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Record Fee Payment">
      <div className="payment-modal-content">
        <div className="payment-summary-box">
          <div className="summary-row">
            <span>Student:</span>
            <strong>{student?.fullName || 'Student'}</strong>
          </div>
          <div className="summary-row">
            <span>Total Fee:</span>
            <span>₹{Number(fee.totalAmount).toLocaleString('en-IN')}</span>
          </div>
          <div className="summary-row">
            <span>Already Paid:</span>
            <span className="text-green">₹{Number(fee.paidAmount).toLocaleString('en-IN')}</span>
          </div>
          <div className="summary-row highlight">
            <span>Outstanding Balance:</span>
            <span className="text-red font-bold">₹{outstanding.toLocaleString('en-IN')}</span>
          </div>
        </div>

        {error && (
          <div className="alert-error">
            <AlertCircle size={16} />
            <span>{error}</span>
          </div>
        )}

        <form onSubmit={handleSubmit} className="erp-form">
          <div className="form-group">
            <div className="label-with-action">
              <label>Payment Amount (INR) <span className="req">*</span></label>
              <button type="button" className="btn-link" onClick={handlePayFull}>
                Pay Full Balance (₹{outstanding})
              </button>
            </div>
            <input
              type="number"
              step="0.01"
              min="1"
              max={outstanding}
              placeholder={`Max ₹${outstanding}`}
              value={amount}
              onChange={(e) => {
                setAmount(e.target.value);
                setError('');
              }}
              required
              autoFocus
            />
          </div>

          <div className="form-group">
            <label>Payment Method <span className="req">*</span></label>
            <select value={paymentMethod} onChange={(e) => setPaymentMethod(e.target.value)}>
              <option value="UPI">UPI / Google Pay / PhonePe</option>
              <option value="Cash">Cash at Counter</option>
              <option value="Card">Debit / Credit Card</option>
              <option value="Bank Transfer">NEFT / RTGS Bank Transfer</option>
            </select>
          </div>

          <div className="form-group">
            <label>Payment Notes (Optional)</label>
            <input
              type="text"
              placeholder="e.g. Installment 1 / Reference ID"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </div>

          <div className="form-actions">
            <button type="button" className="btn-secondary" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={isSubmitting}>
              <CreditCard size={16} />
              <span>{isSubmitting ? 'Processing Transaction...' : 'Confirm & Generate Receipt'}</span>
            </button>
          </div>
        </form>
      </div>
    </Modal>
  );
};
