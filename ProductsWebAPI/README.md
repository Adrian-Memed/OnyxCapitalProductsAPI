# ProductsWebAPI
This Products API is a simple .NET 8 Web API built with JWT Authentication and uses Dapper for data access. It includes CRUD operations on products, filtering by color, health checks, and validation using FluentValidation.

# Features
JWT Authentication: Secure endpoints.
Dapper: For lightweight database access.
FluentValidation: Custom and reusable validation rules.
Health Checks: Basic SQL Server health check.

# Prerequisites
.NET 8 SDK
SQL Server (or you can use Azure SQL Serverless as configured)

# API Endpoints

Authentication
POST /api/auth/login
Description: Logs in a user and returns a JWT token.

Products
GET /api/products
Description: Retrieves all products.
Authorization: Bearer token required.

GET /api/products/{colour}
Description: Retrieves products filtered by color.
Authorization: Bearer token required.

POST /api/products
Description: Creates a new product.
Authorization: Bearer token required.

# Health Check
GET /health