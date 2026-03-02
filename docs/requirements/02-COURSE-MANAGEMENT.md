# MOD-02: Quản Lý Khóa Học - Yêu Cầu

**Module:** Quản Lý Khóa Học
**Mức Độ Ưu Tiên:** Quan Trọng
**Phụ Thuộc:** MOD-01 (Quản Lý Người Dùng)

---

## Yêu Cầu Chức Năng

### REQ-02.01: Quản Lý Danh Mục (Category) - Updated

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Admin quản lý cây danh mục khóa học (tạo mới, chỉnh sửa, phân cấp)

**Tiêu Chí Chấp Nhận:**

- **Tạo mới:**
  - Trường bắt buộc: tên danh mục, slug (URL-friendly).
  - Trường tùy chọn: `parent_id` (nếu là danh mục con), mô tả, icon.
  - Nếu không chọn `parent_id`, hệ thống hiểu là Danh mục gốc (Root Category).
- **Phân cấp:** Hỗ trợ cấu trúc cây đa cấp (Ví dụ: Lập trình -> Backend -> Java).
- **Validation:** Tên danh mục phải unique trong cùng một cấp cha.
- **Trạng thái:** Active ngay khi tạo.

**Quy Tắc Nghiệp Vụ:** BR-03, Cấu trúc Self-referencing.

---

### REQ-02.02: Chỉnh Sửa/Xóa Danh Mục

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Admin chỉnh sửa hoặc xóa danh mục

**Tiêu Chí Chấp Nhận:**

- **Chỉnh sửa:** Có thể đổi tên, icon, và **di chuyển** danh mục sang cha khác (đổi `parent_id`).
- **Xóa:**
  - Không thể xóa nếu danh mục đang chứa khóa học hoặc đang có danh mục con.
  - Soft delete: ẩn danh mục nhưng giữ dữ liệu.
- Thay đổi tự động cập nhật timestamp.

---

### REQ-02.05: Tạo Khóa Học (Updated)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Giảng viên tạo khóa học mới

**Tiêu Chí Chấp Nhận:**

- Trường bắt buộc: tiêu đề, mô tả, **danh mục (chọn từ cây phân cấp)**, ngôn ngữ, cấp độ.
- Trường tùy chọn: ảnh thumbnail, mục tiêu học tập (`outcomes` - JSON).
- Thumbnail: Upload ảnh, lưu URL vào bảng Course.
- Khóa học được tạo với status `Draft`.
- Tự động gán instructor làm chủ sở hữu.
- Tạo `course_id` duy nhất (UUID).

**Quy Tắc Nghiệp Vụ:** BR-03

---

### REQ-02.06: Chỉnh Sửa Thông Tin Khóa Học

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Giảng viên chỉnh sửa thông tin khóa học

**Tiêu Chí Chấp Nhận:**

- Có thể chỉnh sửa tất cả trường từ REQ-02.05.
- Có thể thay đổi danh mục (chuyển khóa học sang nhánh khác).
- Không thể chỉnh sửa các thông tin cốt lõi nếu status là `PendingApproval` hoặc `Published` (trừ khi Admin cho phép hoặc logic versioning).
- Có thể chỉnh sửa thoải mái khi status là `Draft` hoặc `Rejected`.
- Thay đổi tự động lưu với timestamp.

**Quy Tắc Nghiệp Vụ:** BR-03

---

### REQ-02.07: Xóa Khóa Học

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor, Admin
**Mô Tả:** Xóa khóa học khỏi hệ thống

**Tiêu Chí Chấp Nhận:**

- Instructor chỉ có thể xóa khóa học `Draft`.
- Không thể xóa nếu có học viên đã enroll (dù chỉ 1 người).
- Admin có thể xóa bất kỳ khóa học nào (Soft Delete).
- Videos, quizzes, documents liên quan bị ẩn theo (Cascade Soft Delete).

---

### REQ-02.08: Tạo Section

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Thêm section logic vào cấu trúc khóa học

**Tiêu Chí Chấp Nhận:**

- Trường bắt buộc: tiêu đề section, số thứ tự.
- Sections được đánh số tự động theo `order_index`.
- Có thể sắp xếp lại sections (kéo thả cập nhật index).

---

### REQ-02.09: Tạo Lesson & Upload Media (Updated)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Thêm bài học vào section, tích hợp upload video/tài liệu

**Tiêu Chí Chấp Nhận:**

- **Thông tin cơ bản:** Tiêu đề, loại bài học (`video`, `quiz`, `text`), `order_index`.
- **Upload Video:**
  - Upload file MP4/MOV.
  - Hệ thống xử lý transcoding (MediaConvert).
  - Lưu `video_hls_url` và `duration` trực tiếp vào bảng `Lesson` khi xử lý xong.
  - Trạng thái bài học là `Processing` cho đến khi video sẵn sàng.
- **Upload Tài liệu (CourseDocument):**
  - Upload PDF/DOCX.
  - Lưu trữ metadata và `s3_key` vào bảng `CourseDocument` (Catalog Service).
  - File được link trực tiếp với `lesson_id`.

**Quy Tắc Nghiệp Vụ:** BR-06, BR-07

---

### REQ-02.10: Thiết Lập Giá & Coupon (Updated)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Cấu hình giá khóa học và mã giảm giá

**Tiêu Chí Chấp Nhận:**

- **Giá:** Số nguyên VND, min 50,000.
- **Coupon (Mã giảm giá):**
  - Tạo coupon mới: Code (Unique), Loại (`percent`, `fixed`), Giá trị giảm, Giới hạn số lần dùng (`usage_limit`), Ngày hết hạn.
  - Dữ liệu coupon được lưu và quản lý bởi **Sales Service**.
