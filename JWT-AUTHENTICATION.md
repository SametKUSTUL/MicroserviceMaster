# JWT Authentication Altyapısı

## Genel Bakış

Projeye JWT (JSON Web Token) tabanlı authentication altyapısı eklenmiştir. Tüm mikroservisler artık JWT token ile korunmaktadır.

## Mimari

### Identity Service (Port: 5005)
- Token üretimi ve yönetimi
- Kullanıcı authentication
- Merkezi kimlik doğrulama servisi

### Diğer Servisler
- Customer Service (Port: 5004)
- Order Service (Port: 5001)
- Product Service (Port: 5003)
- Payment Service (Port: 5002)

Tüm servisler JWT token doğrulaması yapmaktadır.

## Kullanım

### 1. Login ve Token Alma

```bash
curl -X POST http://localhost:5005/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer1@test.com",
    "password": "password123"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "customerId": "1",
  "email": "customer1@test.com",
  "expiresAt": "2024-01-01T12:00:00Z"
}
```

### 2. Token ile Servis Çağrısı

```bash
curl -X GET http://localhost:5004/api/customers \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Demo Kullanıcılar

| Email | Password | CustomerId | Role |
|-------|----------|------------|------|
| customer1@test.com | password123 | 1 | Customer |
| customer2@test.com | password123 | 2 | Customer |
| admin@test.com | admin123 | admin | Admin |

## JWT Token İçeriği

Token içinde şu bilgiler bulunur:
- `sub`: Customer ID
- `email`: Kullanıcı email
- `role`: Kullanıcı rolü
- `customerId`: Customer ID (claim olarak)
- `exp`: Token son kullanma tarihi
- `iss`: Token üreten servis (IdentityService)
- `aud`: Token hedef kitlesi (MicroserviceMaster)

## Konfigürasyon

Tüm servislerde aynı JWT ayarları kullanılmalıdır:

```json
{
  "JwtSettings": {
    "SecretKey": "MicroserviceMaster-Super-Secret-Key-Min-32-Characters-Long",
    "Issuer": "IdentityService",
    "Audience": "MicroserviceMaster",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

## Docker Compose ile Çalıştırma

```bash
docker-compose up -d
```

Identity Service otomatik olarak başlatılacaktır.

## Test Senaryosu

1. Identity Service'den token alın
2. Token'ı diğer servislere istek yaparken kullanın
3. Token olmadan istek yaparsanız 401 Unauthorized alırsınız

## Güvenlik Notları

- Production ortamında `SecretKey` mutlaka değiştirilmeli ve güvenli bir şekilde saklanmalıdır
- Token süresi ihtiyaca göre ayarlanmalıdır
- HTTPS kullanılmalıdır
- Demo kullanıcılar production'da kaldırılmalıdır
- Gerçek uygulamada kullanıcı bilgileri database'den gelmelidir

## Servisler Arası İletişim

Servisler arası iletişimde client'tan gelen JWT token kullanılır. Örneğin:
- Order Service, Customer Service'i çağırırken aynı token'ı kullanır
- Bu sayede tüm servisler aynı kullanıcı context'inde çalışır

## Swagger ile Test

Her servisin Swagger UI'ında "Authorize" butonuna tıklayarak token'ı girebilirsiniz:
1. Identity Service'den token alın
2. Swagger UI'da "Authorize" butonuna tıklayın
3. `Bearer YOUR_TOKEN` formatında token'ı girin
4. Artık tüm endpoint'leri test edebilirsiniz
