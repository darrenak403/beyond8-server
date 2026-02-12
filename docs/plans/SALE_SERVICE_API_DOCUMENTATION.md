# Beyond8 Sale Service - API Documentation

## 📋 Tổng quan

Sale Service quản lý toàn bộ hệ thống thanh toán, ví tiền, coupon, và đơn hàng trong nền tảng học tập Beyond8. Service này tích hợp với VNPay để xử lý thanh toán và sử dụng kiến trúc Clean Architecture với PostgreSQL.

### 🎯 Mục tiêu chính

- **Thanh toán khóa học**: Xử lý mua bán khóa học với VNPay
- **Quản lý ví tiền**: Ví giảng viên với hold/reserve cho coupon
- **Hệ thống coupon**: Giảm giá với cơ chế hold tiền
- **Rút tiền**: Admin phê duyệt rút tiền cho giảng viên
- **Ví nền tảng**: Theo dõi doanh thu 30% và chi phí coupon hệ thống

### 🏗️ Kiến trúc

- **Framework**: ASP.NET Core 8 Minimal APIs
- **Database**: PostgreSQL với EF Core
- **Payment Gateway**: VNPay (ATM, Visa, QR Code)
- **Validation**: FluentValidation
- **Authentication**: JWT Bearer Token
- **Authorization**: Role-based (Admin, Staff, Instructor, Student)

---

## 👥 User Roles & Permissions

| Role           | Mô tả                  | Quyền chính                                                      |
| -------------- | ---------------------- | ---------------------------------------------------------------- |
| **Admin**      | Quản trị viên hệ thống | Toàn quyền: quản lý coupon, phê duyệt payout, xem tất cả dữ liệu |
| **Staff**      | Nhân viên hỗ trợ       | Giống Admin nhưng hạn chế hơn                                    |
| **Instructor** | Giảng viên             | Quản lý khóa học, coupon, ví tiền, yêu cầu payout                |
| **Student**    | Học viên               | Mua khóa học, thanh toán, xem lịch sử                            |

---

## 💰 Luồng Thanh Toán Chính (Payment Flow)

### 1. Mua Khóa Học (Student → Order → Payment)

```
Student mua khóa học → Tạo Order → Thanh toán VNPay → Callback → Cập nhật Order
                                                          ↓
                                               Instructor nhận 70% → Platform nhận 30%
```

### 2. Nạp Tiền Ví (Instructor → Wallet Top-up)

```
Instructor yêu cầu nạp tiền → Thanh toán VNPay → Callback → Cộng tiền vào ví
```

### 3. Rút Tiền (Instructor → Payout Request → Admin Approval)

```
Instructor yêu cầu rút tiền → Admin phê duyệt → Chuyển tiền → Cập nhật ví
```

### 4. Coupon Flow (Instructor/Admin → Hold Money → Usage)

```
Tạo coupon → Hold tiền trong ví → Student dùng coupon → Trừ tiền hold
```

---

## 🔗 API Endpoints Chi Tiết

### 🛒 ORDER APIs (`/api/v1/orders`)

#### **Student Endpoints**

| Method | Endpoint                | Mô tả                         | Request             | Response                           |
| ------ | ----------------------- | ----------------------------- | ------------------- | ---------------------------------- |
| `POST` | `/buy-now`              | Mua ngay 1 khóa học           | `BuyNowRequest`     | `ApiResponse<OrderResponse>`       |
| `GET`  | `/{orderId}`            | Xem chi tiết đơn hàng         | -                   | `ApiResponse<OrderResponse>`       |
| `GET`  | `/user/{userId}`        | Lịch sử đơn hàng (phân trang) | `PaginationRequest` | `ApiResponse<List<OrderResponse>>` |
| `GET`  | `/purchased-course-ids` | Danh sách ID khóa học đã mua  | -                   | `ApiResponse<List<Guid>>`          |

#### **Instructor Endpoints**

| Method | Endpoint                     | Mô tả                              | Request             | Response                           |
| ------ | ---------------------------- | ---------------------------------- | ------------------- | ---------------------------------- |
| `GET`  | `/instructor/{instructorId}` | Đơn hàng bán khóa học (phân trang) | `PaginationRequest` | `ApiResponse<List<OrderResponse>>` |

#### **Admin Endpoints**

| Method  | Endpoint            | Mô tả                                 | Request                    | Response                           |
| ------- | ------------------- | ------------------------------------- | -------------------------- | ---------------------------------- |
| `GET`   | `/status/{status}`  | Đơn hàng theo trạng thái (phân trang) | `PaginationRequest`        | `ApiResponse<List<OrderResponse>>` |
| `PATCH` | `/{orderId}/status` | Cập nhật trạng thái đơn hàng          | `UpdateOrderStatusRequest` | `ApiResponse<OrderResponse>`       |

