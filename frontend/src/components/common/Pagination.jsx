import React from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';

export const Pagination = ({ page, totalPages, totalItems, onPageChange }) => {
  if (totalPages <= 1) return null;

  return (
    <div className="erp-pagination">
      <div className="pagination-info">
        Page <span className="font-semibold">{page}</span> of <span className="font-semibold">{totalPages}</span> ({totalItems} total records)
      </div>

      <div className="pagination-buttons">
        <button
          onClick={() => onPageChange(page - 1)}
          disabled={page <= 1}
          className="btn-page"
        >
          <ChevronLeft size={16} />
          <span>Previous</span>
        </button>

        <span className="current-page-pill">{page}</span>

        <button
          onClick={() => onPageChange(page + 1)}
          disabled={page >= totalPages}
          className="btn-page"
        >
          <span>Next</span>
          <ChevronRight size={16} />
        </button>
      </div>
    </div>
  );
};
