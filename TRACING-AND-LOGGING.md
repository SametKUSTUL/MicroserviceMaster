# Distributed Tracing & Logging

## Yapılan İyileştirmeler

### 1. Identity Service - Extension Pattern

**Önceki:** Program.cs'de tüm DI konfigürasyonu
**Yeni:** ServiceCollectionExtensions ile organize edilmiş yapı

```csharp
// Identity.API/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
{
    services.AddDatabase(configuration);
    services.AddJwtAuthentication(configuration);
    services.AddMessaging(configuration);
    services.AddApplicationServices();
    return services;
}
```

**Program.cs (Temiz):**
```csharp
builder.AddObservability("IdentityService");
builder.Services.AddIdentityServices(builder.Configuration);
```

### 2. RabbitMQ Trace ID Propagation

**Identity Service → RabbitMQ:**
```csharp
public void Publish<T>(T message, string routingKey)
{
    var properties = _channel.CreateBasicProperties();
    properties.Headers = new Dictionary<string, object>();
    
    // Add trace context
    if (System.Diagnostics.Activity.Current != null)
    {
        var traceParent = $"00-{Activity.Current.TraceId}-{Activity.Current.SpanId}-01";
        properties.Headers["traceparent"] = Encoding.UTF8.GetBytes(traceParent);
    }
    
    _channel.BasicPublish(exchange, routingKey, properties, body);
}
```

**Customer Service ← RabbitMQ:**
```csharp
consumer.Received += async (model, ea) =>
{
    // Extract trace context
    if (ea.BasicProperties?.Headers?.ContainsKey("traceparent") == true)
    {
        var traceParentBytes = ea.BasicProperties.Headers["traceparent"] as byte[];
        var traceId = Encoding.UTF8.GetString(traceParentBytes);
        _logger.LogInformation("[RabbitMQ] TraceId: {TraceId}", traceId);
    }
};
```

### 3. Raw Message Logging

**CustomerService - UserRegisteredConsumer:**
```csharp
consumer.Received += async (model, ea) =>
{
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
    
    // Log raw message
    _logger.LogInformation("[RabbitMQ] Received raw message: {RawMessage}", message);
    
    // Log event details
    _logger.LogInformation("[RabbitMQ] Processing UserRegisteredEvent - Email: {Email}, CustomerId: {CustomerId}", 
        userEvent.Email, userEvent.CustomerId);
    
    // Log success
    _logger.LogInformation("[RabbitMQ] Successfully processed CustomerId: {CustomerId}", 
        userEvent.CustomerId);
};
```

## Kibana'da Log Sorgulama

### 1. Register → Customer Creation Flow

**Identity Service Logs:**
```
service.name: "IdentityService" AND message: "User registered"
```

**Customer Service Logs:**
```
service.name: "CustomerService" AND message: "[RabbitMQ]"
```

### 2. Trace ID ile İlişkilendirme

**Aynı TraceId'ye sahip tüm loglar:**
```
trace.id: "YOUR_TRACE_ID"
```

**Register işleminden Customer creation'a kadar:**
```
(service.name: "IdentityService" OR service.name: "CustomerService") 
AND trace.id: "YOUR_TRACE_ID"
```

### 3. Raw Message Görüntüleme

**RabbitMQ raw messages:**
```
service.name: "CustomerService" AND message: "Received raw message"
```

**Örnek Log:**
```json
{
  "@timestamp": "2025-01-02T10:30:00.000Z",
  "service.name": "CustomerService",
  "message": "[RabbitMQ] Received raw message from queue 'customer_user_registered_queue': {\"email\":\"test@example.com\",\"customerId\":\"CUST12345678\",\"registeredAt\":\"2025-01-02T10:30:00Z\"}",
  "trace.id": "abc123...",
  "span.id": "def456..."
}
```

## Test Senaryosu

### 1. Register User
```bash
curl -X POST http://localhost:5005/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "trace-test@example.com",
    "password": "test123"
  }'
```

