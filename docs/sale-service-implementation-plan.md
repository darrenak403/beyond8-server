# Sale Service Implementation Plan

**Last Updated:** February 5, 2026  
**Status:** Phase 1 Completed ✅ | Phase 2 In Progress 🚧  
**Detailed Tasks:** See [sale-service-tasks.md](sale-service-tasks.md)

## 📋 Tổng quan Implementation

Sale Service là một microservice quan trọng trong hệ thống Beyond8, xử lý tất cả các hoạt động thương mại bao gồm:

- Quản lý đơn hàng (Orders)
- Xử lý thanh toán (Payments) với VNPay/PayOS
- Quản lý mã giảm giá (Coupons) và usage tracking
- Ví giảng viên (Instructor Wallets) với 14-day escrow
- Thanh toán cho giảng viên (Payouts)
- Ghi log giao dịch (Transactions)
- **Tự động settlement** sau 14 ngày (background job)

## 📚 Required Reading Before Implementation

⚠️ **QUAN TRỌNG:** Đọc kỹ tài liệu requirements TRƯỚC KHI CODE. KHÔNG TỰ Ý MỞ RỘNG SCOPE!

### Main Requirements Document

📖 **[docs/requirements/07-PAYMENT-ENROLLMENT.md](../requirements/07-PAYMENT-ENROLLMENT.md)** - YÊU CẦU BẮT BUỘC ĐỌC

Document này chứa TẤT CẢ requirements và business rules cho Sale Service:

- **REQ-07.01:** Enroll Khóa Học Miễn Phí
- **REQ-07.02:** Thanh Toán Qua VNPay
- **REQ-07.03:** Áp Dụng Mã Giảm Giá (Coupon)
- **REQ-07.04:** Lịch Sử Giao Dịch (Student)
- **REQ-07.06:** Yêu Cầu Hoàn Tiền (Refund)
- **REQ-07.09:** Quản Lý Ví & Rút Tiền (Instructor Payout)

### Business Rules

- **BR-04:** Truy Cập & Enrollment (Free courses enroll immediately)
- **BR-05:** Chính Sách Hoàn Tiền (14 days, <10% progress)
- **BR-11:** Thanh Toán (VNPay, Coupon rules, Transaction encryption)
- **BR-19:** Phân Chia Doanh Thu & Rút Tiền (70-30 split, 14-day escrow, minimum 500k VND)
- **NFR-07.01:** Bảo Mật Thanh Toán (Checksum, Idempotency)
- **NFR-07.02:** Độ Chính Xác Tài Chính (Decimal for money, ACID transactions)

### Required Reading per Service

| Service                     | Required REQs                   | Required BRs                       |
| --------------------------- | ------------------------------- | ---------------------------------- |
| **OrderService**            | REQ-07.01, REQ-07.02, REQ-07.04 | BR-04, BR-11                       |
| **PaymentService**          | REQ-07.02                       | BR-11, BR-19, NFR-07.01, NFR-07.02 |
| **CouponService**           | REQ-07.03                       | BR-11                              |
| **CouponUsageService**      | REQ-07.03                       | BR-11                              |
| **InstructorWalletService** | REQ-07.09                       | BR-19, NFR-07.02                   |
| **SettlementService**       | REQ-07.09                       | BR-05, BR-19                       |
| **PayoutService**           | REQ-07.09                       | BR-19                              |
| **TransactionService**      | REQ-07.09                       | BR-19, NFR-07.02                   |

### ⚠️ Common Scope Creep Warnings

**KHÔNG TỰ Ý THÊM CÁC TÍNH NĂNG SAU:**

1. ❌ **Refund Logic** - Đã comment out trong entities, requirements có nhắc nhưng là Phase 3 (KHÔNG làm ở Phase 2)
2. ❌ **PayOS/ZaloPay Integration** - Chỉ focus VNPay theo REQ-07.02, các gateway khác là optional
3. ❌ **Extra Coupon Types** - Chỉ Percentage và FixedAmount theo CouponType enum
4. ❌ **Extra Transaction Types** - Follow TransactionType enum exactly (Sale, Payout, Settlement, PlatformFee, Adjustment)
5. ❌ **Custom Revenue Split** - Mặc định 70% Instructor - 30% Platform (BR-19), KHÔNG làm configurable
6. ❌ **Auto-approve Payouts** - Requires Admin approval theo REQ-07.09
7. ❌ **Partial Refunds** - Enum có nhưng đã comment, KHÔNG implement
8. ❌ **Subscription/Recurring Payments** - Out of scope hoàn toàn

