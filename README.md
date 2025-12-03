# 🚀 ExpenseVista

A modern full-stack expense tracking application built with **.NET 8**, **PostgreSQL**, and **React + TypeScript**.

[![Status](https://img.shields.io/badge/status-active-success)](https://expensevista-frontend.vercel.app/)
[![Frontend](https://img.shields.io/badge/frontend-react-blue)](https://github.com/samuelcmbah/expensevista)
[![Backend](https://img.shields.io/badge/backend-.NET%208-purple)](https://github.com/samuelcmbah/ExpenseVista.API.git)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

## 🙋‍♂️ Why I Built ExpenseVista

I wanted to build a real-world, full-stack financial application that demonstrates:
* Strong backend architecture
* Modern React frontend development
* Data visualization
* Secure authentication
* Real user workflows

This project represents my growth as a developer and my readiness for professional engineering roles.

---

## 📸 Screenshots

| Dashboard | Analytics |
| :---: | :---: |
| ![Dashboard Screenshot](./screenshots/dashboard.png) | ![Analytics Screenshot](./screenshots/analytics.png) |

---

## 📘 Overview

ExpenseVista helps users track expenses, analyze spending patterns, and visualize their financial activity with modern charts and dashboards.

This project showcases:
* Scalable backend architecture
* Clean API design
* Modern React frontend development
* Authentication flows
* Professional development workflow

---

## 🛠️ Tech Stack

### **Backend**
* [.NET 8 Web API](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [PostgreSQL](https://www.postgresql.org/) (via [Npgsql](https://www.npgsql.org/))
* [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) (ORM for data access)
* [JWT Authentication](https://jwt.io/) (via `Microsoft.AspNetCore.Authentication.JwtBearer`)
* [AutoMapper](https://automapper.org/) (Object-to-object mapping)
* [Serilog](https://serilog.net/) (Structured logging)
* [Resend](https://resend.com/) (Email sending service)
* [Swashbuckle/Swagger](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) (API documentation)
* **Identity:** `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (User management)

### **Frontend**
* [React (TypeScript)](https://react.dev/)
* [Vite](https://vitejs.dev/)
* [Tailwind CSS](https://tailwindcss.com/) + [ShadCN UI](https://ui.shadcn.com/) (Styling and components)
* [React Hook Form](https://react-hook-form.com/) + [Zod](https://zod.dev/) (Form management and validation)
* [Axios](https://axios-http.com/) (HTTP client for API requests)
* [Recharts](https://recharts.org/) (Data visualization/charts)
* [Lucide React](https://lucide.dev/icons/) (Icon library)
* [React Hot Toast](https://react-hot-toast.com/) (Notifications and alerts)
* [Framer Motion](https://www.framer.com/motion/) (Animations and transitions)
* [Radix UI](https://www.radix-ui.com/) (Primitives for component building)
* [React Router DOM](https://reactrouter.com/web/guides/quick-start) (Client-side routing)

---

## ✨ Features

### **Authentication & Security**
* User registration & login
* Secure JWT authentication
* Password hashing

### **Transaction Management**
* Create, update, delete transactions
* Categorized income & expenses
* Filtering, sorting, and pagination
* Date-based queries

### **Category Management**
* Custom categories
* System categories

### **Analytics Dashboard**
* Monthly and yearly spending summaries
* Category breakdown
* Pie and bar charts
* Trend visualization

### **Beautiful, Modern UI**
* Mobile-friendly (responsive design)
* Clean UX with reusable components
* Form validation with Zod

---

## 🏛️ Architecture

### **Backend Architecture**
* Clean Architecture principles 

[Image of Clean Architecture Diagram]

* DTO-based communication
* Repository + Service pattern
* Global exception handling
* Logging with Serilog
* Strong separation of concerns

### **Frontend Architecture**
* Component-driven design
* API abstraction layer
* **Manual Data Fetching & State Management (via Context/Hooks)**
* Form schemas (Zod) for safe client-side validation
* Typed data models (TypeScript)

---

## 📁 Project Structure

### **Backend**
```
ExpenseVista.API/
 ├── Configurations/
 ├── Controllers/
 ├── Data/
 ├── DTOs/
 ├── logs/
 ├── Models/
 ├── Middleware/
 ├── Migrations/
 ├── Services/
 └── Utilities/
```

### **Frontend**
**Frontend repo link:** [https://github.com/samuelcmbah/expensevista](https://github.com/samuelcmbah/expensevista)
```
src/
 ├── components/
 ├── context/
 ├── hooks/
 ├── lib/
 ├── pages/
 ├── schemas/
 ├── services/
 ├── types/
 └── utilities/
```

---

## ⚙️ Installation & Setup

### **Backend Setup**
```bash
1.  git clone https://github.com/samuelcmbah/ExpenseVista.API.git
2.  cd ExpenseVista.API
3.  **Obtain your PostgreSQL Connection String** (from a hosting service like **Render** or a local instance like **Docker/Loom**).
4.  **Set the `ConnectionStrings__DefaultConnection`** environment variable.
5.  `dotnet restore
6.  dotnet ef database update\` (This applies migrations to your chosen database.)
7.  dotnet run

> **Note:** The backend runs by default on: \`https://localhost:7000\`

### **Frontend Setup**
```
1.  git clone https://github.com/samuelcmbah/expensevista.git
2.  cd expensevista
3.  npm install
4.  npm run dev
```

> **Note:** The frontend runs by default on port \`http://localhost:5000\` (or \`http://localhost:5173\`).

---

## 🔐 Environment Variables

### **Backend (`appsettings.json` or environment variables)**
```
ConnectionStrings__DefaultConnection=YourPostgresConnection
Jwt__Key=YourSecretKeyForJWT
Jwt__Issuer=apidomain.com
Jwt__Audience=clientdomain.com
ResendEmailSettings__ApiKey=re_xxxxxxxxxxxxxxxxxxxxxxxx
```
### **Frontend (`.env` file)**
```
VITE_API_URL=https://localhost:7000/api
```
---

## 🚀 Deployment

* **Backend:** [Render](https://expensevista-api.onrender.com)
* **Frontend:** [Vercel](https://expensevista-frontend.vercel.app/)

---

## 🗺️ Roadmap

Planned future improvements:

* Third-party payment integration
* Export data (CSV / PDF)
* Dark mode

---

## 🤝 Contributions

Contributions and suggestions are welcome! Feel free to open an issue or submit a pull request.

---

## 🧑‍💻 Author

**Samuel Mbah**
* **GitHub:** [samuelcmbah](https://github.com/samuelcmbah)
* **LinkedIn:** [Samuel Mbah](https://linkedin.com/in/samuelcmbah)
