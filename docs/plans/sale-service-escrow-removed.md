# Sale Service - Escrow/Settlement Logic Removed (Phase 2)

> **Ngày thay đổi**: 2026-02-06  
> **Lý do**: Đơn giản hóa flow thanh toán Phase 2 — thanh toán xong chuyển tiền cho instructor ngay lập tức, không giữ escrow 14 ngày.  
> **Kế hoạch**: Sau khi hoàn thiện Phase 2, sẽ re-implement escrow/settlement logic theo BR-05, BR-19.

---

## Tổng Quan Thay Đổi

**Trước**: Payment Success → Escrow 14 ngày (PendingBalance) → Settlement Job → AvailableBalance → Payout  
**Sau**: Payment Success → Credit trực tiếp vào AvailableBalance → Payout

---

## Chi Tiết Thay Đổi

### 1. Order.cs (Entity)

**Đã xóa các field:**

```csharp
// Settlement (14-day escrow logic)
public DateTime? SettlementEligibleAt { get; set; }  // PaidAt + 14 days
public bool IsSettled { get; set; } = false;
public DateTime? SettledAt { get; set; }

// Refund Information
// public decimal TotalRefunded { get; set; } = 0;
```

**Khi re-implement cần thêm lại:**

- `SettlementEligibleAt` = `PaidAt + 14 days` (per BR-19)
- `IsSettled` flag để track settlement status
- `SettledAt` timestamp
- Index: `SettlementEligibleAt` với filter `IsSettled = false`

---

### 2. OrderItem.cs (Entity)

**Đã sửa:**

```csharp
// Trước: PlatformFeePercent = 20 (sai)
// Sau:   PlatformFeePercent = 30 (đúng per BR-19: 70% instructor, 30% platform)
```

> ⚠️ Không cần revert — đây là bug fix, không phải escrow logic.

---

### 3. InstructorWallet.cs (Entity)

**Đã xóa các field (3-tier → 1-tier):**

```csharp
// Escrow balance (tiền đang giữ 14 ngày, không rút được)
[Column(TypeName = "decimal(18, 2)")]
public decimal PendingBalance { get; set; } = 0;

// Hold balance (tiền đang xử lý payout)
[Column(TypeName = "decimal(18, 2)")]
public decimal HoldBalance { get; set; } = 0;

// Refund Statistics
// public decimal TotalRefunded { get; set; } = 0;
```

**Khi re-implement cần thêm lại:**

- `PendingBalance`: Tiền escrow chưa settle (credited khi payment success, moved to Available sau 14 ngày)
- `HoldBalance`: Tiền đang xử lý payout (moved from Available khi payout approved, deducted khi payout completed)
- Flow: Payment → PendingBalance ↑ → (14 days) → AvailableBalance ↑ → Payout → HoldBalance ↑ → TotalWithdrawn ↑

---

### 4. TransactionLedger.cs (Entity)

**Đã xóa:**

```csharp
// Escrow release date
public DateTime? AvailableAt { get; set; }
```

**Đã sửa:**

```csharp
// Trước: default Pending (chờ escrow settle)
public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

// Sau: default Completed (credit ngay lập tức)
public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
```

**Khi re-implement:**

- Thêm lại `AvailableAt` = `Order.SettlementEligibleAt`
- Default status lại `Pending` (settlement job sẽ update thành Completed)
- Thêm lại index: `AvailableAt` với filter `Status = 0 (Pending)`

---

### 5. TransactionType.cs (Enum)

**Đã xóa:**

```csharp
Settlement = 3  // Chuyển PendingBalance → AvailableBalance sau 14 ngày
```

**Khi re-implement:** Thêm lại `Settlement` type.

---

### 6. SaleDbContext.cs (Infrastructure)

**Đã xóa 2 indexes:**

```csharp
// Order: Settlement eligibility index
entity.HasIndex(e => e.SettlementEligibleAt)
    .HasFilter("\"IsSettled\" = false AND \"SettlementEligibleAt\" IS NOT NULL");

// TransactionLedger: Pending escrow index
entity.HasIndex(e => e.AvailableAt)
    .HasFilter("\"Status\" = 0 AND \"AvailableAt\" IS NOT NULL");
```

---

### 7. Files Đã Xóa Hoàn Toàn

| File                                                   | Mục đích                                        |
| ------------------------------------------------------ | ----------------------------------------------- |
| `Dtos/Settlements/SettlementStatusResponse.cs`         | DTO tracking settlement status per order        |
| `Dtos/Settlements/SettlementStatisticsResponse.cs`     | DTO thống kê settlement (by instructor, by day) |
| `Services/Interfaces/ISettlementService.cs`            | Interface settlement service (7 methods)        |
| `Dtos/Payments/RefundPaymentRequest.cs`                | DTO refund (Phase 3)                            |
| `Dtos/Payments/PayOSCallbackRequest.cs`                | DTO PayOS (out of scope)                        |
| `Validators/Payments/RefundPaymentRequestValidator.cs` | Validator refund (Phase 3)                      |

**Khi re-implement cần tạo lại:**

