# Antigravity Phase Prompt — Frontend Design + Authentication

## Student Management Mini-ERP | Known Template Visual Reference

**Design reference:** https://preview.themewagon.com/known/

**Role:** Act as a careful senior React/.NET engineer working inside an existing codebase. Improve the frontend presentation and complete the authentication journey without breaking the application that already exists.

---

## 1. Main Objective

Implement the next frontend phase of the existing **Student Management Mini-ERP** using the ThemeWagon **Known** education template as a **visual reference**. Add a complete Login / Sign-up / Logout user journey and make the existing application feel visually consistent with the reference while preserving all existing working functionality.

This is an enhancement to an existing project, **not a rebuild**. Do not generate a replacement application, scaffold a second app, or rewrite working modules from scratch.

## 2. Non-Negotiable Preservation Rules

Before touching code, inspect the active workspace and the current Git status/diff.

1. Preserve the current repository, folder structure, routes, backend, database, DTO contracts, business rules, and working student/fee/dashboard functionality.
2. Never run destructive commands such as `git reset --hard`, `git clean`, mass file deletion, or overwrite the project with a template.
3. Do not delete, rename, move, or wholesale-rewrite existing files. Make the smallest targeted edits required for the new design/authentication. Prefer new, scoped components and CSS files over broad refactors.
4. Do not alter existing student, fee, payment, receipt, or dashboard behavior to achieve a visual result. Existing form fields, role rules, validations, API calls, pagination, search/filter behavior, and business calculations must continue to work.
5. Do not replace the .NET API or SQL Server database. No schema changes are allowed unless code inspection proves that registration cannot be supported with the existing `User` fields. If a schema change is truly necessary, stop and explain why before applying it.
6. Do not change existing endpoint paths or request/response shapes. Add a new endpoint only if a required capability does not exist.
7. Do not remove existing dependencies or add a UI framework just for styling. Reuse the current React/Vite/React Router/Axios setup and CSS approach unless the repository proves otherwise.
8. Use the reference for design inspiration, not as a reason to copy a full downloaded template into this repository. Do not import jQuery or an unrelated Bootstrap template into the React app.
9. Keep the frontend presentation changes focused and scoped. Existing screens may receive targeted visual styling for consistency, but must retain their current content and behavior.
10. If current uncommitted user changes exist, preserve them. Do not discard or silently overwrite them.

**Expected approach:** make targeted UI/style changes and additive authentication work; preserve existing application behavior and data contracts.

---

## 3. Inspect the Existing Project First

Do not assume this prompt describes every current file accurately. Inspect the active repository and report the findings before edits.

Expected project architecture is:

- Frontend: React + Vite + React Router + Axios.
- Backend: ASP.NET Core Web API, C#, Entity Framework Core.
- Database: Microsoft SQL Server.
- Authentication: JWT, roles `Admin` and `Staff`.

In the current scaffold, relevant frontend files include (confirm actual paths before editing):

```text
frontend/src/App.jsx
frontend/src/styles.css
frontend/src/context/AuthContext.jsx
frontend/src/api/client.js
frontend/src/api/authApi.js
frontend/src/components/layout/AppLayout.jsx
frontend/src/components/layout/ProtectedRoute.jsx
frontend/src/pages/Login.jsx
frontend/src/pages/Dashboard.jsx
frontend/src/pages/Students/StudentList.jsx
frontend/src/pages/Students/StudentCreate.jsx
frontend/src/pages/Students/StudentEdit.jsx
frontend/src/pages/Students/StudentDetail.jsx
```

The existing API has been designed around these authentication endpoints:

```http
POST /api/auth/login
GET  /api/auth/me
POST /api/auth/logout
```

Inspect `AuthController`, `IAuthService`/`AuthService`, DTOs, user entity, password hashing, role enum, and uniqueness constraints before adding sign-up. **Do not create a duplicate endpoint or parallel auth state system if equivalent functionality already exists.**

### Phase 0 output (inspection only)

Before coding, provide:

- Git status and any existing uncommitted changes (summarize; do not revert them).
- Existing routes and authentication behavior.
- Existing frontend styling approach and whether a CSS framework is configured.
- Existing backend auth endpoints and whether registration already exists.
- A short, file-specific plan describing the smallest set of changes.

