# IdentityCore
## 1. Overview

`IdentityCore` is a microservice responsible for managing identity and authentication within the platform.

It provides a centralized and consistent way to handle:

- User registration
- Authentication (login)
- Credential management
- Email verification
- Password recovery
- Role-based identity context
- Token-based access (JWT)

The service is designed to support a platform where users can both consume and offer services, using a unified identity model where different actor types (e.g., Client, Partner, Admin) are represented as users with different roles.

---

### Design Principles

The design of `IdentityCore` follows a set of practical and industry-aligned principles:

- **Separation of concerns** between identity, authentication, and business-domain logic
- **Layered architecture** to keep domain logic independent from infrastructure and frameworks
- **Unified identity model** (single User entity with roles)
- **Incremental evolution**, starting from a secure MVP and growing toward more advanced security features

---

### References and Practices

The design decisions in this service are informed by widely adopted security and identity best practices, including:

- **OWASP (Open Web Application Security Project)**
  - Authentication and Authorization Cheat Sheets
  - Session Management guidelines
  - Security best practices for web applications

- **NIST Digital Identity Guidelines (SP 800-63)**
  - Concepts such as authentication assurance levels
  - Identity lifecycle and credential handling

- **General industry practices**
  - Token-based authentication (JWT)
  - Role-Based Access Control (RBAC)
  - Secure credential management
  - Minimal and explicit identity boundaries

These references are used as guidance, while adapting decisions to the needs and scope of the MVP.

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

At this stage, `Client` and `Partner` are not modeled as separate identity entities. They are represented as users with different roles. [REVIEW]

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

The authorization model defines what an authenticated user is allowed to do within the system.

### 6.1 Authorization Approach

For the MVP, the platform uses a **role-based access control (RBAC)** approach.

Users are assigned one or more roles, and access decisions are made based on those roles.

---

### 6.2 Roles

The initial authorization model includes the following roles:

- **Client**
- **Partner**
- **Admin**

These roles define the high-level authorization context of each user.

---

### 6.3 Role Purpose

- **Client**
  - Can access user capabilities related to consuming or requesting services

- **Partner**
  - Can access capabilities related to offering services through the platform

- **Admin**
  - Can access administrative and operational capabilities

---

### 6.4 Authorization Boundary

`IdentityCore` is responsible for providing the basic authorization context of the authenticated user, such as:

- User identity
- Assigned role(s)
- Account status

However, domain-specific authorization rules should remain outside of the core identity service.

For example, business rules related to marketplace actions, service coordination, or operational constraints should be handled by the corresponding domain services.

---

### 6.5 MVP Decision

For the MVP, authorization is intentionally kept simple:

- Access is primarily based on user roles
- Fine-grained permission models are out of scope for the initial version
- Future versions may introduce more advanced authorization mechanisms if required

## 7. Flows

This section describes the main identity and authentication flows supported by the service.

### 7.1 User Registration

1. A user submits the registration data
2. A new user account is created in `PendingVerification` status
3. The system triggers the email verification process
4. The user must verify the email address before obtaining full authenticated access

---

### 7.2 Email Verification

1. The system generates a verification token
2. The verification token is sent to the user's email address
3. The user follows the verification process
4. If the token is valid, the account status changes from `PendingVerification` to `Active`

---

### 7.3 Login

1. The user submits email and password
2. The system validates the credentials
3. The system checks whether the account is eligible for authentication
4. If authentication succeeds, a JWT access token is issued
5. If authentication fails, access is denied and the attempt is logged

---

### 7.4 Password Recovery

1. The user initiates a password recovery request
2. The system generates a password reset token
3. The token is sent to the user's email address
4. The user provides a new password through the reset flow
5. If the reset process is valid, the credential is updated

---

### 7.5 Future Flows

Future versions may include additional flows such as:

- Logout
- Refresh token renewal
- Multi-factor authentication
- Administrative account unlock
- Partner onboarding extensions

## 8. Session / Token Strategy

The system uses a token-based authentication approach to maintain authenticated sessions.

### 8.1 Token Type

For the MVP, the system issues:

- **JWT access tokens**

These tokens represent the authenticated identity of the user and are used to authorize requests to protected endpoints.

---

### 8.2 Token Lifecycle

- Tokens are issued after successful authentication
- Tokens have a limited lifetime (short-lived)
- Once expired, the user must authenticate again

