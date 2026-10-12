[absolute_cinema_api_readme.md](https://github.com/user-attachments/files/33347203/absolute_cinema_api_readme.md)
# 🎬 AbsoluteCinema-API

A robust, production-grade RESTful Web API built with ASP.NET Core (.NET 8/9). This service acts as the decoupled backend engine powering movie catalogs, secure JWT authentication, role-based authorization, and Stripe payment processing.

---

## 🏗️ System Architecture

AbsoluteCinema-API follows a strict **N-Tier Layered Architecture** pattern, ensuring high separation of concerns, testability, and maintainability.

```
+-----------------------------------------------------------------+
|                       PRESENTATION LAYER                        |
|  - API Controllers ([ApiController], Routing, Action Filters)   |
|  - Global Exception Handling Middleware                         |
+---------------------------------+-------------------------------+
                                  |
                                  v
+-----------------------------------------------------------------+
|                    BUSINESS LOGIC LAYER                         |
|  - Service Interfaces & Implementations                         |
|  - Domain Rules, Validation Logic, & DTO Mapping                |
+---------------------------------+-------------------------------+
                                  |
                                  v
+-----------------------------------------------------------------+
|                      DATA ACCESS LAYER                          |
|  - Entity Framework Core DbContext                              |
|  - Repository & Unit of Work Pattern                            |
|  - Fluent API Database Configurations                           |
+---------------------------------+-------------------------------+
                                  |
                                  v
+-----------------------------------------------------------------+
|                    EXTERNAL SERVICES & DB                       |
|  - Microsoft SQL Server (Persistence)                           |
|  - ASP.NET Core Identity (User Stores)                          |
|  - Stripe API SDK (Payment Gateway)                             |
+-----------------------------------------------------------------+
```

---

## 📐 Design Patterns & Core Principles

1. **Repository & Unit of Work Pattern:** Abstracts database operations away from controllers, ensuring transactions are safely scoped and data access code is decoupled.
2. **DTO (Data Transfer Objects) & Auto-Mapping:** Shields internal domain entities from external exposure, preventing over-posting vulnerabilities and shaping clean response payloads.
3. **Stateless JWT Authentication:** Issues JSON Web Tokens embedded with user roles and claims, allowing horizontal scaling without server-side session overhead.
4. **Dependency Injection (DI):** Leverages the native .NET IoC container to manage service lifetimes (`Scoped` for business and data layers, `Singleton` for configuration/utility services).
5. **Gateway / Strategy Pattern:** Encapsulates the Stripe payment processing logic behind a clean service wrapper (`IPaymentService`), insulating the core booking domain from third-party SDK changes.

---

## 🔑 Key Features & Endpoints Structure

- **Authentication & Authorization (`/api/Auth`):** Secure endpoints for user registration, login, token refresh, and role-based access control (`Admin`, `User`).
- **Movie Catalog Management (`/api/Movies`):** CRUD endpoints for browsing, filtering, and managing movies, showtimes, and genres.
- **Booking & Payments (`/api/Bookings`, `/api/Payments`):** Endpoints to lock cinema seats, generate orders, and integrate with Stripe Checkout/Payment Intents.

---

## 🛠️ Tech Stack

- **Framework:** ASP.NET Core Web API (.NET 8/9)
- **Language:** C#
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core (Code-First Migrations)
- **Security:** ASP.NET Core Identity, JWT Bearer Authentication
- **Payments:** Stripe .NET SDK
- **Documentation:** Swashbuckle / OpenAPI (Swagger UI)

---

## ⚙️ Configuration & Setup

### 1. Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Stripe Account & API Keys](https://dashboard.stripe.com/)

### 2. Environment Configuration (`appsettings.json`)
Configure your database connection, JWT signing tokens, and Stripe secrets inside your `appsettings.json` file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AbsoluteCinemaApiDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JWT": {
    "Issuer": "AbsoluteCinemaApi",
    "Audience": "AbsoluteCinemaClients",
    "Key": "YOUR_SUPER_SECRET_JWT_SIGNING_KEY_MIN_32_CHARS",
    "DurationInDays": 7
  },
  "Stripe": {
    "SecretKey": "sk_test_..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 3. Database Migration & Execution

Open your terminal in the root directory of the API project and execute:

```bash
# Apply database migrations
dotnet ef database update

# Run the Web API
dotnet run
```

Once running, navigate to `https://localhost:5001/swagger` to explore and test endpoints interactively via the Swagger UI.

---

## 🔒 Security Practices

- **Password Hashing:** Handled securely via ASP.NET Core Identity's default PBKDF2 implementation.
- **Bearer Token Protection:** Endpoints requiring authorization are protected with `[Authorize]` attributes and validated against cryptographic JWT signatures.
- **CORS Policy:** Configured to strictly allow designated client application origins.

---

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.
