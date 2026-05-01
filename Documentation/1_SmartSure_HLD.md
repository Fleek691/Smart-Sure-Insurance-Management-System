# SmartSure Platform — High-Level Design (HLD)

**Version:** 2.0  
**Stack:** .NET 10 · Angular 17 · SQL Server · RabbitMQ · Ocelot API Gateway  

---

## 1. Executive Summary

SmartSure is a full-stack insurance management platform that enables customers to browse insurance products, purchase policies, file and track claims, and manage their accounts. Administrators can review claims, manage users, generate reports, and view a complete audit trail. The system is built on a microservices architecture to ensure independent deployability, clear domain boundaries, and horizontal scalability.

---

## 2. System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                                  │
│                                                                      │
│   ┌──────────────────────────────────────────────────────────────┐  │
│   │              Angular 17 SPA  (localhost:4200)                │  │
│   │   Customer Portal  │  Admin Console  │  Auth Pages           │  │
│   └──────────────────────────────┬───────────────────────────────┘  │
└─────────────────────────────────┼───────────────────────────────────┘
                                  │  HTTPS / REST (JSON)
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        GATEWAY LAYER                                 │
│                                                                      │
│   ┌──────────────────────────────────────────────────────────────┐  │
│   │         Ocelot API Gateway  (localhost:5000)                 │  │
│   │   JWT Validation · Rate Limiting (100 req/s) · Routing       │  │
│   └──────┬──────────────┬──────────────┬──────────────┬──────────┘  │
└──────────┼──────────────┼──────────────┼──────────────┼─────────────┘
           │              │              │              │
           ▼              ▼              ▼              ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│  Identity    │ │   Policy     │ │   Claims     │ │   Admin      │
│  Service     │ │   Service    │ │   Service    │ │   Service    │
│  :5001       │ │   :5002      │ │   :5003      │ │   :5004      │
│              │ │              │ │              │ │              │
│  AuthDb      │ │  PolicyDb    │ │  ClaimsDb    │ │  AdminDb     │
│  (SQL Server)│ │  (SQL Server)│ │  (SQL Server)│ │  (SQL Server)│
└──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘
        │                │                │                │
        └────────────────┴────────────────┴────────────────┘
                                  │
                                  ▼
                    ┌─────────────────────────┐
                    │   RabbitMQ Message Bus   │
                    │   (Event-Driven Comms)   │
                    └─────────────────────────┘
                                  │
              ┌───────────────────┼───────────────────┐
              ▼                   ▼                   ▼
     PolicyService          ClaimsService        AdminService
     (publishes)            (publishes)          (consumes all)
