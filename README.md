# SmartSure Platform

SmartSure is a modular, full-stack insurance management platform designed to handle policy management, claims processing, user authentication, and administration. It is built with a microservices architecture using .NET 10 for the backend and Angular 17 for the frontend.

## Project Structure

```
Smart-Sure/
├── Backend/
│   ├── SmartSure.Gateway/         # API Gateway (Ocelot)
│   ├── SmartSure.Services/
│   │   ├── SmartSure.AdminService/    # Admin microservice
│   │   ├── SmartSure.ClaimsService/   # Claims microservice
│   │   ├── SmartSure.IdentityService/ # Authentication & user management
│   │   └── SmartSure.PolicyService/   # Policy management
│   ├── SmartSure.Shared/          # Shared code and contracts
│   └── SmartSure.Tests/           # Automated tests
├── Frontend/
│   ├── smartsure-ui/              # Angular 17+ SPA
│   ├── index.html, app.js, styles.css
└── Documentation/                # Project docs
```

## Main Functionalities
- **User Authentication & Authorization** (JWT, roles)
- **Policy Management** (create, update, view, assign policies)
- **Claims Management** (file, track, review, approve/reject claims)
- **Admin Tools** (user management, reporting, audit logs)
- **Document Storage** (Mega.nz integration for claim docs)
- **Messaging** (RabbitMQ for service communication)
- **API Gateway** (Ocelot for routing, aggregation, and security)
- **Swagger API Docs** for all services

## Tools & Technologies
- **Backend:** .NET 10, ASP.NET Core, Entity Framework Core, MassTransit, RabbitMQ, Ocelot, MegaApiClient, Serilog
- **Frontend:** Angular 17, RxJS, jsPDF, TypeScript
- **Database:** SQL Server (EF Core)
- **API Docs:** Swagger/OpenAPI
- **Other:** DotNetEnv for config, Mega.nz for file storage

## Versioning
- .NET: 10.0+
- Angular: 17.3+
- Node.js: 18+ (for frontend dev)
- SQL Server: 2019+

## Getting Started
1. **Clone the repository**
2. **Configure environment variables** for each service (see each service's README)
3. **Run database migrations** as needed
4. **Start backend services**:
   - Via Visual Studio or `dotnet run` in each service directory
5. **Start frontend**:
   - `cd Frontend/smartsure-ui`
   - `npm install`
   - `npm start`
6. **Access the app** at `http://localhost:4200` (default Angular port)

## Development & Testing
- Each microservice is independently runnable and testable
- Automated tests in `Backend/SmartSure.Tests/`
- Swagger UI available for each service at `/swagger`

## Security & Best Practices
- **Never commit credentials or secrets**; use `.env` and environment variables
- Add `appsettings.*.json` and `.env` to `.gitignore`
- Use strong JWT keys and secure RabbitMQ credentials

## License
This project is proprietary and part of the SmartSure platform.