### 🔍 Validation Checklist

**Trước khi code:**

- ✅ Đã đọc requirements document (07-PAYMENT-ENROLLMENT.md)
- ✅ Đã hiểu business rules liên quan
- ✅ Đã xem entity design và relationships
- ✅ Đã review interface methods cần implement

**Trong khi code:**

- ✅ Cross-check mỗi feature với requirements
- ✅ KHÔNG thêm fields/properties ngoài entity design
- ✅ KHÔNG thêm methods ngoài interface đã define
- ✅ Follow exactly error messages và validation rules từ requirements

**Trước khi PR:**

- ✅ Verify KHÔNG có scope creep (features không có trong requirements)
- ✅ All acceptance criteria met
- ✅ Code comments reference requirements (e.g., "// Per BR-19: 70-30 split")

### 🚨 When Requirements Conflict

Nếu gặp xung đột giữa documents:

1. **Requirements (07-PAYMENT-ENROLLMENT.md) > Implementation Plan > Entity Comments**
2. Nếu business rule không rõ → ASK, đừng tự ý quyết định
3. Nếu có idea hay nhưng không trong requirements → Document lại để discuss sau, KHÔNG implement ngay

**Ví dụ xung đột đã phát hiện:**

- Entity OrderItem có `PlatformFeePercent` default 20%
- BR-19 yêu cầu Platform 30% - Instructor 70%
- **Resolution:** Follow BR-19 (requirements win)

---

## �️ Architecture Status

### ✅ Phase 1: Foundation (COMPLETED)

**Database Schema:**

- ✅ All 8 entities updated with ~30 new fields
- ✅ All 6 enums enhanced with documentation
- ✅ DbContext configured with 30+ indexes (UNIQUE, filtered, composite)
- ✅ JSONB columns configured (PaymentDetails, Metadata, BankAccountInfo)
- ✅ Migration generated and ready: `20260205085030_UpdateSaleEntities`

**Service Interfaces (8 total):**

1. ✅ **ICouponService** (9 methods - refactored)
   - GetActiveCouponsAsync, GetCouponsByInstructorAsync, ToggleCouponStatusAsync
2. ✅ **ICouponUsageService** (8 methods - NEW)
   - ValidateCouponAsync, RecordUsageAsync, GetCouponUsageStatisticsAsync
3. ✅ **IOrderService** (8 methods - refactored)
   - GetOrdersByStatusAsync, GetOrderStatisticsAsync
4. ✅ **IPaymentService** (8 methods - refactored)
   - HandleVNPayCallbackAsync, HandlePayOSCallbackAsync, CheckPaymentStatusAsync
5. ✅ **IInstructorWalletService** (4 methods)
6. ✅ **ITransactionService** (5 methods)
7. ✅ **ISettlementService** (7 methods - NEW)
   - ProcessPendingSettlementsAsync, SettleOrderAsync, ForceSettleAsync
8. ✅ **IPayoutService** (6 methods)

**DTOs Created (9 new classes):**

- ✅ Settlement DTOs: SettlementStatusResponse, SettlementStatisticsResponse
- ✅ Order DTOs: OrderStatisticsResponse
- ✅ CouponUsage DTOs: CouponValidationResult, CouponUsageResponse, CouponUsageStatisticsResponse, CreateCouponUsageRequest
- ✅ Payment DTOs: VNPayCallbackRequest, PayOSCallbackRequest

---

## �🎯 Thứ tự Ưu tiên Triển khai

### 1️⃣ IOrderService (Ưu tiên cao nhất - Core functionality)

**Lý do:** Đây là service cốt lõi, tất cả flow khác phụ thuộc vào Order

#### Chức năng chính:

- ✅ Tạo đơn hàng từ cart
- ✅ Quản lý trạng thái đơn hàng
- ✅ Hủy đơn hàng

#### Dependencies:

- Chỉ phụ thuộc vào repositories cơ bản
- Không phụ thuộc vào services khác

#### Implementation Steps:

1. Tạo `OrderService` class implement `IOrderService`
2. Implement CRUD operations cho Order entity
3. Thêm business logic cho order status transitions
4. Test với data sample

---

### 2️⃣ IPaymentService (Ưu tiên cao - Critical path)

**Lý do:** Thanh toán là bước tiếp theo ngay sau tạo order

#### Chức năng chính:

- ✅ Tích hợp VNPay gateway
- ✅ Xử lý webhooks và confirmations
- ✅ Refund processing

