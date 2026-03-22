# IdentityCore

## 1. Overview

## 2. Scope
### 2.1 Responsibilities

The `IdentityCore` microservice is responsible for managing the identity and authentication layer of the system.

Its responsibilities include:

- User registration (account creation)
- Authentication (login)
- Credential validation (e.g., email and password)
- Email verification process
- Password recovery and reset
- Account lifecycle management (e.g., active, pending verification, locked, suspended)
- Role assignment (e.g., Client, Partner, Admin)
- Issuing authentication tokens (e.g., JWT)
- Basic authorization context (roles and identity claims)
- Security event logging (e.g., login attempts, password resets)

The service manages a unified identity model where all actors (clients, partners, admins) are represented as users with different roles or capabilities.

---

### 2.2 Non-Responsibilities

The `IdentityCore` microservice does NOT handle business domain logic outside of identity and authentication.

This includes:

- Service marketplace logic (offering, booking, coordination)
- Partner business profile management (services, pricing, availability)
- Payments and billing
- Notifications (except those strictly required for authentication, such as email verification or password recovery triggers)
- Authorization rules tied to business logic (beyond basic role checks)
- Any domain-specific workflows unrelated to identity

The service should remain focused on identity, authentication, and access control boundaries.
## 3. Actors

### 3.1 Actor Types

The system currently defines the following actor types:

- **Client**: a user who consumes or requests a service through the application.
- **Partner**: a user who offers services through the application.
- **Admin**: an internal user responsible for operational, support, moderation, or administrative tasks.

At the identity level, all actors are represented as users with different roles.

---

### 3.2 Actor Purpose

Each actor interacts with the system in a different way:

- **Client**
  - Registers an account
  - Authenticates into the platform
  - Requests or coordinates services

- **Partner**
  - Registers an account
  - Authenticates into the platform
  - Offers services through the platform
  - Manages their service-related participation

- **Admin**
  - Accesses administrative capabilities
  - Manages or reviews users when necessary
  - Supports operational and moderation processes

The actor model may evolve in future iterations if the platform introduces additional internal roles, external integrations, or machine-to-machine communication.
## 4. Identity Model

The identity model is centered around a single core entity: **User**.

All actors in the system are represented as users, and their behavior or access level is determined by assigned roles.

### 4.1 Core Identity Entity

The main identity entity is:

- **User**

A user represents any authenticated person in the platform, regardless of whether they consume services, offer services, or perform administrative tasks.

---

### 4.2 Roles

The system currently supports the following roles:

- **Client**
- **Partner**
- **Admin**

Roles define the high-level access context of a user.

At this stage, `Client` and `Partner` are not modeled as separate identity entities. They are represented as users with different roles.

---

### 4.3 Account Status

A user account can be in one of the following states:

- **PendingVerification**: the account was created but the email has not yet been verified.
- **Active**: the account is valid and can authenticate normally.
- **Locked**: the account is temporarily or permanently restricted due to security or operational reasons.
- **Suspended**: the account is disabled by administrative decision or platform policy.

These states are part of the account lifecycle and affect authentication and access decisions.

---

### 4.4 Identity Attributes

At a high level, a user identity includes:

- Unique identifier
- Email
- Credential data
- Email verification status
- Account status
- Assigned role(s)
- Creation and update metadata

The exact persistence model may evolve, but the identity concept should remain centered around a unified user entity.

---

### 4.5 Modeling Decision

The platform adopts a unified identity model:

- A person has a single user identity
- Access and behavior are determined by roles
- Business-specific partner information should remain outside the core identity model unless strictly required for authentication or access control
## 5. Authentication Model

The authentication model defines how users prove their identity and obtain authenticated access to the platform.

### 5.1 Authentication Method

For the initial MVP, users authenticate using:

- **Email + password**

The email acts as the primary login identifier.

Additional authentication methods such as username-based login, phone-based login, social login, or multi-factor authentication may be considered in future iterations.

---

### 5.2 Account Eligibility for Authentication

Authentication behavior depends on the account status:

- **Active**: the user can authenticate normally.
- **PendingVerification**: the user cannot complete normal authentication until the email verification process is completed.
- **Locked**: the user cannot authenticate.
- **Suspended**: the user cannot authenticate.

This ensures that only valid and enabled accounts can obtain authenticated access.

---

### 5.3 Authentication Failure Handling

When authentication fails:

- The system should return a generic response without exposing whether the email or password was incorrect.
- Failed authentication attempts should be logged for security and auditing purposes.
- Repeated failed attempts may later be used to trigger additional protection mechanisms such as rate limiting or temporary lockout.

The exact anti-abuse strategy may evolve in future iterations.

---

### 5.4 Locked and Suspended Accounts

Locked or suspended accounts must not receive authenticated access.

Authentication attempts for these accounts should be denied and logged.

Account recovery or administrative review may be required depending on the cause of the restriction.

---

### 5.5 Authentication Result

When authentication succeeds, the system issues an authenticated access artifact in the form of:

- **JWT access token**

The token represents the authenticated identity and may include basic claims such as:

- User identifier
- Email
- Assigned role(s)

The token content should remain minimal and should not contain unnecessary business-domain data.

---

### 5.6 Initial MVP Decision

For the MVP, the authentication model is intentionally simple:

- Email and password authentication
- Email verification required before full access
- Restricted accounts cannot authenticate
- Generic failure responses
- JWT-based authenticated access

This model provides a secure and understandable starting point while leaving room for future improvements such as refresh tokens, multi-factor authentication, and stronger anti-abuse controls.
## 6. Authorization Model

## 7. Flows

### Registration

### Email Verification

### Login

### Password Recovery

## 8. Session / Token Strategy

## 9. Security Considerations

## 10. Data Model (High Level)

## 11. Architecture Notes

## 12. Open Questions

## 13. Evolution Plan
