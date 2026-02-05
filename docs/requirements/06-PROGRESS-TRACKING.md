# MODULE 06: THEO DÕI TIẾN ĐỘ & THỐNG KÊ (PROGRESS TRACKING)

**Module ID:** MOD-06
**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Phụ Thuộc:** MOD-03 (Video), MOD-04 (Quiz), Analytics Service

---

## Yêu Cầu Chức Năng

### REQ-06.01: Theo Dõi Tiến Độ Video (Heartbeat)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** System (Player Client)
**Mô Tả:** Ghi nhận vị trí xem video và trạng thái hoàn thành

**Tiêu Chí Chấp Nhận:**

- **Cơ chế Heartbeat:** Client gửi request mỗi 30 giây về `Learning Service`.
  - Data: `lesson_id`, `position` (giây).
- **Lưu trữ:** Cập nhật vào bảng `LessonProgress`.
- **Logic Hoàn thành:**
  - Tự động đánh dấu `completed` nếu `position >= 95% duration`.
  - Hoặc khi User bấm nút "Mark as Complete".
- **Resume:** Khi User mở lại bài học, API trả về `last_position` để Player tua đến đúng đoạn đó.

**Quy Tắc Nghiệp Vụ:** BR-14

---

### REQ-06.02: Tracking Quiz & Assignment

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** System
**Mô Tả:** Cập nhật trạng thái bài học khi nộp bài tập

**Tiêu Chí Chấp Nhận:**

- **Trigger:** Ngay sau khi Student nộp bài (Submit Quiz/Assignment).
- **Logic:**
  - Quiz: Nếu `is_passed = true` -> Update Lesson Status = `Completed`.
  - Assignment: Nếu `status = submitted` -> Update Lesson Status = `Completed` (hoặc chờ chấm tùy cấu hình).
- **Tính toán:** Service bắn Event `LessonCompleted` để Analytics Service tính lại % hoàn thành khóa học.

---

### REQ-06.03: Dashboard Học Tập (My Learning)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student
**Mô Tả:** Hiển thị danh sách và tiến độ các khóa học đã mua

**Tiêu Chí Chấp Nhận:**

- Dữ liệu lấy từ bảng `Enrollment` và `AggCourseStats` (cá nhân hóa).
- Hiển thị:
  - Thanh tiến độ (Progress Bar %).
  - Trạng thái: Đang học / Đã hoàn thành.
  - Button "Tiếp tục học" (Resume) dẫn đến bài học dang dở gần nhất.
- Sắp xếp: Truy cập gần nhất.

---

### REQ-06.05: Cấp Chứng Chỉ (Certificate)

**Mức Độ Ưu Tiên:** Có Nếu Có Thời Gian
**Tác Nhân:** System
**Mô Tả:** Tự động cấp chứng chỉ khi hoàn thành khóa học

**Tiêu Chí Chấp Nhận:**

- **Điều kiện:** 100% Lesson hoàn thành + Điểm trung bình Quiz >= 70% (nếu có quiz) + Điểm trung bình Assignment >= 5 (nếu có assignment; tất cả bài nộp phải đã được chấm).
- **Process:**
  - Learning Service kiểm tra điều kiện sau mỗi lần `LessonCompleted`.
  - Nếu đủ điều kiện -> Generate PDF -> Upload S3 -> Lưu URL vào bảng `Certificate`.
- **Verification:** Mỗi chứng chỉ có mã định danh (UUID) và QR Code để xác thực công khai.
- **Truy cập:** Student tải về từ trang Dashboard.

**Quy Tắc Nghiệp Vụ:** BR-13

---

### REQ-06.07: Instructor Analytics (Thống Kê Giảng Viên)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Xem báo cáo hiệu quả khóa học và doanh thu

**Tiêu Chí Chấp Nhận:**

- **Nguồn dữ liệu:** `Analytics Service` (Các bảng `AggInstructorRevenue`, `AggCourseStats`).
- **Các chỉ số chính (Metrics):**
  - Tổng doanh thu (Thực nhận & Chờ duyệt).
  - Tổng số học viên.
  - Rating trung bình.
- **Biểu đồ:**
  - Doanh thu theo tháng.
  - Số lượng Enrollment mới theo ngày.
- **Chi tiết khóa học:**
  - Top khóa học bán chạy.
  - Tỷ lệ hoàn thành (Completion Rate) của từng khóa.

**Quy Tắc Nghiệp Vụ:** BR-19 (Revenue)

---

### REQ-06.08: Admin Dashboard (Thống Kê Hệ Thống)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Xem sức khỏe toàn hệ thống

**Tiêu Chí Chấp Nhận:**

- **Nguồn dữ liệu:** `Analytics Service` (Bảng `AggSystemOverview`).
- **Metrics:**
  - Tổng User (Student/Instructor).
  - Tổng Doanh thu sàn (GMV).
  - Số tiền đang giữ (Escrow).
  - Tỷ lệ hoàn tiền (Refund Rate).

---

## Yêu Cầu Phi Chức Năng

### NREQ-06.01: Hiệu Năng

- **Write:** Heartbeat API chịu tải cao (Write-heavy), response < 200ms.
- **Read:** Dashboard load < 1s (do lấy từ bảng Aggregate có sẵn).

### NREQ-06.02: Độ Chính Xác

- Dữ liệu Analytics (Dashboard) chấp nhận độ trễ (Eventual Consistency) từ 1-5 phút so với thực tế.
- Dữ liệu Tracking (Resume bài học) phải chính xác tức thì (Strong Consistency).

---

## Ma Trận Truy Xuất

| Yêu Cầu   | Use Case(s) | Business Rule | Module Dependency |
| :-------- | :---------- | :------------ | :---------------- |
| REQ-06.01 | UC-06.01    | BR-14         | Learning Service  |
| REQ-06.02 | UC-06.02    | BR-14         | Learning Service  |
| REQ-06.05 | UC-06.05    | BR-13         | Learning Service  |
| REQ-06.07 | UC-06.07    | BR-19         | Analytics Service |