```

---

## 3. Component Responsibilities

### 3.1 API Gateway (Ocelot)
- Single entry point for all client traffic at `localhost:5000/gateway/...`
- JWT Bearer token validation before forwarding requests
- Route-based forwarding to downstream services (ports 5001–5004)
- Global rate limiting: 100 requests/second per client
- Circuit breaker: 3 exceptions before breaking, 30-second recovery window
- Swagger aggregation across all services

### 3.2 IdentityService (Port 5001)
- OTP-based user registration with email verification (10-minute OTP window)
- Email/password login with BCrypt password hashing
- Google OAuth 2.0 sign-in with account linking
- JWT access token issuance (60-minute expiry, multi-audience)
- Refresh token rotation (7-day expiry, single-use)
- Password reset via 6-digit OTP (15-minute window)
- Role-based access control: CUSTOMER and ADMIN roles
- Admin user management: activate/deactivate accounts, change roles

### 3.3 PolicyService (Port 5002)
- Insurance product catalogue (Vehicle and Home types, 26 products seeded)
- Monthly premium calculation: `basePremium × (coverage / 100,000) × max(termMonths / 12, 0.5)`
- Direct policy purchase (non-payment flow)
- Razorpay two-step payment flow: create order → verify HMAC signature → activate policy
- Policy lifecycle management: Active → Cancelled | Expired
- Admin product CRUD and policy status management
- Product catalogue cached for 60 minutes; user policies cached for 2 minutes

### 3.4 ClaimsService (Port 5003)
- Claim creation against active policies (cross-service policy status verification)
- Claim state machine: Draft → Submitted → UnderReview → Approved | Rejected
- Document upload: base64-encoded, stored in Mega.nz with local filesystem fallback (15-second timeout)
- Supported document types: PDF, JPG, PNG (max 10 MB)
- Admin review workflow with enforced state transitions
- Event publishing on every status change

### 3.5 AdminService (Port 5004)
- Audit log persistence: consumes all domain events from RabbitMQ
- Policy, claims, and revenue reports with date range filtering
- PDF report export (raw PDF 1.4 generation)
- Paginated audit log queries (up to 200 per page)
- Dashboard stats endpoint (audit-log based aggregation)

---

## 4. Technology Stack

| Layer | Technology | Version |
|---|---|---|
| Frontend | Angular | 17.3+ |
| Frontend language | TypeScript | 5.x |
| Backend framework | ASP.NET Core | .NET 10 |
| API Gateway | Ocelot | Latest |
| ORM | Entity Framework Core | .NET 10 |
| Database | SQL Server | 2019+ |
| Message broker | RabbitMQ via MassTransit | Latest |
| Authentication | JWT Bearer + BCrypt | — |
| OAuth | Google OAuth 2.0 | — |
| Payment | Razorpay | — |
| File storage | Mega.nz (MegaApiClient) | — |
| Email | SMTP via MailKit | — |
| Logging | Serilog (file + console) | — |
| API docs | Swagger / OpenAPI | — |
| Config | DotNetEnv (.env files) | — |

---

## 5. Authentication & Security Architecture

```
┌─────────────┐     POST /gateway/auth/login      ┌──────────────────┐
│   Browser   │ ─────────────────────────────────► │  IdentityService │
│             │ ◄─────────────────────────────────  │                  │
│             │   { token, refreshToken, roles }    │  Issues JWT      │
│             │                                     │  (60 min expiry) │
└─────────────┘                                     └──────────────────┘
       │
       │  Authorization: Bearer <JWT>
       ▼
┌─────────────┐     Validates JWT signature        ┌──────────────────┐
│   Gateway   │ ─────────────────────────────────► │  Downstream      │
│   (Ocelot)  │     Forwards if valid              │  Service         │
└─────────────┘                                     └──────────────────┘
```

**JWT Claims:** `sub` (userId), `email`, `role` (CUSTOMER/ADMIN), `aud` (5 audiences for all services)

**Token Refresh Flow:**
```
Browser ──► POST /gateway/auth/refresh-token ──► IdentityService
         ◄── New JWT + New RefreshToken (old invalidated)
```

---

## 6. Event-Driven Messaging Architecture

All inter-service communication for audit and notification purposes uses RabbitMQ via MassTransit.

```
┌──────────────────────────────────────────────────────────────────┐
│                     RabbitMQ Exchange                             │
│                                                                   │
│  Publishers:                    Consumers:                        │
│                                                                   │
│  IdentityService ──────────────► AdminService (audit-audit-logs) │
│    UserRegisteredEvent                                            │
│                                                                   │
│  PolicyService ────────────────► AdminService                    │
│    PolicyActivatedEvent         IdentityService (logging)        │
│    PolicyCancelledEvent                                           │
│                                                                   │
│  ClaimsService ────────────────► AdminService                    │
│    ClaimSubmittedEvent          IdentityService (logging)        │
│    ClaimStatusChangedEvent      PolicyService (logging)          │
│    ClaimApprovedEvent                                             │
│    ClaimRejectedEvent                                             │
└──────────────────────────────────────────────────────────────────┘
```

**Event Resilience:** All event publishing is non-fatal. If RabbitMQ is unavailable, the primary operation (policy activation, claim status change) still succeeds and a warning is logged.

---

## 7. Data Architecture

Each microservice owns its own SQL Server database. There are no cross-service database joins.

```
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│    AuthDb        │  │    PolicyDb      │  │    ClaimsDb      │  │    AdminDb       │
│                  │  │                  │  │                  │  │                  │
│  Users           │  │  InsuranceTypes  │  │  Claims          │  │  AuditLogs       │
│  Passwords       │  │  InsuranceSubTypes│  │  ClaimDocuments  │  │  Reports         │
│  Roles           │  │  Policies        │  │  ClaimStatusHist │  │                  │
│  UserRoles       │  │  PolicyDetails   │  │                  │  │                  │
│  PasswordReset   │  │  VehicleDetails  │  │                  │  │                  │
│  Tokens          │  │  HomeDetails     │  │                  │  │                  │
│                  │  │  Premiums        │  │                  │  │                  │
│                  │  │  Payments        │  │                  │  │                  │
└─────────────────┘  └─────────────────┘  └─────────────────┘  └─────────────────┘
```

---

## 8. Key Business Flows

### 8.1 User Registration Flow

```
Customer ──► POST /gateway/auth/register
              │
              ▼
         IdentityService validates input
              │
              ▼
         OTP generated & cached (10 min)
              │
              ▼
         Email sent via SMTP
              │
         Customer submits OTP
              │
              ▼
         POST /gateway/auth/register/verify-otp
              │
              ▼
         User created in AuthDb
         JWT + RefreshToken issued
         UserRegisteredEvent published ──► AdminService (audit log)
