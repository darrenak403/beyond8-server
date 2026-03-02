# MOD-05: Tính Năng AI - Yêu Cầu

**Module:** Tính Năng AI
**Mức Độ Ưu Tiên:** Cao
**Phụ Thuộc:** MOD-02 (Quản Lý Khóa Học), MOD-04 (Hệ Thống Quiz & Assignment)

---

## Yêu Cầu Chức Năng

### REQ-05.01: Xử Lý Tài Liệu (Text Extraction)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** System (Background Job)
**Mô Tả:** Tự động trích xuất nội dung văn bản từ tài liệu giáo trình để phục vụ AI

**Tiêu Chí Chấp Nhận:**

- **Trigger:** Ngay sau khi Instructor upload file thành công (tại REQ-02.09).
- **Hỗ trợ:** PDF (Text layer & OCR cơ bản), DOCX.
- **Xử lý:**
  - Download file từ S3 (dựa trên `s3_key` từ Catalog Service).
  - Trích xuất toàn bộ text sạch.
  - Lưu trữ nội dung text vào Cache hoặc NoSQL DB (phục vụ context cho AI).
- **Trạng thái:** Cập nhật trạng thái xử lý tài liệu (`Processing` -> `Ready for AI`).

**Quy Tắc Nghiệp Vụ:** BR-07

---

### REQ-05.02: AI Tạo Câu Hỏi (Generate Questions)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Tạo câu hỏi trắc nghiệm tự động từ tài liệu của bài học/khóa học

**Tiêu Chí Chấp Nhận:**

- **Input Source:** Chọn 1 hoặc nhiều tài liệu (`CourseDocument`) đã upload.
- **Cấu hình:**
  - Số lượng câu hỏi (5-50).
  - Độ khó (Dễ/Trung bình/Khó - Default 30:50:20).
  - **Tags (Trọng tâm):** Nhập keyword để AI tập trung vào chủ đề đó (Thay thế Topic).
- **Process:** Gọi Gemini API với context là text đã trích xuất ở REQ-05.01.
- **Output (Preview):** Hiển thị bảng danh sách câu hỏi AI vừa tạo.
- **Action:**
  - Chỉnh sửa trực tiếp (Inline Edit).
  - Xóa câu không ưng ý.
  - "Regenerate" (Tạo lại) nếu kết quả không tốt.
  - **Lưu:** Lưu vào `QuestionBank` hoặc thêm thẳng vào `Quiz` đang soạn.

**Quy Tắc Nghiệp Vụ:** BR-09, BR-15

---

### REQ-05.03: Validation Câu Hỏi AI

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** System
**Mô Tả:** Kiểm tra logic câu hỏi trước khi trả về cho Instructor

**Tiêu Chí Chấp Nhận:**

- **Cấu trúc:** Đảm bảo 1 câu hỏi có đúng 4 đáp án, 1 đáp án đúng.
- **Nội dung:** Không có đáp án rỗng, độ dài câu hỏi phù hợp.
- **Auto-retry:** Nếu AI trả về định dạng sai (JSON lỗi), hệ thống tự động thử lại tối đa 3 lần.
- **Gắn nhãn:** Các câu hỏi tạo ra phải có metadata `source = AI`.

**Quy Tắc Nghiệp Vụ:** BR-15

---

### REQ-05.04: AI Chấm Điểm (AI Grading for Essay)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** System (Async Worker)
**Mô Tả:** Chấm điểm bài tự luận/assignment tự động

**Tiêu Chí Chấp Nhận:**

- **Điều kiện:** Bài tập phải có **Rubric** (Tiêu chí chấm) do Instructor cung cấp.
- **Luồng xử lý (Async):**
  - Khi Student nộp bài -> Đẩy vào hàng đợi (Queue).
  - Worker lấy bài làm + Rubric -> Gửi sang Gemini API.
- **Kết quả:**
  - Điểm số (`ai_score`).
  - Nhận xét chi tiết (`feedback`: Điểm mạnh, điểm yếu).
  - Độ tin cậy (`confidence`).
- **Flagging:** Nếu độ tin cậy < 70%, đánh dấu `needs_manual_review` và thông báo cho Instructor.
- **Hiển thị:** Student thấy điểm AI chấm (có badge "AI Graded") kèm ghi chú "Điểm có thể thay đổi bởi giáo viên".

**Quy Tắc Nghiệp Vụ:** BR-10

---

### REQ-05.05: Instructor Điều Chỉnh Điểm AI

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Giảng viên xem xét và chốt điểm cuối cùng

**Tiêu Chí Chấp Nhận:**

- Instructor xem danh sách bài đã chấm bởi AI.
- So sánh bài làm và Feedback của AI.
- Quyền hạn:
  - **Approve:** Chấp nhận điểm AI -> Điểm này trở thành `score` chính thức.
  - **Override:** Sửa điểm và sửa nhận xét -> Lưu điểm mới.
- Hệ thống ghi log việc sửa điểm để audit.

**Quy Tắc Nghiệp Vụ:** BR-10

---

## Yêu Cầu Phi Chức Năng

### NREQ-05.01: Hiệu Năng & Trải Nghiệm

- **Quiz Generation:** < 30 giây cho 10 câu hỏi (Hiển thị Loading UI rõ ràng).
- **Async Grading:** Xử lý xong trong vòng 5 phút kể từ khi nộp bài.

### NREQ-05.02: Độ Tin Cậy

- **Fallback:** Nếu Gemini API lỗi, hệ thống phải Queue lại request để thử lại sau (không báo lỗi ngay cho Student khi nộp bài).
- **Quota Management:** Theo dõi số lượng Token sử dụng, cảnh báo Admin nếu sắp hết hạn mức API.

### NREQ-05.03: Bảo Mật Dữ Liệu AI

- **Anonymization:** Không gửi thông tin cá nhân (Tên, Email, ID) của Student sang Gemini API. Chỉ gửi nội dung bài làm.
- **Data Privacy:** Dữ liệu text trích xuất từ giáo trình chỉ dùng cho mục đích tạo quiz/chấm điểm, không train model công khai.

---

## Ma Trận Truy Xuất

| Yêu Cầu   | Use Case(s) | Business Rule | Module Dependency  |
| :-------- | :---------- | :------------ | :----------------- |
| REQ-05.01 | -           | BR-07         | Catalog Service    |
| REQ-05.02 | UC-05.02    | BR-09, BR-15  | Assessment Service |
| REQ-05.03 | -           | BR-15         | Assessment Service |
| REQ-05.04 | UC-05.04    | BR-10         | Assessment Service |
| REQ-05.05 | UC-05.05    | BR-10         | Assessment Service |