#### Dependencies:

- Phụ thuộc vào `IOrderService` (cần order để thanh toán)
- Cần VNPay SDK/API integration

#### Implementation Steps:

1. Tạo `PaymentService` class implement `IPaymentService`
2. Implement VNPay payment URL generation
3. Handle payment confirmations và webhooks
4. Implement refund logic
5. Test với VNPay sandbox environment

---

### 3️⃣ ICouponService (Ưu tiên trung bình - Có thể parallel)

**Lý do:** Coupon có thể áp dụng trong lúc tạo order hoặc riêng biệt

#### Chức năng chính:

- ✅ Tạo/sửa/xóa coupon
- ✅ Validate và áp dụng coupon cho order

#### Dependencies:

- Có thể độc lập, hoặc integrate với `IOrderService`
- Có thể implement parallel với OrderService

#### Implementation Steps:

1. Tạo `CouponService` class implement `ICouponService`
2. Implement CRUD operations cho Coupon entity
3. Thêm validation logic cho coupon codes
4. Implement coupon application logic
5. Test với various coupon scenarios

---

### 4️⃣ IInstructorWalletService (Ưu tiên trung bình - Sau payment)

**Lý do:** Ví instructor cập nhật sau khi có doanh thu từ payment

#### Chức năng chính:

- ✅ Quản lý số dư ví
- ✅ Thêm/trừ tiền từ sales

#### Dependencies:

- Phụ thuộc vào `IPaymentService` (cần payment thành công)
- Cần events từ payment completion

#### Implementation Steps:

1. Tạo `InstructorWalletService` class implement `IInstructorWalletService`
2. Implement wallet balance management
3. Handle revenue distribution logic
4. Listen to payment success events
5. Test với mock payment data

---

### 5️⃣ IPayoutService (Ưu tiên thấp - End of flow)

**Lý do:** Rút tiền là bước cuối, sau khi có tiền trong ví

#### Chức năng chính:

- ✅ Tạo yêu cầu rút tiền
- ✅ Approve/reject payouts

#### Dependencies:

- Phụ thuộc vào `IInstructorWalletService`
- Cần bank transfer integration

#### Implementation Steps:

1. Tạo `PayoutService` class implement `IPayoutService`
2. Implement payout request workflow
3. Add admin approval logic
4. Integrate với bank transfer APIs
5. Test với sandbox bank accounts

---

### 6️⃣ ITransactionService (Ưu tiên thấp - Logging/Analytics)

**Lý do:** Ghi log giao dịch cho tất cả operations, có thể làm cuối

#### Chức năng chính:

- ✅ Ghi log tất cả transactions
- ✅ Báo cáo revenue

#### Dependencies:

- Phụ thuộc vào tất cả services khác để log events
- Có thể implement cuối cùng

#### Implementation Steps:

1. Tạo `TransactionService` class implement `ITransactionService`
2. Implement transaction logging
3. Add reporting và analytics features
4. Integrate với tất cả other services
5. Test với comprehensive transaction data

---

## 📦 PHASE 3: API Endpoints & Validation

### Task 3.1: Coupon API Endpoints

**Priority:** P1 - High | **Estimate:** 5 SP | **Dependencies:** Task 2.1, Task 2.2

**Endpoints:**

- POST /api/v1/coupons (Admin/Instructor)
- GET /api/v1/coupons/{code} (Public)
- PUT /api/v1/coupons/{id}, DELETE /api/v1/coupons/{id}
- GET /api/v1/coupons, GET /api/v1/coupons/active (cached)
- GET /api/v1/coupons/instructor/{instructorId}
- PATCH /api/v1/coupons/{id}/toggle-status
- POST /api/v1/coupons/validate

**Tasks:** Create CouponEndpoints.cs, add rate limiting, authorization, OpenAPI docs, test with Postman

---

### Task 3.2: Order API Endpoints

**Priority:** P0 - Critical | **Estimate:** 5 SP | **Dependencies:** Task 2.3

**Endpoints:**

- POST /api/v1/orders, GET /api/v1/orders/{id}
- PATCH /api/v1/orders/{id}/status, POST /api/v1/orders/{id}/cancel
- GET /api/v1/orders/my-orders, GET /api/v1/orders/instructor/{instructorId}
- GET /api/v1/orders/status/{status}, GET /api/v1/orders/statistics

**Tasks:** Create OrderEndpoints.cs, add rate limiting, authorization, OpenAPI docs, test all endpoints

---

### Task 3.3: Payment API Endpoints

