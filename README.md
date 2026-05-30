# PRN232 Lab 2 - Advanced REST API & Security

Welcome to the Learning Management System (LMS) Web API project for **PRN232 Lab 2 - Advanced REST API & Security**. 

This version extends Lab 1 by integrating advanced security features (JWT Authentication, Refresh Token Rotation, BCrypt Hashing, Role-Based Access Control), strict validations, Content Negotiation (JSON/XML), API Versioning, custom middlewares, and containerized Docker configurations.

---

## Key Features in Lab 2

1. **Security & Hashing**: Secure user authentication via `BCrypt.Net-Next` password hashing (no plain text passwords saved in database).
2. **JWT Authentication & Rotation**: Fully-featured JWT authentication with Random Refresh Token Rotation and secure SHA-256 database token hashing.
3. **Role-Based Authorization (RBAC)**: Fine-grained access control (e.g. GET endpoints accessible by `Admin` and `Student` roles; POST/PUT/DELETE endpoints restricted to `Admin` only).
4. **API Versioning**: URL segment-based API versioning for Students:
   - `/api/v1/students` (does not return the `phone` property)
   - `/api/v2/students` (returns the `phone` property)
5. **Advanced Nested Resource**: Route `/api/courses/{courseId}/students` to fetch distinct student listings dynamically registered in a course through enrollments.
6. **Advanced Content Negotiation**: Supports both `application/json` and `application/xml` formats. Rejects unsupported formats with an HTTP `406 Not Acceptable` response.
7. **Robust Input Validation**: Multi-level validation employing Data Annotations, `FluentValidation` (for Semesters), and custom `DateOfBirth` check.
8. **Centralized Middleware**:
   - **Global Exception Handling**: Safe, centralized try-catch middleware that intercepts exceptions and responds with a unified HTTP `500` JSON error layout.
   - **Request Logging**: Safe, diagnostic request-execution logging (HTTP Method, Request Path, Response Code, Elapsed Milliseconds) that excludes credentials and secrets.
9. **Swagger JWT Testing**: Swagger UI equipped with Bearer Authorize support and dynamic lock icons on protected endpoints only.

---

## Pre-requisites & Local Environment Configuration

1. **Docker**: Ensure Docker Desktop is installed and running on your system.
2. **SSMS**: SQL Server Management Studio or Azure Data Studio for executing database scripts.
3. **Environment Setup**:
   - Duplicate `.env.example` in the root workspace directory and rename it to `.env`.
   - Provide custom strong password values for SQL Server and JWT signing keys:
     ```env
     MSSQL_SA_PASSWORD=YourStrongPassword123!
     JWT_SECRET=YourSuperLongSecureJwtSecretKeyWithMinimum256BitsOfStrength!
     ```
   *(The `.env` file is excluded in `.gitignore` to keep credentials secure).*

---

## How to Run the Project with Docker

1. **Build and start the containerized services**:
   Run the following terminal command in the root workspace folder:
   ```bash
   docker compose up --build -d
   ```
   This initializes:
   - SQL Server Database container at host port **`12433`** (container port `1433`).
   - Web API container at host port **`8081`** (container port `8080`).

2. **Initialize and Seed the Database**:
   - Connect to SQL Server using SSMS or Azure Data Studio:
     - **Server Name**: `localhost,12433`
     - **Authentication**: SQL Server Authentication
     - **Login**: `sa`
     - **Password**: *[The MSSQL_SA_PASSWORD value specified in your local `.env` file]*
   - Open and execute the database script located at:
     ```text
     database/PRN232_LMS_full_database.sql
     ```
     *(This script automatically drops existing tables in safe order, creates the schema, and seeds default records including Admin/Student credentials).*

3. **Access Swagger UI**:
   Open your browser and navigate to:
   **[http://localhost:8081/swagger](http://localhost:8081/swagger)**

---

## Testing Credentials

The SQL script seeds two testing user accounts equipped with BCrypt hashed passwords:

| Username | Password | Role |
| :--- | :--- | :--- |
| **admin** | `Admin@123` | **Admin** |
| **student** | `Student@123` | **Student** |

---

## Key Testing Endpoints & Workflow

1. **Obtain Access Token**:
   - Send `POST /api/auth/login` with `admin` or `student` credentials.
   - Copy the `accessToken` string from the JSON response.
2. **Authorize Swagger UI**:
   - Click the green **Authorize** button in Swagger UI.
   - Paste the token in the text input box and click **Authorize**.
3. **Test Versioning**:
   - Use the Swagger dropdown selector to toggle between `"v1"` and `"v2"` docs.
   - Verify `GET /api/v1/students` does not contain `phone` fields.
   - Verify `GET /api/v2/students` does contain `phone` fields.
4. **Test Role-Based Access Control**:
   - **Student token**: Calling `GET /api/students` returns `200 OK`. Calling `POST /api/students` yields `403 Forbidden` with a standard `ApiResponse` layout.
   - **Admin token**: Calling `POST /api/students` completes successfully (`201 Created`).
   - **Invalid or Missing Token**: Calling protected endpoints yields `401 Unauthorized` with `WWW-Authenticate: Bearer` headers.
5. **Test Nested Endpoint**:
   - Call `GET /api/courses/{courseId}/students` to check registered students. Try `expand=enrollments` to see filtered course-specific enrollments.
