# Luồng Hoạt Động Chính - Nền Tảng Học Trực Tuyến

**Dự án:** Nền tảng Học trực tuyến với Hỗ trợ AI
**Phiên bản:** 3.5 (Cập nhật Logic: Hierarchical Context Search & Scoped Docs)
**Cập nhật:** 13 tháng 1, 2026

---

## 1. Tổng Quan Các Luồng Chính

| Luồng       | Mô tả                                           | Người tham gia    | Module chính                           | Kết quả đầu ra        |
| ----------- | ----------------------------------------------- | ----------------- | -------------------------------------- | --------------------- |
| **Luồng 1** | Học viên trở thành giảng viên                   | Học viên, Admin   | MOD-01                                 | Vai trò Giảng viên    |
| **Luồng 2** | Giảng viên tạo & phê duyệt khóa học             | Giảng viên, Admin | MOD-02, MOD-03, MOD-04, MOD-05         | Khóa học được công bố |
| **Luồng 3** | Học viên đăng ký, học tập & hoàn thành khóa học | Học viên, Khách   | MOD-02, MOD-03, MOD-04, MOD-06, MOD-07 | Chứng chỉ             |
| **Luồng 4** | Quản lý hệ thống (Hỗ trợ)                       | Admin             | MOD-01, MOD-02, MOD-07, MOD-09         | Hệ thống vận hành     |
| **Luồng 5** | **Hệ thống Analytics & Báo cáo (Mới)**          | System, Admin, GV | MOD-06, MOD-09, Analytics Service      | Dashboard Số liệu     |

---

## 2. Chi Tiết Từng Luồng Chính

### Luồng 1: Học viên Trở Thành Giảng Viên

**Mục tiêu:** Học viên nộp đơn → Admin phê duyệt → Trở thành Giảng viên
**BR liên quan:** BR-02
**Module:** MOD-01

**Các bước chính:**

1. **Nộp Đơn Giảng Viên** (MOD-01 - REQ-01.07)

   - Học viên điền mẫu đơn: lĩnh vực chuyên môn, tiểu sử, đề cương khóa học mẫu
   - Tải lên tài liệu xác thực danh tính: PDF/DOCX/JPG, tối đa 10MB tổng cộng

   **AI Review Hồ Sơ (Gemini API):**

   - Hệ thống tự động review hồ sơ trước khi nộp
   - AI kiểm tra: khả năng viết, tính hợp lệ tài liệu, phù hợp tiêu chuẩn
   - Nếu độ tin cậy AI <60%: hiển thị cảnh báo & gợi ý cải tiến
   - Học viên có thể chỉnh sửa lại hoặc bỏ qua cảnh báo & nộp

   - Nộp đơn → Trạng thái: **Đang Chờ Xét Duyệt**
   - Chỉ được nộp một lần, không thể tạo khóa học cho đến khi phê duyệt

2. **Gửi Thông Báo** (MOD-01)

   - Thông báo được gửi đến tất cả admin
   - Người dùng nhận email xác nhận đã nhận đơn

3. **Admin Phê Duyệt/Từ Chối** (MOD-01 - REQ-01.08)

   - Admin xem danh sách đơn chờ xử lý
   - Xem chi tiết đơn: mẫu, tài liệu đã tải

   **Nếu phê duyệt:**

   - Vai trò người dùng chuyển thành **Instructor** ngay lập tức
   - **Hệ thống tự động khởi tạo Ví (InstructorWallet) cho giảng viên**
   - Người dùng vẫn giữ quyền truy cập các khóa học đã đăng ký (vai trò Student)
   - Email xác nhận được gửi đến người dùng

   **Nếu từ chối:**

   - Admin cung cấp lý do tối thiểu 20 ký tự
   - Người dùng có thể nộp lại sau 7 ngày
   - Email với quyết định & lý do được gửi

**Kết quả:** Người dùng có quyền Giảng viên, có thể tạo khóa học

---

### Luồng 2: Giảng Viên Tạo & Phê Duyệt Khóa Học (Cập nhật Logic)

