
# 🚀 ExpenseVista API

The robust backend API for **ExpenseVista**, a modern Personal Finance tracking platform. Built with **.NET 8**, **PostgreSQL**, and **Entity Framework Core**, designed using Clean Architecture principles.

[![Status](https://img.shields.io/badge/status-active-success)](https://expensevista-api.onrender.com/swagger/index.html)
[![Frontend Repo](https://img.shields.io/badge/frontend-React-blue)](https://github.com/samuelcmbah/expensevista)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

## 📘 Overview

This API serves as the core engine for ExpenseVista, handling data persistence, business logic, authentication, and secure communication with the client. It demonstrates professional backend engineering practices including DTO mapping, structured logging, and JWT security.

**Live API/Swagger:** [https://expensevista-api.onrender.com/swagger](https://expensevista-api.onrender.com/swagger)

---

## 🛠️ Tech Stack

* **Framework:** [.NET 8 Web API](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* **Database:** [PostgreSQL](https://www.postgresql.org/) (via Npgsql)
* **ORM:** [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
* **Identity & Auth:** Microsoft Identity + [JWT Bearer Authentication](https://jwt.io/)
* **Mapping:** [AutoMapper](https://automapper.org/)
* **Logging:** [Serilog](https://serilog.net/) (Structured logging)
* **Email Service:** [Resend](https://resend.com/)
* **Documentation:** [Swashbuckle/Swagger](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

---

## 🏗️ Architecture & Design

The **ExpenseVista** backend is engineered as a **Layered Monolith** using **ASP.NET 8+**. This architectural choice prioritizes maintainability, strict separation of concerns, and robust testability by adhering to modern C# design principles.

### 📂 Core Architectural Layers
The application is logically divided into three distinct layers, organized by function within the API project:

| Layer | Component | Responsibilities |
| :--- | :--- | :--- |
| **Presentation** | `Controllers` | Handles API interactions, routing, HTTP request reception, input validation, and final response generation. **Controllers remain thin** and contain no business logic. |
| **Business Logic** | `Services` | The core application brain. Contains all business rules, performs complex calculations, orchestrates data flow, and manages external integrations (e.g., JWT generation, Email services). |
| **Persistence** | `Data` & `Models` | Manages data access and storage. **Models** define the database schema (via EF Core), while the **Data** folder encapsulates `DbContext` and migration logic. |

### 🧩 Key Design Patterns & Principles

#### 1. Separation of Concerns (Service-First Approach)
We adhere strictly to the **Controller-Service Pattern**. Controllers are designed to be "thin"—they purely handle HTTP concerns and immediately delegate functional tasks to the appropriate service interface (e.g., `IAuthService`, `IAnalyticsService`).

#### 2. Dependency Injection (DI)
The application leverages the native .NET DI container to ensure loose coupling. All external dependencies (such as `UserManager`, `ILogger`, and Service Interfaces) are injected via constructors. This makes the system highly modular and simplifies unit testing via mocking.

#### 3. DTO-Based Communication
To protect the internal database structure and prevent over-posting attacks, **Data Transfer Objects (DTOs)** are used exclusively for external communication:
*   **Input DTOs:** (e.g., `RegisterDTO`) Used to deserialize and validate incoming JSON requests.
*   **Output DTOs:** (e.g., `ApplicationUserDTO`) Used to shape the data returned to the client.
*   *Note: Internal Domain Models are never exposed directly to the API consumer.*

#### 4. Global Error Handling
A custom **Exception Handling Middleware** acts as a centralized safety net. Business logic exceptions (e.g., a custom `BadRequestException`) are caught globally and standardized into consistent HTTP error responses before reaching the client, ensuring a uniform API experience.

### 🔐 Security & Authentication
The API implements a secure, stateful **JWT (JSON Web Token)** implementation with **Refresh Token Rotation**:

*   **Access Tokens:** Short-lived tokens used for Authorization headers.
*   **Refresh Tokens:** Long-lived tokens stored securely in **HttpOnly cookies** (inaccessible to client-side JavaScript) and persisted in the database.
*   **Token Rotation:** The `/auth/refresh` endpoint invalidates the old Refresh Token and issues a new pair, providing maximum security against token hijacking and replay attacks.

---

## ✨ API Features

* **Authentication:** User registration, login, and JWT token generation.
* **Transaction Management:** CRUD operations for expenses and income with filtering, sorting, and pagination.
* **Analytics Data:** Aggregated endpoints providing data for charts (monthly breakdowns, category summaries).
* **Category System:** Management of system-defined and user-defined categories.

---

## 📁 Project Structure

```text
ExpenseVista.API/
 ├── Configurations/   # DI configs and AutoMapper profiles
 ├── Controllers/      # API Endpoints
 ├── Data/             # DbContext and seeding
 ├── DTOs/             # Data Transfer Objects
 ├── Models/           # Domain Entities
 ├── Middleware/       # Exception handling, logging
 ├── Migrations/       # EF Core Migrations
 ├── Services/         # Business Logic
 └── Utilities/        # Helper functions

```
## ⚙️ Installation & Setup
# Prerequisites
- .NET 8 SDK
- PostgreSQL

Steps
1. Clone the repository
```bash
git clone https://github.com/samuelcmbah/ExpenseVista.API.git
cd ExpenseVista.API
```

2. Configure Environment Variables
Update appsettings.json or set User Secrets:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ExpenseVistaDb;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "Your_Super_Secret_Key_Must_Be_Long_Enough",
    "Issuer": "https://localhost:7000",
    "Audience": "https://localhost:5173"
  },
  "ResendEmailSettings": {
    "ApiKey": "re_your_api_key"
  }
}
```

3. Apply Migrations
```bash
dotnet restore
dotnet ef database update
```

4. Run the Application
```bash
dotnet run
```

The API will start on https://localhost:7000 (or the port configured in launchSettings).

## 🚀 Deployment
* The API is currently deployed on Render.
* Base URL: https://expensevista-api.onrender.com

## 🤝 Contributions
Contributions are welcome! Please fork the repository and submit a pull request.

## 🧑‍💻 Author
* Samuel Mbah
* GitHub: [samuelcmbah](https://github.com/samuelcmbah)
* LinkedIn: [Samuel Mbah](https://linkedin.com/in/samuelcmbah)
