# Clean Project Structure - Identity Service

## ✅ Temizlenmiş Yapı

### Identity.API (Presentation Layer)
```
Identity.API/
├── Controllers/          ← HTTP endpoints
│   └── AuthController.cs
├── Models/              ← Request/Response DTOs
│   ├── LoginRequest.cs
│   └── RegisterRequest.cs
├── appsettings.json
├── Program.cs
└── Dockerfile
```

**Sorumluluk:** HTTP handling, request/response mapping

### Identity.Application (Business Logic Layer)
```
Identity.Application/
├── Commands/            ← CQRS Commands
│   └── RegisterUserCommand.cs
├── Handlers/            ← Command/Query Handlers
│   └── RegisterUserHandler.cs
├── Services/            ← Application Services
│   ├── IAuthenticationService.cs
│   └── AuthenticationService.cs
├── Events/              ← Domain Events
│   └── UserRegisteredEvent.cs
└── Interfaces/          ← Abstractions
    ├── IIdentityDbContext.cs
    └── IMessagePublisher.cs
```

**Sorumluluk:** Business logic, use cases, orchestration

### Identity.Domain (Core Layer)
```
Identity.Domain/
└── Entities/            ← Domain Entities
    └── UserCredential.cs
```

**Sorumluluk:** Business entities, domain logic

### Identity.Infrastructure (Infrastructure Layer)
```
Identity.Infrastructure/
├── Data/                ← Database
│   └── IdentityDbContext.cs
├── Messaging/           ← Message Broker
│   └── RabbitMqPublisher.cs
└── Configuration/       ← Settings
    └── RabbitMqSettings.cs
```

**Sorumluluk:** External concerns (DB, messaging, external APIs)

## Karşılaştırma: OrderService vs IdentityService

### OrderService.API
```
OrderService.API/
├── Controllers/
├── BackgroundServices/
├── Extensions/
├── Middleware/
├── Configuration/
├── Program.cs
└── Dockerfile
```

### Identity.API (Temizlenmiş)
```
Identity.API/
├── Controllers/         ✅ Sadece presentation
├── Models/             ✅ Sadece DTOs
├── Program.cs
└── Dockerfile
```

## Kaldırılan Klasörler

❌ **Identity.API/Application/** → Identity.Application'a taşındı
❌ **Identity.API/Domain/** → Identity.Domain'a taşındı
❌ **Identity.API/Data/** → Identity.Infrastructure'a taşındı
❌ **Identity.API/Infrastructure/** → Identity.Infrastructure'a taşındı
❌ **Identity.API/Messaging/** → Identity.Infrastructure'a taşındı
❌ **Identity.API/Events/** → Identity.Application'a taşındı
❌ **Identity.API/Configuration/** → Identity.Infrastructure'a taşındı
❌ **Identity.API/Services/** → Identity.Application'a taşındı

## Prensip: Separation of Concerns

### ✅ Doğru Yapı
```
Identity.API/
├── Controllers/         ← Sadece HTTP
└── Models/             ← Sadece DTOs

Identity.Application/
├── Commands/           ← Business logic
├── Handlers/
└── Services/

Identity.Infrastructure/
├── Data/               ← Database
└── Messaging/          ← RabbitMQ
```

### ❌ Yanlış Yapı (Önceki)
```
Identity.API/
├── Controllers/
├── Application/        ← Yanlış katman!
├── Domain/            ← Yanlış katman!
├── Data/              ← Yanlış katman!
└── Infrastructure/    ← Yanlış katman!
```

## Tüm Servisler Aynı Pattern

### CustomerService
```
CustomerService/
├── CustomerService.API/
├── CustomerService.Application/
├── CustomerService.Domain/
└── CustomerService.Infrastructure/
```

### OrderService
```
OrderService/
├── OrderService.API/
├── OrderService.Application/
├── OrderService.Domain/
└── OrderService.Infrastructure/
```

### ProductService
```
ProductService/
├── ProductService.API/
├── ProductService.Application/
├── ProductService.Domain/
└── ProductService.Infrastructure/
```

### PaymentService
```
PaymentService/
├── PaymentService.API/
├── PaymentService.Application/
├── PaymentService.Domain/
└── PaymentService.Infrastructure/
```

### IdentityService ✅
```
IdentityService/
├── Identity.API/
├── Identity.Application/
├── Identity.Domain/
└── Identity.Infrastructure/
```

## Avantajlar

✅ **Consistency**: Tüm servisler aynı yapıda
✅ **Clean Architecture**: Katmanlar net ayrılmış
✅ **Maintainability**: Her şey doğru yerde
✅ **Testability**: Katmanlar bağımsız test edilebilir
✅ **Scalability**: Katmanlar bağımsız scale edilebilir

## API Katmanında Olması Gerekenler

✅ **Controllers**: HTTP endpoints
✅ **Models**: Request/Response DTOs
✅ **Program.cs**: DI configuration, middleware setup
✅ **appsettings.json**: Configuration
✅ **Dockerfile**: Container configuration

## API Katmanında Olmaması Gerekenler

❌ **Business Logic**: Application katmanında olmalı
❌ **Domain Entities**: Domain katmanında olmalı
❌ **Database Context**: Infrastructure katmanında olmalı
❌ **Message Publishers**: Infrastructure katmanında olmalı
❌ **External Services**: Infrastructure katmanında olmalı

## Sonuç

Identity Service artık diğer servislerle aynı clean structure'a sahip:
- ✅ API katmanı sadece presentation
- ✅ Business logic Application'da
- ✅ Domain entities Domain'de
- ✅ Infrastructure concerns Infrastructure'da
- ✅ Tüm servisler tutarlı yapıda