For the MVP, no refresh token mechanism is implemented.

---

### 8.3 Token Usage

- The client is responsible for storing the token
- The token must be included in subsequent requests to access protected resources
- Each request is validated based on the token contents

---

### 8.4 Token Content (Claims)

The token contains minimal identity information, such as:

- User identifier
- Email
- Assigned role(s)

The token should not include sensitive or business-specific data.

---

### 8.5 Token Expiration Strategy

The system uses short-lived tokens to reduce risk in case of token leakage.

The exact duration may be configured, but should remain within a reasonable time window (e.g., 15–60 minutes).

---

### 8.6 Token Revocation (MVP Decision)

For the initial MVP:

- No active token revocation mechanism is implemented
- Tokens become invalid only after expiration

Future iterations may introduce refresh tokens, token rotation, or revocation strategies.
## 9. Security Considerations

Security is a fundamental concern in the identity and authentication layer.

The following measures are considered for the MVP:

---

### 9.1 Password Security

- Passwords must never be stored in plain text
- Secure hashing algorithms must be used
- Password handling must follow industry best practices

---

### 9.2 Protection Against Brute Force Attacks

- Repeated failed login attempts should be monitored
- Future iterations may introduce rate limiting or temporary account lockout

---

### 9.3 Generic Error Responses

- Authentication errors must not reveal whether the email or password was incorrect
- This prevents user enumeration attacks

---

### 9.4 Logging and Auditing

The system should log relevant security events, including:

- Login attempts (successful and failed)
- Password reset requests
- Account state changes

Logs must not contain sensitive information such as raw passwords.

---

### 9.5 Input Validation

- All inputs must be validated
- Special care must be taken to prevent injection attacks or malformed requests

---

### 9.6 Token Security

- Tokens should be short-lived
- Tokens must not include sensitive data
- Token validation must be enforced in protected endpoints

---

### 9.7 Future Improvements

Future versions may include:

- Rate limiting
- Multi-factor authentication (MFA)
- Advanced anomaly detection
- Device-based trust mechanisms

## 10. Data Model (High Level)

The identity system is built around a small set of core entities.

### Core Entities

- **User**
  - Represents the identity of a person in the system

- **Role**
  - Defines access level (Client, Partner, Admin)

- **Credential**
  - Stores authentication-related data (e.g., password hash)

- **VerificationToken**
  - Used for email verification processes

- **PasswordResetToken**
  - Used for password recovery flows

---

The exact database schema may evolve, but these conceptual entities define the core identity model.

## 11. Architecture Notes

This section describes the high-level architectural direction of the `IdentityCore` microservice.

### 11.1 Architectural Style

`IdentityCore` follows a layered architecture in order to separate concerns and keep identity-related logic maintainable as the system evolves.

The current design is conceptually organized into the following layers:

- **Domain**
- **Application**
- **Infrastructure**
- **Presentation**

This structure aims to keep identity rules and business decisions separated from transport, persistence, and framework concerns.

---

### 11.2 Layer Responsibilities

#### Domain

The Domain layer contains the core identity concepts and business rules related to identity.

This includes concepts such as:

- User
- Roles
- Account status
- Identity lifecycle rules

The Domain layer should remain independent from framework-specific concerns.

---

#### Application

The Application layer coordinates use cases and application flows.

Examples include:

- User registration
- Login
- Email verification
- Password recovery
- Role assignment

This layer orchestrates the execution of identity-related operations using domain concepts and infrastructure services.

---

#### Infrastructure

The Infrastructure layer provides technical implementations required by the application.

This may include:

- Database access
- Persistence
- Identity provider integrations
- Token generation
- Email delivery integrations
- Framework-specific identity services

Infrastructure should support the domain and application layers without leaking unnecessary technical complexity into them.

---

#### Presentation

The Presentation layer exposes the microservice capabilities to external consumers.

This includes:

- HTTP endpoints
- Request/response contracts
- Authentication entry points
- Transport-level validation and mapping

Presentation is responsible for receiving external requests and delegating them to the application layer.

---

### 11.3 Identity Service Boundary

`IdentityCore` is intended to remain focused on identity and authentication concerns.

It should handle responsibilities such as:

- Account creation
- Authentication
- Role-based identity context
- Email verification
- Password recovery
- Token issuance

It should avoid taking ownership of business-domain concerns that belong to other services, such as:

- Service marketplace behavior
- Booking or coordination workflows
- Pricing and availability
- Payment processing
- Operational business logic unrelated to identity

---

### 11.4 Current Technical Direction

The current implementation direction includes:

- .NET-based microservice architecture
- Layered project structure
- JWT-based authentication approach
- A unified user identity model with role-based differentiation

This provides a practical foundation for the MVP while allowing future evolution.

---

### 11.5 Design Intent

The design intent of `IdentityCore` is to provide a clear and maintainable identity boundary for the platform.

As the project evolves, the architecture should preserve the following principles:

- Identity logic should remain centralized and explicit
- Authentication and authorization should remain separated from business-domain workflows
- Roles should define access context, not duplicate identity entities
- The service should evolve incrementally without losing clarity of responsibility

## 12. Open Questions

This section captures open design decisions that require further discussion or validation as the system evolves.

---

### 12.1 Identity Model

- Can a user have multiple roles simultaneously (e.g., Client and Partner)?
- Should a user be able to switch roles dynamically?
- Is a Partner just a role, or will it require a separate domain entity in the future?
- Should additional identity attributes (e.g., phone number, profile data) be part of `IdentityCore` or handled by another service?

---

### 12.2 Registration and Onboarding

- Should Partner capabilities be assigned during registration or through a separate onboarding process?
- Will Partner activation require additional verification beyond email (e.g., manual approval)?
- Should there be different registration flows for Client vs Partner?

---

### 12.3 Authentication

- Should email verification be strictly required before allowing login?
- Will multi-factor authentication (MFA) be required for certain roles (e.g., Admin, Partner)?
- Should the system support alternative authentication methods in the future (e.g., social login, phone-based login)?

---

### 12.4 Session and Token Strategy

- Should refresh tokens be introduced after the MVP?
- Should token revocation be supported (e.g., logout, compromised account)?
- What should be the final token expiration policy?

---

### 12.5 Account Security

- What is the strategy for handling repeated failed login attempts?
- Should accounts be temporarily locked after a number of failed attempts?
- Who is responsible for unlocking accounts (automatic vs admin-driven)?

---

### 12.6 Authorization

- Will the system require more fine-grained permissions beyond roles?
- Should authorization rules evolve into a policy-based or attribute-based model in the future?

---

### 12.7 Architecture and Boundaries

- Should partner-specific data eventually live in a separate microservice?
- How should `IdentityCore` integrate with other services in terms of identity propagation?
- Should the system move toward an external identity provider in the future (e.g., OAuth, third-party IdP)?

---

### 12.8 Operational Concerns

- What level of logging and auditing is required for production?
- How should sensitive security events be monitored?
- What is the strategy for handling incidents related to compromised accounts?

---

These questions are intentionally left open to guide future iterations and ensure that architectural decisions remain explicit and deliberate.

## 13. Evolution Plan

The `IdentityCore` service is designed to evolve incrementally as the platform grows in complexity, usage, and security requirements.

---

### Level 1 – MVP (Current Stage)

Focus: simplicity, clarity, and essential security

- Email + password authentication
- Email verification required for account activation
- Basic role-based access control (Client, Partner, Admin)
- JWT access tokens (no refresh tokens)
- Password recovery flow
- Basic logging of authentication events
- Minimal security protections (generic errors, input validation)

---

### Level 2 – Enhanced Security and Reliability

Focus: protecting real users and reducing risk

- Introduction of **refresh tokens**
- Improved session management and token lifecycle
- Rate limiting and anti-abuse mechanisms
- Account lockout strategies after repeated failed attempts
- Multi-factor authentication (MFA) for sensitive roles (e.g., Admin, Partner)
- Improved auditing and monitoring of security events
- More robust email and recovery workflows

---

### Level 3 – Advanced Identity and Scalability

Focus: scalability, trust, and advanced identity capabilities

- Support for additional authentication methods (e.g., social login, phone-based login)
- Stronger authentication mechanisms (e.g., phishing-resistant methods)
- Device-aware or risk-based authentication
- Token revocation and session invalidation strategies
- Integration with external identity providers (OAuth, third-party IdP)
- Fine-grained authorization models (beyond RBAC)
- Identity verification processes for high-trust users (if required by the business)

---

### Long-Term Vision

The long-term goal is for `IdentityCore` to act as a robust and well-defined identity provider within the platform, maintaining clear boundaries and supporting secure, scalable authentication and authorization across services.