```

### 8.2 Policy Purchase (Razorpay) Flow

```
Customer ──► POST /gateway/policies/payment/create-order
              │
              ▼
         PolicyService validates input
         Razorpay order created (amount in paise)
         PendingPurchase cached (30 min, keyed by token)
              │
              ▼
         Frontend opens Razorpay checkout widget
              │
         Customer completes payment in browser
              │
              ▼
         POST /gateway/policies/payment/verify
              │
              ▼
         HMAC-SHA256 signature verified
         Pending cache entry removed (replay prevention)
         Policy created in PolicyDb (status: ACTIVE)
         Payment record saved
         PolicyActivatedEvent published ──► AdminService (audit log)
```

### 8.3 Claim Lifecycle Flow

```
Customer ──► POST /gateway/claims  (creates Draft)
              │
              ▼
         ClaimsService verifies policy is ACTIVE
         (HTTP call to PolicyService with JWT forwarded)
              │
              ▼
         Claim created (status: DRAFT)
              │
         Customer uploads documents (base64 → Mega.nz / local)
              │
              ▼
         POST /gateway/claims/{id}/submit
         Claim transitions to SUBMITTED
         ClaimSubmittedEvent published
              │
         Admin reviews
              │
              ▼
         PUT /gateway/claims/admin/{id}/review  → UNDER_REVIEW
         PUT /gateway/claims/admin/{id}/approve → APPROVED
         PUT /gateway/claims/admin/{id}/reject  → REJECTED
              │
              ▼
         ClaimApprovedEvent / ClaimRejectedEvent published
         ──► AdminService (audit log)
```

---

## 9. API Surface Summary

### IdentityService (`/gateway/auth/...`, `/gateway/users/...`)

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/auth/register` | Public | Start registration, send OTP |
| POST | `/auth/register/verify-otp` | Public | Complete registration |
| POST | `/auth/register/resend-otp` | Public | Resend registration OTP |
| POST | `/auth/login` | Public | Email/password login |
| GET | `/auth/google` | Public | Get Google OAuth URL |
| GET | `/auth/google/callback` | Public | Handle OAuth callback |
| POST | `/auth/refresh-token` | Public | Rotate refresh token |
| POST | `/auth/forgot-password` | Public | Send password reset OTP |
| POST | `/auth/reset-password` | Public | Reset password with OTP |
| GET | `/users/profile` | Bearer | Get own profile |
| PUT | `/users/profile` | Bearer | Update own profile |
| GET | `/users` | Admin | List all users |
| PUT | `/users/{id}/status` | Admin | Activate/deactivate user |
| PUT | `/users/{id}/role` | Admin | Change user role |

