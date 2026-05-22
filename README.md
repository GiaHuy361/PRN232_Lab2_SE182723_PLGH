## Summary

This project is an ASP.NET Core RESTful API for a simple Learning Management System (LMS). It was developed for PRN232 Lab 1 and follows a 3-layer architecture, including API, Service, and Repository layers.

The system manages basic learning data such as students, semesters, subjects, courses, and enrollments. It supports CRUD operations and provides list API features such as searching, sorting, paging, field selection, and expansion of related data.

The project uses Entity Framework Core with a database-first approach. The database was created first in SQL Server, then entity models and `LmsDbContext` were scaffolded from the existing database.

To separate responsibilities clearly, the project uses four model types: Entity Models, Business Models, Request Models, and Response Models. Entity Models are used for database mapping, Business Models are used in the service layer, Request Models are used for client input, and Response Models are used for API output.

Swagger is integrated for API documentation and testing. Docker is also configured so that both the API and SQL Server database can run inside containers using Docker Compose.
