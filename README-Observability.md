# Observability Stack

## Bileşenler

### 1. **Serilog + Elasticsearch + Kibana (ELK)**
- **Serilog**: Structured logging
- **Elasticsearch**: Log storage
- **Kibana**: Log visualization (http://localhost:5601)

### 2. **OpenTelemetry + Jaeger**
- **OpenTelemetry**: Distributed tracing
- **Jaeger**: Trace visualization (http://localhost:16686)

## Başlatma

```bash
# ELK ve Jaeger'ı başlat
docker-compose -f docker-compose.observability.yml up -d

# Servisleri başlat
dotnet run --project src/OrderService/OrderService.API
dotnet run --project src/PaymentService/PaymentService.API
```

## Özellikler

### Distributed Tracing
- Her request için unique TraceId
- OrderService → RabbitMQ → PaymentService akışı tek TraceId ile takip edilir
- Jaeger UI'da end-to-end görünürlük

### Structured Logging
- JSON formatında loglar
- TraceId ve SpanId otomatik eklenir
- Elasticsearch'te indexlenir
- Kibana'da filtreleme ve arama

### Log Enrichment
- ServiceName
- MachineName
- EnvironmentName
- TraceId (OpenTelemetry'den)
- SpanId (OpenTelemetry'den)

## Kibana Index Pattern

1. Kibana'ya git: http://localhost:5601
2. Management → Stack Management → Index Patterns
3. Create index pattern: `logs-*`
4. Discover'da logları görüntüle

## Jaeger Trace Görüntüleme

1. Jaeger'a git: http://localhost:16686
2. Service seç: OrderService veya PaymentService
3. Find Traces
4. TraceId ile tüm akışı gör

## Örnek Log Sorguları

```
# Belirli bir TraceId'yi bul
TraceId: "00-abc123..."

# Hata logları
level: "Error"

# Belirli bir servis
ServiceName: "OrderService"

# Belirli bir müşteri
CustomerId: "customer-123"
```
