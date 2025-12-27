# 🚀 ExpenseVista API

The robust backend API for **ExpenseVista**, a modern Personal Finance tracking platform. Built with **.NET 8**, **PostgreSQL**, and **Entity Framework Core**, designed using Clean Architecture principles.

[![Status](https://img.shields.io/badge/status-active-success)](https://expensevista-api.samuelcmbah.com.ng/swagger/index.html)
[![Frontend Repo](https://img.shields.io/badge/frontend-React-blue)](https://github.com/samuelcmbah/expensevista)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

<!-- Table of Contents -->
## 🔎 Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [API Features & Endpoints](#api-features--endpoints)
  - [Authentication](#authentication)
  - [Dashboard](#dashboard)
  - [Transactions](#transactions)
  - [Budgets](#budgets)
  - [Categories](#categories)
  - [Analytics](#analytics)
  - [Currency](#currency)
  - [Report Export](#report-export)
- [Feature Spotlight: Production-Grade Excel Export](#feature-spotlight-production-grade-excel-export)
- [Core Data Models (DTOs)](#core-data-models-dtos)
- [Architecture & Design](#architecture--design)
  - [Core Architectural Layers](#core-architectural-layers)
  - [Key Design Patterns & Principles](#key-design-patterns--principles)
- [Security & Authentication](#security--authentication)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation & Setup](#installation--setup)
- [Deployment](#deployment)
- [Roadmap](#roadmap)
- [Contributions](#contributions)
- [Author](#author)

---

<a name="overview"></a>
## 📘 Overview

This API serves as the core engine for ExpenseVista, handling data persistence, business logic, authentication, and secure communication with the client. It demonstrates professional backend engineering practices including DTO mapping, structured logging, and JWT security.

[**Live API/Swagger **](https://expensevista-api.samuelcmbah.com.ng/swagger)

---

<a name="tech-stack"></a>
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

<a name="api-features--endpoints"></a>
## ✨ API Features & Endpoints

The API provides a comprehensive and secure RESTful interface for all application functionalities. Below is a detailed breakdown of the available endpoints.

| Feature Group | Endpoint | Method | Description |
| :--- | :--- | :--- | :--- |
| **Authentication** | `/api/auth/register` | `POST` | Register a new user account. |
| | `/api/auth/login` | `POST` | Authenticate a user and receive JWT access/refresh tokens. |
| | `/api/auth/google-login` | `POST` | Authenticate a user via Google's OAuth 2.0 flow using an authorization code. |
| | `/api/auth/refresh` | `POST` | Use a valid refresh token to get a new access token. |
| | `/api/auth/logout` | `POST` | Invalidate the user's refresh token to log them out securely. |
| | `/api/auth/confirm-email` | `POST` | Confirm a user's email address using a provided token. |
| | `/api/auth/resend-verification` | `POST` | Resend the email verification link. |
| | `/api/auth/forgot-password` | `POST` | Initiate the password reset process for an email address. |
| | `/api/auth/reset-password` | `POST` | Set a new password using a valid reset token. |
| **Dashboard** | `/api/dashboard` | `GET` | Get aggregated key insights and summary data for the main dashboard. |
| **Transactions** | `/api/transactions/filter` | `GET` | Get a paginated and filtered list of transactions. |
| | `/api/transactions` | `POST` | Create a new transaction (income or expense). |
| | `/api/transactions/{id}` | `GET` | Get a single transaction by its unique ID. |
| | `/api/transactions/{id}` | `PUT` | Update an existing transaction. |
| | `/api/transactions/{id}` | `DELETE`| Delete a specific transaction. |
| **Budgets** | `/api/budgets` | `GET` | Get all budgets for the authenticated user. |
| | `/api/budgets` | `POST` | Create a new budget for a specific category. |
| | `/api/budgets/status` | `GET` | Get a summary status of all active budgets. |
| | `/api/budgets/{id}` | `PUT` | Update an existing budget. |
| | `/api/budgets/{id}` | `DELETE`| Delete a specific budget. |
| **Categories** | `/api/categories` | `GET` | Get all system-defined and user-created categories. |
| | `/api/categories` | `POST` | Create a new custom category. |
| | `/api/categories/{id}` | `GET` | Get a single category by its ID. |
| | `/api/categories/{id}` | `PUT` | Update an existing category. |
| | `/api/categories/{id}` | `DELETE`| Delete a user-created category. |
| **Analytics** | `/api/analytics` | `GET` | Retrieve detailed financial data for charts and analysis. |
| **Currency** | `/api/currency/supported` | `GET` | Get a list of supported currencies. |
| | `/api/currency/rate` | `GET` | Get the exchange rate between two currencies. |
| **Report Export** | `/api/report-export/export` | `POST` | Generates and downloads a multi-sheet Excel report of financial analytics. |

---

<a name="feature-spotlight-production-grade-excel-export"></a>
### ✨ Feature Spotlight: Production-Grade Excel Export

To provide users with a professional offline reporting tool, the API includes a robust, multi-sheet Excel export feature, engineered for scalability and data integrity.

#### Architectural Approach

The export pipeline was designed using **Clean Architecture principles** to ensure long-term maintainability:

1.  **Decoupled DTOs:** A dedicated set of immutable `ExportDTOs` was created to define a stable, versioned "export contract." This completely decouples the report's structure from the internal API models, preventing accidental breaking changes when the main `AnalyticsDTO` evolves.
2.  **Service-Oriented Design:** The entire process is orchestrated by dedicated services:
    *   `IAnalyticsService`: Gathers the raw financial data.
    *   `FinancialReportExportMapper`: Transforms the analytics data into the stable export DTO format.
    *   `IFinancialReportExporter`: Consumes the export DTO and uses **ClosedXML** to build the `.xlsx` file in memory.
3.  **Cross-Platform Compatibility:** The system was engineered to handle complex, real-world deployment challenges, including:
    *   **Culture Invariance:** Explicitly setting `CultureInfo.InvariantCulture` during the file generation process to ensure number formats (`.` vs `,`) are consistent, solving critical bugs between local (Windows) and production (Linux) environments.
    *   **Maximum Compatibility Formulas:** Using simple `SUM()` formulas instead of modern `SUBTOTAL()` functions to guarantee that the "Totals" rows render correctly even on older versions of Excel and third-party spreadsheet applications (like Google Sheets or WPS Office).

#### Generated Report Structure

The generated `.xlsx` workbook contains multiple, pre-formatted sheets for easy analysis:
*   **Overview:** A summary dashboard with key metrics and user details.
*   **Budget Breakdown:** A detailed, month-by-month view of budget vs. actual spending.
*   **Spending by Category:** A tabular breakdown of expenses by category with totals.
*   **Income vs Expenses:** A monthly summary of cash flow.
*   **All Transactions:** A complete, sortable log of every transaction within the selected period.
---

<a name="core-data-models-dtos"></a>
### 📦 Core Data Models (DTOs)

To ensure a secure and clean API contract, the system uses Data Transfer Objects (DTOs) for all client communication. This decouples the API's public shape from the internal database models. Key DTOs include:

*   **Auth:** `AppicationUserDTO`, `GoogleLoginRequestDTO`, `RefreshRequestDTO`, `TokenResponseDTO`, `RegisterDTO`, `LoginDTO`, `ForgotPasswordDTO`, `ResetPasswordDTO`, `VerifyEmailDTO`
*   **Transactions:** `TransactionCreateDTO`, `TransactionUpdateDTO`, `TransactionDTO`, `TransactionDTOPagedResponse`
*   **Budgets:** `BudgetCreateDTO`, `BudgetUpdateDTO`, `BudgetDTO`, `BudgetStatusDTO`, `BudgetProgressDTO`
*   **Categories:** `CreateCategoryDTO`, `UpdateCategoryDTO`, `CategoryDTO`
*   **Analytics & Export:** `FinancialDataDTO`, `MonthlyBudgetDetailDTO`, `SummaryDTO`, `KeyInsightsDTO`, `SpendingCategoryDTO`, `IncomeExpenseDataDTO`, `FinancialReportExport`

---

<a name="architecture--design"></a>
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

---

<a name="security--authentication"></a>
### 🔐 Security & Authentication

The API implements a secure, stateful **JWT (JSON Web Token)** implementation with **Refresh Token Rotation**:

*   **Access Tokens:** Short-lived tokens used for Authorization headers.
*   **Refresh Tokens:** Long-lived tokens stored securely in **HttpOnly cookies** (inaccessible to client-side JavaScript) and persisted in the database.
*   **Token Rotation:** The `/auth/refresh` endpoint invalidates the old Refresh Token and issues a new pair, providing maximum security against token hijacking and replay attacks.

#### External Authentication (Google OAuth 2.0)

To provide a modern and seamless user experience, the API supports external authentication using Google's OAuth 2.0 and OpenID Connect (OIDC) protocols.

**The Authentication Flow:**

1.  **Frontend Initiates:** The React client redirects the user to Google's consent screen.
2.  **Authorization Code:** After user consent, Google redirects back to the frontend with a single-use **Authorization Code**.
3.  **Backend Token Exchange:** The frontend sends this code to the `/api/auth/google-login` endpoint. The backend then performs a secure, **server-to-server** request to Google, exchanging the code and its `client_secret` for an `id_token`.
4.  **Identity & Account Linking:** The backend validates the `id_token` from Google and performs smart account provisioning:
    *   **Login:** If a user with the unique Google ID exists, they are logged in.
    *   **Link:** If a user with the same verified email exists but has no Google link, the provider details are automatically linked to the existing account to prevent duplicates.
    *   **Register:** If no user is found, a new account is created.
5.  **Session Unification:** In all cases, the API generates its **own** JWT Access and Refresh Tokens for the user. This unifies the session management, meaning a Google-authenticated user is treated identically to a password-based user by the rest of the API.

---

<a name="project-structure"></a>
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
<a name="getting-started"></a>
## ⚙️ Getting Started

<a name="prerequisites"></a>
### Prerequisites
*   [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
*   [PostgreSQL](https://www.postgresql.org/download/) Server
*   A code editor like [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio](https://visualstudio.microsoft.com/).

<a name="installation--setup"></a>
### Installation & Setup

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/samuelcmbah/ExpenseVista.API.git
    cd ExpenseVista.API
    ```

2. **Set up Google Authentication (Required)**
     * Navigate to the Google Cloud Console.
     * Create a new project and go to "APIs & Services" > "Credentials".
     * Click "Create Credentials" > "OAuth client ID", select "Web application".
     *Under "Authorized redirect URIs", add your frontend's callback URL (e.g., http://localhost:5000/auth/google/callback).
     * Click "Create" and copy the Client ID and Client Secret.

3.  **Configure Environment Variables for Local Development:**
    In the root of the `ExpenseVista.API` project, create a new file named `appsettings.Development.json`.

    Copy the following JSON structure into the file and replace the placeholder values with your local configuration details.

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Database=ExpenseVistaDb;Username=postgres;Password=your_db_password"
      },
      "Jwt": {
        "Key": "Your_Super_Secret_Key_For_JWT_Must_Be_Long_Enough_To_Be_Secure",
        "Issuer": "https://localhost:7000",
        "Audience": "https://localhost:5173"
      },
      "ResendEmailSettings": {
        "ApiKey": "re_your_resend_api_key"
      },
      "Google": {
        "ClientId": "PASTE_YOUR_GOOGLE_CLIENT_ID_HERE",
        "ClientSecret": "PASTE_YOUR_GOOGLE_CLIENT_SECRET_HERE"
      }
    }
    ```
    *Note: The `.gitignore` file is configured to ignore `appsettings.Development.json`, so your local secrets will not be committed to the repository.*

4.  **Apply Database Migrations:**
    Ensure your PostgreSQL server is running, then execute the following commands in the terminal:
    ```bash
    dotnet restore
    dotnet ef database update
    ```

5.  **Run the Application:**
    ```bash
    dotnet run
    ```
    The API will now be running on `https://localhost:7000`. You can access the Swagger UI for testing at `https://localhost:7000/swagger`.

---

<a name="deployment"></a>
## 🚀 Deployment

The API is continuously deployed to **Render** from the `main` branch. 
*   **Service Type:** Render Web Service
*   **Build:** Uses the standard .NET buildpack.
*   **Database:** A managed PostgreSQL instance on Render.
*   **Base URL:** `https://expensevista-api.onrender.com`

<a name="roadmap"></a>
## 🗺️ Roadmap

This project is actively maintained. Future enhancements include:
*   [ ] Adding third-party payment integration 
*   [ ] Introducing unit and integration tests to improve code coverage.

<a name="contributions"></a>
## 🤝 Contributions
Contributions are welcome! Please fork the repository and submit a pull request.

<a name="author"></a>
## 🧑‍💻 Author
* Samuel Mbah
* GitHub: [samuelcmbah](https://github.com/samuelcmbah)
* LinkedIn: [Samuel Mbah](https://linkedin.com/in/samuelcmbah)