Do not start implementation until this inspection summary is complete.

---

## 4. Use the Known Template as a Visual Reference

Review the reference page in a browser before implementation:

https://preview.themewagon.com/known/

The Known page is an education-oriented landing-page design. Use its visual language—clean white/light surfaces, green accent color, spacious sections, clear typography, simple navigation, polished cards, restrained imagery, form emphasis, and responsive behavior—where appropriate for this ERP.

**Important adaptation:** this is a Student Management **ERP**, not a public online-course marketing site. Do not copy the full marketing-page structure into the secured ERP and do not add irrelevant course catalog, testimonials, teachers, newsletter, or contact sections. Translate the style into a professional education-administration interface.

### Visual direction

- Bright, clean, approachable education-related visual identity.
- White/light neutral backgrounds with a consistent green accent inspired by the reference.
- Strong, readable page headings and understated secondary text.
- Comfortable whitespace and consistent spacing.
- Rounded but restrained cards, clean form fields, subtle borders/shadows, clear focus states.
- Consistent button hierarchy and readable status badges.
- Responsive layouts for desktop, tablet, and mobile.
- Use existing assets first. Do not add random remote image URLs or large image libraries. If a decorative image is necessary, use an existing local asset or a properly licensed asset and provide a graceful fallback.
- Use CSS variables and scoped class names rather than broad selectors that unexpectedly affect unrelated screens.
- Avoid excessive gradients, animations, oversized marketing hero sections, and decorative elements that reduce the density/readability needed for an ERP.

### Suggested visual mapping

- **Login / Sign-up:** a polished, education-inspired split or contained layout with a branded visual panel and focused form area. On mobile, stack the panels. Keep the form compact and accessible.
- **Protected ERP shell:** retain the current navigation structure and screen information architecture; apply only the scoped styling required for a consistent theme. Do not remove or rename working navigation destinations.
- **Dashboard:** preserve the current metrics and actions; improve spacing, card hierarchy, labels, and responsive arrangement only.
- **Student and Fee screens:** preserve all existing data fields/actions and table behavior; improve visual consistency of forms, tables, filters, pagination, badges, empty/loading/error states.

Do not chase pixel-perfect copies of an education landing page if that would damage the ERP workflows. Aim for a polished, recognizable adaptation of the reference design.

---

## 5. Authentication Requirements

Complete and verify the full user journey, using existing mechanisms wherever possible.

### A. Login

- Keep the existing `/login` route unless the active codebase already uses a different established route.
- Continue using the current login API request contract (inspect it; likely `usernameOrEmail` and `password`).
- Preserve existing JWT and AuthContext behavior.
- Include accessible labels, password visibility toggle, pending/loading state, validation, API error message, and disabled submit behavior while the request is in progress.
- Add a link to Sign up.
- On success, navigate to the originally requested protected route when supported, otherwise to `/dashboard`.
- If a user is already authenticated, avoid showing the login form unnecessarily and redirect to the appropriate protected page.

### B. Sign-up / Registration

- Add a dedicated route such as `/signup` (or `/register` if that naming already exists).
- Form fields should align exactly with the registration DTO and available user model. At minimum, where supported: username, email, password, confirm password.
- Add client-side checks for required fields, valid email, password confirmation, and reasonable password length. Server validation remains authoritative.
- Display duplicate username/email and other server validation errors in a readable way.
- Show a pending state and prevent duplicate submissions.
- On successful registration, show a clear success message and direct the user to login, unless an existing secure flow intentionally signs them in.
- Include a link back to Login.
- Never store or return the plain-text password.

### C. Registration API: additive only

First verify whether a registration endpoint already exists. If not, add the smallest compatible endpoint, likely:

```http
POST /api/auth/register
```

Use the project's existing API response/error convention, DTO conventions, dependency injection, exception handling, and BCrypt implementation.

Security rules:

