# Student Management Mini-ERP — Master Prompt & Directives

## 1. Project Objective
Build a web-based **Student Management Mini-ERP** that allows authorized users to manage students and their fees.
- Scope: 2–3 working days assessment project.
- Focus: Correctness, clean architecture, maintainability, security, database integrity, testability, and demonstrable functionality.

## 2. Core Modules Implemented
1. Dashboard (KPI counts, total fees billed/collected/outstanding, recent feeds)
2. Student Admission (STU-YYYY-XXXXX code generator, validation)
3. Student Viewing & Profile Detail
4. Student Editing & Update
5. Student Soft Deactivation/Activation
6. Student Listing with server-side Search, Filters, and Pagination
7. Fee Management & Creation
8. Partial and Full Payments with ACID transactions
9. Payment Receipts with unique sequential numbering (REC-YYYY-XXXXXX)
10. Outstanding Balance Calculation & Overpayment Prevention Guard
11. JWT-based Authentication & Authorization (Admin vs Staff)
12. Centralized Global Exception Handling (`ApiResponse<T>`)
13. SQL Server relational persistence with EF Core 9 Fluent API
14. Change Request CR-01: Printable Receipts (@media print) & CSV Export

## 3. Technology Stack
- **Frontend**: React 18, Vite 6, React Router v6, Axios, Lucide React, Enterprise CSS
- **Backend**: ASP.NET Core 9 Web API, C#, Entity Framework Core 9, JWT Bearer, BCrypt.Net-Next, Swagger
- **Database**: Microsoft SQL Server (`decimal(18,2)` currency precision, `DeleteBehavior.Restrict`)
