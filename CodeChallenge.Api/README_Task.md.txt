# Question 1 – Approach

- Implemented RESTful API using ASP.NET Core Web API.
- Designed organization-based routing to support multi-tenant architecture.
- Followed Repository Pattern for separation of concerns.
- Used async/await for scalability and non-blocking operations.
- Implemented DTOs (CreateMessageRequest, UpdateMessageRequest) to prevent over-posting.
- Returned appropriate HTTP status codes (200 OK, 201 Created, 404 NotFound, 204 NoContent).
- Logging added using ILogger for tracking missing resources.
- Ensured data isolation by validating organizationId for every request.



Question 2 – Improvements
If more time:
Introduce DTOs
Add FluentValidation
Add global exception middleware
Add pagination/filtering
Add authentication/authorization
Add integration tests
Add structured logging
Add caching