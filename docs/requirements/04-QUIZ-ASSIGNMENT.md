# MOD-04: Hệ Thống Quiz & Assignment - Yêu Cầu

**Module:** Hệ Thống Quiz & Assignment
**Mức Độ Ưu Tiên:** Cao
**Phụ Thuộc:** MOD-02 (Quản Lý Khóa Học), MOD-05 (Tính Năng AI)

---

## Yêu Cầu Chức Năng

### REQ-04.01: Tạo Quiz (Lesson Type: Quiz)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Tạo bài kiểm tra trắc nghiệm (được xem như một bài học)

**Tiêu Chí Chấp Nhận:**

- **Cấu hình cơ bản:** Tiêu đề, Thời gian làm bài (phút), Điểm đạt (Pass score %).
- **Cấu hình nâng cao:**
  - Cho phép làm lại (Retake policy): Số lần tối đa.
  - Shuffle: Xáo trộn câu hỏi và đáp án.
  - Review: Cho phép xem đáp án đúng sau khi nộp bài.
- **Nguồn câu hỏi:**
  - Nhập thủ công từng câu.
  - Hoặc Chọn từ Ngân hàng câu hỏi (Lọc theo **Tags**).
  - Hoặc Generate từ AI (xem MOD-05).

**Quy Tắc Nghiệp Vụ:** BR-08, BR-09

---

### REQ-04.02: Quản Lý Câu Hỏi (Question Bank)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Thêm/Sửa/Xóa câu hỏi trong ngân hàng cá nhân

**Tiêu Chí Chấp Nhận:**

- **Loại câu hỏi:** MCQ (Trắc nghiệm), Essay (Tự luận).
- **Nội dung:** Rich text, hỗ trợ ảnh đính kèm.
- **Tags (Quan trọng):** Gán nhãn cho câu hỏi (VD: "Chapter 1", "Khó", "Java") để dễ tìm kiếm lại (Thay thế cho Category Tree).
- **Đáp án:** Đánh dấu đáp án đúng (với MCQ).
- **Giải thích:** Nhập lời giải chi tiết (hiện ra khi xem kết quả).

**Quy Tắc Nghiệp Vụ:** BR-09, BR-16 (Simplified Tags)

---

### REQ-04.04: Xáo Trộn & Hiển Thị Quiz

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** System
**Mô Tả:** Xáo trộn câu hỏi khi học viên làm bài

**Tiêu Chí Chấp Nhận:**

- Thuật toán Fisher-Yates để xáo trộn thứ tự câu hỏi.
- Xáo trộn thứ tự đáp án (A, B, C, D) nếu được bật.
- Mỗi lượt làm bài (Attempt) có một seed riêng để tái hiện lại nếu cần.
- Đảm bảo tỷ lệ câu hỏi Dễ/Khó nếu cấu hình random pick.

**Quy Tắc Nghiệp Vụ:** BR-08

---

### REQ-04.05: Làm Quiz (Student Attempt)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student
**Mô Tả:** Giao diện làm bài thi

**Tiêu Chí Chấp Nhận:**

- **Timer:** Đồng hồ đếm ngược (nếu có giới hạn thời gian). Tự động nộp khi hết giờ.
- **Navigation:** Có thể đánh dấu (flag) câu hỏi để xem lại.
- **Auto-save:** Lưu câu trả lời tạm vào LocalStorage hoặc Server mỗi 30s.
- **Submit:** Cảnh báo nếu còn câu chưa trả lời.

---

### REQ-04.06: Chấm Điểm Tự Động (MCQ)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** System
**Mô Tả:** Hệ thống tự động chấm điểm trắc nghiệm

**Tiêu Chí Chấp Nhận:**

- Chấm ngay lập tức sau khi Submit.
- Logic: Đúng = Full điểm, Sai = 0.
- Kết quả lưu vào bảng `QuizSubmission`: `score`, `is_passed`, `answers` (snapshot).
- Nếu pass: Update tiến độ bài học thành Completed.

---

### REQ-04.08: Tạo Assignment (Bài Tập)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Tạo bài tập tự luận/nộp file

**Tiêu Chí Chấp Nhận:**

- **Thông tin:** Tiêu đề, Đề bài (Rich text/File đính kèm), Hạn nộp.
- **Loại nộp:** Text Submission hoặc File Submission (PDF, ZIP, Code).
- **Rubric (Quan trọng cho AI):** Nhập tiêu chí chấm điểm để AI dựa vào đó chấm.
- **Cấu hình chấm:** Chọn "AI Grading" hoặc "Manual Only".

**Quy Tắc Nghiệp Vụ:** BR-10

---

### REQ-04.09: Nộp Assignment

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student
**Mô Tả:** Học viên nộp bài

**Tiêu Chí Chấp Nhận:**

- Upload file lên S3 (thông qua Presigned URL).
- Hỗ trợ Re-submit (nộp lại) nếu chưa hết hạn.
- Ghi nhận thời gian nộp (`submitted_at`).
- Trạng thái chuyển thành `Submitted` (Chờ chấm).

---

### REQ-04.10: Chấm Điểm AI (Async Grading)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** System (AI Worker)
**Mô Tả:** Quy trình chấm điểm tự động cho Assignment/Essay

**Tiêu Chí Chấp Nhận:**

- **Trigger:** Sau khi Student nộp bài -> Đẩy message vào Queue.
- **Processing:** Worker gọi Gemini API với prompt gồm: Đề bài + Bài làm + Rubric.
- **Output:**
  - Điểm số đề xuất (`ai_score`).
  - Nhận xét chi tiết (`feedback`).
  - Độ tin cậy (`confidence`).
- **Lưu trữ:** Cập nhật vào `AssignmentSubmission`.
- **Thông báo:** Gửi noti cho Instructor: "Bài tập X đã được AI chấm xong, vui lòng duyệt".

**Quy Tắc Nghiệp Vụ:** BR-10

---

### REQ-04.12: Sổ Điểm (Gradebook)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Quản lý điểm số và duyệt kết quả AI

**Tiêu Chí Chấp Nhận:**

- Danh sách bài nộp của học viên.
- Hiển thị rõ: Điểm AI chấm (Gợi ý) vs Điểm chốt (Final).
- Instructor có thể:
  - Chấp nhận kết quả AI.
  - Sửa điểm và nhận xét của AI.
  - Trả bài yêu cầu làm lại.
- Sau khi Instructor chốt điểm -> Student mới nhận được thông báo kết quả.

---

## Yêu Cầu Phi Chức Năng

### NREQ-04.01: Hiệu Năng

- Tải đề thi < 1s.
- Chấm điểm trắc nghiệm < 500ms.
- Chấm điểm AI (Async): < 30s (người dùng không phải chờ).

### NREQ-04.02: Độ Tin Cậy & Bảo Mật

- **Anti-cheat:** Không gửi đáp án về Client khi tải đề.
- **Data Integrity:** Snapshot đề thi và đáp án tại thời điểm thi (đề phòng Instructor sửa câu hỏi sau khi thi).
- **Retry:** Cơ chế Retry 3 lần nếu gọi AI API thất bại.

---

## Ma Trận Truy Xuất

| Yêu Cầu   | Use Case(s) | Business Rule |
| :-------- | :---------- | :------------ |
| REQ-04.01 | UC-04.01    | BR-08, BR-09  |
| REQ-04.02 | UC-04.02    | BR-09, BR-16  |
| REQ-04.05 | UC-04.05    | BR-08         |
| REQ-04.06 | UC-04.06    | -             |
| REQ-04.08 | UC-04.08    | BR-10         |
| REQ-04.10 | UC-04.10    | BR-10         |
