# 🎫 Coupon APIs - As-Built Quick Guide

## Tổng quan

Tài liệu này phản ánh **code thực tế** trong `src/Services/Sale`.

- Coupon platform do **Admin** quản lý
- Coupon riêng do **Instructor** quản lý
- Validation coupon tách riêng qua `coupon-usages`
- Response thống nhất theo `ApiResponse<T>`

---

## API Endpoints

### 1) Coupon Management (`/api/v1/coupons`)

#### Admin

```http
POST   /api/v1/coupons/admin
GET    /api/v1/coupons/admin
PATCH  /api/v1/coupons/admin/{couponId}/toggle-status
```

#### Instructor

```http
POST   /api/v1/coupons/instructor
GET    /api/v1/coupons/instructor
```

#### Admin + Instructor (owner-based)

```http
PUT    /api/v1/coupons/{couponId}
DELETE /api/v1/coupons/{couponId}
```

#### Public

```http
GET    /api/v1/coupons/code/{code}
GET    /api/v1/coupons/active
```

---

### 2) Coupon Usage (`/api/v1/coupon-usages`)

#### Authenticated user

```http
GET    /api/v1/coupon-usages/history
```

#### Public validation

```http
GET    /api/v1/coupon-usages/can-use?userId={userId}&couponCode={couponCode}
POST   /api/v1/coupon-usages/validate
```

---

## Request mẫu

### Tạo coupon platform (Admin)

```json
{
  "code": "WELCOME50K",
  "couponType": "FixedAmount",
  "value": 50000,
  "minOrderAmount": 500000,
  "usageLimit": 1000,
  "usagePerUser": 1,
  "validFrom": "2026-03-01T00:00:00Z",
  "validTo": "2026-12-31T23:59:59Z",
  "description": "Coupon platform chào mừng"
}
```

### Tạo coupon instructor

```json
{
  "code": "MYCOURSE20",
  "couponType": "Percentage",
  "value": 20,
  "maxDiscountAmount": 200000,
  "usageLimit": 100,
  "usagePerUser": 1,
  "validFrom": "2026-03-01T00:00:00Z",
  "validTo": "2026-06-30T23:59:59Z",
  "applicableCourseId": "00000000-0000-0000-0000-000000000001"
}
```

### Validate coupon trước checkout

```json
{
  "couponCode": "WELCOME50K",
  "userId": "00000000-0000-0000-0000-000000000005",
  "courseIds": ["00000000-0000-0000-0000-000000000101", "00000000-0000-0000-0000-000000000102"],
  "orderSubtotal": 750000
}
```

---

## Authorization matrix

- **Admin**: full quản lý coupon platform + toggle status
- **Instructor**: quản lý coupon của chính mình
- **User đăng nhập**: xem lịch sử dùng coupon
- **Public**: tra coupon code, list active, check/validate coupon

---

## Quy tắc cần nhớ

- `code` phải unique
- `couponType = Percentage` nên có `maxDiscountAmount`
- `validFrom <= validTo`
- `usageLimit` và `usagePerUser` phải hợp lệ
- Nên gọi `POST /coupon-usages/validate` trước khi tạo order/checkout

---

## Response format

```json
{
  "isSuccess": true,
  "data": {},
  "message": "Success"
}
```

---

## Liên quan thực thi trong Sale Service

- Payment callback: `GET /api/v1/payments/vnpay/callback`
- Order flows: `/api/v1/orders/buy-now`, `/api/v1/cart/checkout`
- Settlement đang hoạt động qua `/api/v1/settlements/*` + Hangfire job
