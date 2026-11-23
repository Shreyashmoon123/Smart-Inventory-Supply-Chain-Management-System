# Smart Inventory & Supply Chain Management System

A full-stack web application designed to manage inventory, products, suppliers, customers, warehouses, purchases, sales, and stock transfers from a centralized dashboard.

The main goal of this project is to simplify inventory operations, maintain accurate stock records, and manage supply chain activities efficiently.

---

## 1. Project Overview

The Smart Inventory & Supply Chain Management System helps businesses manage their daily inventory and supply chain operations.

Using this application, users can:

- Manage products and categories.
- Manage suppliers and customers.
- Manage multiple warehouses.
- Track available stock and low-stock products.
- Create and manage purchase orders.
- Create and manage sales orders.
- Record payments.
- Transfer stock between warehouses.
- View reports and stock transactions.
- Monitor user activities through audit logs.

The project follows a frontend-backend architecture. Angular handles the user interface, ASP.NET Core Web API handles business logic, and SQL Server stores application data.

---

## 2. Technologies Used

### Backend

- C#
- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- SQL Server
- LINQ
- JWT Authentication
- Role-Based Authorization
- REST APIs

### Frontend

- Angular
- TypeScript
- HTML5
- SCSS
- CSS
- Angular Router
- Angular HttpClient

### Development Tools

- Visual Studio Code
- SQL Server Management Studio (SSMS)
- Git
- GitHub
- Swagger UI
- Node.js and npm

---

## 3. Project Architecture

The project is divided into two main applications.

### Backend – SmartInventory.API

The backend is responsible for:

- Handling HTTP requests.
- Implementing business logic.
- Validating incoming data.
- Communicating with SQL Server using Entity Framework Core.
- Managing authentication and authorization.
- Returning API responses to Angular.

### Frontend – SmartInventory.Angular

The frontend is responsible for:

- Displaying the admin dashboard.
- Providing forms and data tables.
- Communicating with backend APIs.
- Managing login and logout.
- Sending JWT tokens with authorized requests.
- Displaying reports and inventory information.

### Application Flow

```text
User
  |
  v
Angular Frontend
  |
  | HTTP Requests
  v
ASP.NET Core Web API
  |
  | Business Logic
  v
Entity Framework Core
  |
  v
SQL Server Database
```

---

## 4. Main Project Modules

### 1. Authentication & Authorization

- User registration and login.
- JWT-based authentication.
- Secure password handling through the backend password service.
- Role-based access control.
- Protected frontend routes.
- JWT interceptor for attaching access tokens to API requests.

**Available Roles:**

- Admin
- Manager
- Staff

Each role has different permissions depending on backend authorization rules.

### 2. Dashboard

- Centralized admin dashboard.
- Inventory and business overview.
- Summary information for important operations.

### 3. Category Management

- Add categories.
- View categories.
- Update categories.
- Delete categories.

### 4. Product Management

- Add products.
- View product details.
- Update product information.
- Delete products.
- Manage product categories.

### 5. Supplier Management

- Add suppliers.
- View supplier information.
- Update supplier details.
- Delete suppliers.

### 6. Warehouse Management

- Manage warehouse information.
- View available warehouses.
- Maintain warehouse records.

### 7. Customer Management

- Add customers.
- View customer information.
- Update customer details.
- Delete customers.

### 8. Inventory Management

- View inventory records.
- Track product stock by warehouse.
- Check available quantities.
- Filter inventory records.
- Identify low-stock products.
- Monitor reorder levels.

### 9. Purchase Management

- Create purchase orders.
- View purchase orders.
- Manage purchase order information.
- Track purchase operations.

### 10. Sales Management

- Create sales orders.
- View sales orders.
- Manage sales operations.
- Track sales information.

### 11. Payment Management

- Record and manage payment information.
- View payment records.
- Track payment status.

### 12. Stock Transfer Management

Manage stock movement between warehouses.

**Transfer Workflow:**

```text
Pending
   |
   v
Approved
   |
   v
InTransit
   |
   v
Completed
```

- Create stock transfers.
- Approve transfers.
- Dispatch stock from the source warehouse.
- Complete transfers at the destination warehouse.
- Cancel eligible transfers.
- Record stock transactions for transfer operations.

### 13. Reports

The reporting module provides:

- Sales Reports.
- Purchase Reports.
- Inventory Reports.
- Stock Transaction Reports.
- Date-based filtering where supported.
- Low-stock information.

### 14. Audit Logs

- Monitor important user activities.
- View action details.
- Filter logs by user, action, entity, and date.
- View individual audit log details.

---

## 5. Backend Architecture

The backend is developed using ASP.NET Core Web API.

The application uses Entity Framework Core to communicate with SQL Server.

Main backend concepts used:

- Controllers – Handle HTTP requests and responses.
- Services – Contain business logic.
- Interfaces – Define service contracts.
- Models/Entities – Represent database tables.
- DTOs – Transfer data between API layers and clients.
- DbContext – Manage database operations.
- Middleware – Handle cross-cutting request processing.
- JWT – Authenticate users.
- Authorization – Restrict access based on user roles.

The backend also includes validation, exception handling, and concurrency handling for supported operations.

---

## 6. Frontend Architecture

The frontend is developed using Angular with TypeScript and standalone components.

The application uses a feature-based folder structure to keep code organized.

