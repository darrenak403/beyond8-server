# Sale Service - ClickUp Tasks Breakdown

**Last Updated:** February 5, 2026  
**Sprint:** Sale Service Implementation  
**Epic:** Beyond8 - Sale Service

---

## 📦 PHASE 1: Foundation & Database (COMPLETED ✅)

### Task 1.1: Entity & Enum Updates ✅

- [x] Update Order entity (9 fields: PaidAt, SettlementEligibleAt, IsSettled, SettledAt, IpAddress, UserAgent, Notes, PaymentDetails, TotalRefunded)
- [x] Update Payment entity (6 fields: PaymentNumber, PaymentMethod, FailureReason, ExpiredAt, Metadata, refund fields)
- [x] Update OrderItem entity (7 fields: revenue split calculations)
- [x] Update InstructorWallet entity (6 fields: HoldBalance, TotalEarnings, BankAccountInfo)
- [x] Update TransactionLedger entity (9 fields: AvailableAt for 14-day escrow)
- [x] Update PayoutRequest entity (11 fields: approval workflow)
- [x] Update Coupon entity (5 fields: applicability constraints)
- [x] Update CouponUsage entity (1 field: DiscountAmount)
- [x] Enhance all enums with XML documentation
- [x] Code cleanup - remove excessive XML summaries

### Task 1.2: DbContext Configuration ✅

- [x] Configure Order entity (5 indexes, JSONB PaymentDetails)
- [x] Configure Payment entity (6 indexes, JSONB Metadata)
- [x] Configure Coupon entity (4 indexes, filtered indexes)
- [x] Configure InstructorWallet entity (2 indexes, JSONB BankAccountInfo)
- [x] Configure PayoutRequest entity (5 indexes)
- [x] Configure TransactionLedger entity (5 indexes, filtered AvailableAt)
- [x] Configure soft delete query filters for all entities

### Task 1.3: Database Migration ✅

- [x] Generate migration: UpdateSaleEntities
- [x] Review migration Up/Down methods
- [x] Apply migration via AppHost

### Task 1.4: Interface Design ✅

- [x] Create ISettlementService interface (7 methods)
- [x] Create ICouponUsageService interface (8 methods)
- [x] Refactor ICouponService (+3 methods)
- [x] Refactor IPaymentService (+3 methods for webhooks)
- [x] Refactor IOrderService (+2 methods for statistics)
- [x] Create all required DTOs (9 classes)

---

## 📦 PHASE 2: Core Services Implementation

### Task 2.1: Coupon Management Service

**Priority:** P1 - High  
**Estimate:** 5 story points  
**Dependencies:** None

**Subtasks:**

- [ ] Implement CouponService class
  - [ ] CreateCouponAsync (validation: code uniqueness, dates)
  - [ ] GetCouponByCodeAsync (with caching)
  - [ ] UpdateCouponAsync (business rules validation)
  - [ ] DeleteCouponAsync (soft delete)
  - [ ] GetCouponsAsync (paginated, filtered by status)
  - [ ] GetActiveCouponsAsync (public endpoint, cache 5 mins)
  - [ ] GetCouponsByInstructorAsync (instructor-owned coupons)
  - [ ] ToggleCouponStatusAsync (activate/deactivate)
  - [ ] ApplyCouponAsync (discount calculation logic)
- [ ] Write unit tests (minimum 80% coverage)
- [ ] Integration tests with mock data
- [ ] Add FluentValidation validators
  - [ ] CreateCouponRequestValidator
  - [ ] UpdateCouponRequestValidator

**Acceptance Criteria:**

- ✅ All CRUD operations work correctly
- ✅ Discount calculation accurate (percentage vs fixed amount)
- ✅ Validation prevents invalid coupon creation
- ✅ Caching implemented for frequently accessed coupons

---

### Task 2.2: Coupon Usage Tracking Service

**Priority:** P1 - High  
**Estimate:** 8 story points  
**Dependencies:** Task 2.1

**Subtasks:**

