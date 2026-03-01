# MODULE 09: TRANG QUẢN TRỊ (ADMIN PANEL)

**Module ID:** MOD-09
**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Phụ Thuộc:** Tất cả các module khác (Analytics Service, Sales Service, Catalog Service)

---

## Yêu Cầu Chức Năng

### REQ-09.01: Admin Dashboard Overview

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Dashboard tổng quan sức khỏe hệ thống

**Tiêu Chí Chấp Nhận:**

- **Nguồn dữ liệu:** `AggSystemOverview` (Analytics Service).
- **Thẻ Metrics Tài Chính (Quan trọng):**
  - Tổng Doanh Thu (GMV).
  - Doanh Thu Thực (Net Revenue - Phần của sàn).
  - **Tiền Đang Giữ (Escrow Balance):** Tổng tiền đang nằm trong 14 ngày chờ.
  - **Chờ Duyệt Chi (Pending Payout):** Tổng tiền giảng viên đang yêu cầu rút.
- **Thẻ Metrics Vận Hành:**
  - User mới / Active User.
  - Khóa học chờ duyệt.
  - Report chưa xử lý.
- **Biểu đồ:** Tăng trưởng User, Doanh thu theo ngày.

**Quy Tắc Nghiệp Vụ:** BR-01, BR-19

---

### REQ-09.02: Cấu Hình Hệ Thống (System Settings)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Cài đặt tham số vận hành toàn sàn

**Tiêu Chí Chấp Nhận:**

- **Cấu hình Tài chính (BR-19):**
  - Tỷ lệ chia sẻ doanh thu (Mặc định: Instructor 70% - Platform 30%).
  - Hạn mức rút tiền tối thiểu (Mặc định: 500.000 VNĐ).
  - Thời gian giữ tiền (Escrow): Mặc định 14 ngày.
- **Cấu hình Học tập:**
  - Điểm đạt Quiz mặc định.
  - Dung lượng upload tối đa.
- **Lưu trữ:** Các thay đổi được log lại (Audit Log).

---

### REQ-09.03: Báo Cáo & Phân Tích (Analytics)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Phân tích sâu dữ liệu hệ thống

**Tiêu Chí Chấp Nhận:**

- **Báo cáo Tài chính:**
  - Chi tiết dòng tiền (Cashflow).
  - Top Giảng viên có doanh thu cao nhất.
  - Tỷ lệ hoàn tiền (Refund Rate).
- **Báo cáo Nội dung:**
  - Các khóa học "Chết" (Không có lượt xem).
  - Từ khóa tìm kiếm phổ biến.
- **Xuất dữ liệu:** PDF/Excel.

---

### REQ-09.04: Quản Lý Rút Tiền (Payout Management) - NEW

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Duyệt các yêu cầu rút tiền từ Instructor

**Tiêu Chí Chấp Nhận:**

- **Danh sách yêu cầu:** Lấy từ bảng `PayoutRequest` (Sales Service) có status `Requested`.
- **Thông tin hiển thị:**
  - Tên giảng viên.
  - Số tiền yêu cầu.
  - Thông tin ngân hàng (Snapshot lúc yêu cầu).
  - Số dư khả dụng hiện tại (để đối chiếu).
- **Hành động:**
  - **Approve (Đã chuyển khoản):** Admin xác nhận đã chuyển tiền thủ công -> Hệ thống trừ tiền trong ví -> Gửi email thông báo.
  - **Reject (Từ chối):** Nhập lý do -> Hoàn lại tiền vào ví -> Gửi email.

**Quy Tắc Nghiệp Vụ:** BR-19

---

### REQ-09.05: Kiểm Duyệt Nội Dung (Moderation) - NEW

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Xử lý các báo cáo vi phạm (Review, Khóa học)

**Tiêu Chí Chấp Nhận:**

- **Review Reports:**
  - Danh sách các review bị user báo cáo (`ReviewReport` từ Catalog Service).
  - Hành động: **Xóa Review** (Ẩn) hoặc **Bỏ qua Report**.
- **Course Moderation:**
  - Danh sách khóa học bị report (nếu có tính năng report khóa học).
  - Khóa học đang chờ duyệt (`PendingApproval`).

**Quy Tắc Nghiệp Vụ:** BR-12, BR-03

---

### REQ-09.06: Quản Lý Người Dùng & Phân Quyền

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** (Giữ nguyên logic cũ nhưng bổ sung Wallet View)

**Tiêu Chí Chấp Nhận:**

- Danh sách User.
- **Chi tiết User (Instructor):** Xem thêm tab "Ví tiền" (Số dư hiện tại, Lịch sử giao dịch) để hỗ trợ support.
- Action: Ban/Unban, Reset Password.

---

## Yêu Cầu Phi Chức Năng

### NFR-09.01: Bảo Mật Cao

- Các hành động liên quan đến tiền (Duyệt Payout, Đổi tỷ lệ hoa hồng) yêu cầu xác thực 2 lớp (2FA) hoặc nhập lại mật khẩu Admin.
- IP Whitelist: Chỉ cho phép truy cập Admin Panel từ các IP nội bộ (nếu cần).

### NFR-09.02: Audit Log (Nhật Ký Hệ Thống)

- Ghi lại **mọi** hành động của Admin:
  - Ai đã duyệt khóa học X?
  - Ai đã từ chối lệnh rút tiền Y?
  - Ai đã đổi cấu hình hệ thống?
- Log không được phép xóa/sửa.

---

## Ma Trận Truy Xuất

| Requirement ID | Use Case ID | Business Rule | Module Dependency |
| :------------- | :---------- | :------------ | :---------------- |
| REQ-09.01      | UC-09.01    | BR-19         | Analytics Service |
| REQ-09.02      | UC-09.02    | BR-19         | Sales Service     |
| REQ-09.04      | UC-09.04    | BR-19         | Sales Service     |
| REQ-09.05      | UC-09.05    | BR-12, BR-03  | Catalog Service   |
