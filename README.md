
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

## 🏛️ Architecture

This project follows **Clean Architecture** principles to ensure separation of concerns and maintainability.

* **Repository + Service Pattern:** Decouples business logic from data access.
* **DTO-based Communication:** Ensures the internal domain model is never exposed directly to the client.
* **Global Exception Handling:** Centralized middleware for handling errors gracefully.
* **Dependency Injection:** Heavy use of .NET built-in DI container.

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
##⚙️ Installation & Setup
#Prerequisites
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

🚀 Deployment
The API is currently deployed on Render.
Base URL: https://expensevista-api.onrender.com

🤝 Contributions
Contributions are welcome! Please fork the repository and submit a pull request.

🧑‍💻 Author
Samuel Mbah
GitHub: [samuelcmbah](https://github.com/samuelcmbah)
LinkedIn: [Samuel Mbah](https://linkedin.com/in/samuelcmbah)