**Priority:** P0 - Critical | **Estimate:** 5 SP | **Dependencies:** Task 2.4

**Endpoints:**

- POST /api/v1/payments/process
- POST /api/v1/payments/vnpay/callback (AllowAnonymous)
- POST /api/v1/payments/payos/callback (AllowAnonymous)
- GET /api/v1/payments/{id}/status, GET /api/v1/payments/order/{orderId}
- GET /api/v1/payments/my-payments

**Tasks:** Create PaymentEndpoints.cs, webhook authentication, error handling, test with provider sandbox

---

### Task 3.4: Wallet & Payout API Endpoints

**Priority:** P2 - Medium | **Estimate:** 3 SP | **Dependencies:** Task 2.5, Task 2.8

**Wallet Endpoints:**

- GET /api/v1/wallets/my-wallet (Instructor)
- GET /api/v1/wallets/{instructorId} (Admin)
- GET /api/v1/wallets/{instructorId}/transactions

**Payout Endpoints:**

- POST /api/v1/payouts/request, GET /api/v1/payouts/{id}
- POST /api/v1/payouts/{id}/approve, POST /api/v1/payouts/{id}/reject
- GET /api/v1/payouts, GET /api/v1/payouts/my-requests

**Tasks:** Create WalletEndpoints.cs & PayoutEndpoints.cs, authorization, OpenAPI docs

---

### Task 3.5: Settlement API Endpoints (Admin Only)

**Priority:** P2 - Medium | **Estimate:** 3 SP | **Dependencies:** Task 2.7

**Endpoints:**

- POST /api/v1/settlements/process (manual trigger)
- POST /api/v1/settlements/{orderId}/settle
- POST /api/v1/settlements/{orderId}/force-settle
- GET /api/v1/settlements/pending, GET /api/v1/settlements/{orderId}/status
- GET /api/v1/settlements/upcoming, GET /api/v1/settlements/statistics

**Tasks:** Create SettlementEndpoints.cs, Admin-only authorization, OpenAPI docs

---

## 📦 PHASE 4: Event-Driven Integration

### Task 4.1: Event Definitions

**Priority:** P1 - High | **Estimate:** 2 SP

**Events to create:**

- OrderCreatedEvent, OrderCompletedEvent, OrderCancelledEvent, OrderRefundedEvent
- SettlementCompletedEvent, PayoutRequestedEvent, PayoutCompletedEvent

---

### Task 4.2: Event Publishers

**Priority:** P1 - High | **Estimate:** 3 SP | **Dependencies:** Task 4.1, Task 2.3, Task 2.4

**Tasks:** Publish OrderCompletedEvent, OrderCancelledEvent, SettlementCompletedEvent, PayoutCompletedEvent, add retry policy

---

### Task 4.3: Event Consumers

**Priority:** P1 - High | **Estimate:** 5 SP | **Dependencies:** Task 4.1

**Tasks:** Create FreeEnrollmentOrderRequestConsumer, InstructorApprovalEventConsumer, error handling, test event flow

---

## 📦 PHASE 5: Testing & Quality Assurance

### Task 5.1: Unit Tests

**Priority:** P1 - High | **Estimate:** 13 SP

**Coverage:** 80% minimum for all 8 services, mock external dependencies

---

### Task 5.2: Integration Tests

**Priority:** P1 - High | **Estimate:** 8 SP

**Tests:** End-to-end order flow, coupon scenarios, webhook handling, settlement job, event pub/sub

---

### Task 5.3: Load & Performance Tests

**Priority:** P2 - Medium | **Estimate:** 5 SP

**Tests:** 1000 orders/min, concurrent webhooks, 10k order settlement, query optimization

---

## 📦 PHASE 6: Documentation & Deployment

### Task 6.1: API Documentation

**Priority:** P2 - Medium | **Estimate:** 3 SP

**Tasks:** Complete OpenAPI/Swagger, XML comments, Postman collection, API usage guide

---

### Task 6.2: Architecture Documentation

**Priority:** P2 - Medium | **Estimate:** 2 SP

**Tasks:** Update CONCEPTUAL_DATA_MODEL.md, sequence diagrams, event-driven architecture docs

---

### Task 6.3: Deployment Configuration

**Priority:** P1 - High | **Estimate:** 3 SP

**Tasks:** Configure AppHost, connection strings, VNPay credentials (User Secrets), RabbitMQ, background job schedule, docker-compose, health checks

---

## 📊 Summary & Sprint Planning