### PolicyService (`/gateway/policies/...`)

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/policies/products` | Public | List all products |
| GET | `/policies/products/{id}` | Public | Get product by ID |
| POST | `/policies/products` | Admin | Create product |
| PUT | `/policies/products/{id}` | Admin | Update product |
| DELETE | `/policies/products/{id}` | Admin | Delete product |
| GET | `/policies/calculate-premium` | Customer | Calculate premium |
| POST | `/policies/purchase` | Customer | Direct purchase |
| POST | `/policies/payment/create-order` | Customer | Create Razorpay order |
| POST | `/policies/payment/verify` | Customer | Verify payment & activate |
| GET | `/policies/my-policies` | Customer | List own policies |
| GET | `/policies/{id}` | Customer | Get policy by ID |
| POST | `/policies/{id}/cancel` | Customer | Cancel policy |
| GET | `/policies/admin/all` | Admin | List all policies |
| PUT | `/policies/admin/{id}/status` | Admin | Update policy status |

### ClaimsService (`/gateway/claims/...`)

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/claims` | Customer | Create claim (Draft) |
| GET | `/claims/my-claims` | Customer | List own claims |
| GET | `/claims/{id}` | Customer/Admin | Get claim by ID |
| POST | `/claims/{id}/submit` | Customer | Submit claim |
| POST | `/claims/{id}/documents` | Customer | Upload document |
| GET | `/claims/{id}/documents` | Customer/Admin | List documents |
| DELETE | `/claims/{id}/documents/{docId}` | Customer | Delete document |
| GET | `/claims/admin/all` | Admin | List all claims |
| PUT | `/claims/admin/{id}/review` | Admin | Mark under review |
| PUT | `/claims/admin/{id}/approve` | Admin | Approve claim |
| PUT | `/claims/admin/{id}/reject` | Admin | Reject claim |

### AdminService (`/gateway/admin/...`)

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/admin/dashboard/stats` | Admin | Dashboard KPIs |
| GET | `/admin/reports/policies` | Admin | Policy report |
| GET | `/admin/reports/claims` | Admin | Claims report |
| GET | `/admin/reports/revenue` | Admin | Revenue report |
| GET | `/admin/reports/export` | Admin | Export report as PDF |
| GET | `/admin/audit-logs` | Admin | Paginated audit logs |

---

## 10. Caching Strategy

| Data | Cache Key | TTL | Invalidation |
|---|---|---|---|
| Insurance products | `insurance_types_all` | 60 min | On product create/update/delete |
| User policies | `user_policies_{userId}` | 2 min | On policy create/cancel/status change |
| Single policy | `policy_{policyId}` | 5 min | On policy status change |
| User claims | `user_claims_{userId}` | 2 min | On claim create/submit/status change |
| Single claim | `claim_{claimId}` | 2 min | On claim status change |
| User roles | `user_role_{userId}` | 10 min | Not explicitly invalidated |
| Report snapshots | `report_{reportId}` | 10 min | Not invalidated |

All caching uses in-process `IMemoryCache` (no distributed cache).

---

## 11. Non-Functional Characteristics

| Concern | Approach |
|---|---|
| **Security** | JWT validation at gateway, BCrypt password hashing, OTP single-use tokens, HMAC-SHA256 payment signature verification, silent response on unknown emails (anti-enumeration) |
| **Resilience** | Circuit breaker at gateway (3 exceptions / 30s break), non-fatal event publishing, Mega.nz upload with 15s timeout and local fallback |
| **Observability** | Serilog structured logging to daily rolling files per service, structured log context with service name |
| **Scalability** | Stateless services (JWT auth, no server-side sessions), in-memory cache per instance |
| **CORS** | Explicit origin allowlist per service (localhost:4200, localhost:3000) |
| **Rate Limiting** | 100 requests/second at gateway level |

---

## 12. Deployment Topology

```
Developer Machine
├── localhost:4200  ── Angular Dev Server (ng serve)
├── localhost:5000  ── Ocelot API Gateway
├── localhost:5001  ── IdentityService
├── localhost:5002  ── PolicyService
├── localhost:5003  ── ClaimsService
├── localhost:5004  ── AdminService
├── localhost:5672  ── RabbitMQ (AMQP)
├── localhost:15672 ── RabbitMQ Management UI
└── localhost:1433  ── SQL Server
    ├── AuthDb
    ├── PolicyDb
    ├── ClaimsDb
    └── AdminDb
```

External services:
- **Razorpay** — payment processing (HTTPS to api.razorpay.com)
- **Google OAuth** — authentication (HTTPS to accounts.google.com)
- **Mega.nz** — document storage (HTTPS to g.api.mega.co.nz)
- **SMTP** — email delivery (configurable host, default smtp.gmail.com:587)
