import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { studentApi } from '../../api/studentApi';
import { ArrowLeft, Check, AlertCircle } from 'lucide-react';

export const StudentCreate = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    dateOfBirth: '2004-01-01',
    gender: 'Male',
    email: '',
    phone: '',
    address: '',
    admissionDate: new Date().toISOString().slice(0, 10),
    course: 'MCA',
  });

  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const validate = () => {
    const errs = {};
    if (!formData.firstName.trim()) errs.firstName = 'First name is required';
    if (!formData.lastName.trim()) errs.lastName = 'Last name is required';
    if (!formData.email.trim()) {
      errs.email = 'Email is required';
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      errs.email = 'Invalid email address';
    }
    if (!formData.phone.trim()) errs.phone = 'Phone number is required';
    if (!formData.dateOfBirth) errs.dateOfBirth = 'Date of birth is required';
    if (!formData.admissionDate) errs.admissionDate = 'Admission date is required';
    if (!formData.course) errs.course = 'Course selection is required';

    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: null }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setServerError('');

    if (!validate()) return;

    try {
      setIsSubmitting(true);
      const res = await studentApi.createStudent(formData);
      if (res.success && res.data) {
        navigate(`/students/${res.data.id}`);
      }
    } catch (err) {
      const msg = err.response?.data?.message || 'Failed to admit student. Please try again.';
      setServerError(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="student-form-page">
      <div className="form-header-bar">
        <Link to="/students" className="btn-back">
          <ArrowLeft size={18} />
          <span>Back to Students</span>
        </Link>
        <h1 className="page-title">New Student Admission</h1>
      </div>

      {serverError && (
        <div className="alert-error">
          <AlertCircle size={18} />
          <span>{serverError}</span>
        </div>
      )}

      <div className="panel-card">
        <form onSubmit={handleSubmit} className="erp-form">
          <div className="form-section-title">Personal Information</div>
          <div className="form-row-2">
            <div className="form-group">
              <label>First Name <span className="req">*</span></label>
              <input
                type="text"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                placeholder="e.g. Aarav"
                className={errors.firstName ? 'input-error' : ''}
              />
              {errors.firstName && <span className="error-text">{errors.firstName}</span>}
            </div>

            <div className="form-group">
              <label>Last Name <span className="req">*</span></label>
              <input
                type="text"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                placeholder="e.g. Sharma"
                className={errors.lastName ? 'input-error' : ''}
              />
              {errors.lastName && <span className="error-text">{errors.lastName}</span>}
            </div>
          </div>

          <div className="form-row-3">
            <div className="form-group">
              <label>Date of Birth <span className="req">*</span></label>
              <input
                type="date"
                name="dateOfBirth"
                value={formData.dateOfBirth}
                onChange={handleChange}
                className={errors.dateOfBirth ? 'input-error' : ''}
              />
              {errors.dateOfBirth && <span className="error-text">{errors.dateOfBirth}</span>}
            </div>

            <div className="form-group">
              <label>Gender <span className="req">*</span></label>
              <select name="gender" value={formData.gender} onChange={handleChange}>
                <option value="Male">Male</option>
                <option value="Female">Female</option>
                <option value="Other">Other</option>
              </select>
            </div>

            <div className="form-group">
              <label>Course <span className="req">*</span></label>
              <select name="course" value={formData.course} onChange={handleChange}>
                <option value="MCA">MCA</option>
                <option value="BCA">BCA</option>
                <option value="B.Tech Computer Science">B.Tech Computer Science</option>
                <option value="MBA">MBA</option>
              </select>
            </div>
          </div>

          <div className="form-section-title">Contact & Enrollment</div>
          <div className="form-row-2">
            <div className="form-group">
              <label>Email Address <span className="req">*</span></label>
              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                placeholder="student@example.com"
                className={errors.email ? 'input-error' : ''}
              />
              {errors.email && <span className="error-text">{errors.email}</span>}
            </div>

            <div className="form-group">
              <label>Phone Number <span className="req">*</span></label>
              <input
                type="tel"
                name="phone"
                value={formData.phone}
                onChange={handleChange}
                placeholder="+91 9876543210"
                className={errors.phone ? 'input-error' : ''}
              />
              {errors.phone && <span className="error-text">{errors.phone}</span>}
            </div>
          </div>

          <div className="form-row-2">
            <div className="form-group">
              <label>Residential Address</label>
              <input
                type="text"
                name="address"
                value={formData.address}
                onChange={handleChange}
                placeholder="City, Street, State"
              />
            </div>

            <div className="form-group">
              <label>Admission Date <span className="req">*</span></label>
              <input
                type="date"
                name="admissionDate"
                value={formData.admissionDate}
                onChange={handleChange}
                className={errors.admissionDate ? 'input-error' : ''}
              />
              {errors.admissionDate && <span className="error-text">{errors.admissionDate}</span>}
            </div>
          </div>

          <div className="form-actions">
            <Link to="/students" className="btn-secondary">
              Cancel
            </Link>
            <button type="submit" className="btn-primary" disabled={isSubmitting}>
              <Check size={18} />
              <span>{isSubmitting ? 'Registering...' : 'Admit Student'}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