---

### 💳 PAYMENT APIs (`/api/v1/payments`)

#### **Student Endpoints**

| Method | Endpoint              | Mô tả                                                               | Request                 | Response                             |
| ------ | --------------------- | ------------------------------------------------------------------- | ----------------------- | ------------------------------------ |
| `POST` | `/process`            | Khởi tạo thanh toán VNPay cho đơn hàng<br>**Purpose: OrderPayment** | `ProcessPaymentRequest` | `ApiResponse<PaymentUrlResponse>`    |
| `GET`  | `/{paymentId}/status` | Kiểm tra trạng thái thanh toán                                      | -                       | `ApiResponse<PaymentResponse>`       |
| `GET`  | `/order/{orderId}`    | Thanh toán theo đơn hàng                                            | -                       | `ApiResponse<List<PaymentResponse>>` |
| `GET`  | `/my-payments`        | Lịch sử thanh toán (phân trang)                                     | `PaginationRequest`     | `ApiResponse<List<PaymentResponse>>` |

#### **System/Webhook Endpoints**

| Method | Endpoint          | Mô tả                                                                       | Request      | Response             |
| ------ | ----------------- | --------------------------------------------------------------------------- | ------------ | -------------------- |
| `GET`  | `/vnpay/callback` | VNPay callback - xử lý kết quả thanh toán<br>**HMAC verification required** | Query params | Redirect to frontend |

---

### 👛 WALLET APIs (`/api/v1/wallets`)

#### **Instructor Endpoints**

| Method | Endpoint                  | Mô tả                                                 | Request             | Response                                       |
| ------ | ------------------------- | ----------------------------------------------------- | ------------------- | ---------------------------------------------- |
| `GET`  | `/my-wallet`              | Xem thông tin ví của mình                             | -                   | `ApiResponse<InstructorWalletResponse>`        |
| `POST` | `/top-up`                 | Nạp tiền vào ví qua VNPay<br>**Purpose: WalletTopUp** | `TopUpRequest`      | `ApiResponse<PaymentUrlResponse>`              |
| `GET`  | `/my-wallet/transactions` | Lịch sử giao dịch ví (phân trang)                     | `PaginationRequest` | `ApiResponse<List<WalletTransactionResponse>>` |

#### **Admin Endpoints**

| Method | Endpoint                                  | Mô tả                                        | Request             | Response                                       |
| ------ | ----------------------------------------- | -------------------------------------------- | ------------------- | ---------------------------------------------- |
| `GET`  | `/instructor/{instructorId}`              | Xem ví của giảng viên khác                   | -                   | `ApiResponse<InstructorWalletResponse>`        |
| `GET`  | `/instructor/{instructorId}/transactions` | Lịch sử giao dịch ví giảng viên (phân trang) | `PaginationRequest` | `ApiResponse<List<WalletTransactionResponse>>` |
| `POST` | `/create/{instructorId}`                  | Tạo ví cho giảng viên (internal)             | -                   | `ApiResponse<InstructorWalletResponse>`        |

---

### 💰 PAYOUT APIs (`/api/v1/payouts`)

#### **Instructor Endpoints**

| Method | Endpoint       | Mô tả                                   | Request               | Response                                   |
| ------ | -------------- | --------------------------------------- | --------------------- | ------------------------------------------ |
| `POST` | `/request`     | Yêu cầu rút tiền (min 500k VND)         | `CreatePayoutRequest` | `ApiResponse<PayoutRequestResponse>`       |
| `GET`  | `/my-requests` | Danh sách yêu cầu rút tiền (phân trang) | `PaginationRequest`   | `ApiResponse<List<PayoutRequestResponse>>` |
| `GET`  | `/{payoutId}`  | Chi tiết yêu cầu rút tiền               | -                     | `ApiResponse<PayoutRequestResponse>`       |

#### **Admin Endpoints**

| Method | Endpoint              | Mô tả                                | Request             | Response                                   |
| ------ | --------------------- | ------------------------------------ | ------------------- | ------------------------------------------ |
| `GET`  | `/`                   | Tất cả yêu cầu rút tiền (phân trang) | `PaginationRequest` | `ApiResponse<List<PayoutRequestResponse>>` |
| `POST` | `/{payoutId}/approve` | Phê duyệt yêu cầu rút tiền           | -                   | `ApiResponse<bool>`                        |
| `POST` | `/{payoutId}/reject`  | Từ chối yêu cầu rút tiền             | `RejectPayoutDto`   | `ApiResponse<bool>`                        |

---

### 🎫 COUPON APIs (`/api/v1/coupons`)

#### **Admin/Instructor Endpoints**

