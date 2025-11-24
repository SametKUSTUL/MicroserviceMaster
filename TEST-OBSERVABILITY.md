# Observability Test Adımları

## 1. Development Stack'i Başlat

```bash
docker-compose -f docker-compose.development.yml up -d
```

## 2. Servisleri Rider'dan Başlat

- Rider'da "All Services" run configuration'ı çalıştır
- Veya OrderService ve PaymentService'i ayrı ayrı F5 ile başlat

## 3. Test Request Gönder

```bash
curl -X POST http://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "customer-123",
    "items": [
      {
        "productId": "product-1",
        "quantity": 2,
        "price": 10.50
      }
    ]
  }'
```

## 4. Elasticsearch'te Logları Kontrol Et

```bash
# Index'leri listele
curl http://localhost:9200/_cat/indices/logs-*?v

# OrderService loglarını getir
curl -X GET "http://localhost:9200/logs-orderservice-*/_search?pretty" \
  -H 'Content-Type: application/json' \
  -d '{
    "size": 10,
    "sort": [{"@timestamp": "desc"}]
  }'

# PaymentService loglarını getir
curl -X GET "http://localhost:9200/logs-paymentservice-*/_search?pretty" \
  -H 'Content-Type: application/json' \
  -d '{
    "size": 10,
    "sort": [{"@timestamp": "desc"}]
  }'
```

## 5. Kibana'da Logları Görüntüle

1. http://localhost:5601 → Kibana aç
2. ☰ → Management → Stack Management
3. Kibana → Data Views → Create data view
4. Pattern: `logs-*`, Timestamp: `@timestamp`
5. ☰ → Analytics → Discover
6. Logları gör

## 6. Jaeger'da Trace'leri Görüntüle

1. http://localhost:16686 → Jaeger UI aç
2. Service dropdown'dan "OrderService" seç
3. "Find Traces" butonuna tıkla
4. Trace'leri gör

## Troubleshooting

### Loglar Elasticsearch'e Gitmiyor

**Kontrol 1: Elasticsearch çalışıyor mu?**
```bash
curl http://localhost:9200/_cluster/health
```

**Kontrol 2: Servis loglarında hata var mı?**
- Rider'da console output'u kontrol et
- Serilog initialization hatası var mı?

**Kontrol 3: Elasticsearch URL doğru mu?**
- appsettings.Development.json'da `Elasticsearch:Url` = `http://localhost:9200`

**Çözüm:**
- Servisleri yeniden başlat (Rider'da Stop → Start)
- Elasticsearch'i yeniden başlat: `docker-compose -f docker-compose.development.yml restart elasticsearch`

### Jaeger'da Trace Görünmüyor

**Kontrol 1: Jaeger çalışıyor mu?**
```bash
curl http://localhost:16686
```

**Kontrol 2: OTLP endpoint doğru mu?**
- appsettings.Development.json'da `OpenTelemetry:OtlpEndpoint` = `http://localhost:4317`

**Kontrol 3: OpenTelemetry exporter eklendi mi?**
- ObservabilityExtensions.cs'de `AddOtlpExporter` var mı?

**Çözüm:**
- Servisleri yeniden başlat
- Jaeger'ı yeniden başlat: `docker-compose -f docker-compose.development.yml restart jaeger`

### Kibana'da Data View Oluşturulamıyor

**Sebep:** Henüz hiç log yazılmamış, index yok

**Çözüm:**
1. En az bir request gönder (order oluştur)
2. Elasticsearch'te index'i kontrol et: `curl http://localhost:9200/_cat/indices/logs-*?v`
3. Index varsa Kibana'da data view oluştur

## Beklenen Sonuçlar

### Elasticsearch
```
health status index                           uuid   pri rep docs.count
yellow open   logs-orderservice-2024.11.24    abc123   1   1         10
yellow open   logs-paymentservice-2024.11.24  def456   1   1          5
```

### Kibana Discover
```
@timestamp: 2024-11-24T10:30:00.000Z
level: Information
message: Creating order for CustomerId: customer-123
ServiceName: OrderService
TraceId: 00-abc123def456...
CustomerId: customer-123
```

### Jaeger
```
Service: OrderService
Operation: POST /api/orders
Duration: 150ms
Spans:
  - OrderService: POST /api/orders (150ms)
    - CreateOrderHandler (120ms)
    - RabbitMQ Publish (20ms)
  - PaymentService: ProcessPayment (80ms)
    - ProcessPaymentHandler (70ms)
```

## Log Formatı Örneği

```json
{
  "@timestamp": "2024-11-24T10:30:00.000Z",
  "level": "Information",
  "message": "Creating order for CustomerId: customer-123",
  "ServiceName": "OrderService",
  "MachineName": "DESKTOP-ABC123",
  "EnvironmentName": "Development",
  "TraceId": "00-abc123def456789...",
  "SpanId": "def456789...",
  "CustomerId": "customer-123",
  "fields": {
    "SourceContext": "OrderService.Application.Handlers.CreateOrderHandler"
  }
}
```
