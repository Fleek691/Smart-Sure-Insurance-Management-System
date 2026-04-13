# Smart-Sure Insurance Management System

## Overview
Smart-Sure is a comprehensive insurance management platform designed to streamline policy administration, claims processing, user authentication, and reporting for insurance providers and customers. The system is built using a modern microservices architecture with a robust Angular frontend and .NET backend.

## Features
- **Gateway API**: Central entry point for all backend services, API routing, and aggregation.
- **Admin Service**: Manage users, policies, claims, and generate reports for administrators.
- **Claims Service**: Submit, review, approve, and track insurance claims with document uploads.
- **Identity Service**: User authentication, registration, password management, and role-based access control.
- **Policy Service**: Policy creation, management, and lifecycle operations.
- **Shared Library**: Common DTOs, events, exceptions, and middleware for all backend services.
- **Frontend (Angular)**: User-friendly web interface for customers, admins, and agents to interact with the system.

## Tech Stack
- **Frontend**: Angular, TypeScript, HTML, CSS
- **Backend**: .NET 10, C#, ASP.NET Core Web API, Ocelot API Gateway
- **Database**: Entity Framework Core (with migrations)
- **Authentication**: JWT, OAuth (Google)
- **Messaging/Eventing**: RabbitMQ (for inter-service communication)
- **Testing**: NUnit (backend), Jasmine/Karma (frontend)
- **CI/CD**: GitHub Actions (recommended)

## Project Structure
- `Backend/SmartSure.Gateway` - API Gateway
- `Backend/SmartSure.Services/SmartSure.AdminService` - Admin microservice
- `Backend/SmartSure.Services/SmartSure.ClaimsService` - Claims microservice
- `Backend/SmartSure.Services/SmartSure.IdentityService` - Identity/auth microservice
- `Backend/SmartSure.Services/SmartSure.PolicyService` - Policy microservice
- `Backend/SmartSure.Shared` - Shared code and contracts
- `Backend/SmartSure.Tests` - Backend unit/integration tests
- `Frontend/smartsure-ui` - Angular frontend app

## Getting Started
1. **Clone the repository**
2. **Backend**:
   - Install .NET 10 SDK
   - Navigate to `Backend/` and run `dotnet build`
   - Run services individually or via the gateway
   - Run tests: `dotnet test SmartSure.slnx --collect:"XPlat Code Coverage"`
3. **Frontend**:
   - Navigate to `Frontend/smartsure-ui`
   - Run `npm install`
   - Run `ng serve` to start the Angular app
4. **Configuration**:
   - Set up environment variables and connection strings in `appsettings.json` and `.env` files as needed

## Contributing
- Fork the repo and create feature branches for new functionality
- Write meaningful commit messages and keep PRs focused
- Ensure all tests pass before submitting PRs