**Mục tiêu:** Tạo Knowledge Base phân tầng (Course/Section) → Tạo Quiz dựa trên ngữ cảnh được tổng hợp từ Section xuống Lesson.
**BR liên quan:** BR-03, BR-06, BR-07, BR-09
**Module:** MOD-02, MOD-03, MOD-04, MOD-05

**Các bước chính:**

1. **Thiết Lập Khóa Học & Tài Liệu Chung (Global Scope)** (MOD-02)

   - Giảng viên điền thông tin: tiêu đề, mô tả, danh mục, ngôn ngữ, cấp độ.
   - Tải lên thumbnail.
   - **Upload Tài Liệu Cấp Khóa Học:**
     - Tải lên giáo trình tổng quan hoặc Syllabus (PDF/DOCX).
     - **Hệ thống:** Vector hóa với scope `section_id = NULL` (Tài liệu toàn cục).

2. **Tạo Cấu Trúc & Tải Lên Nội Dung (Local Scope)** (MOD-03, MOD-04)

   - **Tạo Sections (Ngữ cảnh Cấp 1):**

     - Nhập **Tên Section** và **Mô tả Section** chi tiết.
     - _(Tùy chọn)_ Upload tài liệu chuyên sâu cho chương đó -> Hệ thống vector hóa với scope `section_id = Current_UUID`.

   - **Tạo Lessons (Ngữ cảnh Cấp 2):**

     - Nhập **Tên Lesson** và **Mô tả Lesson**.
     - Upload Video: File tải lên S3 -> Transcode sang HLS -> Gắn vào Lesson.

   - **Tạo Quiz & Assignment (Smart AI Context):**

     - **Quiz (Trắc nghiệm - Hierarchical RAG):**

       - Giảng viên chọn: "Generate Quiz from Course Material".
       - **Cơ chế Xây dựng Query:** Hệ thống tự động ghép chuỗi tìm kiếm theo thứ tự:
         > **Query = `[Tên Section]` + `[Mô tả Section]` + `[Tên Lesson]` + `[Mô tả Lesson]`**
       - **Cơ chế Tìm kiếm (Retrieval):** Dùng Query quét Vector DB, ưu tiên tài liệu thuộc Section hiện tại, sau đó đến tài liệu chung.
       - Kết quả: AI sinh câu hỏi bám sát chủ đề bài học trong chương cụ thể.

     - **Assignment (Tự luận - Direct Context):**
       - Thiết lập đề bài và **Rubric (Tiêu chí chấm điểm)**.
       - Rubric được lưu dạng Text để gửi trực tiếp cho AI khi chấm bài (Không qua Vector Search).

3. **Thiết Lập Giá & Khuyến Mãi** (MOD-02, MOD-07)

   - Đặt giá khóa học (VND).
   - Tạo mã giảm giá (Coupon): Code, % giảm, số lượng, hạn dùng.

4. **Nộp Duyệt** (MOD-02 - REQ-02.11)

   - Kiểm tra điều kiện (Validate): ≥1 section, ≥3 lessons, ≥1 quiz, videos ready.
   - Trạng thái → **PendingApproval**.
   - Thông báo gửi đến admin.

5. **Admin Phê Duyệt Khóa Học** (MOD-02 - REQ-02.12)

   - Admin xem trước nội dung.
   - **Phê duyệt:** Trạng thái → **Published**. Khóa học xuất hiện trên Catalog.
   - **Từ chối:** Trạng thái → **Rejected**. Kèm lý do phản hồi.

**Kết quả:** Khóa học được công bố, hiển thị trong danh mục, sẵn sàng để học viên đăng ký.

---

### Luồng 3: Học Viên Đăng Ký, Học Tập & Hoàn Thành

**Mục tiêu:** Tìm khóa học → Đăng ký (tập trung trả phí) → Học tập → Hoàn thành & nhận chứng chỉ
**BR liên quan:** BR-01, BR-08, BR-12, BR-19
**Module:** MOD-02, MOD-03, MOD-04, MOD-06, MOD-07

**Các bước chính:**

1. **Duyệt & Tìm Kiếm Khóa Học** (MOD-02)

   - Student xem catalog, lọc theo danh mục/giá/đánh giá.
   - Xem video preview (HLS streaming có watermark).

