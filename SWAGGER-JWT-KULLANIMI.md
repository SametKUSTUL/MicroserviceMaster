# Swagger'da JWT Token Kullanımı

## ✅ Token Başarıyla Çalışıyor!

Test sonuçları token'ın tüm servislerde çalıştığını gösteriyor.

## Adım Adım Kullanım

### 1. Identity Service'den Token Alın

**Swagger URL:** http://localhost:5005/swagger

1. `/api/auth/login` endpoint'ini açın
2. "Try it out" butonuna tıklayın
3. Request body'yi doldurun:
```json
{
  "email": "customer1@test.com",
  "password": "password123"
}
```
4. "Execute" butonuna tıklayın
5. Response'dan `token` değerini kopyalayın

**Örnek Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "customerId": "1",
  "email": "customer1@test.com",
  "expiresAt": "2025-01-02T12:00:00Z"
}
```

### 2. Diğer Servislerde Token'ı Kullanın

**Herhangi bir servisin Swagger sayfasına gidin:**
- Customer Service: http://localhost:5004/swagger
- Order Service: http://localhost:5001/swagger
- Product Service: http://localhost:5003/swagger
- Payment Service: http://localhost:5002/swagger

**Token'ı Ekleyin:**
1. Sayfanın sağ üst köşesindeki **"Authorize"** 🔓 butonuna tıklayın
2. Açılan pencereye şunu yazın:
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
   (Kendi token'ınızı yapıştırın, "Bearer " önekini unutmayın!)
3. **"Authorize"** butonuna tıklayın
4. Pencereyi kapatın

**Artık tüm endpoint'leri kullanabilirsiniz!** 🎉

### 3. Token Olmadan İstek Yapmayı Deneyin

Token eklemeden herhangi bir endpoint'i çağırırsanız **401 Unauthorized** hatası alırsınız.

## Demo Kullanıcılar

| Email | Password | Customer ID | Role |
|-------|----------|-------------|------|
| customer1@test.com | password123 | 1 | Customer |
| customer2@test.com | password123 | 2 | Customer |
| admin@test.com | admin123 | admin | Admin |

## cURL ile Test

```bash
# 1. Token al
curl -X POST http://localhost:5005/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "customer1@test.com", "password": "password123"}'

# 2. Token ile istek yap
curl -X GET http://localhost:5004/api/customers \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Sorun Giderme

### Token çalışmıyor mu?

1. **Token'ın başında "Bearer " var mı?**
   - ✅ Doğru: `Bearer eyJhbGc...`
   - ❌ Yanlış: `eyJhbGc...`

2. **Token süresi dolmuş olabilir**
   - Token 60 dakika geçerlidir
   - Yeni token alın

3. **Servislerin hepsi ayakta mı?**
   ```bash
   docker-compose ps
   ```

4. **Doğru portu kullanıyor musunuz?**
   - Identity: 5005
   - Customer: 5004
   - Order: 5001
   - Product: 5003
   - Payment: 5002

## Token İçeriği

Token decode edildiğinde şu bilgileri içerir:
- `sub`: Customer ID
- `email`: Kullanıcı email
- `role`: Kullanıcı rolü
- `customerId`: Customer ID (claim)
- `exp`: Token son kullanma tarihi
- `iss`: IdentityService
- `aud`: MicroserviceMaster

Token'ı decode etmek için: https://jwt.io