- Có thể set khóa học là Miễn phí.

**Quy Tắc Nghiệp Vụ:** BR-11

---

### REQ-02.11: Nộp Khóa Học Để Duyệt

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Yêu cầu admin review để xuất bản khóa học

**Tiêu Chí Chấp Nhận:**

- Validate điều kiện: ≥1 section, ≥3 lessons, ≥1 quiz, giá hợp lệ.
- Kiểm tra media: Tất cả video phải có status `Ready`.
- Status chuyển sang `PendingApproval`.
- Gửi notification đến Admin.
- Khóa tính năng chỉnh sửa nội dung trong khi chờ duyệt.

**Quy Tắc Nghiệp Vụ:** BR-03

---

### REQ-02.12: Admin Phê Duyệt Khóa Học

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Phê duyệt khóa học để xuất bản

**Tiêu Chí Chấp Nhận:**

- Admin xem trước nội dung khóa học (như góc nhìn học viên).
- Approve: Status chuyển sang `Published`.
- Khóa học xuất hiện trong Catalog công khai.
- Instructor nhận email thông báo.

**Quy Tắc Nghiệp Vụ:** BR-03

---

### REQ-02.13: Admin Từ Chối Khóa Học

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Từ chối khóa học với feedback

**Tiêu Chí Chấp Nhận:**

- Từ chối yêu cầu nhập lý do (`rejection_reason`).
- Status chuyển sang `Rejected`.
- Instructor nhận thông báo kèm lý do.
- Đếm số lần từ chối trong tuần (giới hạn 5 lần/tuần).

**Quy Tắc Nghiệp Vụ:** BR-03

---

### REQ-02.14: Xuất Bản/Hủy Xuất Bản (Unpublish)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor, Admin
**Mô Tả:** Tạm ẩn khóa học khỏi Catalog

**Tiêu Chí Chấp Nhận:**

- Instructor có thể Unpublish khóa học đang Published.
- Khóa học Unpublished: Không tìm thấy trên trang chủ/search, không thể mua mới.
- **Quan trọng:** Học viên cũ vẫn vào học bình thường.
- Muốn Publish lại phải qua quy trình duyệt của Admin.

---

### REQ-02.15: Duyệt Danh Sách Khóa Học (Catalog)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Guest, Student
**Mô Tả:** Xem danh sách khóa học theo bộ lọc

**Tiêu Chí Chấp Nhận:**

- Hiển thị khóa học `Published`.
- Card hiển thị: Thumbnail, Title, Instructor Name, Price, Rating (lấy từ `AggCourseStats`).
- Bộ lọc: Danh mục (theo cây), Giá, Rating, Cấp độ.
- Sắp xếp: Phổ biến nhất (theo số học viên), Mới nhất, Giá tăng/giảm.
- Phân trang: 12 item/page.

---

### REQ-02.16: Xem Chi Tiết Khóa Học

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Guest, Student
**Mô Tả:** Trang landing page của khóa học

**Tiêu Chí Chấp Nhận:**

- Hiển thị đầy đủ thông tin: Curriculum, Instructor Bio, Reviews.
- Nút Action: "Mua ngay" / "Thêm vào giỏ" hoặc "Vào học" (nếu đã mua).
- Hiển thị Preview Video (HLS Stream) cho Guest.
- Badge: "Bán chạy nhất", "Mới".

**Quy Tắc Nghiệp Vụ:** BR-04

---

### REQ-02.17: Dashboard Giảng Viên (Updated)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Quản lý danh sách khóa học của tôi

**Tiêu Chí Chấp Nhận:**

- Danh sách bảng: Tên khóa học, Trạng thái, Giá, Ngày tạo.
- Thống kê nhanh (Lấy từ **Analytics Service**):
  - Tổng học viên.
  - Rating trung bình.
  - Tổng doanh thu (ước tính).
- Action: Edit, Delete (nếu Draft), Submit for Review.

---

### REQ-02.18: Dashboard Học Tập (My Learning)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student
**Mô Tả:** Danh sách khóa học đã mua

**Tiêu Chí Chấp Nhận:**

- Danh sách dạng lưới/list.
- Thanh tiến độ (Progress bar) cho từng khóa (lấy từ `Learning Service`).
- Hiển thị khóa học đã Hoàn thành vs Đang học.
- Nút "Tiếp tục học" dẫn thẳng vào bài học gần nhất.

**Quy Tắc Nghiệp Vụ:** BR-04

---

### REQ-02.19: Đánh Dấu Video Preview

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Chọn video cho phép xem thử

**Tiêu Chí Chấp Nhận:**

- Trong màn hình chỉnh sửa Lesson, có checkbox "Allow Preview".
- Giới hạn: Tối đa 3 video preview/khóa học.
- Guest User xem video này sẽ có watermark "PREVIEW".

**Quy Tắc Nghiệp Vụ:** BR-03

---

## Yêu Cầu Phi Chức Năng

### NREQ-02.01: Hiệu Năng

- Catalog load < 2s.
- Search query < 1s.
- Tạo khóa học (API response) < 500ms.

### NREQ-02.02: Khả Năng Mở Rộng

- Hỗ trợ không giới hạn số lượng Section/Lesson.
- Xử lý đồng thời 50 video upload/transcoding jobs.

### NREQ-02.03: Tính Dễ Sử Dụng

- Drag & Drop để sắp xếp thứ tự Section/Lesson.
- Auto-save draft khi soạn nội dung.