- [ ] Implement CouponUsageService class
  - [ ] ValidateCouponAsync (comprehensive validation)
    - [ ] Check expiry dates (ValidFrom <= Now <= ValidUntil)
    - [ ] Check global usage limit (UsageCount < UsageLimit)
    - [ ] Check per-user limit (user usage count < UsageLimitPerUser)
    - [ ] Check applicability (instructor, course, category)
    - [ ] Check minimum order amount
    - [ ] Calculate discount with max cap
  - [ ] RecordUsageAsync (after order completion)
  - [ ] GetUserUsageCountAsync (for limit enforcement)
  - [ ] GetUserUsageHistoryAsync (paginated)
  - [ ] GetCouponUsageStatisticsAsync (admin analytics)
  - [ ] GetUsageByOrderAsync (order details)
  - [ ] GetTopCouponsAsync (top 10 performers)
  - [ ] CanUserUseCouponAsync (quick check for UI)
- [ ] Write unit tests (minimum 80% coverage)
- [ ] Integration tests with various scenarios
- [ ] Add FluentValidation validators
  - [ ] CreateCouponUsageRequestValidator

**Acceptance Criteria:**

- ✅ Validation catches all edge cases
- ✅ Usage tracking is accurate
- ✅ Per-user limits enforced correctly
- ✅ Statistics provide meaningful insights

---

### Task 2.3: Order Service Implementation

**Priority:** P0 - Critical  
**Estimate:** 13 story points  
**Dependencies:** Task 2.1, Task 2.2

**Subtasks:**