| Phase                           | Tasks  | Story Points | Duration       |
| ------------------------------- | ------ | ------------ | -------------- |
| Phase 1: Foundation (DONE)      | 4      | ~13 SP       | ✅ Complete    |
| Phase 2: Core Services          | 8      | 81 SP        | 4 sprints      |
| Phase 3: API Endpoints          | 5      | 21 SP        | 2 sprints      |
| Phase 4: Event Integration      | 3      | 10 SP        | 1 sprint       |
| Phase 5: Testing                | 3      | 26 SP        | 2 sprints      |
| Phase 6: Documentation & Deploy | 3      | 8 SP         | 1 sprint       |
| **TOTAL**                       | **26** | **159 SP**   | **10 sprints** |

**Sprint Allocation (2 weeks each):**

- Sprint 1-2: Tasks 2.1, 2.2, 2.3 (Coupon + Order)
- Sprint 3-4: Tasks 2.4, 3.1, 3.2 (Payment + Endpoints)
- Sprint 5-6: Tasks 2.5, 2.6, 3.3, 4.1 (Wallet + Transaction + Events)
- Sprint 7-8: Tasks 2.7, 3.5, 4.2, 4.3 (Settlement + Event Integration)
- Sprint 9: Tasks 2.8, 3.4, 5.1 (Payout + Unit Tests)
- Sprint 10: Tasks 5.2, 5.3, 6.1, 6.2, 6.3 (Integration Tests + Docs + Deploy)

**Estimated Completion:** 20 weeks (5 months)

---

## 🚀 Quick Start for Development

### Immediate Actions:

1. ✅ Phase 1 completed - Database ready
2. 🚧 Start Task 2.1 (CouponService) - No dependencies
3. 🚧 Setup VNPay sandbox account for Task 2.4
4. 🚧 Define API contracts with Catalog service
5. 🚧 Setup test database for integration tests

## 📋 ClickUp Import Format

**For quick ClickUp import, use this task naming format:**

### Sprint 1-2 Tasks (Coupon + Order):

```
[BE-2.1] Implement CouponService - CRUD + validation + caching (5 SP)
[BE-2.2] Implement CouponUsageService - Validation + tracking + statistics (8 SP)
[BE-2.3] Implement OrderService - Cart to order + status management + events (13 SP)
```

### Sprint 3-4 Tasks (Payment + Endpoints):

```
[BE-2.4] Implement PaymentService - VNPay integration + webhooks + 14-day escrow (21 SP)
[BE-3.1] Create Coupon API Endpoints - Minimal APIs + authorization (5 SP)
[BE-3.2] Create Order API Endpoints - Minimal APIs + authorization (5 SP)
```

### Sprint 5-6 Tasks (Wallet + Events):

```
[BE-2.5] Implement InstructorWalletService - Balance management + transactions (8 SP)
[BE-2.6] Implement TransactionService - Logging + reporting (5 SP)
[BE-3.3] Create Payment API Endpoints - Webhooks + status checks (5 SP)
[BE-4.1] Define Sale Service Events - OrderCompleted, Settlement, Payout (2 SP)
```

### Sprint 7-8 Tasks (Settlement):

```
[BE-2.7] Implement SettlementService - 14-day processing + background job (13 SP)
[BE-3.5] Create Settlement API Endpoints - Admin endpoints (3 SP)
[BE-4.2] Implement Event Publishers - Publish on state changes (3 SP)
[BE-4.3] Implement Event Consumers - Free enrollment + wallet creation (5 SP)
```

### Sprint 9 Tasks (Payout + Tests):

```
[BE-2.8] Implement PayoutService - Withdrawal workflow + approval (8 SP)
[BE-3.4] Create Wallet & Payout API Endpoints - Instructor + Admin (3 SP)
[BE-5.1] Write Unit Tests - 80% coverage for all services (13 SP)
```

### Sprint 10 Tasks (Final):

```
[BE-5.2] Write Integration Tests - End-to-end flows (8 SP)
[BE-5.3] Load & Performance Tests - Optimization (5 SP)
[BE-6.1] Complete API Documentation - Swagger + Postman (3 SP)
[BE-6.2] Update Architecture Documentation - Sequence diagrams (2 SP)
[BE-6.3] Configure Deployment - AppHost + Docker + Health checks (3 SP)
```

---

_Document created: February 5, 2026_
_Author: Beyond8 Development Team_</content>
<parameter name="filePath">d:\ChuyenNganh7\SWD392\Beyond8\beyond8-server\docs\sale-service-implementation-plan.md