2. **Đăng Ký Khóa Học Trả Phí** (MOD-07)

   - Student click "Enroll Now" → Trang thanh toán.
   - Chọn phương thức VNPay (Thẻ/QR).

   **Callback thành công:**

   - Tạo `Enrollment` (Active).
   - Tạo `Order` (Paid).
   - **Ghi nhận doanh thu vào Ví Giảng Viên (Trạng thái Pending - Giữ 14 ngày).**
   - Chuyển hướng đến trang học.

3. **Học Tập** (MOD-03, MOD-04)

   - **Xem Video:**

     - Client gửi **Heartbeat** mỗi 30s về Learning Service (LessonProgress).
     - Tự động đánh dấu hoàn thành nếu xem ≥95%.

   - **Làm Quiz & Assignment:**
     - Quiz MCQ: Hệ thống chấm điểm ngay lập tức.
     - Assignment (Tự luận): Nộp bài (Upload File/Text).
       - **AI Grading Worker:** Hệ thống lấy bài làm + **Rubric (Direct Context)** gửi cho AI. AI chấm điểm và trả về nhận xét chi tiết.

4. **Hoàn Thành Khóa Học** (MOD-06)

   - Điều kiện: 100% lessons + Quiz passed + Assignment graded.
   - Hệ thống tự động cấp **Chứng chỉ (PDF)** có mã xác thực.
   - Trạng thái Enrollment → **Completed**.

5. **Đánh Giá & Bình Luận** (MOD-08)

   - Student đánh giá sao (1-5) và viết review.
   - Hệ thống tự động tính lại Rating trung bình của khóa học (Cache).

**Kết quả:** Học viên hoàn thành khóa học, nhận chứng chỉ.

---

### Luồng 5: Hệ Thống Analytics & Báo Cáo (Mới)

**Mục tiêu:** Thu thập dữ liệu hành vi → Tổng hợp → Hiển thị Dashboard Real-time
**Cơ chế:** Event-Driven Architecture
**Module:** Analytics Service, MOD-06, MOD-09

**Các bước chính:**

1. **Phát Sinh Sự Kiện (Event Emission)**

   - Các service nghiệp vụ bắn event khi có thay đổi trạng thái:
     - _Sales Service:_ Bắn event `OrderPaid`, `RefundProcessed`.
     - _Learning Service:_ Bắn event `LessonCompleted`, `EnrollmentCreated`.
     - _Catalog Service:_ Bắn event `ReviewPosted`.

2. **Xử Lý & Tổng Hợp (Aggregation)**

   - **Analytics Service (Worker)** lắng nghe các event từ Queue.
   - Tính toán và cộng dồn số liệu vào các bảng Aggregate:
     - `AggInstructorRevenue`: Cộng doanh thu, trừ hoàn tiền.
     - `AggCourseStats`: Tăng số học viên, tính lại % hoàn thành.
     - `AggSystemOverview`: Cộng dồn doanh thu toàn sàn (cho Admin).

3. **Hiển Thị Dashboard (Consumption)**
   - **Instructor Dashboard:** Gọi API lấy dữ liệu từ `AggInstructorRevenue`. Hiển thị doanh thu, top khóa học bán chạy ngay lập tức.
   - **Admin Dashboard:** Gọi API lấy `AggSystemOverview`. Xem dòng tiền (Escrow) và lợi nhuận ròng.

**Kết quả:** Báo cáo được cập nhật gần như tức thì (Near Real-time) mà không làm chậm hệ thống chính.

---

## 4. Quản Lý Hệ Thống (Luồng hỗ trợ)

**Mục tiêu:** Admin phê duyệt giảng viên & khóa học, quản lý người dùng, xử lý dòng tiền
**Người tham gia:** Admin
**Module:** MOD-01, MOD-02, MOD-07, MOD-09

**Các bước chính:**

1. **Phê duyệt Giảng viên & Khóa học** (Như Luồng 1 & 2).