- Public sign-up must **never** allow the client to choose `Admin` or another privileged role.
- Publicly registered users must receive the least-privileged normal role (`Staff`, or the exact existing non-admin role used by the codebase).
- Ignore/reject any client-supplied role field; never bind privileged properties directly from a request.
- Hash the password using the already-installed password hashing mechanism (BCrypt in the current scaffold) before storing it.
- Normalize username/email consistently with the existing login and uniqueness strategy.
- Handle duplicate username/email as a validation/conflict response, not an unhandled server error.
- Respect existing unique database indexes/constraints and catch uniqueness races appropriately.
- Set the new account's active state according to the existing user model's safe default.
- Do not add a database migration if existing columns are sufficient.
- Do not alter the seeded Admin account or existing logins.

If registration is not appropriate for the assessment's security model or the active project already has another intended provisioning flow, **do not invent public Admin creation**. Explain the assumption and implement Staff-only sign-up, with Admin creation continuing through the existing seed/admin process.

### D. Logout

- Preserve the existing logout action and `signOut` logic.
- Keep logout visible and easy to reach in the current authenticated app shell (header/user menu is preferred).
- Call the existing logout API if implemented; always clear the local client auth state according to the current app's established behavior, even if a stateless JWT logout request fails.
- After logout, redirect to `/login` and prevent access to protected routes.
- Do not claim JWT revocation on the server if the API is only stateless and does not maintain a token blacklist/session store.

### E. Route authorization

- Keep all existing protected routes protected.
- `Admin`-only routes such as student create/edit/deactivate and fee assignment remain Admin-only where the backend currently requires it.
- `Staff` can use only the features permitted by existing policies, such as viewing records and recording payments where currently allowed.
- Frontend role checks are for navigation/UX only. The backend remains authoritative.
- Do not weaken or bypass `ProtectedRoute`, the API authorization attributes, or JWT validation.
- Handle expired/invalid tokens (HTTP 401) via the current Axios interceptor/auth state behavior, avoiding redirect loops.

### F. Token handling

- Inspect and preserve the current token-storage strategy unless there is a critical demonstrated defect. Do not introduce a second source of truth for tokens/user state.
- Do not log tokens, credentials, password values, or full sensitive auth payloads.
- If you believe token storage requires a security-related architecture change, explain the trade-off first and do not silently rewrite the flow.

---

## 6. Frontend Design Implementation Requirements

1. Create a small, coherent set of design tokens for colors, spacing, radius, typography, borders, shadows, and focus states.
2. Prefer a new scoped theme stylesheet such as `known-theme.css` or `auth-theme.css` over a wholesale replacement of the current stylesheet.
3. Keep styles scoped to the affected pages/components. Do not use broad rules that unintentionally restyle all forms, buttons, or tables without checking their impact.
4. Preserve the app's existing pages, routes, fields, actions, and API integration. Update only their presentation as required by the theme.
5. Add clear loading, empty, validation, success, and failure states wherever the existing workflow needs them.
6. Ensure forms are keyboard accessible, inputs have labels, focus indicators are visible, contrast is readable, and status is not conveyed by color alone.
7. Ensure responsive layouts at approximately 360px, 768px, 1024px, and desktop widths. Do not allow horizontal overflow on mobile.
8. Keep the UI consistent between Login, Sign-up, dashboard, students, fees, and shared layout while preserving existing behavior.
9. Do not hardcode API results, dashboard metrics, users, payment history, or demo data into UI components. Continue using the current backend API.
10. Do not introduce unrelated features or pages.

---

## 7. Implementation Plan — One Bounded Task at a Time

Because the available Antigravity model has limited weekly usage, do not implement the entire phase in one huge generation. Complete one task, test it, summarize, then proceed to the next prompt.

### Task 1 — Workspace Audit (read-only)

Inspect the repository, Git status, current frontend, auth flow, backend auth endpoint, and design reference. Produce the file-specific plan. Make no code changes during this task.

**Acceptance:** clearly lists which files are already working, what can be reused, the minimum necessary new/modified files, and any registration API gaps.

### Task 2 — Design Foundation and Auth Screens

Implement the Known-inspired, ERP-appropriate styling with a scoped stylesheet and update the existing Login screen with minimal targeted markup/style changes. Add the Sign-up page and route. Preserve all existing app modules and behavior.

**Acceptance:** Login and Sign-up are polished, responsive, accessible, use the existing auth state/API integration patterns, show validation/loading/error states, and link to each other. Existing protected pages continue to render.

