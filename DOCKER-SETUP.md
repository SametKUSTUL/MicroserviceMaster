# Docker Setup - Tüm Servisler

## ✅ Çalışan Servisler

| Servis | Port | Swagger | Status |
|--------|------|---------|--------|
| Identity Service | 5005 | http://localhost:5005/swagger | ✅ Running |
| Customer Service | 5004 | http://localhost:5004/swagger | ✅ Running |
| Order Service | 5001 | http://localhost:5001/swagger | ✅ Running |
| Product Service | 5003 | http://localhost:5003/swagger | ✅ Running |
| Payment Service | 5002 | http://localhost:5002/swagger | ✅ Running |

## Infrastructure

| Servis | Port | UI | Status |
|--------|------|-----|--------|
| PostgreSQL | 5432 | - | ✅ Running |
| RabbitMQ | 5672 | http://localhost:15672 | ✅ Running |
| Elasticsearch | 9200 | http://localhost:9200 | ✅ Running |
| Kibana | 5601 | http://localhost:5601 | ✅ Running |
| Jaeger | 16686 | http://localhost:16686 | ✅ Running |

## Yapılan Düzeltmeler

### 1. CustomerService RabbitMQ Konfigürasyonu

**appsettings.json:**
```json
{
  "RabbitMQ": {
    "Host": "localhost"
  }
}
```

**docker-compose.yml:**
```yaml
customerservice:
  environment:
    - RabbitMQ__Host=rabbitmq
  depends_on:
    rabbitmq:
      condition: service_healthy
```

### 2. UserRegisteredConsumer Retry Mekanizması

**Sorun:** RabbitMQ henüz hazır değilken bağlantı hatası
**Çözüm:** Retry mekanizması eklendi

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await Task.Delay(5000, stoppingToken); // Wait for RabbitMQ
    
    var retryCount = 0;
    while (retryCount < 5)
    {
        try
        {
            _connection = factory.CreateConnection();
            break;
        }
        catch (Exception ex)
        {
            retryCount++;
            if (retryCount >= 5) throw;
            await Task.Delay(3000, stoppingToken);
        }
    }
}
```

### 3. launchSettings.json Eklendi

**Identity.API/Properties/launchSettings.json:**
- Port: 5005
- Swagger: Enabled

**CustomerService.API/Properties/launchSettings.json:**
- Port: 5004
- Swagger: Enabled

## Docker Commands

### Tüm Servisleri Başlat
```bash
docker-compose up -d
```

### Belirli Servisi Rebuild Et
```bash
docker-compose up -d --build customerservice
```

### Logları Görüntüle
```bash
docker-compose logs -f customerservice
```

### Servislerin Durumunu Kontrol Et
```bash
docker-compose ps
```

### Tüm Servisleri Durdur
```bash
docker-compose down
```

### Tüm Servisleri Durdur ve Volume'leri Sil
```bash
docker-compose down -v
```

## Test Senaryosu

### 1. Register (Identity Service)
```bash
curl -X POST http://localhost:5005/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "test123"
  }'
```

**Response:**
```json
{
  "message": "User registered successfully",
  "email": "test@example.com",
  "customerId": "CUST12345678"
}
```

### 2. RabbitMQ Event (Otomatik)
- Identity Service → RabbitMQ'ya event publish eder
- Customer Service → Event'i consume eder
- Customer otomatik oluşturulur

### 3. Login (Identity Service)
```bash
curl -X POST http://localhost:5005/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "test123"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGc...",
  "customerId": "CUST12345678",
  "email": "test@example.com",
  "expiresAt": "2025-01-02T12:00:00Z"
}
```

### 4. Customer Kontrolü
```bash
curl http://localhost:5004/api/customers?email=test@example.com \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Troubleshooting

### CustomerService Başlamıyor
**Sorun:** RabbitMQ bağlantı hatası
**Çözüm:** 
- RabbitMQ'nun çalıştığından emin olun
- Retry mekanizması 5 kez deneyecek
- Logları kontrol edin: `docker-compose logs customerservice`

### RabbitMQ Management UI
- URL: http://localhost:15672
- Username: guest
- Password: guest

**Kontrol Edilecekler:**
- Exchange: `identity_exchange` var mı?
- Queue: `customer_user_registered_queue` var mı?
- Binding: `user.registered` routing key ile bağlı mı?

### Database Bağlantı Hatası
```bash
# PostgreSQL'in hazır olmasını bekleyin
docker-compose logs postgres | grep "ready to accept connections"
```

## Environment Variables

### Tüm Servislerde Ortak
- `ASPNETCORE_ENVIRONMENT=Development`
- `ASPNETCORE_HTTP_PORTS=8080`
- `JwtSettings__SecretKey=...`
- `JwtSettings__Issuer=IdentityService`
- `JwtSettings__Audience=MicroserviceMaster`
- `Elasticsearch__Url=http://elasticsearch:9200`
- `OpenTelemetry__OtlpEndpoint=http://jaeger:4318`

### RabbitMQ Kullanan Servisler
- `RabbitMQ__Host=rabbitmq`

### Identity Service Özel
- `CustomerService__Url=http://customerservice:8080`
- `RabbitMQ__IdentityExchange=identity_exchange`
- `RabbitMQ__UserRegisteredRoutingKey=user.registered`

## Health Checks

### PostgreSQL
```bash
docker exec microservicemaster-postgres-1 pg_isready -U postgres
```

### RabbitMQ
```bash
docker exec microservicemaster-rabbitmq-1 rabbitmq-diagnostics ping
```

### Servisler
```bash
curl http://localhost:5005/api/auth/health  # Identity
curl http://localhost:5004/api/customers    # Customer (with token)
curl http://localhost:5001/api/orders       # Order (with token)
curl http://localhost:5003/api/products     # Product (with token)
curl http://localhost:5002/api/payments     # Payment (with token)
```

## Sonuç

✅ Tüm servisler başarıyla çalışıyor
✅ RabbitMQ event-driven architecture aktif
✅ JWT authentication tüm servislerde çalışıyor
✅ Observability (Elasticsearch, Kibana, Jaeger) aktif
✅ launchSettings.json eklendi (IDE'den çalıştırma için)
