# 🚀 ExpenseVista  
A modern full-stack expense tracking application built with **.NET 8**, **PostgreSQL**, and **React + TypeScript**.

![Status](https://img.shields.io/badge/status-active-success)
![Frontend](https://img.shields.io/badge/frontend-react-blue)
![Backend](https://img.shields.io/badge/backend-.NET%208-purple)
![License](https://img.shields.io/badge/license-MIT-green)

---

## 📸 Screenshots

![Dashboard](./screenshots/dashboard.png)
![Analytics](./screenshots/analytics.png)

---

## 📘 Overview
ExpenseVista helps users track expenses, analyze spending patterns, and visualize their financial activity with modern charts and dashboards.

This project showcases:
- scalable backend architecture  
- clean API design  
- modern React frontend development  
- authentication flows  
- professional development workflow  

---

## 🛠️ Tech Stack

### **Backend**
- .NET 8 Web API  
- Entity Framework Core  
- PostgreSQL  
- Serilog  
- AutoMapper  
- FluentValidation  

### **Frontend**
- React (TypeScript)  
- Vite  
- TailwindCSS  
- ShadCN UI  
- React Query  
- React Hook Form + Zod  
- Axios
- Recharts  

---

## ✨ Features
Authentication & Security

User registration & login

Secure JWT auth

Refresh token support

Password hashing

Transaction Management

Create, update, delete transactions

Categorized income & expenses

Filtering, sorting & pagination

Date-based queries

Category Management

Custom categories

System categories

Analytics Dashboard

Monthly and yearly spending summaries

Category breakdown

Pie and bar charts

Trend visualization

Beautiful, Modern UI

Mobile-friendly

Clean UX with reusable components

Form validation with Zod

🏛️ Architecture
Backend Architecture

Clean Architecture principles

DTO-based communication

Repository + Service pattern

Global exception handling

Logging with Serilog

Strong separation of concerns

Frontend Architecture

Component-driven design

API abstraction layer

React Query (caching + data synchronization)

Form schemas (Zod) for safe client-side validation

Typed data models (TypeScript)

📁 Project Structure
Backend
ExpenseVista.API/
 ├── Controllers/
 ├── DTOs/
 ├── Models/
 ├── Services/
 ├── Repositories/
 ├── Mappings/
 ├── Middleware/
 └── Migrations/

Frontend (separate repo)

Frontend repo link: add your GitHub link here

src/
 ├── components/
 ├── pages/
 ├── hooks/
 ├── api/
 ├── types/
 └── utils/

⚙️ Installation & Setup
Backend Setup
git clone <backend-repository-url>
cd ExpenseVista.API

dotnet restore
dotnet ef database update
dotnet run


Backend runs by default on:
https://localhost:7000

Frontend Setup
git clone <frontend-repository-url>
cd expensevista-frontend

npm install
npm run dev


Frontend runs on Vite’s default port:
http://localhost:5173

🔐 Environment Variables
Backend
ConnectionStrings__Default=YourPostgresConnection
JWT__Key=YourSecretKey
JWT__Issuer=ExpenseVista
Cloudinary__CloudName=...
Cloudinary__ApiKey=...
Cloudinary__ApiSecret=...

Frontend
VITE_API_URL=https://localhost:7000/api

📸 Screenshots

(Add screenshots in this section — recruiters love visuals)

Dashboard

Add Transaction Page

Analytics

Category View

Example placeholder:

![Dashboard Screenshot](./screenshots/dashboard.png)

🧪 Testing (Optional)

Unit tests for services

Integration tests for controllers

🚀 Deployment

Backend: Render / Azure / Railway

Frontend: Vercel

Database: Supabase / Railway / Azure PostgreSQL

(Add actual deployment links when ready.)

🗺️ Roadmap

Planned future improvements:

Budget planning

Recurring expenses

Export data (CSV / PDF)

Email verification

Dark mode

Mobile app version (React Native)

🙋‍♂️ Why I Built ExpenseVista

I wanted to build a real-world, full-stack financial application that demonstrates:

Strong backend architecture

Modern React frontend development

Data visualization

Secure authentication

Real user workflows

This project represents my growth as a developer and my readiness for professional engineering roles.

🤝 Contributions

Contributions and suggestions are welcome.

🧑‍💻 Author

Samuel Mbah
GitHub: add link
LinkedIn: add profile link