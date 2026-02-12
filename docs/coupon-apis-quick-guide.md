# 🎫 Coupon APIs - Quick Guide

## 📋 Tổng quan

- **Admin Coupon**: Áp dụng cho tất cả khóa học (platform-wide)
- **Instructor Coupon**: Chỉ áp dụng cho khóa học của instructor đó

---

## 🔑 API Endpoints

### 👑 Admin APIs

```http
POST   /api/v1/coupons/admin              # Tạo coupon platform
GET    /api/v1/coupons/admin              # Xem tất cả coupon
PATCH  /api/v1/coupons/admin/{id}/toggle-status  # Bật/tắt coupon
```

### 👨‍🏫 Instructor APIs

```http
POST   /api/v1/coupons/instructor         # Tạo coupon riêng
GET    /api/v1/coupons/instructor         # Xem coupon của mình
```

### 🔄 Shared APIs (Admin + Instructor)

```http
PUT    /api/v1/coupons/{id}               # Update coupon (owner only)
DELETE /api/v1/coupons/{id}               # Delete coupon (owner only)
```

### 🌐 Public APIs

```http
GET    /api/v1/coupons/code/{code}        # Lấy coupon theo code
GET    /api/v1/coupons/active             # Danh sách coupon active
```

---

## 🎯 Coupon Usage APIs

### 👤 User APIs

```http
GET    /api/v1/coupon-usages/history      # Lịch sử sử dụng coupon
```

### 🌐 Public Validation

```http
GET    /api/v1/coupon-usages/can-use?userId=X&couponCode=Y  # Quick check
POST   /api/v1/coupon-usages/validate     # Full validation
```

---

## 📝 Request Examples

### Tạo Admin Coupon

```json
POST /api/v1/coupons/admin
{
  "code": "WELCOME50K",
  "type": "FixedAmount",
  "value": 50000,
  "minOrderAmount": 500000,
  "usageLimit": 1000,
  "validFrom": "2026-02-13T00:00:00Z",
  "validTo": "2026-05-13T23:59:59Z"
}
```

### Tạo Instructor Coupon

```json
POST /api/v1/coupons/instructor
{
  "code": "MYCOURSE20",
  "type": "Percentage",
  "value": 20,
  "maxDiscountAmount": 200000,
  "usageLimit": 100,
  "validFrom": "2026-02-13T00:00:00Z",
  "validTo": "2026-04-13T23:59:59Z"
}
```

### Validate Coupon

```json
POST /api/v1/coupon-usages/validate
{
  "couponCode": "WELCOME50K",
  "userId": "00000000-0000-0000-0000-000000000005",
  "courseIds": ["course-id-1", "course-id-2"],
  "orderSubtotal": 750000
}
```

**🎯 Mục đích:** Validate coupon trước khi tạo order

- ✅ Kiểm tra coupon có tồn tại và active không
- ✅ Kiểm tra user có thể sử dụng coupon không (chưa hết lượt)
- ✅ Kiểm tra coupon có áp dụng cho các khóa học trong giỏ hàng không
- ✅ Tính toán số tiền giảm giá
- ✅ Trả về tổng tiền sau khi áp dụng coupon

**📊 Response:**

```json
{
  "isSuccess": true,
  "data": {
    "isValid": true,
    "coupon": {
      "id": "coupon-id",
      "code": "WELCOME50K",
      "type": "FixedAmount",
      "value": 50000
    },
    "discountAmount": 50000,
    "finalAmount": 700000,
    "errorMessage": null
  }
}
```

**⚠️ Khi nào dùng:** Trước khi tạo order để đảm bảo coupon hợp lệ và tính đúng tiền!

---

## ⚠️ Important Rules

### 🔐 Authorization

- **Admin**: Quản lý tất cả coupon
- **Instructor**: Chỉ quản lý coupon của mình
- **Student**: Xem lịch sử sử dụng
- **Public**: Validate và xem coupon

### 📊 Coupon Types

- **FixedAmount**: Giảm tiền cố định (VD: 50,000 VND)
- **Percentage**: Giảm theo % (cần `maxDiscountAmount`)

### ✅ Validation Rules

- Code unique, max 50 chars
- Percentage cần `maxDiscountAmount`
- `validFrom` <= `validTo`
- `usageLimit`: Tổng lượt sử dụng
- `usagePerUser`: Lượt/user

### 🚫 Common Errors

- `401`: Unauthorized
- `403`: Forbidden (không phải owner)
- `404`: Coupon không tồn tại
- `"Coupon đã hết hạn"`
- `"Đã đạt giới hạn sử dụng"`

---

## 🧪 Quick Test

### 1. Tạo coupon

```bash
curl -X POST "http://localhost:5000/api/v1/coupons/admin" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"code":"TEST50K","type":"FixedAmount","value":50000,"validFrom":"2026-02-13T00:00:00Z","validTo":"2026-12-31T23:59:59Z"}'
```

### 2. Validate coupon

```bash
curl -X POST "http://localhost:5000/api/v1/coupon-usages/validate" \
  -H "Content-Type: application/json" \
  -d '{"couponCode":"TEST50K","userId":"user-id","courseIds":[],"orderSubtotal":500000}'
```

---

## 💡 Pro Tips

1. **Validate trước**: Dùng `/validate` trước khi tạo order
2. **Check ownership**: Instructor chỉ update coupon của mình
3. **Monitor usage**: Theo dõi `usedCount` để biết hiệu quả
4. **Set limits**: Đặt `usageLimit` và `usagePerUser` hợp lý
5. **Handle errors**: Luôn check `isSuccess` trong response

## 📊 Response Format

````json
{
  "isSuccess": true,
  "data": { /* actual data */ },
  "message": "Success message"
}
```</content>
<parameter name="filePath">d:\ChuyenNganh7\SWD392\Beyond8\beyond8-server\docs\coupon-apis-quick-guide.md
````
