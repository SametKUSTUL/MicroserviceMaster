# MicroserviceMaster

Onion Architecture ile geliştirilmiş .NET 8 tabanlı microservice projesi.

## Mimari

- **OrderService**: Sipariş yönetimi
- **PaymentService**: Ödeme işlemleri
- **PostgreSQL**: Veritabanı
- **RabbitMQ**: Mesajlaşma

## Proje Yapısı

```
MicroserviceMaster/
├── src/
│   ├── OrderService/
│   │   ├── OrderService.Domain
│   │   ├── OrderService.Application
│   │   ├── OrderService.Infrastructure
│   │   └── OrderService.API
│   └── PaymentService/
│       ├── PaymentService.Domain
│       ├── PaymentService.Application
│       ├── PaymentService.Infrastructure
│       └── PaymentService.API
└── tests/
    ├── OrderService.Tests
    └── PaymentService.Tests
```

## Çalıştırma

### Production (Tüm Servisler)
```bash
docker compose up -d --build
```

### Development (Sadece DB ve Queue)
```bash
docker compose -f docker-compose.development.yml up -d
```
Bu modda sadece PostgreSQL ve RabbitMQ çalışır. Servisleri IDE'den debug modunda çalıştırabilirsiniz.

## Servisler

- **OrderService API**: http://localhost:5001/api/orders
- **OrderService Swagger**: http://localhost:5001/swagger
- **PaymentService API**: http://localhost:5002/api/payments
- **PaymentService Swagger**: http://localhost:5002/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **PostgreSQL**: localhost:5432 (postgres/postgres)

## Test

```bash
dotnet test
```

## Logları Görüntüleme

```bash
docker compose logs -f orderservice
docker compose logs -f paymentservice
```

## Durdurma

```bash
docker compose down
```
