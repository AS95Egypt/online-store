# 🛍️ Online Store

A single-vendor online store for browsing products, managing a shopping cart, and placing Cash-on-Delivery orders. This repository includes both the customer storefront and administrator tools.

| Application | Technology | Location |
| --- | --- | --- |
| Frontend | Angular, TypeScript | [`frontend/`](frontend/) |
| Backend | ASP.NET Core Web API, Entity Framework Core, SQL Server | [`backend/`](backend/) |

---

## 🚀 Features

### Storefront

* 🛍️ Browse, search, filter, and sort products
* 🗂️ View product categories and product details
* 🛒 Add products to a cart and update quantities
* 📦 Checkout with saved shipping details and governorate-based fees
* 🧾 Place Cash-on-Delivery orders and view order history
* 👤 Register, sign in, manage a profile, and change a password

### Administration

* 🔐 Customer and administrator roles with backend authorization
* 🏷️ Manage categories, brands, and products
* 📋 Search, filter, and manage orders through the status workflow
* 💵 Confirm Cash-on-Delivery payments
* 🚚 View and update governorate shipping fees

### API

* 🔑 JWT authentication and role-based authorization
* 🗄️ SQL Server persistence with EF Core migrations
* 📖 Swagger / OpenAPI documentation
* ✅ Health endpoint at `GET /api/health`

---

## 🛠️ Technology Stack

* **Frontend:** Angular 22, TypeScript, RxJS
* **Backend:** ASP.NET Core Web API on .NET 8
* **Data access:** Entity Framework Core and SQL Server
* **Authentication:** JWT bearer tokens
* **API documentation:** Swagger / OpenAPI
* **Testing:** Vitest (frontend), xUnit and `WebApplicationFactory` (backend)

---

## ⚙️ Getting Started

### Prerequisites

Install the following:

* [Node.js LTS](https://nodejs.org/) and npm
* [.NET SDK 8.0.423](https://dotnet.microsoft.com/download/dotnet/8.0) (see [`backend/global.json`](backend/global.json))
* SQL Server, such as SQL Server Express LocalDB or SQL Server Express
* Git

The default backend connection string uses the `.\SQLEXPRESS` instance. Configure a different SQL Server connection string before running the API if needed.

---

## 📥 Installation

Clone the repository and enter its directory:

```bash
git clone https://github.com/AS95Egypt/online-store.git
cd online-store
```

Restore and build the backend:

```bash
cd backend
dotnet restore
dotnet build
```

Install frontend dependencies:

```bash
cd ../frontend
npm install
```

---

## 🗄️ Backend Configuration

### Database connection

The API reads the connection string named `DefaultConnection`. It defaults to:

```text
Server=.\SQLEXPRESS;Database=OnlineStoreDb;Trusted_Connection=True;TrustServerCertificate=True;
```

Set a local connection string with .NET user-secrets from the `backend` directory:

```bash
cd backend
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\SQLEXPRESS;Database=OnlineStoreDb;Trusted_Connection=True;TrustServerCertificate=True;" --project src/OnlineStore.Api
```

### JWT settings

The API requires `Jwt:Secret`. Set a sufficiently long, private value and the issuer/audience using user-secrets from the `backend` directory:

```bash
cd backend
dotnet user-secrets set "Jwt:Secret" "replace-with-a-long-local-secret-value" --project src/OnlineStore.Api
dotnet user-secrets set "Jwt:Issuer" "online-store-local" --project src/OnlineStore.Api
dotnet user-secrets set "Jwt:Audience" "online-store-client" --project src/OnlineStore.Api
```

Never commit local secrets or production credentials.

### Development administrator account

When running in the `Development` environment, the API can seed an administrator if both settings are configured:

```bash
cd backend
dotnet user-secrets set "Seed:AdminEmail" "admin@example.com" --project src/OnlineStore.Api
dotnet user-secrets set "Seed:AdminPassword" "replace-with-a-strong-local-password" --project src/OnlineStore.Api
```

Public registration always creates a `CUSTOMER`; administrator accounts are not created through public registration.

---

## 🗃️ Database Migrations

Install the EF Core command-line tool if needed:

```bash
dotnet tool install --global dotnet-ef
```

Apply migrations from the `backend` directory:

```bash
cd backend
dotnet ef database update --project src/OnlineStore.Infrastructure --startup-project src/OnlineStore.Api
```

The application does not automatically apply migrations. Apply them before starting the API against a new or updated database.

---

## ▶️ Running the Applications

Start the backend from the `backend` directory:

```bash
cd backend
dotnet run --project src/OnlineStore.Api
```

The configured HTTP URL is `http://localhost:5188`. Swagger is available at:

```text
http://localhost:5188/swagger
```

In a second terminal, start the Angular application:

```bash
cd frontend
npm start
```

The frontend is served at `http://localhost:4200` and uses the API at `http://localhost:5188/api` in its configured environments.

---

## 🧪 Testing and Quality Checks

### Backend

Run the backend integration tests from the `backend` directory:

```bash
cd backend
dotnet test OnlineStore.sln
```

The current integration tests use `WebApplicationFactory<Program>` and exercise basic health and authorization behavior. They do not require Docker or a separate SQL Server test container.

### Frontend

Run the Angular unit tests:

```bash
cd frontend
npm test -- --watch=false
```

Run lint and a production build:

```bash
npm run lint
npm run build
```

---

## 🔗 API Overview

* Public catalog: `/api/products`, `/api/categories`, `/api/brands`
* Authentication and customer profile: `/api/auth`, `/api/customers`
* Cart and checkout: `/api/cart`, `/api/shipping/options`, `/api/orders`
* Admin catalog: `/api/admin/categories`, `/api/admin/brands`, `/api/admin/products`
* Admin orders and shipping fees: `/api/admin/orders`, `/api/admin/shipping/governorates`

Admin API routes require an administrator JWT. Customer-specific cart and order routes require customer authentication. The backend enforces access independently of frontend navigation.

---

## 📌 Project Status

🚧 **Under active development.** Features are implemented incrementally, and the API and user interface may evolve as additional stories are completed.

## 📄 License

No license has been specified for this repository.