| Method   | Endpoint      | Mô tả           | Request               | Response                      |
| -------- | ------------- | --------------- | --------------------- | ----------------------------- |
| `POST`   | `/`           | Tạo coupon mới  | `CreateCouponRequest` | `ApiResponse<CouponResponse>` |
| `PUT`    | `/{couponId}` | Cập nhật coupon | `UpdateCouponRequest` | `ApiResponse<CouponResponse>` |
| `DELETE` | `/{couponId}` | Xóa coupon      | -                     | `ApiResponse<bool>`           |

#### **Admin Only Endpoints**

| Method  | Endpoint                    | Mô tả                                | Request             | Response                            |
| ------- | --------------------------- | ------------------------------------ | ------------------- | ----------------------------------- |
| `GET`   | `/`                         | Danh sách tất cả coupon (phân trang) | `PaginationRequest` | `ApiResponse<List<CouponResponse>>` |
| `PATCH` | `/{couponId}/toggle-status` | Bật/tắt trạng thái coupon            | -                   | `ApiResponse<bool>`                 |

#### **Instructor Endpoints**

| Method | Endpoint      | Mô tả                        | Request             | Response                            |
| ------ | ------------- | ---------------------------- | ------------------- | ----------------------------------- |
| `GET`  | `/instructor` | Coupon của mình (phân trang) | `PaginationRequest` | `ApiResponse<List<CouponResponse>>` |

#### **Public Endpoints**

| Method | Endpoint       | Mô tả                           | Request | Response                            |
| ------ | -------------- | ------------------------------- | ------- | ----------------------------------- |
| `GET`  | `/code/{code}` | Thông tin coupon theo mã        | -       | `ApiResponse<CouponResponse>`       |
| `GET`  | `/active`      | Danh sách coupon đang hoạt động | -       | `ApiResponse<List<CouponResponse>>` |

---

### 🏦 PLATFORM WALLET APIs (`/api/v1/platform-wallet`)

#### **Admin Only Endpoints**

| Method | Endpoint | Mô tả                 | Request | Response                              |
| ------ | -------- | --------------------- | ------- | ------------------------------------- |
| `GET`  | `/`      | Thông tin ví nền tảng | -       | `ApiResponse<PlatformWalletResponse>` |

---

## 🔄 Business Logic & Validation Rules

### 💰 Revenue Split (BR-19)

- **Giảng viên**: 70% giá khóa học
- **Nền tảng**: 30% commission
- **Ví dụ**: Khóa học 1.000.000 VND
  - Giảng viên: 700.000 VND
  - Nền tảng: 300.000 VND

### 🎫 Coupon Hold Mechanism

- **Instructor Coupon**: Khi tạo coupon, tiền được hold trong ví
- **Hold Amount**: `Value × UsageLimit` (FixedAmount) hoặc `MaxDiscountAmount × UsageLimit` (Percentage)
- **Usage**: Khi student dùng coupon, trừ tiền từ hold balance
- **Release**: Khi coupon hết hạn/không hoạt động, tiền hold được trả về available balance

### 💳 Payment Rules

- **VNPay Integration**: HMAC-SHA512 signature verification
- **Payment Purpose**: `OrderPayment` (mua khóa học) vs `WalletTopUp` (nạp ví)
- **Expiry**: 15 phút cho mỗi payment URL
- **Idempotency**: Callback xử lý 1 lần, tránh duplicate

### 👛 Wallet Rules

- **Available Balance**: Tiền có thể sử dụng/rút
- **Hold Balance**: Tiền bị khóa cho coupon commitment
- **Minimum Payout**: 500.000 VND
- **Admin Approval**: Required cho mọi payout request

### 📊 Platform Wallet

- **Singleton Entity**: Chỉ có 1 ví nền tảng
- **Negative Balance**: Có thể âm khi chi phí coupon hệ thống > doanh thu
- **Auto Offset**: Doanh thu mới sẽ bù đắp số âm

---

## 🔐 Security & Authorization

### Authentication

- **JWT Bearer Token**: Required cho hầu hết endpoints
- **VNPay Callback**: AllowAnonymous nhưng verify HMAC signature

### Authorization Matrix

| API Group       | Student         | Instructor      | Admin/Staff       |
| --------------- | --------------- | --------------- | ----------------- |
| Order           | ✅ Read own     | ✅ Read sales   | ✅ Full access    |
| Payment         | ✅ Process/Read | ❌              | ✅ Read all       |
| Wallet          | ❌              | ✅ Own wallet   | ✅ All wallets    |
| Payout          | ❌              | ✅ Own requests | ✅ Approve/Reject |
| Coupon          | ❌              | ✅ Own coupons  | ✅ All coupons    |
| Platform Wallet | ❌              | ❌              | ✅ Read only      |

### Rate Limiting

