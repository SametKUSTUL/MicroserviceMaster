# Identity Service - Database Kurulumu

## Yapılan Değişiklikler

### 1. Mimari Kararlar

✅ **Ayrı UserCredentials Tablosu**
- Customer tablosundan bağımsız
- Şifreler BCrypt ile hash'lenerek saklanıyor
- Email unique constraint
- LastLoginAt tracking

✅ **Customer Service Entegrasyonu**
- Identity Service, Customer Service'e HTTP çağrısı yapıyor
- Email ile customer doğrulaması yapılıyor
- Token'da gerçek CustomerId kullanılıyor

### 2. Database Şeması

```sql
CREATE TABLE UserCredentials (
    Id UUID PRIMARY KEY,
    CustomerId VARCHAR(50) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role VARCHAR(50) DEFAULT 'Customer',
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    LastLoginAt TIMESTAMP NULL
);

CREATE INDEX idx_usercredentials_email ON UserCredentials(Email);
CREATE INDEX idx_usercredentials_customerid ON UserCredentials(CustomerId);
```

### 3. Kullanım Akışı

#### Yeni Kullanıcı Kaydı (Register)

```bash
# 1. Önce Customer Service'de customer oluştur
curl -X POST http://localhost:5004/api/customers \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "CUST999",
    "name": "Test",
    "surname": "User",
    "email": "test@example.com",
    "phone": "+905551234567"
  }'

# 2. Identity Service'de kullanıcı kaydı yap
curl -X POST http://localhost:5005/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "SecurePass123!"
  }'
```

#### Login

```bash
curl -X POST http://localhost:5005/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "SecurePass123!"
  }'
```

### 4. Mevcut Customerlar İçin Kullanıcı Oluşturma

Database'de zaten customer'lar var. Bunlar için kullanıcı oluşturmak için:

```bash
# Mevcut bir customer'ın email'ini kullanarak register ol
curl -X POST http://localhost:5005/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "password123"
  }'
```

### 5. Güvenlik Özellikleri

✅ **Şifre Güvenliği**
- BCrypt hash algoritması
- Salt otomatik ekleniyor
- Şifreler asla plain text saklanmıyor

✅ **Token Güvenliği**
- JWT token 60 dakika geçerli
- Token'da gerçek customer bilgileri
- Customer Service'den doğrulama

✅ **Database Güvenliği**
- Email unique constraint
- Index'ler performans için
- IsActive flag ile soft delete

### 6. API Endpoints

#### POST /api/auth/register
Yeni kullanıcı kaydı

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "message": "User registered successfully",
  "email": "user@example.com"
}
```

#### POST /api/auth/login
Kullanıcı girişi

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGc...",
  "customerId": "CUST999",
  "email": "user@example.com",
  "expiresAt": "2025-01-02T12:00:00Z"
}
```

### 7. Hata Durumları

- **User already exists**: Email zaten kayıtlı
- **Customer not found**: Customer Service'de email bulunamadı
- **Invalid credentials**: Email veya şifre yanlış
- **User not found**: Kullanıcı kayıtlı değil

### 8. Production Önerileri

🔒 **Güvenlik**
- HTTPS kullan
- Rate limiting ekle
- Password policy uygula (min 8 karakter, büyük/küçük harf, rakam)
- 2FA ekle
- Account lockout mekanizması

📊 **Monitoring**
- Failed login attempts logla
- Suspicious activity detection
- Token usage tracking

🔄 **Bakım**
- Inactive user cleanup
- Password expiration policy
- Audit log tutma

### 9. Test Senaryosu

```bash
# 1. Mevcut customer'ı kontrol et
curl http://localhost:5004/api/customers?email=john@example.com

# 2. Bu customer için kullanıcı oluştur
curl -X POST http://localhost:5005/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "john@example.com", "password": "test123"}'

# 3. Login ol
curl -X POST http://localhost:5005/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "john@example.com", "password": "test123"}'

# 4. Token ile customer bilgilerini çek
curl http://localhost:5004/api/customers \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Database Tabloları

- **identitydb.UserCredentials**: Kullanıcı kimlik bilgileri
- **customerdb.Customers**: Customer bilgileri (mevcut)

İki tablo birbirinden bağımsız ama email ile ilişkilendirilmiş durumda.