### Task 3 — Add Registration API Only If Missing

If the audit confirms no registration endpoint, add a minimal Staff-only registration capability using the existing entities, DTOs, services, response conventions, password hash, validation, and database uniqueness constraints. Do not change existing login/logout contracts.

**Acceptance:** valid registration creates an active least-privilege account with a password hash; invalid requests and duplicates fail safely; client-supplied Admin role cannot be granted; existing Admin login still works.

### Task 4 — Connect and Verify Full Auth Flow

Wire Sign-up to the real API, make login/logout flows work, preserve the current auth state/interceptors, and keep existing route guards intact.

**Acceptance:** user can register as Staff, return to login, log in, navigate authorized pages, log out, and cannot access protected routes after logout. A Staff account cannot gain Admin-only access.

### Task 5 — Carefully Align Existing ERP Screens

Apply scoped theme styles to the existing app shell, dashboard, student list/forms/details, and fee/payment views only as needed to create a coherent style. Do not remove fields/actions or change data flow. If this task risks changing working markup extensively, stop and report the smallest safer alternative.

**Acceptance:** all existing dashboard/student/fee functions remain intact; style is consistent and responsive; the reference influences the visual system without turning the application into a marketing landing page.

### Task 6 — Regression Verification

Run the available checks and report truthfully what passed or could not run.

**Acceptance:** see checklist below; no unverified claim that an API/build/test passed.

---

## 8. Required Test Checklist

### Frontend

- `npm install` only if dependencies are not already installed or the lockfile requires it; do not regenerate dependency files unnecessarily.
- Run the existing production build command (likely `npm run build`).
- Verify Login renders and submits the expected login contract.
- Verify validation, loading, invalid credentials, and server error messages.
- Verify Sign-up validation, password confirmation, duplicate error handling, and success flow.
- Verify logout clears auth state and redirects to Login.
- Verify direct navigation to `/dashboard` while signed out redirects to Login.
- Verify valid login can access permitted routes.
- Verify Staff cannot access Admin-only screens/actions.
- Verify Admin's existing student create/edit/deactivate and fee assignment workflows still work.
- Verify student search, filters, pagination, fee payment, receipt, and dashboard data are not broken.
- Check desktop and mobile layouts, keyboard focus, and browser console for errors.

### Backend / Database (if registration API is added)

- Valid registration succeeds.
- Duplicate username and email are safely rejected.
- Invalid email/empty fields/weak password are rejected.
- Client attempts to request the Admin role do not create an Admin.
- Stored password is a hash, not plain text.
- Existing admin seed/login still works.
- Existing login, `/me`, and logout endpoints remain compatible.
- Build/test the API and verify against SQL Server when the SDK/database are available.

Do not claim tests passed unless you ran them and observed successful results. If a required SDK, SQL Server instance, or environment variable is unavailable, report the exact blocker and give the command the developer should run locally.

---

## 9. Constraints for Code Changes

- Keep controllers thin and reuse existing service conventions.
- Reuse the existing DTO and response envelope pattern.
- Keep all financial logic untouched in this phase.
- Avoid unrelated package upgrades.
- Do not hardcode credentials, connection strings, signing keys, tokens, or personal data.
- Do not change seeded demo credentials.
- Do not silently adjust CORS, JWT validation, database configuration, or environment files unrelated to registration.
- Do not implement fake login or local-only registration.
- Do not add fake dashboard data to make the design look fuller.

---

## 10. Final Handoff Required

At the end of each task, provide:

1. What changed and why.
2. Exact files created or modified.
3. Confirmation that existing modules/API contracts were preserved.
4. Commands executed and actual test/build outcomes.
5. Any test that could not be run and the reason.
6. Known limitations or security decisions.
7. The next smallest recommended task.

At the end of this phase, update the existing README/API docs only where needed to document sign-up and auth configuration; do not rewrite the documentation wholesale. If there is an existing `WORK_LOG.md`, add a short, factual entry describing what AI assistance was used for and which checks were actually run.

## Start Here

Begin with **Task 1 — Workspace Audit (read-only)**. Do not edit, delete, move, or overwrite any project files until you have inspected the current codebase and provided the file-specific implementation plan. Then wait for my confirmation before beginning Task 2.
