# MODULE 07: THANH TOÁN & ĐĂNG KÝ (PAYMENT & ENROLLMENT)

**Module ID:** MOD-07
**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Phụ Thuộc:** MOD-01 (User), MOD-02 (Course), MOD-10 (Revenue - Ví)

---

## Yêu Cầu Chức Năng

### REQ-07.01: Enroll Khóa Học Miễn Phí

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student
**Mô Tả:** Đăng ký vào khóa học có giá 0đ

**Tiêu Chí Chấp Nhận:**

- **Trigger:** Click "Enroll Free" trên trang chi tiết.
- **Xử lý:**
  - Tạo bản ghi `Enrollment` (status: active).
  - Tạo `Order` giá trị 0đ để lưu lịch sử.
  - Snapshot dữ liệu khóa học vào `OrderItem`.
- **Kết quả:** Chuyển hướng ngay vào màn hình học.

**Quy Tắc Nghiệp Vụ:** BR-04

---

### REQ-07.02: Thanh Toán Qua VNPay

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student
**Mô Tả:** Mua khóa học trả phí qua cổng thanh toán

**Tiêu Chí Chấp Nhận:**

- **Checkout:**
  - Hiển thị thông tin đơn hàng: Tên khóa học, Giá gốc, Coupon (nếu có), Thành tiền.
  - Chọn phương thức: Thẻ ATM/Visa/QR (VNPay).
- **Processing:**
  - Tạo `Order` (status: pending).
  - Chuyển hướng sang VNPay Gateway.
- **Callback (IPN):**
  - Nhận kết quả từ VNPay.
  - Nếu thành công:
    - Update `Order` status = `Paid`.
    - Tạo `Enrollment`.
    - Ghi nhận `Transaction` vào Ví Admin (Tiền vào sàn).
    - **Snapshot:** Lưu tên giảng viên, tên khóa học vào `OrderItem` (để report sau này).
  - Nếu thất bại: Update `Order` status = `Failed`.

**Quy Tắc Nghiệp Vụ:** BR-11, BR-19

---

### REQ-07.03: Áp Dụng Mã Giảm Giá (Coupon)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student
**Mô Tả:** Sử dụng mã giảm giá khi thanh toán

**Tiêu Chí Chấp Nhận:**

- Nhập mã vào ô "Coupon Code".
- **Validation:** Kiểm tra mã tồn tại, còn hạn, còn lượt dùng.
- **Tính toán:** Trừ tiền theo % hoặc số tiền cố định.
- **Lưu:** Ghi nhận mã coupon đã dùng vào đơn hàng.

---

### REQ-07.04: Lịch Sử Giao Dịch (Student)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student
**Mô Tả:** Xem lại các khóa học đã mua

**Tiêu Chí Chấp Nhận:**

- Danh sách đơn hàng (Ngày mua, Số tiền, Trạng thái).
- Xem chi tiết đơn hàng (Invoice View).
- Trạng thái hoàn tiền (nếu có).

---

### REQ-07.06: Yêu Cầu Hoàn Tiền (Refund) - Updated

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student
**Mô Tả:** Yêu cầu hoàn tiền trong thời hạn quy định

**Tiêu Chí Chấp Nhận:**

- **Điều kiện (Auto-check):**
  - Thời gian mua <= 14 ngày (BR-05).
  - Tiến độ học < 10%.
- **Quy trình:**
  - Student gửi request kèm lý do.
  - Admin duyệt trên Dashboard.
  - Nếu Admin Approve:
    - Gọi API hoàn tiền VNPay.
    - Hủy `Enrollment` (Status: `Refunded`).
    - Trừ tiền tạm giữ của Instructor (xem REQ-07.09).

**Quy Tắc Nghiệp Vụ:** BR-05 (14 ngày)

---

### REQ-07.09: Quản Lý Ví & Rút Tiền (Instructor Payout) - NEW

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Quản lý doanh thu và rút tiền về ngân hàng

**Tiêu Chí Chấp Nhận:**

- **Dashboard Ví (InstructorWallet):**
  - Hiển thị: Số dư khả dụng (`Available`), Số dư chờ duyệt (`Pending`).
  - Lịch sử biến động số dư (Ledger).
- **Cơ chế Escrow (14 ngày):**
  - Khi có đơn hàng mới: Tiền cộng vào `Pending Balance`.
  - Sau 14 ngày (Job chạy): Tiền chuyển từ `Pending` sang `Available`.
- **Yêu Cầu Rút Tiền (Payout Request):**
  - Nhập số tiền muốn rút (<= Available Balance).
  - Hệ thống tạo yêu cầu `PayoutRequest` (Status: Requested).
  - Admin duyệt -> Chuyển khoản -> Status: Completed.

**Quy Tắc Nghiệp Vụ:** BR-19

---

## Yêu Cầu Phi Chức Năng

### NFR-07.01: Bảo Mật Thanh Toán

- **Checksum:** Kiểm tra chữ ký điện tử (Checksum) của mọi request từ VNPay để chống giả mạo.
- **Idempotency:** Đảm bảo xử lý IPN (Callback) không bị trùng lặp (xử lý 1 đơn hàng nhiều lần).

### NFR-07.02: Độ Chính Xác Tài Chính

- Sử dụng kiểu dữ liệu `Decimal` (không dùng Float) cho tiền tệ.
- Giao dịch Ví (Wallet Transaction) phải tuân thủ ACID (Không được phép mất tiền hoặc âm tiền vô lý).

---

## Ma Trận Truy Xuất

| Yêu Cầu   | Use Case(s) | Business Rule | Module Dependency |
| :-------- | :---------- | :------------ | :---------------- |
| REQ-07.01 | UC-07.01    | BR-04         | Sales Service     |
| REQ-07.02 | UC-07.02    | BR-11         | Sales Service     |
| REQ-07.06 | UC-07.06    | BR-05         | Sales Service     |
| REQ-07.09 | UC-10.xx    | BR-19         | Sales Service     |
