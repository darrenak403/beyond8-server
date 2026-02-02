# MODULE 08: ĐÁNH GIÁ & NHẬN XÉT (RATING & REVIEW)

**Module ID:** MOD-08
**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Phụ Thuộc:** MOD-01, MOD-02, MOD-07

---

## Yêu Cầu Chức Năng

### REQ-08.01: Gửi Đánh Giá (Submit Review)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student
**Mô Tả:** Học viên đánh giá khóa học sau khi học xong

**Tiêu Chí Chấp Nhận:**

- **Điều kiện hiển thị nút Review:**
  - User đã Enroll.
  - Trạng thái Enrollment là `Active` (Không phải `Refunded` hay `Expired`).
  - Đã hoàn thành ≥ 80% thời lượng khóa học (Dựa trên `Learning Service`).
  - Chưa từng review khóa học này.
- **Form Review:**
  - Số sao (1-5, bắt buộc).
  - Nội dung (Text, tùy chọn, max 1000 ký tự).
- **Lưu trữ:** Tạo bản ghi trong bảng `CourseReview` (Catalog Service).
- **Quyền sửa:** Cho phép chỉnh sửa trong vòng 30 ngày.

**Quy Tắc Nghiệp Vụ:** BR-12

---

### REQ-08.02: Hiển Thị Đánh Giá (Display Reviews)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Guest, Student
**Mô Tả:** Hiển thị danh sách đánh giá công khai

**Tiêu Chí Chấp Nhận:**

- **Thống kê tổng quan:**
  - Rating trung bình (Lấy từ cache trong bảng `Course`).
  - Tổng số lượt review.
  - Biểu đồ phân bố (5 sao: x%, 4 sao: y%...).
- **Danh sách chi tiết:**
  - Avatar & Tên học viên.
  - Số sao & Nội dung.
  - Ngày đăng.
- **Bộ lọc & Sắp xếp:** Mới nhất, Cao nhất/Thấp nhất.
- **Phân trang:** 10 review/trang.

---

### REQ-08.03: Đánh Dấu Hữu Ích (Helpful Vote)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student, Guest
**Mô Tả:** User đánh giá chất lượng của review

**Tiêu Chí Chấp Nhận:**

- Nút "Hữu ích" dưới mỗi review.
- Mỗi User/IP chỉ được vote 1 lần cho 1 review.
- Tăng `helpful_count` trong bảng `CourseReview`.
- Review có nhiều vote hữu ích sẽ được ưu tiên hiển thị lên đầu (nếu chọn filter).

---

### REQ-08.04: Báo Cáo Vi Phạm (Report Review)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor, Student
**Mô Tả:** Báo cáo các review spam hoặc không phù hợp

**Tiêu Chí Chấp Nhận:**

- **Lý do:** Spam, Ngôn từ đả kích, Quảng cáo, Fake.
- **Xử lý:**
  - Tạo bản ghi `ReviewReport` (Catalog Service).
  - Trạng thái ban đầu: `Pending`.
- **Admin Action:**
  - Nếu Admin xác nhận vi phạm -> Đánh dấu `is_hidden = true` trong bảng `CourseReview`.
  - Review bị ẩn sẽ không hiển thị ra ngoài (Soft Delete).

---

### REQ-08.05: Tính Toán Rating Trung Bình

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** System
**Mô Tả:** Cập nhật điểm số khóa học

**Tiêu Chí Chấp Nhận:**

- **Trigger:** Ngay sau khi User Submit, Edit hoặc Delete review.
- **Logic:**
  - Tính lại trung bình cộng của tất cả review (trừ các review bị ẩn/hidden).
  - Cập nhật giá trị vào trường `average_rating` và `total_reviews` trong bảng `Course`.
- **Mục đích:** Để khi hiển thị danh sách khóa học (Catalog) không phải query count/avg lại từ đầu -> Tăng tốc độ load.

**Quy Tắc Nghiệp Vụ:** BR-12

---

## Yêu Cầu Phi Chức Năng

### NFR-08.01: Hiệu Năng

- API lấy danh sách review < 500ms.
- Việc tính toán lại Rating trung bình không được làm chậm thao tác Submit của user (có thể xử lý Async nếu cần).

### NFR-08.02: Tính Toàn Vẹn

- Một User chỉ được có 1 Review duy nhất cho 1 Course (Unique Constraint: `user_id` + `course_id`).
- Khi khóa học bị xóa, các Review liên quan cũng bị ẩn/xóa theo (Cascade).

### NFR-08.03: Kiểm Duyệt (Moderation)

- Tự động ẩn các từ ngữ thô tục (Profanity Filter - Optional) bằng cách thay thế bằng ký tự `***` trước khi lưu hoặc hiển thị.

---

## Ma Trận Truy Xuất

| Requirement ID | Use Case ID | Business Rule | Module Dependency                 |
| :------------- | :---------- | :------------ | :-------------------------------- |
| REQ-08.01      | UC-08.01    | BR-12         | Catalog Service, Learning Service |
| REQ-08.02      | UC-08.02    | BR-12         | Catalog Service                   |
| REQ-08.03      | UC-08.03    | BR-12         | Catalog Service                   |
| REQ-08.04      | UC-08.04    | BR-12         | Catalog Service                   |
| REQ-08.05      | -           | BR-12         | Catalog Service                   |
