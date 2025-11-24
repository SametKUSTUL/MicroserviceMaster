# Kibana Index Pattern Kurulumu

## 1. Servisleri Başlat

```bash
docker-compose up -d
```

## 2. Kibana'ya Eriş

http://localhost:5601

## 3. Index Pattern Oluştur

### Adım 1: Management'a Git
- Sol menüden **☰ (hamburger menu)** → **Management** → **Stack Management**

### Adım 2: Data Views (Index Patterns) Oluştur
- Sol menüden **Kibana** → **Data Views**
- **Create data view** butonuna tıkla

### Adım 3: OrderService Index Pattern
- **Name**: `OrderService Logs`
- **Index pattern**: `logs-orderservice-*`
- **Timestamp field**: `@timestamp`
- **Create data view** butonuna tıkla

### Adım 4: PaymentService Index Pattern
- **Create data view** butonuna tekrar tıkla
- **Name**: `PaymentService Logs`
- **Index pattern**: `logs-paymentservice-*`
- **Timestamp field**: `@timestamp`
- **Create data view** butonuna tıkla

### Adım 5: Tüm Loglar İçin Genel Pattern
- **Create data view** butonuna tekrar tıkla
- **Name**: `All Services Logs`
- **Index pattern**: `logs-*`
- **Timestamp field**: `@timestamp`
- **Create data view** butonuna tıkla

## 4. Logları Görüntüle

### Discover'a Git
- Sol menüden **☰** → **Analytics** → **Discover**
- Üstten data view seç: `All Services Logs` veya `OrderService Logs`

### Filtreleme Örnekleri

**Belirli bir servis:**
```
ServiceName: "OrderService"
```

**Hata logları:**
```
level: "Error"
```

**Belirli bir müşteri:**
```
CustomerId: "customer-123"
```

**Belirli bir TraceId:**
```
TraceId: "00-abc123..."
```

**Belirli bir OrderId:**
```
OrderId: "guid-here"
```

## 5. Log Formatı

Elasticsearch'e gönderilen log formatı:

```json
{
  "@timestamp": "2024-01-15T10:30:00.000Z",
  "level": "Information",
  "message": "Creating order for CustomerId: customer-123",
  "ServiceName": "OrderService",
  "MachineName": "orderservice-container",
  "EnvironmentName": "Development",
  "TraceId": "00-abc123def456...",
  "SpanId": "def456...",
  "CustomerId": "customer-123",
  "OrderId": "guid-here",
  "TotalAmount": 100.50,
  "fields": {
    "SourceContext": "OrderService.Application.Handlers.CreateOrderHandler"
  }
}
```

## 6. Yararlı KQL Sorguları

**Son 15 dakikadaki hatalar:**
```
level: "Error" AND @timestamp >= now-15m
```

**Belirli bir handler'dan loglar:**
```
fields.SourceContext: "*CreateOrderHandler*"
```

**Yüksek tutarlı siparişler:**
```
TotalAmount > 1000
```

**Belirli bir zaman aralığı:**
```
@timestamp >= "2024-01-15T10:00:00" AND @timestamp <= "2024-01-15T11:00:00"
```

## 7. Dashboard Oluşturma

### Adım 1: Dashboard'a Git
- Sol menüden **☰** → **Analytics** → **Dashboard**
- **Create dashboard** butonuna tıkla

### Adım 2: Visualization Ekle
- **Create visualization** butonuna tıkla
- Visualization tipini seç (örn: Bar chart, Pie chart, Line chart)

### Örnek Visualizations:

**1. Log Seviyeleri Dağılımı (Pie Chart)**
- Field: `level.keyword`
- Aggregation: Count

**2. Servis Bazında Log Sayısı (Bar Chart)**
- X-axis: `ServiceName.keyword`
- Y-axis: Count

**3. Zaman İçinde Log Trendi (Line Chart)**
- X-axis: `@timestamp` (Date Histogram)
- Y-axis: Count

**4. En Çok Hata Veren Handler'lar (Table)**
- Rows: `fields.SourceContext.keyword`
- Metrics: Count
- Filter: `level: "Error"`

## 8. Alerts Kurma

### Adım 1: Alerting'e Git
- Sol menüden **☰** → **Management** → **Stack Management** → **Alerts and Insights** → **Rules**

### Adım 2: Rule Oluştur
- **Create rule** butonuna tıkla
- Rule type: **Elasticsearch query**

### Örnek Alert: Yüksek Hata Oranı
- **Name**: High Error Rate
- **Index**: `logs-*`
- **Query**: `level: "Error"`
- **Threshold**: Count > 10 in last 5 minutes
- **Action**: Email, Slack, webhook vb.

## 9. Index Lifecycle Management (ILM)

Logları otomatik silmek için:

### Adım 1: ILM Policy Oluştur
- **Management** → **Stack Management** → **Data** → **Index Lifecycle Policies**
- **Create policy** butonuna tıkla

### Örnek Policy:
```json
{
  "policy": {
    "phases": {
      "hot": {
        "actions": {}
      },
      "delete": {
        "min_age": "7d",
        "actions": {
          "delete": {}
        }
      }
    }
  }
}
```

Bu policy 7 gün sonra logları otomatik siler.

## 10. Saved Searches

Sık kullanılan sorguları kaydet:

- Discover'da sorguyu yaz
- Üstten **Save** butonuna tıkla
- İsim ver: "OrderService Errors", "High Value Orders" vb.
- Daha sonra **Open** → **Saved searches** ile hızlıca erişebilirsin

## Troubleshooting

**Index görünmüyor:**
1. Servislerin çalıştığından emin ol
2. En az bir log üretilmiş olmalı (bir API çağrısı yap)
3. Elasticsearch'te index'i kontrol et: http://localhost:9200/_cat/indices

**Loglar gelmiyor:**
1. Elasticsearch URL'i doğru mu kontrol et
2. Container loglarını kontrol et: `docker logs orderservice`
3. Elasticsearch health: http://localhost:9200/_cluster/health
