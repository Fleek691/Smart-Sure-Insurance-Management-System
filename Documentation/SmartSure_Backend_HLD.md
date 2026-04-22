# SmartSure Backend High-Level Design (HLD)

## 1. System Architecture

**SmartSure** uses a microservices architecture for modularity, scalability, and maintainability. The main backend components are:

- **Gateway** (API Gateway)
- **AdminService**
- **ClaimsService**
- **IdentityService**
- **PolicyService**
- **Shared Library**

### Architecture Diagram

Below is a Mermaid diagram representing the backend architecture:

```mermaid
graph TD
  Gateway --> AdminService
  Gateway --> ClaimsService
  Gateway --> IdentityService
  Gateway --> PolicyService
  AdminService --> Shared
  ClaimsService --> Shared
  IdentityService --> Shared
  PolicyService --> Shared
```

---

## 2. Component Details

### 2.1 Gateway (SmartSure.Gateway)
- **Role:** Entry point for all client requests. Uses Ocelot for routing to backend services.
- **Key Configs:** `ocelot.json` (routes), `appsettings.json` (env config)
- **Startup:** `Program.cs`

### 2.2 AdminService
- **Role:** Admin dashboard, reports, audit logs.
- **Controllers:** `AdminController.cs`
- **Services:** `AdminService.cs`
- **Repositories:** `AdminRepository.cs`
- **Models:** `AuditLog.cs`, `Report.cs`

### 2.3 ClaimsService
- **Role:** Insurance claim management (creation, review, docs).
- **Controllers:** `ClaimsController.cs`
- **Services:** `ClaimService.cs`, `ClaimAdminService.cs`, `ClaimEventPublisher.cs`
- **Repositories:** `ClaimRepository.cs`
- **Models:** `Claim.cs`, `ClaimDocument.cs`, `ClaimStatusHistory.cs`
- **Uploads:** Document storage for claims

### 2.4 IdentityService
- **Role:** Authentication, authorization, user/role management.
- **Controllers:** `AuthController.cs`, `UsersController.cs`
- **Services:** `AuthService.cs`, `UserAdministrationService.cs`, `EmailService.cs`, `GoogleAuthService.cs`
- **Repositories:** `UserRepository.cs`, `RoleRepository.cs`
- **Models:** `User.cs`, `Role.cs`, `UserRole.cs`, `Password.cs`, `PasswordResetToken.cs`

### 2.5 PolicyService
- **Role:** Insurance products, policy management, premium calculation, payments.
- **Controllers:** `PoliciesController.cs`
- **Services:** `PolicyService.cs`, `RazorpayService.cs`, `PolicyEventPublisher.cs`
- **Repositories:** `PolicyRepository.cs`
- **Models:** `Policy.cs`, `PolicyDetail.cs`, `Premium.cs`, `Payment.cs`, `InsuranceType.cs`, `InsuranceSubType.cs`, `VehicleDetail.cs`, `HomeDetail.cs`

### 2.6 Shared Library (SmartSure.Shared)
- **Purpose:** Common code and contracts for all services.
- **Key Areas:**
  - **DTOs:** `ApiResponse.cs`
  - **Events:** `ClaimApprovedEvent.cs`, `ClaimRejectedEvent.cs`, `ClaimStatusChangedEvent.cs`, `ClaimSubmittedEvent.cs`, `PolicyActivatedEvent.cs`, `PolicyCancelledEvent.cs`, `UserRegisteredEvent.cs`
  - **Messaging:** `RabbitMqOptions.cs`
  - **Middleware:** `GlobalExceptionHandlerMiddleware.cs`
  - **Exceptions:** `BusinessRuleException.cs`, `ConflictException.cs`, `ForbiddenException.cs`, `HttpServiceException.cs`, `NotFoundException.cs`, `SmartSureException.cs`, `UnauthorizedException.cs`, `ValidationException.cs`
  - **Constants:** `ClaimStatus.cs`, `PolicyStatus.cs`, `Roles.cs`
  - **Extensions:** `MiddlewareExtensions.cs`, `SerilogExtensions.cs`

---

## 3. Interactions

- **Gateway** routes all client requests to the appropriate microservice.
- Each service manages its own data and business logic.
- **Shared** library provides common contracts, events, and middleware.
- Services communicate via REST and publish/consume events (RabbitMQ).

---

## 4. Images

- The Mermaid diagram above can be rendered as an image in documentation tools that support Mermaid.
- For more detailed sequence or flow diagrams, specify which flows you want visualized (e.g., claim submission, user registration).

---

## 5. Extensibility & Best Practices

- Each service is independently deployable and testable.
- Shared code ensures consistency and reduces duplication.
- Event-driven design allows for future integrations and scalability.

---
