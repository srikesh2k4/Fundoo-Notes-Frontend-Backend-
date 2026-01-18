# Notes Application – ASP.NET Core Backend

**Daily Progress Report**

**Author:** *Kotipalli Srikesh*
**Technology Stack:** ASP.NET Core Web API, Entity Framework Core, SQL Server, JWT Authentication
**Project Type:** Backend REST API

---

## Project Overview

This project implements the backend for a Notes application using **ASP.NET Core Web API**.
The system supports user authentication, note management, labels, and collaboration features.
Development was completed incrementally following a strict daily milestone plan.

---

## Daily Progress Summary

---

## 📅 Day 1 – User Registration & Authentication

**Completion Date:** 05/01 (Monday)

### Implemented Features

* User registration with email and password
* Secure login using JWT authentication
* Email verification workflow
* Forgot password functionality
* Password reset with token validation

### API Endpoints

```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/verify-email
POST /api/auth/forgot-password
POST /api/auth/reset-password
```

### Technical Highlights

* ASP.NET Core Identity for user management
* JWT-based authentication and authorization
* Password hashing and token-based verification
* Input validation and standardized API responses

---

## 📅 Day 2 – Notes: Basic CRUD Operations

**Completion Date:** 06/01 (Tuesday)

### Implemented Features

* Create a new note
* Retrieve all notes for a user
* Retrieve a note by ID
* Update an existing note
* Delete a note

### API Endpoints

```
GET    /api/notes
GET    /api/notes/{id}
POST   /api/notes
PUT    /api/notes/{id}
DELETE /api/notes/{id}
```

### Technical Highlights

* Entity Framework Core with repository pattern
* Authorization enforced on all note operations
* Proper HTTP status codes and error handling
* Soft ownership checks to prevent unauthorized access

---

## 📅 Day 3 – Notes: Advanced Features

**Completion Date:** 07/01 (Wednesday)

### Implemented Features

* Search notes by title or content
* Pin and unpin notes
* Archive and unarchive notes
* Change note color
* Bulk delete notes

### API Endpoints

```
GET    /api/notes/search
PATCH  /api/notes/{id}/pin
PATCH  /api/notes/{id}/archive
PATCH  /api/notes/{id}/color
DELETE /api/notes/bulk
```

### Technical Highlights

* Optimized search queries
* Partial updates using PATCH
* Bulk operations with transaction safety
* Enum-based note states (Pinned, Archived)

---

## 📅 Day 4 – Labels Management

**Completion Date:** 08/01 (Thursday)

### Implemented Features

* Create labels
* Retrieve all labels
* Update label names
* Delete labels

### API Endpoints

```
GET    /api/labels
POST   /api/labels
PUT    /api/labels/{id}
DELETE /api/labels/{id}
```

### Technical Highlights

* One-to-many relationship between users and labels
* Validation to prevent duplicate labels per user
* Cascade handling for label deletion

---

## 📅 Day 5 – Collaborators

**Completion Date:** 09/01 (Friday)

### Implemented Features

* Add collaborators to notes
* View collaborators for a specific note
* Update collaborator permissions
* Remove collaborators

### API Endpoints

```
GET    /api/collaborators/note/{noteId}
POST   /api/collaborators
DELETE /api/collaborators/{id}
PATCH  /api/collaborators/{id}/permission
```

### Technical Highlights

* Role/permission-based access control
* Many-to-many relationship handling
* Secure permission updates
* Ownership checks before collaborator actions

---

## Overall Outcome

* Fully functional ASP.NET Core backend API
* Secure authentication and authorization
* Clean RESTful API design
* Scalable and maintainable architecture
* Ready for frontend or mobile integration

---

## Future Enhancements

* API rate limiting
* Logging with Serilog
* Unit and integration testing
* Swagger customization
* Deployment to cloud (Azure/AWS)


