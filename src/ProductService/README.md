# Product Service

Product Service, ürün yönetimi, stok takibi ve fiyat geçmişi için kullanılan bir mikroservistir.

## Özellikler

- **Ürün Yönetimi**: Ürün oluşturma, güncelleme ve listeleme
- **Stok Takibi**: Ürün stok miktarlarının yönetimi
- **Fiyat Geçmişi**: Ürün fiyat değişikliklerinin historical olarak tutulması
- **CQRS Pattern**: Command ve Query ayrımı ile temiz mimari
- **MediatR**: Request/Response pattern implementasyonu
- **Onion Architecture**: Domain-centric mimari
- **FluentValidation**: İş kuralları validasyonu

## Mimari

```
ProductService/
├── ProductService.API          # Presentation Layer
├── ProductService.Application  # Application Layer (CQRS, Handlers)
├── ProductService.Domain       # Domain Layer (Entities, Repositories)
└── ProductService.Infrastructure # Infrastructure Layer (Data, Repositories)
```

## API Endpoints

### Products

- `GET /api/products` - Tüm ürünleri listele
- `GET /api/products/{id}` - Ürün detayını getir
- `POST /api/products` - Yeni ürün oluştur
- `PUT /api/products/{id}/stock` - Stok güncelle
- `PUT /api/products/{id}/price` - Fiyat güncelle (otomatik olarak price history'ye eklenir)

## Order Service Entegrasyonu

Order Service, sipariş oluştururken Product Service'i kullanarak:
- ProductId'nin geçerli olduğunu doğrular
- Güncel fiyat bilgisini alır
- Stok durumunu kontrol eder

## Çalıştırma

### Lokal
```bash
cd src/ProductService/ProductService.API
dotnet run
```

### Docker Compose
```bash
docker-compose up productservice
```

Port: `5003`