- `ISettlementService` với methods: `ProcessPendingSettlementsAsync`, `SettleOrderAsync`, `GetPendingSettlementsAsync`, `GetSettlementStatusAsync`, `ForceSettleAsync`, `GetUpcomingSettlementsAsync`, `GetSettlementStatisticsAsync`
- Settlement DTOs
- Background job (IHostedService hoặc Hangfire) chạy daily 2:00 AM UTC

---

### 8. Interfaces Đã Sửa

**IPaymentService.cs** — Xóa 2 methods:

```csharp
Task<ApiResponse<bool>> HandlePayOSCallbackAsync(PayOSCallbackRequest request);  // Out of scope
Task<ApiResponse<bool>> RefundPaymentAsync(Guid orderId, RefundPaymentRequest request);  // Phase 3
```

**IInstructorWalletService.cs** — Thay đổi hoàn toàn:

```csharp
// Trước (generic add/deduct):
Task<ApiResponse<bool>> AddFundsAsync(Guid instructorId, decimal amount, string description);
Task<ApiResponse<bool>> DeductFundsAsync(Guid instructorId, decimal amount, string description);

// Sau (specific operations, no escrow):
Task<ApiResponse<bool>> CreditEarningsAsync(Guid instructorId, decimal amount, Guid orderId, string description);
Task<ApiResponse<bool>> DeductForPayoutAsync(Guid instructorId, decimal amount, Guid payoutId, string description);
Task<ApiResponse<InstructorWalletResponse>> CreateWalletAsync(Guid instructorId);
```

**Khi re-implement:** Thêm methods cho escrow flow:

- `CreditPendingBalanceAsync` (payment success → PendingBalance ↑)
- `SettleToAvailableAsync` (settlement → PendingBalance ↓, AvailableBalance ↑)
- `HoldForPayoutAsync` (payout approved → AvailableBalance ↓, HoldBalance ↑)
- `CompletePayoutAsync` (payout completed → HoldBalance ↓, TotalWithdrawn ↑)
- `ReleaseHoldAsync` (payout rejected → HoldBalance ↓, AvailableBalance ↑)

**ITransactionService.cs** — Sửa nhỏ:

```csharp
// Trước: GetTransactionsByUserAsync(Guid userId, ...)
// Sau:   GetTransactionsByWalletAsync(Guid walletId, ...)
```

---

### 9. DTOs Đã Sửa

**OrderResponse.cs** — Thêm fields thiếu:

- `OrderNumber`, `SubTotal`, `DiscountAmount`, `Currency`, `CouponId`, `PaidAt`, `Notes`

**OrderItemResponse.cs** — Thêm revenue fields:

- `CourseThumbnail`, `InstructorId`, `InstructorName`, `OriginalPrice`, `UnitPrice`, `DiscountPercent`, `LineTotal`, `PlatformFeePercent`, `PlatformFeeAmount`, `InstructorEarnings`

**InstructorWalletResponse.cs** — Match entity mới:

- Bỏ `Balance` (generic) → dùng `AvailableBalance` (specific)
- Thêm `Id`, `Currency`, `LastPayoutAt`, `IsActive`

**WalletTransactionResponse.cs** — Match TransactionLedger entity:

- Thêm `WalletId`, `Status`, `Currency`, `BalanceBefore`, `BalanceAfter`, `ReferenceId`, `ReferenceType`, `ExternalTransactionId`

**TransactionLedgerResponse.cs** — Match entity:

- Thay `UserId` → `WalletId`, thêm `Status`, `Currency`, `BalanceBefore`, `BalanceAfter`, `ReferenceId`, `ReferenceType`

**CreateTransactionRequest.cs** — Match entity:

- Thay `UserId` → `WalletId`, `string Type` → `TransactionType Type` (enum)

**OrderStatisticsResponse.cs** — Xóa `PendingSettlements`

---

## Checklist Re-implement Escrow (Phase 3)

- [ ] Thêm lại fields vào `Order.cs` (SettlementEligibleAt, IsSettled, SettledAt)
- [ ] Thêm lại `PendingBalance`, `HoldBalance` vào `InstructorWallet.cs`
- [ ] Thêm lại `AvailableAt` vào `TransactionLedger.cs`, default status → `Pending`
- [ ] Thêm lại `Settlement` vào `TransactionType` enum
- [ ] Thêm lại indexes trong `SaleDbContext.cs`
- [ ] Tạo lại `ISettlementService` + implementation
- [ ] Tạo lại Settlement DTOs
- [ ] Tạo background job (daily 2:00 AM UTC)
- [ ] Update `IInstructorWalletService` cho 3-tier balance flow
- [ ] Thêm lại `PendingSettlements` vào `OrderStatisticsResponse`
- [ ] Tạo EF Core migration cho schema changes
- [ ] Test settlement flow end-to-end
- [ ] Thêm refund logic (RefundPaymentRequest, validator, IPaymentService.RefundPaymentAsync)

---

## Business Rules Tham Chiếu

- **BR-05**: Refund policy — 14 days, <10% progress
- **BR-19**: Revenue split — 70% Instructor, 30% Platform, 14-day escrow, min 500k payout
- **NFR-07.01**: Security — Checksum verification, Idempotency
- **NFR-07.02**: Financial accuracy — Decimal type, ACID transactions
