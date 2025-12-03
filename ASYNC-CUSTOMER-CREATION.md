# Asenkron Customer Oluşturma - RabbitMQ Event-Driven

## Mimari

```
┌─────────────────┐         RabbitMQ          ┌──────────────────┐
│ Identity.API    │ ────────────────────────► │ Customer.API     │
│                 │  user.registered event     │                  │
│ POST /register  │                            │ Auto Create      │
└─────────────────┘                            └──────────────────┘
```

## Akış

### 1. Kullanıcı Kaydı (Identity Service)

```bash
POST http://localhost:5005/api/auth/register
{
  "email": "newuser@example.com",
  "password": "SecurePass123!"
}
```

**İşlem Adımları:**
1. Email ve şifre kontrolü
2. Unique CustomerId üretimi (örn: `CUST4A2B3C4D`)
3. Şifre hash'leme (BCrypt)
4. UserCredentials tablosuna kayıt
5. **RabbitMQ'ya event publish** → `identity_exchange` / `user.registered`

**Response:**
```json
{
  "message": "User registered successfully",
  "email": "newuser@example.com",
  "customerId": "CUST4A2B3C4D"
}
```

### 2. Event İşleme (Customer Service)

**Consumer:** `UserRegisteredConsumer` (Background Service)

**Dinlenen:**
- Exchange: `identity_exchange`
- Queue: `customer_user_registered_queue`
- Routing Key: `user.registered`

**Event Payload:**
```json
{
  "email": "newuser@example.com",
  "customerId": "CUST4A2B3C4D",
  "registeredAt": "2025-01-02T10:30:00Z"
}
```

**İşlem:**
1. Event'i consume et
2. Customer oluştur:
   - CustomerId: Event'ten gelen
   - Name: Email'in @ öncesi kısmı
   - Surname: "User" (default)
   - Email: Event'ten gelen
   - Phone: "+900000000000" (default)
3. Database'e kaydet
4. ACK gönder

### 3. Login

Artık kullanıcı login olabilir:

```bash
POST http://localhost:5005/api/auth/login
{
  "email": "newuser@example.com",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGc...",
  "customerId": "CUST4A2B3C4D",
  "email": "newuser@example.com",
  "expiresAt": "2025-01-02T11:30:00Z"
}
```

## RabbitMQ Yapılandırması

### Exchange
- **Name:** `identity_exchange`
- **Type:** Topic
- **Durable:** true

### Queue
- **Name:** `customer_user_registered_queue`
- **Durable:** true
- **Exclusive:** false
- **Auto Delete:** false

### Binding
- **Queue:** `customer_user_registered_queue`
- **Exchange:** `identity_exchange`
- **Routing Key:** `user.registered`

## Avantajlar

✅ **Loose Coupling**: Identity ve Customer servisleri birbirinden bağımsız
✅ **Asenkron**: Register işlemi hızlı tamamlanır
✅ **Scalability**: Customer creation işlemi ayrı scale edilebilir
✅ **Reliability**: RabbitMQ message persistence ile güvenli
✅ **Retry Mechanism**: Hata durumunda NACK ile retry

## Test Senaryosu

```bash
# 1. Yeni kullanıcı kaydı
curl -X POST http://localhost:5005/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "test123"
  }'

# Response: {"message":"User registered successfully","email":"test@example.com","customerId":"CUST12345678"}

# 2. Birkaç saniye bekle (RabbitMQ işleme süresi)
sleep 3

# 3. Customer'ın oluştuğunu kontrol et
curl http://localhost:5004/api/customers?email=test@example.com

# 4. Login ol
curl -X POST http://localhost:5005/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "test123"
  }'

# 5. Token ile customer bilgilerini çek
curl http://localhost:5004/api/customers \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Hata Yönetimi

### Consumer Hataları

**Senaryo 1: Database hatası**
- NACK gönderilir
- Message queue'ya geri döner
- Retry edilir

**Senaryo 2: Validation hatası**
- Log'lanır
- NACK gönderilir (requeue: true)

**Senaryo 3: Duplicate customer**
- Log'lanır
- ACK gönderilir (tekrar işlenmemesi için)

## Monitoring

### RabbitMQ Management UI
http://localhost:15672
- Username: guest
- Password: guest

**Kontrol Edilecekler:**
- Exchange: `identity_exchange` var mı?
- Queue: `customer_user_registered_queue` var mı?
- Binding doğru mu?
- Message count
- Consumer count

### Logs

**Identity Service:**
```
User registered: test@example.com, CustomerId: CUST12345678
```

**Customer Service:**
```
Customer created from user registration: CUST12345678, test@example.com
```

## Production Önerileri

🔒 **Güvenlik**
- RabbitMQ authentication ekle
- SSL/TLS kullan
- Message encryption

📊 **Monitoring**
- Dead Letter Queue ekle
- Message TTL ayarla
- Consumer health check

🔄 **Reliability**
- Idempotency kontrolü
- Duplicate detection
- Saga pattern (gelecekte)

## CustomerId Format

Format: `CUST` + 8 karakter hexadecimal (uppercase)

Örnekler:
- `CUST4A2B3C4D`
- `CUSTF1E2D3C4`
- `CUST12345678`

Unique olması Guid ile garanti edilir.
