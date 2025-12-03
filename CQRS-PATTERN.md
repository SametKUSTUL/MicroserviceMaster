# CQRS Pattern - Identity Service

## Mimari Değişiklik

### Önceki Yapı (Anti-Pattern)
```
Controller → Direct DB Access + Business Logic
```

❌ **Sorunlar:**
- Controller'da business logic
- Presentation layer'da domain logic
- Test edilmesi zor
- Yeniden kullanılamaz

### Yeni Yapı (CQRS + MediatR)
```
Controller → MediatR → Command/Query Handler → Domain Logic
```

✅ **Avantajlar:**
- Separation of Concerns
- Single Responsibility
- Testable
- Reusable
- Clean Architecture

## Katmanlar

### 1. Presentation Layer (Controller)
**Sorumluluk:** HTTP request/response, validation

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    var command = new RegisterUserCommand(request.Email, request.Password);
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

### 2. Application Layer (Command/Handler)
**Sorumluluk:** Business logic, orchestration

**Command:**
```csharp
public record RegisterUserCommand(string Email, string Password) 
    : IRequest<RegisterUserResult>;
```

**Handler:**
```csharp
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    public async Task<RegisterUserResult> Handle(RegisterUserCommand request)
    {
        // 1. Validation
        // 2. Business logic
        // 3. Database operations
        // 4. Event publishing
        // 5. Return result
    }
}
```

### 3. Domain Layer
**Sorumluluk:** Entities, domain logic

```csharp
public class UserCredential
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    // ...
}
```

### 4. Infrastructure Layer
**Sorumluluk:** External services, messaging

```csharp
public interface IMessagePublisher
{
    void Publish<T>(T message, string routingKey);
}
```

## MediatR Pipeline

```
Request (RegisterUserCommand)
    ↓
MediatR Mediator
    ↓
RegisterUserHandler
    ↓
    1. Check existing user
    2. Generate CustomerId
    3. Hash password
    4. Save to database
    5. Publish event to RabbitMQ
    ↓
Response (RegisterUserResult)
```

## Dependency Injection

```csharp
// Program.cs
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
```

## Test Edilebilirlik

### Unit Test - Handler
```csharp
[Fact]
public async Task Handle_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var handler = new RegisterUserHandler(mockDb, mockPublisher, mockSettings, mockLogger);
    var command = new RegisterUserCommand("test@test.com", "pass123");
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.Equal("User registered successfully", result.Message);
}
```

### Integration Test - Controller
```csharp
[Fact]
public async Task Register_ValidRequest_Returns200()
{
    // Arrange
    var request = new RegisterRequest { Email = "test@test.com", Password = "pass123" };
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/auth/register", request);
    
    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

## Proje Yapısı

```
Identity.API/
├── Controllers/
│   └── AuthController.cs          # Presentation Layer
├── Application/
│   ├── Commands/
│   │   └── RegisterUserCommand.cs # Command Definition
│   └── Handlers/
│       └── RegisterUserHandler.cs # Business Logic
├── Domain/
│   └── UserCredential.cs          # Domain Entity
├── Infrastructure/
│   ├── PasswordHasher.cs          # Utility
│   └── CustomerServiceClient.cs   # External Service
├── Messaging/
│   ├── IMessagePublisher.cs       # Interface
│   └── RabbitMqPublisher.cs       # Implementation
└── Data/
    └── IdentityDbContext.cs       # Database Context
```

## Command vs Query

### Command (Write Operation)
- Değişiklik yapar (Create, Update, Delete)
- `IRequest<TResponse>` implement eder
- Örnek: `RegisterUserCommand`, `UpdateUserCommand`

### Query (Read Operation)
- Sadece okuma yapar
- `IRequest<TResponse>` implement eder
- Örnek: `GetUserByEmailQuery`, `GetAllUsersQuery`

## Best Practices

✅ **Do:**
- Command/Query'leri immutable yap (record kullan)
- Handler'da tek sorumluluk prensibi
- Dependency injection kullan
- Exception handling yap
- Logging ekle

❌ **Don't:**
- Controller'da business logic yazma
- Handler'da HTTP context'e erişme
- Static method kullanma
- Global state kullanma

## Örnek Akış

### Register User

1. **HTTP Request**
```json
POST /api/auth/register
{
  "email": "user@test.com",
  "password": "pass123"
}
```

2. **Controller**
```csharp
var command = new RegisterUserCommand(request.Email, request.Password);
var result = await _mediator.Send(command);
```

3. **Handler**
```csharp
// Validation
if (await _dbContext.UserCredentials.AnyAsync(u => u.Email == request.Email))
    throw new InvalidOperationException("User already exists");

// Business Logic
var customerId = GenerateCustomerId();
var userCredential = CreateUserCredential(request, customerId);

// Persistence
await _dbContext.SaveChangesAsync();

// Event
_messagePublisher.Publish(new UserRegisteredEvent(...));

// Response
return new RegisterUserResult(...);
```

4. **HTTP Response**
```json
{
  "message": "User registered successfully",
  "email": "user@test.com",
  "customerId": "CUST12345678"
}
```

## Gelecek Geliştirmeler

🔄 **Pipeline Behaviors**
- Validation behavior (FluentValidation)
- Logging behavior
- Transaction behavior
- Caching behavior

📊 **CQRS Separation**
- Read database (Query)
- Write database (Command)
- Event sourcing

🎯 **Domain Events**
- Domain event dispatcher
- Event handlers
- Saga pattern

## Sonuç

MediatR ve CQRS pattern kullanarak:
- ✅ Clean Architecture
- ✅ Testable code
- ✅ Maintainable structure
- ✅ Scalable design
- ✅ Separation of Concerns
