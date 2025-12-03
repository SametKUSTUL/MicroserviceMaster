# Onion Architecture - Identity Service

## Proje Yapısı

```
IdentityService/
├── Identity.Domain/              # Core Layer (En içteki katman)
│   └── Entities/
│       └── UserCredential.cs
│
├── Identity.Application/         # Application Layer
│   ├── Commands/
│   │   └── RegisterUserCommand.cs
│   ├── Handlers/
│   │   └── RegisterUserHandler.cs
│   ├── Events/
│   │   └── UserRegisteredEvent.cs
│   └── Interfaces/
│       ├── IIdentityDbContext.cs
│       └── IMessagePublisher.cs
│
├── Identity.Infrastructure/      # Infrastructure Layer
│   ├── Data/
│   │   └── IdentityDbContext.cs
│   └── Messaging/
│       └── RabbitMqPublisher.cs
│
└── Identity.API/                 # Presentation Layer (En dıştaki katman)
    ├── Controllers/
    │   └── AuthController.cs
    └── Models/
        ├── LoginRequest.cs
        └── RegisterRequest.cs
```

## Katman Bağımlılıkları

```
┌─────────────────────────────────────────┐
│         Identity.API (Web API)          │  ← Presentation
│  - Controllers                          │
│  - Startup/Program.cs                   │
└────────────────┬────────────────────────┘
                 │ depends on
                 ▼
┌─────────────────────────────────────────┐
│    Identity.Infrastructure              │  ← Infrastructure
│  - DbContext                            │
│  - RabbitMQ Implementation              │
│  - External Services                    │
└────────────────┬────────────────────────┘
                 │ depends on
                 ▼
┌─────────────────────────────────────────┐
│    Identity.Application                 │  ← Application
│  - Commands/Queries                     │
│  - Handlers                              │
│  - Interfaces (IDbContext, IPublisher)  │
└────────────────┬────────────────────────┘
                 │ depends on
                 ▼
┌─────────────────────────────────────────┐
│    Identity.Domain                      │  ← Domain (Core)
│  - Entities                             │
│  - Domain Logic                         │
│  - NO DEPENDENCIES                      │
└─────────────────────────────────────────┘
```

## Katman Açıklamaları

### 1. Domain Layer (Core)
**Sorumluluk:** Business entities, domain logic

**Bağımlılık:** YOK (Hiçbir şeye bağımlı değil)

```csharp
// Identity.Domain/Entities/UserCredential.cs
public class UserCredential
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    // Pure domain entity
}
```

### 2. Application Layer
**Sorumluluk:** Business logic, use cases, orchestration

**Bağımlılık:** Sadece Domain'e

```csharp
// Identity.Application/Commands/RegisterUserCommand.cs
public record RegisterUserCommand(string Email, string Password) 
    : IRequest<RegisterUserResult>;

// Identity.Application/Handlers/RegisterUserHandler.cs
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IIdentityDbContext _dbContext;  // Interface
    private readonly IMessagePublisher _publisher;   // Interface
    
    // Business logic implementation
}

// Identity.Application/Interfaces/IIdentityDbContext.cs
public interface IIdentityDbContext
{
    DbSet<UserCredential> UserCredentials { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

### 3. Infrastructure Layer
**Sorumluluk:** External concerns (DB, messaging, external APIs)

**Bağımlılık:** Application ve Domain'e

```csharp
// Identity.Infrastructure/Data/IdentityDbContext.cs
public class IdentityDbContext : DbContext, IIdentityDbContext
{
    // EF Core implementation
}

// Identity.Infrastructure/Messaging/RabbitMqPublisher.cs
public class RabbitMqPublisher : IMessagePublisher
{
    // RabbitMQ implementation
}
```

### 4. Presentation Layer (API)
**Sorumluluk:** HTTP endpoints, request/response handling

**Bağımlılık:** Application ve Infrastructure'a

```csharp
// Identity.API/Controllers/AuthController.cs
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var command = new RegisterUserCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
```

## Dependency Injection (Program.cs)

```csharp
// Infrastructure registrations
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IIdentityDbContext>(provider => 
    provider.GetRequiredService<IdentityDbContext>());

builder.Services.AddScoped<IMessagePublisher>(sp => 
    new RabbitMqPublisher(rabbitMqHost, rabbitMqExchange));

// Application registrations
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));
```

## Onion Architecture Prensipleri

### ✅ Dependency Rule
- Bağımlılıklar **içe doğru** akar
- Domain hiçbir şeye bağımlı değil
- Application sadece Domain'e bağımlı
- Infrastructure Application ve Domain'e bağımlı
- API tüm katmanlara bağımlı olabilir

### ✅ Separation of Concerns
- Her katman kendi sorumluluğuna odaklanır
- Domain: Business entities
- Application: Business logic
- Infrastructure: Technical details
- API: HTTP handling

### ✅ Testability
```csharp
// Unit test - Handler
[Fact]
public async Task Handle_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var mockDbContext = new Mock<IIdentityDbContext>();
    var mockPublisher = new Mock<IMessagePublisher>();
    var handler = new RegisterUserHandler(mockDbContext.Object, mockPublisher.Object, logger);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.NotNull(result);
}
```

### ✅ Maintainability
- Değişiklikler izole edilmiş
- Infrastructure değişirse sadece Infrastructure katmanı değişir
- Business logic değişirse sadece Application katmanı değişir

## Diğer Servislerle Karşılaştırma

### Customer Service (Mevcut)
```
CustomerService/
├── CustomerService.Domain/
├── CustomerService.Application/
├── CustomerService.Infrastructure/
└── CustomerService.API/
```

### Identity Service (Yeni)
```
IdentityService/
├── Identity.Domain/
├── Identity.Application/
├── Identity.Infrastructure/
└── Identity.API/
```

**Aynı pattern!** ✅

## Avantajlar

✅ **Clean Architecture**: Katmanlar net ayrılmış
✅ **Testable**: Her katman bağımsız test edilebilir
✅ **Maintainable**: Değişiklikler izole
✅ **Scalable**: Katmanlar bağımsız scale edilebilir
✅ **Technology Independent**: Infrastructure değiştirilebilir
✅ **Framework Independent**: Domain framework'e bağımlı değil

## SOLID Principles

- **S**ingle Responsibility: Her katman tek sorumluluğa sahip
- **O**pen/Closed: Extension'a açık, modification'a kapalı
- **L**iskov Substitution: Interface'ler değiştirilebilir
- **I**nterface Segregation: Küçük, spesifik interface'ler
- **D**ependency Inversion: Abstraction'lara bağımlılık

## Sonuç

Identity Service artık diğer servislerle aynı Onion Architecture pattern'ini kullanıyor:

- ✅ 4 ayrı class library projesi
- ✅ Net katman ayrımı
- ✅ Dependency Inversion
- ✅ CQRS + MediatR
- ✅ Clean Architecture
- ✅ Testable ve maintainable