- **Applied to all endpoints**: `RequireRateLimiting("Fixed")`
- **Configuration**: Trong `appsettings.json`

---

## 📡 Integration với Services Khác

### 🔗 Catalog Service

- **Validate Course**: Kiểm tra khóa học tồn tại và giá
- **Update Statistics**: Cập nhật số học viên sau khi thanh toán thành công

### 🔗 Identity Service

- **Verify Instructor**: Kiểm tra trạng thái instructor trước khi cho phép tạo coupon
- **User Info**: Lấy thông tin user cho authorization

### 📨 MassTransit Events

- **OrderCompletedEvent**: Phát sau khi thanh toán thành công
- **CacheInvalidateEvent**: Invalidate cache khi có thay đổi

### 🔔 Notification Service

- **Payment Success**: Thông báo thanh toán thành công
- **Payout Approved**: Thông báo phê duyệt rút tiền

---

## ⚠️ Error Handling & Validation

### Common Error Responses

```json
{
  "isSuccess": false,
  "message": "Error message in Vietnamese",
  "data": null
}
```

### Validation Rules

- **Amount**: Min 10.000 VND cho top-up
- **Payout**: Min 500.000 VND
- **Coupon**: Hold amount phải ≤ Available balance
- **Payment**: Expiry trong 15 phút

### Business Logic Errors

- Insufficient funds cho coupon creation
- Invalid coupon code/expiry
- Unauthorized access
- Payment already processed

---

## 🔄 Complete Flow Examples

### 🎓 Student mua khóa học với coupon

1. Student chọn khóa học → `POST /api/v1/orders/buy-now`
2. System tạo Order với coupon → Validate coupon và tính tổng
3. Student thanh toán → `POST /api/v1/payments/process`
4. VNPay redirect → Student thanh toán
5. VNPay callback → `GET /api/v1/payments/vnpay/callback`
6. System cập nhật Order → Credit instructor 70%, platform 30%
7. Trừ tiền hold coupon nếu có
8. Phát `OrderCompletedEvent`

### 👨‍🏫 Instructor tạo coupon

1. Instructor tạo coupon → `POST /api/v1/coupons/`
2. System validate đủ tiền trong ví
3. Hold tiền: Available → Hold balance
4. Tạo coupon với `HoldAmount` và `RemainingHoldAmount`

### 👨‍🏫 Instructor rút tiền

1. Instructor yêu cầu rút → `POST /api/v1/payouts/request`
2. Admin xem và phê duyệt → `POST /api/v1/payouts/{id}/approve`
3. System chuyển tiền Available → TotalWithdrawn
4. Phát notification

### 👨‍🏫 Instructor nạp ví

1. Instructor yêu cầu nạp → `POST /api/v1/wallets/top-up`
2. System tạo Payment với Purpose=WalletTopUp
3. VNPay redirect → Instructor thanh toán
4. VNPay callback → Credit tiền vào Available balance

---

## 📊 Monitoring & Analytics

### Platform Wallet Tracking

- **Total Revenue**: Tổng doanh thu 30% từ tất cả sales
- **Total Coupon Cost**: Tổng chi phí coupon hệ thống
- **Available Balance**: Số dư hiện tại (có thể âm)

### Instructor Wallet Tracking

- **Available Balance**: Tiền có thể rút
- **Hold Balance**: Tiền bị khóa cho coupon
- **Total Earnings**: Tổng thu nhập tích lũy
- **Total Withdrawn**: Tổng đã rút

### Transaction Ledger

- **Audit Trail**: Mọi thay đổi balance được ghi lại
- **Reconciliation**: Có thể đối chiếu tại bất kỳ thời điểm nào

---

## 🚀 Deployment & Configuration

### Environment Variables

```env
# Database
CONNECTIONSTRINGS__DEFAULTCONNECTION=postgresql://...

# VNPay
VNPAY__TMNCODE=...
VNPAY__HASHSECRET=...
VNPAY__BACKENDCALLBACKURL=...
VNPAY__RETURNURL=...

# JWT
JWT__SECRET=...
JWT__ISSUER=...
JWT__AUDIENCE=...
```

### Docker Compose

- **Database**: PostgreSQL container
- **Service**: .NET 8 container
- **Migrations**: Auto-run on startup (Development only)

---

## 🧪 Testing Strategy

### Unit Tests

- Service layer logic
- Validation rules
- Business calculations

### Integration Tests

- API endpoints
- Database operations
- VNPay callback simulation

### E2E Tests

- Complete payment flows
- Multi-service interactions
- Error scenarios

---

_Document version: 1.0_
_Last updated: February 10, 2026_
_Service: Beyond8 Sale Service_</content>
<filePath="d:\ChuyenNganh7\SWD392\Beyond8\beyond8-server\SALE_SERVICE_API_DOCUMENTATION.md
