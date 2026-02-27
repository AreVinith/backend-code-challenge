Question 5: Testing Strategy

Answer:

I followed a unit testing approach focusing on isolating the business logic layer from external dependencies. The repository was mocked using Moq to simulate different data scenarios such as existing messages, duplicates, and missing records. This ensures tests are fast, reliable, and independent of data storage.

I used:

xUnit as the testing framework

Moq for dependency mocking

FluentAssertions for readable and expressive assertions

The tests cover both positive and negative scenarios including validation failures, conflicts, and not-found cases to ensure business rules behave correctly.

Question 6: Additional Real-World Scenarios

Answer:

In a production environment, I would also test:

Title uniqueness during update (excluding same record)

Boundary value testing (Title length 3 and 200, Content 10 and 1000)

UpdatedAt is set correctly on update

Delete operation only for active messages

Exception handling scenarios (repository failures)

Concurrency scenarios

Authorization and access control validation

Performance tests for large datasets