2. **Quyết Toán Doanh Thu (Tự động)**

   - Job chạy hàng ngày quét các giao dịch đã qua 14 ngày.
   - Chuyển tiền từ `Pending` -> `Available` trong Ví Giảng Viên.

3. **Xử Lý Yêu Cầu Rút Tiền (Payout)**

   - Admin xem danh sách yêu cầu rút tiền từ Giảng viên.
   - Kiểm tra số dư & thông tin ngân hàng.
   - Duyệt (Chuyển khoản thủ công) hoặc Từ chối (Hoàn tiền về ví).

4. **Xử Lý Hoàn Tiền** (MOD-07)
   - Kiểm tra điều kiện (<= 14 ngày, học < 10%).
   - Phê duyệt -> Hoàn tiền VNPay -> Trừ tiền ví Instructor.

## 5. Sơ Đồ Tổng Quan

### Luồng 1: Học viên Trở Thành Giảng viên

```
+---------------------------------------------------------------+
|                       STUDENT                                 |
+---------------------------------------------------------------+
                              |
                              | Submit Instructor App
                              v
+---------------------------------------------------------------+
|           PROFILE INFORMATION (Application Form)              |
|  - Area of Expertise                                          |
|  - Biography (Bio)                                            |
|  - Sample Course Outline                                      |
|  - Verification Documents                                     |
+---------------------------------------------------------------+
                              |
                              v
+---------------------------------------------------------------+
|              AI PROFILE REVIEW (Gemini API)                   |
|  - Check writing capability                                   |
|  - Check document validity                                    |
|  - Check standard compliance                                  |
|                                                               |
|  [Decision: Confidence < 60%?]                                |
|  -- YES: Warn & Suggest improvements -> Edit or Submit        |
|  -- NO:  Continue                                             |
+---------------------------------------------------------------+
                              |
                              | (Application Submitted)
                              v
+---------------------------------------------------------------+
|                ADMIN APPROVAL / REJECTION                     |
|  (BR-02: Rejected must wait 7 days to re-apply)               |
+---------------------------------------------------------------+
                              |
                              | (Approved)
                              v
+---------------------------------------------------------------+
|                 BECOME INSTRUCTOR                             |
|  - Role: Instructor + Student                                 |
|  - Create Wallet (InstructorWallet)                           |
|  -> Can create courses                                        |
+---------------------------------------------------------------+
```

### Luồng 2: Giảng viên Tạo & Phê Duyệt Khóa Học

```
+---------------------------------------------------------------+
|                       INSTRUCTOR                              |
+---------------------------------------------------------------+
                              |
                              | 1. Create Course & Upload Base
                              v
+---------------------------------------------------------------+
|             COURSE SETUP & KNOWLEDGE BASE                     |
|  - Course Info (Title, Category, Price)                       |
|  - **Upload Course Material (PDF/DOCX)** |
|      |-> Common Svc: Chunking -> Embedding -> Vector DB       |
+---------------------------------------------------------------+
                              |
                              | 2. Create Lesson Structure
                              v
+---------------------------------------------------------------+
|                  CREATE LESSON CONTENT                        |
|  - Define Sections & Lessons                                  |
|  - Lesson Title + Description (Key for AI Search)             |
|  - Upload Video (Transcode HLS)                               |
+---------------------------------------------------------------+
                              |
                              | 3. Create Assessments
                              v
+---------------------------+   +-------------------------------+
|  QUIZ GENERATION (RAG)    |   |  ASSIGNMENT SETUP (DIRECT)    |
|---------------------------|   |-------------------------------|
| - Input: Sections Context |   | - Input: Instruction & Rubric |
| - Process: Vector Search  |   | - Process: Save Rubric Text   |
|   -> Get Chunks -> LLM    |   |   (No Vector Search needed)   |
| - Output: MCQ Questions   |   | - Usage: Passed directly to   |
|                           |   |   AI prompt when grading      |
+---------------------------+   +-------------------------------+
                              |
                              | 4. Submit -> Admin Approve
                              v
+---------------------------------------------------------------+
|                   STATUS: PUBLISHED                           |
+---------------------------------------------------------------+
```

### Luồng 3: Học viên Đăng Ký & Hoàn Thành

