import React from 'react';

export const Badge = ({ status, type = 'status' }) => {
  const getBadgeClass = () => {
    const val = String(status).toLowerCase();
    if (val === 'active' || val === 'true' || val === 'paid') return 'badge-success';
    if (val === 'inactive' || val === 'false' || val === 'overdue') return 'badge-danger';
    if (val === 'partial') return 'badge-warning';
    if (val === 'pending') return 'badge-info';
    return 'badge-neutral';
  };

  const displayText = () => {
    if (typeof status === 'boolean') {
      return status ? 'Active' : 'Inactive';
    }
    return status;
  };

  return <span className={`badge ${getBadgeClass()}`}>{displayText()}</span>;
};
