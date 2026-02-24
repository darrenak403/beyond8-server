## TÓM TẮT THAY ĐỔI (VIỆT) — Sale Service: Escrow & Settlement

Mục tiêu: áp lại cơ chế giữ tiền giảng viên 14 ngày (escrow) và tự động giải ngân sau 14 ngày; hỗ trợ tùy chọn giữ phí nền tảng (PoC) và đảm bảo tính chất atomic, idempotent cho quá trình release.

1. Những thay đổi chính (ngắn gọn)

- Schema & Domain:
  - `Order`: thêm `SettlementEligibleAt`, `IsSettled`, `SettledAt`.
  - `InstructorWallet`: thêm `PendingBalance` (tiền đang giữ cho instructor).
  - `TransactionLedger`: thêm `AvailableAt` (thời điểm release) và `Status` (Pending/Completed).
  - `TransactionType`: thêm `Settlement`.
  - `PlatformWallet` (PoC): thêm `PendingBalance`; `PlatformWalletTransaction` có `AvailableAt`.

2. Luồng xử lý (ngắn gọn)

- Khi payment thành công:
  - Ghi platform fee (30%) vào `PlatformWallet.AvailableBalance` theo mặc định (hoặc vào pending nếu bật PoC).
  - Ghi instructor share (70%) vào `InstructorWallet.PendingBalance` và tạo `TransactionLedger` với `Status = Pending` và `AvailableAt = PaidAt + 14 days`.
- Release sau 14 ngày: `SettlementBackgroundService` gọi `SettlementService.ProcessPendingSettlementsAsync()`
  - Job query các transaction `Type == Sale && Status == Pending && AvailableAt <= now`.
  - Với mỗi transaction: xử lý trong DB transaction — re-query và lock → giảm `PendingBalance`, tăng `AvailableBalance` → tạo transaction `Settlement` (ghi audit, BalanceBefore/After) → đánh dấu transaction gốc `Completed` → nếu liên quan thì `Order.IsSettled = true` → commit → publish `SettlementCompletedEvent`.
  - Sau đó job xử lý các `PlatformWalletTransaction` pending tương tự (nếu PoC bật).

3. API / DTO / Mapping

- Endpoint admin: `GET /api/v1/settlements/upcoming` (filter by from/to) — trả `UpcomingSettlementResponse` (TransactionId, WalletId, OrderId, Amount, Currency, AvailableAt, CreatedAt).
- `InstructorWalletResponse` expose `PendingBalance`.
- `TransactionLedgerResponse` và `PlatformWalletTransactionResponse` expose `AvailableAt`.

4. Migrations & running

- Đã có migration `20260224161702_ReintroduceSettlement` (thêm cột, + SQL mark các order lịch sử đã settled để tránh double-credit). Kiểm tra kỹ SQL trước khi áp lên production.
- Sau khi thay đổi domain cho platform PoC, tạo migration mới: `PlatformWalletPendingAndAvailableAt` và chạy:

```bash
cd src/Services/Sale/Beyond8.Sale.Infrastructure
dotnet ef migrations add PlatformWalletPendingAndAvailableAt
dotnet ef database update

cd src/Orchestration/Beyond8.AppHost
dotnet run
```

5. Kiểm tra nhanh (SQL)

- Orders ready to settle:

```sql
SELECT COUNT(*) FROM "Orders" WHERE "IsSettled" = false AND "SettlementEligibleAt" <= now();
```

- Pending transactions available now:

```sql
SELECT * FROM "TransactionLedgers" WHERE "Status" = 0 /*Pending*/ AND "AvailableAt" <= now() ORDER BY "AvailableAt" LIMIT 50;
```

6. Rủi ro & khuyến nghị

- Refunds: cần định nghĩa rõ rollback cho pending txs nếu xảy ra refund trước khi release.
- Reconciliation: implement job so sánh `InstructorWallet.PendingBalance` vs tổng pending `TransactionLedger` cho mỗi wallet.
- Migration caution: `ReintroduceSettlement` chứa SQL mark-order — review before production. Run migrations on staging first.

7. File chính (tham khảo)

- `src/Services/Sale/Beyond8.Sale.Application/Services/Implements/SettlementService.cs`
- `src/Services/Sale/Beyond8.Sale.Application/Services/Implements/SettlementBackgroundService.cs`
- `src/Services/Sale/Beyond8.Sale.Application/Services/Implements/PaymentService.cs`
- `src/Services/Sale/Beyond8.Sale.Application/Services/Implements/InstructorWalletService.cs`
- `src/Services/Sale/Beyond8.Sale.Application/Services/Implements/PlatformWalletService.cs`
- `src/Services/Sale/Beyond8.Sale.Infrastructure/Migrations/20260224161702_ReintroduceSettlement.cs`

8. Next actions (tùy bạn chọn)

- (A) Tôi tạo migration `PlatformWalletPendingAndAvailableAt` và commit.
- (B) Tôi viết unit/integration tests cho flow VNPay → Pending → Settlement (bao gồm refund case).
- (C) Tôi thêm SQL audit queries + Postman collection để test nhanh.

Ngắn gọn: escrow instructor đã được tái áp dụng (PendingBalance + AvailableAt + job release). Platform fee hiện default vào Available nhưng có PoC để giữ vào Pending và release tương tự. Tôi đang tiến hành ghi lại tài liệu này (xong). Muốn tôi làm tiếp (A)/(B)/(C)?