### 2. Identity Service Logs
```
[IdentityService] User registered: trace-test@example.com, CustomerId: CUST12345678
[IdentityService] Publishing event to RabbitMQ with TraceId: abc123...
```

### 3. RabbitMQ Message (with TraceId in headers)
```json
Headers: {
  "traceparent": "00-abc123...-def456...-01"
}
Body: {
  "email": "trace-test@example.com",
  "customerId": "CUST12345678",
  "registeredAt": "2025-01-02T10:30:00Z"
}
```

### 4. Customer Service Logs
```
[CustomerService] [RabbitMQ] Received raw message: {"email":"trace-test@example.com",...}
[CustomerService] [RabbitMQ] TraceId from message: 00-abc123...-def456...-01
[CustomerService] [RabbitMQ] Processing UserRegisteredEvent - Email: trace-test@example.com, CustomerId: CUST12345678
[CustomerService] Customer created from user registration: CUST12345678, trace-test@example.com
[CustomerService] [RabbitMQ] Successfully processed and acknowledged message for CustomerId: CUST12345678
```

## Kibana Dashboard Queries

### Query 1: Full Registration Flow
```
service.name: ("IdentityService" OR "CustomerService") 
AND (message: "User registered" OR message: "[RabbitMQ]")
| sort @timestamp asc
```

### Query 2: RabbitMQ Events Only
```
service.name: "CustomerService" 
AND message: "[RabbitMQ]"
| sort @timestamp asc
```

### Query 3: Failed Events
```
service.name: "CustomerService" 
AND message: "Error processing user registered event"
```

### Query 4: Specific Customer
```
(service.name: "IdentityService" OR service.name: "CustomerService")
AND (message: *CUST12345678* OR customerId: "CUST12345678")
```

## OpenTelemetry Trace Visualization

**Jaeger UI:** http://localhost:16686

**Trace Structure:**
```
Identity.API: POST /api/auth/register
  ├─ RegisterUserHandler
  │   ├─ Database: INSERT UserCredential
  │   └─ RabbitMQ: Publish UserRegisteredEvent
  │       └─ traceparent: 00-{traceId}-{spanId}-01
  │
Customer.API: UserRegisteredConsumer
  ├─ RabbitMQ: Consume (with same traceId)
  ├─ CreateCustomerHandler
  └─ Database: INSERT Customer
```

## Middleware Stack

### Identity Service
```
RequestResponseLoggingMiddleware  ← Logs all HTTP requests/responses
↓
Authentication
↓
Authorization
↓
Controllers
```

### Customer Service
```
RequestResponseLoggingMiddleware  ← Logs all HTTP requests/responses
↓
ExceptionHandlingMiddleware
↓
Authentication
↓
Authorization
↓
Controllers
```

## Log Levels

**Information:**
- HTTP requests/responses
- RabbitMQ messages (raw + processed)
- Business operations (user registered, customer created)

**Warning:**
- RabbitMQ connection retries
- Authentication failures

**Error:**
- RabbitMQ processing errors
- Database errors
- Unhandled exceptions

## Structured Logging

**All logs include:**
- `service.name`: Service identifier
- `trace.id`: Distributed trace ID
- `span.id`: Current span ID
- `@timestamp`: ISO 8601 timestamp
- `message`: Log message
- Custom fields: `customerId`, `email`, etc.

## Sonuç

✅ **Extension Pattern**: Identity Service temiz ve organize
✅ **Trace ID Propagation**: RabbitMQ üzerinden trace ID taşınıyor
✅ **Raw Message Logging**: Tüm RabbitMQ mesajları loglanıyor
✅ **Distributed Tracing**: Register → RabbitMQ → Customer creation akışı izlenebilir
✅ **Kibana Integration**: Tüm loglar Elasticsearch'te
✅ **Jaeger Integration**: Trace visualization