- [ ] Implement OrderService class
  - [ ] CreateOrderAsync (cart → order conversion)
    - [ ] Validate courses exist (call Catalog service)
    - [ ] Check instructor verification status
    - [ ] Apply coupon if provided (call CouponUsageService)
    - [ ] Calculate totals (subtotal, discount, final)
    - [ ] Calculate platform fee (20%) and instructor earnings
    - [ ] Create Order + OrderItems
    - [ ] Generate unique OrderNumber (ORDER-YYYYMMDD-XXXXX)
    - [ ] Set Status = Pending
    - [ ] Capture IpAddress, UserAgent for audit
  - [ ] GetOrderByIdAsync (with authorization check)
  - [ ] UpdateOrderStatusAsync (state machine validation)
    - [ ] Pending → Completed (when payment success)
    - [ ] Pending → Cancelled (timeout or user cancel)
    - [ ] Completed → Refunded (admin action)
  - [ ] CancelOrderAsync (business rules)
    - [ ] Can only cancel Pending orders
    - [ ] Restore coupon usage count if used
  - [ ] GetOrdersByUserAsync (paginated user orders)
  - [ ] GetOrdersByInstructorAsync (instructor's sales)
  - [ ] GetOrdersByStatusAsync (filtered by status)
  - [ ] GetOrderStatisticsAsync (revenue, counts, averages)
- [ ] Implement event publishing
  - [ ] OrderCreatedEvent
  - [ ] OrderCompletedEvent (after payment)
  - [ ] OrderCancelledEvent
- [ ] Write unit tests (minimum 80% coverage)
- [ ] Integration tests with mock Catalog service
- [ ] Add FluentValidation validators
  - [ ] CreateOrderRequestValidator
  - [ ] UpdateOrderStatusRequestValidator

**Acceptance Criteria:**

- ✅ Orders created correctly with all calculations
- ✅ Status transitions follow business rules
- ✅ Coupon application integrates seamlessly
- ✅ Events published for downstream services

---

### Task 2.4: Payment Service Implementation

**Priority:** P0 - Critical  
**Estimate:** 21 story points  
**Dependencies:** Task 2.3

**Subtasks:**

- [ ] Setup VNPay Integration
  - [ ] Add VNPay configuration to appsettings.json
  - [ ] Create VNPayHelper utility class
    - [ ] GeneratePaymentUrl (HMAC signature)
    - [ ] VerifySignature (webhook validation)
    - [ ] ParseCallbackData
- [ ] Setup PayOS Integration (optional)
  - [ ] Add PayOS configuration
  - [ ] Create PayOSHelper utility class
- [ ] Implement PaymentService class
  - [ ] ProcessPaymentAsync (create payment URL)
    - [ ] Validate order exists and is Pending
    - [ ] Generate unique PaymentNumber (PAY-YYYYMMDD-XXXXX)
    - [ ] Create Payment record with Status=Pending
    - [ ] Call VNPay/PayOS to generate payment URL
    - [ ] Set ExpiredAt (15-30 minutes)
    - [ ] Store provider transaction ID
  - [ ] HandleVNPayCallbackAsync (webhook handler)
    - [ ] Verify HMAC signature
    - [ ] Validate transaction data
    - [ ] Update Payment status (Success/Failed)
    - [ ] If success:
      - [ ] Update Order (Status=Completed, PaidAt=Now)
      - [ ] Calculate SettlementEligibleAt (PaidAt + 14 days)
      - [ ] Create TransactionLedger (Status=Pending, AvailableAt=SettlementEligibleAt)
      - [ ] Update InstructorWallet (PendingBalance += InstructorEarnings)
      - [ ] Record CouponUsage
      - [ ] Publish OrderCompletedEvent → Learning creates Enrollment
    - [ ] If failed: Update Payment with FailureReason
  - [ ] HandlePayOSCallbackAsync (webhook handler)
  - [ ] ConfirmPaymentAsync (manual confirmation/check)
  - [ ] CheckPaymentStatusAsync (poll payment status)
  - [ ] RefundPaymentAsync (refund logic - future)
  - [ ] GetPaymentsByOrderAsync
  - [ ] GetPaymentsByUserAsync (paginated)
- [ ] Implement webhook endpoints (Minimal API)
  - [ ] POST /api/v1/payments/vnpay/callback
  - [ ] POST /api/v1/payments/payos/callback
- [ ] Write unit tests (minimum 80% coverage)
- [ ] Integration tests with VNPay sandbox
- [ ] Add FluentValidation validators
  - [ ] ProcessPaymentRequestValidator

**Acceptance Criteria:**

- ✅ Payment URLs generated correctly
- ✅ Webhook signature verification works
- ✅ Payment success triggers all downstream actions
- ✅ 14-day escrow logic implemented correctly
- ✅ Failed payments handled gracefully

---

### Task 2.5: Instructor Wallet Service

**Priority:** P1 - High  
**Estimate:** 8 story points  
**Dependencies:** Task 2.4

**Subtasks:**

- [ ] Implement InstructorWalletService class
  - [ ] GetWalletByInstructorAsync (create if not exists)
  - [ ] AddFundsAsync (internal use by PaymentService)
    - [ ] Update PendingBalance or AvailableBalance
    - [ ] Create audit TransactionLedger
  - [ ] DeductFundsAsync (for payouts/refunds)
    - [ ] Validate sufficient balance
    - [ ] Update AvailableBalance
    - [ ] Create audit TransactionLedger
  - [ ] GetWalletTransactionsAsync (paginated transaction history)
- [ ] Implement wallet creation on instructor profile approval
  - [ ] Consume InstructorApprovalEvent (from Identity service)
  - [ ] Create InstructorWallet with default values
- [ ] Write unit tests (minimum 80% coverage)
- [ ] Integration tests with mock data

**Acceptance Criteria:**

- ✅ Wallet auto-created for approved instructors
- ✅ Balance tracking is accurate (3 types: Available, Pending, Hold)
- ✅ All fund movements logged in TransactionLedger

---

### Task 2.6: Transaction Ledger Service

**Priority:** P2 - Medium  
**Estimate:** 5 story points  
**Dependencies:** Task 2.5

**Subtasks:**

- [ ] Implement TransactionService class
  - [ ] CreateTransactionAsync (with polymorphic ReferenceId/Type)
  - [ ] GetTransactionByIdAsync
  - [ ] GetTransactionsByUserAsync (via InstructorWallet)
  - [ ] GetAllTransactionsAsync (admin, paginated)
  - [ ] GetTotalRevenueAsync (date range filter)
- [ ] Write unit tests (minimum 80% coverage)
- [ ] Integration tests with various transaction types

**Acceptance Criteria:**

- ✅ All transactions recorded accurately
- ✅ Balance audit trail maintained (BalanceBefore, BalanceAfter)
- ✅ Revenue calculations correct

---

### Task 2.7: Settlement Service (14-Day Escrow)

**Priority:** P1 - High  
**Estimate:** 13 story points  
**Dependencies:** Task 2.4, Task 2.5, Task 2.6

**Subtasks:**

- [ ] Implement SettlementService class
  - [ ] ProcessPendingSettlementsAsync (background job logic)
    - [ ] Query Orders: IsSettled=false AND SettlementEligibleAt <= Now
    - [ ] For each order:
      - [ ] Update TransactionLedger (Status=Pending → Completed)
      - [ ] Move funds: PendingBalance → AvailableBalance
      - [ ] Update Order (IsSettled=true, SettledAt=Now)
      - [ ] Create Settlement audit log
    - [ ] Return count of settled orders
  - [ ] SettleOrderAsync (manual settle specific order)
  - [ ] GetPendingSettlementsAsync (orders waiting for settlement)
  - [ ] GetSettlementStatusAsync (detailed status for order)
  - [ ] ForceSettleAsync (admin emergency settle)
    - [ ] Add approval reason
    - [ ] Skip 14-day check
  - [ ] GetUpcomingSettlementsAsync (orders eligible in N days)
  - [ ] GetSettlementStatisticsAsync (reporting)
- [ ] Implement Background Service (HostedService)
  - [ ] Create SettlementBackgroundJob class
  - [ ] Run daily at 2:00 AM UTC
  - [ ] Call ProcessPendingSettlementsAsync
  - [ ] Log results (success count, failures)
  - [ ] Send admin notification if errors
- [ ] Write unit tests (minimum 80% coverage)
- [ ] Integration tests simulating 14-day passage
- [ ] Load tests for bulk settlement processing

**Acceptance Criteria:**

- ✅ Background job runs reliably on schedule
- ✅ Settlements processed accurately after 14 days
- ✅ Manual/force settle works for emergencies
- ✅ Statistics provide business insights

---

### Task 2.8: Payout Request Service

**Priority:** P2 - Medium  
**Estimate:** 8 story points  
**Dependencies:** Task 2.5, Task 2.7

**Subtasks:**

- [ ] Implement PayoutService class
  - [ ] CreatePayoutRequestAsync (instructor withdrawal)
    - [ ] Validate AvailableBalance >= request amount
    - [ ] Generate unique RequestNumber (PAYOUT-YYYYMMDD-XXXXX)
    - [ ] Capture bank info snapshot from wallet
    - [ ] Create PayoutRequest with Status=Requested
    - [ ] Deduct from AvailableBalance → HoldBalance
    - [ ] Create TransactionLedger
  - [ ] GetPayoutRequestByIdAsync
  - [ ] ApprovePayoutRequestAsync (admin action)
    - [ ] Update Status=Approved
    - [ ] Record ApprovedBy, ApprovedAt
    - [ ] Trigger external bank transfer (mock for now)
    - [ ] If transfer success: Status=Completed
    - [ ] If transfer fail: Status=Failed, restore HoldBalance
  - [ ] RejectPayoutRequestAsync (admin action)
    - [ ] Update Status=Rejected
    - [ ] Record RejectedBy, RejectedAt, RejectionReason
    - [ ] Restore HoldBalance → AvailableBalance
  - [ ] GetPayoutRequestsAsync (admin view, paginated)
  - [ ] GetPayoutRequestsByInstructorAsync (instructor view)
- [ ] Write unit tests (minimum 80% coverage)
- [ ] Integration tests with approval/rejection flows

**Acceptance Criteria:**

- ✅ Payout requests validated correctly
- ✅ Approval workflow complete
- ✅ Balance movements accurate (Available → Hold → Withdrawn)
- ✅ Rejection restores balance

---

## 📦 PHASE 3: API Endpoints & Validation

### Task 3.1: Coupon API Endpoints

**Priority:** P1 - High  
**Estimate:** 5 story points  
**Dependencies:** Task 2.1, Task 2.2

**Subtasks:**

- [ ] Create CouponEndpoints.cs (Minimal API)
  - [ ] POST /api/v1/coupons (Admin/Instructor)
  - [ ] GET /api/v1/coupons/{code} (Public)
  - [ ] PUT /api/v1/coupons/{id} (Admin/Instructor)
  - [ ] DELETE /api/v1/coupons/{id} (Admin/Instructor)
  - [ ] GET /api/v1/coupons (Admin, paginated)
  - [ ] GET /api/v1/coupons/active (Public, cached)
  - [ ] GET /api/v1/coupons/instructor/{instructorId} (Instructor)
  - [ ] PATCH /api/v1/coupons/{id}/toggle-status (Admin)
  - [ ] POST /api/v1/coupons/validate (Public, validation only)
- [ ] Add rate limiting (Fixed policy)
- [ ] Add authorization policies
- [ ] Add OpenAPI documentation
- [ ] Test all endpoints with Postman/REST Client

**Acceptance Criteria:**

- ✅ All endpoints return correct HTTP status codes
- ✅ Authorization enforced properly
- ✅ Swagger documentation complete

---

### Task 3.2: Order API Endpoints

**Priority:** P0 - Critical  
**Estimate:** 5 story points  
**Dependencies:** Task 2.3

**Subtasks:**

- [ ] Create OrderEndpoints.cs (Minimal API)
  - [ ] POST /api/v1/orders (Authenticated)
  - [ ] GET /api/v1/orders/{id} (Owner/Admin)
  - [ ] PATCH /api/v1/orders/{id}/status (Admin)
  - [ ] POST /api/v1/orders/{id}/cancel (Owner/Admin)
  - [ ] GET /api/v1/orders/my-orders (Authenticated, paginated)
  - [ ] GET /api/v1/orders/instructor/{instructorId} (Instructor/Admin)
  - [ ] GET /api/v1/orders/status/{status} (Admin, paginated)
  - [ ] GET /api/v1/orders/statistics (Admin/Instructor)
- [ ] Add rate limiting
- [ ] Add authorization policies
- [ ] Add OpenAPI documentation
- [ ] Test all endpoints

**Acceptance Criteria:**

- ✅ All endpoints work correctly
- ✅ Authorization prevents unauthorized access
- ✅ Swagger documentation complete

---

### Task 3.3: Payment API Endpoints

**Priority:** P0 - Critical  
**Estimate:** 5 story points  
**Dependencies:** Task 2.4

**Subtasks:**

- [ ] Create PaymentEndpoints.cs (Minimal API)
  - [ ] POST /api/v1/payments/process (Authenticated)
  - [ ] POST /api/v1/payments/vnpay/callback (AllowAnonymous)
  - [ ] POST /api/v1/payments/payos/callback (AllowAnonymous)
  - [ ] GET /api/v1/payments/{id}/status (Authenticated)
  - [ ] GET /api/v1/payments/order/{orderId} (Owner/Admin)
  - [ ] GET /api/v1/payments/my-payments (Authenticated, paginated)
- [ ] Add rate limiting
- [ ] Add authorization policies
- [ ] Add OpenAPI documentation
- [ ] Test webhook endpoints with provider sandbox

**Acceptance Criteria:**

- ✅ Payment flow works end-to-end
- ✅ Webhooks authenticated and validated
- ✅ Error handling for failed payments

---

### Task 3.4: Wallet & Payout API Endpoints

**Priority:** P2 - Medium  
**Estimate:** 3 story points  
**Dependencies:** Task 2.5, Task 2.8

**Subtasks:**

- [ ] Create WalletEndpoints.cs
  - [ ] GET /api/v1/wallets/my-wallet (Instructor)
  - [ ] GET /api/v1/wallets/{instructorId} (Admin)
  - [ ] GET /api/v1/wallets/{instructorId}/transactions (Instructor/Admin, paginated)
- [ ] Create PayoutEndpoints.cs
  - [ ] POST /api/v1/payouts/request (Instructor)
  - [ ] GET /api/v1/payouts/{id} (Instructor/Admin)
  - [ ] POST /api/v1/payouts/{id}/approve (Admin)
  - [ ] POST /api/v1/payouts/{id}/reject (Admin)
  - [ ] GET /api/v1/payouts (Admin, paginated)
  - [ ] GET /api/v1/payouts/my-requests (Instructor, paginated)
- [ ] Add rate limiting
- [ ] Add authorization policies
- [ ] Add OpenAPI documentation

**Acceptance Criteria:**

- ✅ Wallet balance visible to instructor
- ✅ Payout workflow complete
- ✅ Admin can approve/reject

---

### Task 3.5: Settlement API Endpoints (Admin Only)

**Priority:** P2 - Medium  
**Estimate:** 3 story points  
**Dependencies:** Task 2.7

**Subtasks:**

- [ ] Create SettlementEndpoints.cs
  - [ ] POST /api/v1/settlements/process (Admin, manual trigger)
  - [ ] POST /api/v1/settlements/{orderId}/settle (Admin)
  - [ ] POST /api/v1/settlements/{orderId}/force-settle (Admin)
  - [ ] GET /api/v1/settlements/pending (Admin, paginated)
  - [ ] GET /api/v1/settlements/{orderId}/status (Admin/Instructor)
  - [ ] GET /api/v1/settlements/upcoming (Admin)
  - [ ] GET /api/v1/settlements/statistics (Admin)
- [ ] Add rate limiting
- [ ] Add authorization (Admin only)
- [ ] Add OpenAPI documentation

**Acceptance Criteria:**

- ✅ Admin can manually trigger settlements
- ✅ Statistics visible
- ✅ Force settle requires reason

---

## 📦 PHASE 4: Event-Driven Integration

### Task 4.1: Event Definitions

**Priority:** P1 - High  
**Estimate:** 2 story points  
**Dependencies:** None

**Subtasks:**

- [ ] Create event classes in Beyond8.Common.Events.Sale
  - [ ] OrderCreatedEvent
  - [ ] OrderCompletedEvent (with OrderItemData list)
  - [ ] OrderCancelledEvent
  - [ ] OrderRefundedEvent (future)
  - [ ] SettlementCompletedEvent
  - [ ] PayoutRequestedEvent
  - [ ] PayoutCompletedEvent

---

### Task 4.2: Event Publishers

**Priority:** P1 - High  
**Estimate:** 3 story points  
**Dependencies:** Task 4.1, Task 2.3, Task 2.4

**Subtasks:**

- [ ] Publish OrderCompletedEvent after payment success
- [ ] Publish OrderCancelledEvent on cancellation
- [ ] Publish SettlementCompletedEvent after settlement
- [ ] Publish PayoutCompletedEvent after payout
- [ ] Add retry policy for event publishing (MassTransit)

---

### Task 4.3: Event Consumers

**Priority:** P1 - High  
**Estimate:** 5 story points  
**Dependencies:** Task 4.1

**Subtasks:**

- [ ] Create FreeEnrollmentOrderRequestConsumer
  - [ ] Consume event from Learning service
  - [ ] Create Order with Amount=0
  - [ ] Set Status=Completed immediately
  - [ ] Skip payment step
- [ ] Create InstructorApprovalEventConsumer
  - [ ] Create InstructorWallet on approval
- [ ] Add error handling and retry policies
- [ ] Test event flow with mock publishers

**Acceptance Criteria:**

- ✅ Free enrollment creates order correctly
- ✅ Wallet created when instructor approved
- ✅ Failed events retried automatically

---

## 📦 PHASE 5: Testing & Quality Assurance

### Task 5.1: Unit Tests

**Priority:** P1 - High  
**Estimate:** 13 story points  
**Dependencies:** Tasks 2.1-2.8

**Subtasks:**

- [ ] CouponService tests (minimum 80% coverage)
- [ ] CouponUsageService tests (minimum 80% coverage)
- [ ] OrderService tests (minimum 80% coverage)
- [ ] PaymentService tests (minimum 80% coverage)
- [ ] InstructorWalletService tests (minimum 80% coverage)
- [ ] TransactionService tests (minimum 80% coverage)
- [ ] SettlementService tests (minimum 80% coverage)
- [ ] PayoutService tests (minimum 80% coverage)
- [ ] Mock all external dependencies (Catalog client, VNPay API)

---

### Task 5.2: Integration Tests

**Priority:** P1 - High  
**Estimate:** 8 story points  
**Dependencies:** Tasks 2.1-2.8, Phase 3

**Subtasks:**

- [ ] End-to-end order flow test (create → pay → settle → payout)
- [ ] Coupon validation scenarios
- [ ] Payment webhook handling tests
- [ ] Settlement background job test
- [ ] Event publishing/consuming tests
- [ ] Use test database with migrations

---

### Task 5.3: Load & Performance Tests

**Priority:** P2 - Medium  
**Estimate:** 5 story points  
**Dependencies:** Phase 3, Task 5.1, Task 5.2

**Subtasks:**

- [ ] Load test order creation (1000 orders/minute)
- [ ] Load test payment confirmations (concurrent webhooks)
- [ ] Performance test settlement processing (10,000 orders)
- [ ] Database query optimization based on results
- [ ] Cache optimization for coupon lookups

---

## 📦 PHASE 6: Documentation & Deployment

### Task 6.1: API Documentation

**Priority:** P2 - Medium  
**Estimate:** 3 story points  
**Dependencies:** Phase 3

**Subtasks:**

- [ ] Complete OpenAPI/Swagger documentation
- [ ] Add XML comments to all endpoints
- [ ] Create Postman collection with examples
- [ ] Write API usage guide in docs/

---

### Task 6.2: Architecture Documentation

**Priority:** P2 - Medium  
**Estimate:** 2 story points  
**Dependencies:** Phase 2, Phase 4

**Subtasks:**

- [ ] Update CONCEPTUAL_DATA_MODEL.md
- [ ] Update sale-service-implementation-plan.md
- [ ] Create sequence diagrams for key flows
  - [ ] Order → Payment → Settlement → Payout
  - [ ] Free enrollment flow
  - [ ] Refund flow (future)
- [ ] Document event-driven architecture

---

### Task 6.3: Deployment Configuration

**Priority:** P1 - High  
**Estimate:** 3 story points  
**Dependencies:** Phase 2, Phase 3

**Subtasks:**

- [ ] Configure Sale service in AppHost
- [ ] Add database connection strings
- [ ] Configure VNPay/PayOS credentials (User Secrets)
- [ ] Configure RabbitMQ for events
- [ ] Configure background job schedule
- [ ] Create docker-compose for local development
- [ ] Health check endpoints

---

## 📊 Summary

| Phase                           | Tasks        | Total Story Points | Priority |
| ------------------------------- | ------------ | ------------------ | -------- |
| Phase 1: Foundation (DONE)      | 4            | ~13 SP             | P0       |
| Phase 2: Core Services          | 8            | 81 SP              | P0-P2    |
| Phase 3: API Endpoints          | 5            | 21 SP              | P0-P2    |
| Phase 4: Event Integration      | 3            | 10 SP              | P1       |
| Phase 5: Testing                | 3            | 26 SP              | P1-P2    |
| Phase 6: Documentation & Deploy | 3            | 8 SP               | P1-P2    |
| **TOTAL**                       | **26 tasks** | **159 SP**         | -        |

**Estimated Sprint Duration:** 6-8 sprints (2 weeks each) = 12-16 weeks

---

## 🚀 Quick Start Checklist (For ClickUp Import)

**Epic:** Sale Service Implementation  
**Sprint 1 (Weeks 1-2):** Tasks 2.1, 2.2, 2.3  
**Sprint 2 (Weeks 3-4):** Tasks 2.4, 3.1, 3.2  
**Sprint 3 (Weeks 5-6):** Tasks 2.5, 2.6, 3.3, 4.1  
**Sprint 4 (Weeks 7-8):** Tasks 2.7, 3.5, 4.2, 4.3  
**Sprint 5 (Weeks 9-10):** Tasks 2.8, 3.4, 5.1  
**Sprint 6 (Weeks 11-12):** Tasks 5.2, 5.3, 6.1, 6.2, 6.3

---

## 📝 Notes for Project Manager

1. **Critical Path:** Phase 2 (Core Services) blocks everything else
2. **Parallelization:** Tasks 2.1-2.2 can run parallel; Task 3.x can start after corresponding Task 2.x
3. **Risk:** VNPay integration testing requires sandbox access (obtain early)
4. **Dependencies:** Catalog service must expose course info API
5. **Technical Debt:** Refund logic marked as future work (commented out in entities)

**Questions/Blockers:**

- [ ] VNPay sandbox credentials obtained?
- [ ] Catalog service API contract agreed?
- [ ] Learning service consumer for OrderCompletedEvent ready?
