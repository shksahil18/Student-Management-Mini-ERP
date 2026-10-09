import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { studentApi } from '../../api/studentApi';
import { ArrowLeft, Save, AlertCircle } from 'lucide-react';

export const StudentEdit = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    gender: 'Male',
    email: '',
    phone: '',
    address: '',
    admissionDate: '',
    course: 'MCA',
  });

  const [studentCode, setStudentCode] = useState('');
  const [loading, setLoading] = useState(true);
  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    fetchStudent();
  }, [id]);

  const fetchStudent = async () => {
    try {
      setLoading(true);
      const res = await studentApi.getStudentById(id);
      if (res.success && res.data) {
        const s = res.data;
        setStudentCode(s.studentCode);
        setFormData({
          firstName: s.firstName,
          lastName: s.lastName,
          dateOfBirth: s.dateOfBirth.slice(0, 10),
          gender: s.gender,
          email: s.email,
          phone: s.phone,
          address: s.address || '',
          admissionDate: s.admissionDate.slice(0, 10),
          course: s.course,
        });
      }
    } catch (err) {
      setServerError('Failed to load student details.');
    } finally {
      setLoading(false);
    }
  };

  const validate = () => {
    const errs = {};
    if (!formData.firstName.trim()) errs.firstName = 'First name is required';
    if (!formData.lastName.trim()) errs.lastName = 'Last name is required';
    if (!formData.email.trim()) errs.email = 'Email is required';
    if (!formData.phone.trim()) errs.phone = 'Phone number is required';
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (errors[name]) setErrors((prev) => ({ ...prev, [name]: null }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      setIsSubmitting(true);
      setServerError('');
      const res = await studentApi.updateStudent(id, formData);
      if (res.success) {
        navigate(`/students/${id}`);
      }
    } catch (err) {
      setServerError(err.response?.data?.message || 'Failed to update student profile.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="page-loading">
        <div className="spinner"></div>
        <p>Loading profile...</p>
      </div>
    );
  }

  return (
    <div className="student-form-page">
      <div className="form-header-bar">
        <Link to={`/students/${id}`} className="btn-back">
          <ArrowLeft size={18} />
          <span>Back to Profile</span>
        </Link>
        <div>
          <h1 className="page-title">Edit Student Profile</h1>
          <p className="page-description">Student Code: <strong className="text-indigo">{studentCode}</strong></p>
        </div>
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
              />
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

          <div className="form-section-title">Contact Information</div>
          <div className="form-row-2">
            <div className="form-group">
              <label>Email Address <span className="req">*</span></label>
              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
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
              />
            </div>

            <div className="form-group">
              <label>Admission Date <span className="req">*</span></label>
              <input
                type="date"
                name="admissionDate"
                value={formData.admissionDate}
                onChange={handleChange}
              />
            </div>
          </div>

          <div className="form-actions">
            <Link to={`/students/${id}`} className="btn-secondary">
              Cancel
            </Link>
            <button type="submit" className="btn-primary" disabled={isSubmitting}>
              <Save size={18} />
              <span>{isSubmitting ? 'Saving...' : 'Save Changes'}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
