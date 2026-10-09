import React from 'react';
import { Modal } from './Modal';
import { Printer, CheckCircle } from 'lucide-react';

export const ReceiptModal = ({ isOpen, onClose, receipt }) => {
  if (!receipt) return null;

  const handlePrint = () => {
    window.print();
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Payment Receipt" maxWidth="600px">
      <div className="printable-receipt" id="printable-receipt">
        <div className="receipt-banner">
          <div className="receipt-icon">
            <CheckCircle size={32} color="#16a34a" />
          </div>
          <div className="receipt-header-text">
            <h2>ACADEMIC FEE PAYMENT RECEIPT</h2>
            <p className="receipt-subtitle">Student Management Mini-ERP • Official Voucher</p>
          </div>
        </div>

        <div className="receipt-meta-grid">
          <div className="meta-box">
            <span className="meta-label">Receipt Number</span>
            <span className="meta-value font-mono font-bold text-indigo">{receipt.receiptNumber}</span>
          </div>
          <div className="meta-box">
            <span className="meta-label">Payment Date</span>
            <span className="meta-value">{new Date(receipt.paymentDate).toLocaleString()}</span>
          </div>
          <div className="meta-box">
            <span className="meta-label">Payment Method</span>
            <span className="meta-value">{receipt.paymentMethod}</span>
          </div>
          <div className="meta-box">
            <span className="meta-label">Cashier / Staff</span>
            <span className="meta-value">{receipt.recordedBy}</span>
          </div>
        </div>

        <div className="receipt-divider"></div>

        <div className="receipt-student-details">
          <h4>Student Particulars</h4>
          <div className="details-row">
            <span>Student Code:</span>
            <span className="font-semibold">{receipt.studentCode}</span>
          </div>
          <div className="details-row">
            <span>Student Name:</span>
            <span className="font-semibold">{receipt.studentName}</span>
          </div>
          <div className="details-row">
            <span>Enrolled Course:</span>
            <span className="font-semibold">{receipt.course}</span>
          </div>
          {receipt.notes && (
            <div className="details-row">
              <span>Payment Notes:</span>
              <span>{receipt.notes}</span>
            </div>
          )}
        </div>

        <div className="receipt-financial-table">
          <table>
            <thead>
              <tr>
                <th>Description</th>
                <th className="text-right">Amount (INR)</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Total Course Fee</td>
                <td className="text-right">₹{Number(receipt.feeTotalAmount).toLocaleString('en-IN', { minimumFractionDigits: 2 })}</td>
              </tr>
              <tr>
                <td>Total Paid to Date</td>
                <td className="text-right">₹{Number(receipt.feePaidAmount).toLocaleString('en-IN', { minimumFractionDigits: 2 })}</td>
              </tr>
              <tr className="highlight-row">
                <td><strong>Amount Paid This Transaction</strong></td>
                <td className="text-right font-bold text-green">₹{Number(receipt.amount).toLocaleString('en-IN', { minimumFractionDigits: 2 })}</td>
              </tr>
              <tr className="balance-row">
                <td><strong>Remaining Outstanding Balance</strong></td>
                <td className="text-right font-bold text-red">₹{Number(receipt.remainingOutstanding).toLocaleString('en-IN', { minimumFractionDigits: 2 })}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div className="receipt-signatures">
          <div className="signature-box">
            <div className="signature-line"></div>
            <span>Authorized Signature</span>
          </div>
          <div className="signature-box">
            <div className="signature-line"></div>
            <span>Depositor Signature</span>
          </div>
        </div>
      </div>

      <div className="receipt-modal-actions no-print">
        <button className="btn-secondary" onClick={onClose}>
          Close
        </button>
        <button className="btn-primary" onClick={handlePrint}>
          <Printer size={16} />
          <span>Print Receipt (PDF)</span>
        </button>
      </div>
    </Modal>
  );
};