```
+---------------------------------------------------------------+
|                 STUDENT (Logged In)                           |
+---------------------------------------------------------------+
                              |
                              | Browse & Select Course
                              v
+---------------------------------------------------------------+
|                  PAID COURSE ENROLLMENT                       |
|  - View Price, Apply Coupon                                   |
|  - Pay via VNPay Gateway                                      |
|    |-- Success: Create Enrollment                             |
|    |-- Failed:  Retry                                         |
+---------------------------------------------------------------+
                              |
                              | (Enrolled)
                              v
+---------------------------------------------------------------+
|           UPDATE INSTRUCTOR WALLET (SALES SVC)                |
|  - Add to Pending Balance                                     |
|  - Hold for 14 days (Escrow)                                  |
+---------------------------------------------------------------+
                              |
                              v
+---------------------------------------------------------------+
|                    LEARNING PROCESS                           |
|  - Watch Video (HLS, Heartbeat 30s)                           |
|  - Take Quiz (Shuffle, Instant grade)                         |
|  - Submit Assignment (Async AI grade if Rubric)               |
|  - Track Progress (Learning Service)                          |
|  - Condition: 100% Lessons + Avg Score >= 70%                 |
+---------------------------------------------------------------+
                              |
                              | (Completed)
                              v
+---------------------------------------------------------------+
|                  COURSE COMPLETION                            |
|  - Status: Completed                                          |
|  - Generate & Send Certificate (PDF)                          |
+---------------------------------------------------------------+
                              |
                              v
+---------------------------------------------------------------+
|                  REVIEW (OPTIONAL)                            |
|  - Rating 1-5 stars                                           |
|  - Comment                                                    |
|  -> Update Course Avg Rating                                  |
+---------------------------------------------------------------+
```

### Luồng 4 (Hỗ trợ): Admin Quản Lý

```
+---------------------------------------------------------------+
|                    ADMIN DASHBOARD                            |
|                                                               |
|  +---------------------------------------------------------+  |
|  |  Instructor Applications Pending (BR-02)                |  |
|  +---------------------------------------------------------+  |
|                                                               |
|  +---------------------------------------------------------+  |
|  |  Courses Pending Approval (BR-03)                       |  |
|  +---------------------------------------------------------+  |
|                                                               |
|  +---------------------------------------------------------+  |
|  |  Refund Requests                                        |  |
|  +---------------------------------------------------------+  |
|                                                               |
|  +---------------------------------------------------------+  |
|  |  Payout Requests from Instructor (BR-19)                |  |
|  +---------------------------------------------------------+  |
|                                                               |
|  +---------------------------------------------------------+  |
|  |  User Management (Ban/Unban)                            |  |
|  +---------------------------------------------------------+  |
|                                                               |
+---------------------------------------------------------------+
```

### Luồng 5: Hệ Thống Analytics & Báo Cáo (Mới)

```
+-------------+       +-------------+       +-------------+
| User Action | ----> |   Services  | ----> |  Event Bus  |
+-------------+       +-------------+       +-------------+
      |                      |                     |
   Purchase             Sales Service          OrderPaid
      |                      |                     |
   Learn               Learning Service     LessonCompleted
      |                      |                     |
   Review              Catalog Service       ReviewPosted
                                                   |
                                                   v
                                        +---------------------+
                                        |  Analytics Worker   |
                                        +---------------------+
                                                   |
                                                   | (Calculate)
                                                   v
+---------------------+                 +---------------------+
|    Dashboard UI     | <-------------- |  Aggregate Tables   |
| (Admin/Instructor)  |     (Query)     | (Revenue/Stats)     |
+---------------------+                 +---------------------+
```

---

## 6. Tài liệu liên quan

- **Yêu cầu:** `/requirements/` (MOD-01 đến MOD-10)
- **Trường hợp sử dụng:** `/use-cases/` (câu chuyện người dùng chi tiết)
- **Thiết kế hệ thống:** `/system-design.md` (kiến trúc, CSDL, API)
- **Tổng quan kế hoạch:** `/plan.md` (lịch trình dự án chính)