Main folders:

```text
src/
|
|-- app/
|   |
|   |-- core/
|   |   |-- models/
|   |   |-- services/
|   |
|   |-- features/
|   |   |-- dashboard/
|   |   |-- categories/
|   |   |-- products/
|   |   |-- suppliers/
|   |   |-- warehouses/
|   |   |-- customers/
|   |   |-- inventory/
|   |   |-- purchases/
|   |   |-- sales/
|   |   |-- payments/
|   |   |-- stock-transfers/
|   |   |-- reports/
|   |   |-- audit-logs/
|   |
|   |-- layouts/
|   |   |-- admin-layout/
|   |
|   |-- guards/
|   |-- interceptors/
|   |
|   |-- app.config.ts
|   |-- app.routes.ts
|
|-- environments/
|-- assets/
```

**Important frontend concepts:**

- Components manage UI and user interactions.
- Services communicate with backend APIs.
- Models define TypeScript data structures.
- Routing manages navigation.
- Guards protect restricted routes.
- Interceptors attach JWT tokens to requests.
- Environment configuration stores the backend API base URL.

---

## 7. Installation & Setup

Follow these steps to run the project on your local machine.

### Prerequisites

Install the following:

1. .NET 10 SDK
2. Node.js and npm
3. Angular CLI
4. SQL Server
5. SQL Server Management Studio (SSMS)
6. Visual Studio Code

### Step 1: Clone Repository

```bash
git clone https://github.com/Shreyashmoon123/Smart-Inventory-Supply-Chain-Management-System.git
```

Open the project folder:

```bash
cd Smart-Inventory-Supply-Chain-Management-System
```

### Step 2: Configure Backend

Open the backend folder:

```bash
cd SmartInventory.API
```

Configure your SQL Server connection string in the appropriate configuration file.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_SQL_SERVER_CONNECTION_STRING"
  }
}
```

Use the actual connection string expected by the backend configuration.

Also configure the JWT settings and initial administrator settings using your local configuration or secure environment variables.

**Important:** Never upload real passwords, JWT signing keys, or database credentials to GitHub.

### Step 3: Run Backend

```bash
dotnet restore
dotnet run
```

Backend API:

http://localhost:5037/

Swagger UI:

http://localhost:5037/swagger

### Step 4: Configure Frontend

Open another terminal:

```bash
cd SmartInventory.Angular
```

Install dependencies:

```bash
npm install
```

Check the environment configuration and make sure the API base URL points to the running backend.

Example:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5037/api'
};
```

Use the actual property names and configuration already present in the project.

### Step 5: Run Angular

```bash
ng serve
```

Open the application:

http://localhost:4200

---

## 8. Authentication Flow

The application uses JWT-based authentication.

Basic login flow:

```text
User enters login details
          |
          v
Angular sends login request
          |
          v
ASP.NET Core validates credentials
          |
          v
JWT Token is generated
          |
          v
Angular stores authentication data
          |
          v
HTTP Interceptor attaches JWT
          |
          v
Protected API validates token
          |
          v
API returns authorized response
```

Role-based authorization is enforced on the backend. Frontend route guards are used to control navigation and improve the user experience.

---

## 9. Important URLs

| Application | URL |
|---|---|
| Angular Frontend | http://localhost:4200 |
| Backend API | http://localhost:5037 |
| Swagger UI | http://localhost:5037/swagger |

---

## 10. Important Development Commands

### Backend

Restore packages:

```bash
dotnet restore
```

Build project:

```bash
dotnet build
```

Run API:

```bash
dotnet run
```

### Frontend

Install dependencies:

```bash
npm install
```

Run Angular:

```bash
ng serve
```

Build Angular:

```bash
ng build
```

---

## 11. Project Highlights

- Full-stack application using Angular and ASP.NET Core Web API.
- RESTful API development.
- SQL Server database integration.
- Entity Framework Core.
- JWT authentication.
- Role-based authorization.
- Feature-based Angular architecture.
- Inventory and warehouse management.
- Purchase and sales workflows.
- Stock transfer workflow.
- Reports and audit logs.
- API testing through Swagger.
- Git and GitHub version control.

---

## 12. Learning Outcomes

While developing this project, I worked with:

- ASP.NET Core Web API development.
- C# and object-oriented programming.
- Entity Framework Core and database operations.
- SQL Server integration.
- DTOs and service-based architecture.
- JWT authentication and role-based authorization.
- Angular standalone components.
- TypeScript.
- Angular services and HTTP communication.
- Angular routing, guards, and interceptors.
- Frontend and backend integration.
- Debugging and testing API-based applications.
- Git and GitHub.

---

## 13. Future Improvements

Possible future enhancements:

- Dashboard charts and analytics.
- Export reports to Excel and PDF.
- Pagination and advanced filtering.
- Email notifications.
- Barcode and QR code support.
- Docker deployment.
- Cloud hosting and deployment.
- Automated unit and integration testing.

---

## 14. Developer

Developed as a full-stack learning and practical development project using .NET and Angular technologies.

**GitHub Repository:**

https://github.com/Shreyashmoon123/Smart-Inventory-Supply-Chain-Management-System

---

**Project Name:** Smart Inventory & Supply Chain Management System

**Backend:** ASP.NET Core Web API

**Frontend:** Angular + TypeScript

**Database:** SQL Server
