# THIẾT KẾ HỆ THỐNG CẤP CAO: NỀN TẢNG E-LEARNING

## 0. As-Built Update (2026-03-01)

Phần này cập nhật nhanh theo code thực tế trong `src/Services` để tránh lệch với bản thiết kế khái niệm.

### 0.1 Endpoint convention thực tế

- Các service đang dùng prefix dạng `/api/v1/...` theo từng domain, ví dụ:
  - Identity: `/api/v1/auth`, `/api/v1/users`, `/api/v1/instructors`
  - Catalog: `/api/v1/courses`, `/api/v1/categories`, `/api/v1/sections`, `/api/v1/lessons`
  - Assessment: `/api/v1/questions`, `/api/v1/quizzes`, `/api/v1/quiz-attempts`, `/api/v1/assignments`
  - Learning: `/api/v1/enrollments`, `/api/v1/certificates`, `/api/v1/course-reviews`
  - Sale: `/api/v1/orders`, `/api/v1/cart`, `/api/v1/payments`, `/api/v1/coupons`, `/api/v1/settlements`, ...
  - Integration: `/api/v1/media`, `/api/v1/ai`, `/api/v1/notifications`, ...
  - Analytic: `/api/v1/analytics/system|courses|instructors|lessons`

### 0.2 Event-driven topology thực tế

- Event contracts nằm tại `shared/Beyond8.Common/Events/*` và đang dùng rộng rãi giữa Identity/Catalog/Assessment/Learning/Sale/Integration/Analytic.
- Ngoài các event business cơ bản, hệ thống có các event vận hành quan trọng như:
  - `CourseUpdatedMetadataEvent`
  - `LessonVideoDurationUpdatedEvent`
  - `OrderItemCompletedEvent`
  - `SettlementCompletedEvent`
  - `CacheInvalidateEvent`

### 0.3 Sale thực tế: có Settlement + Escrow

- Sale API hiện map đầy đủ `MapSettlementApi()` và expose `/api/v1/settlements/*`.
- Có Hangfire recurring jobs:
  - cleanup payment hết hạn
  - process pending settlements theo giờ
- Trong payment success flow, order được set `SettlementEligibleAt = PaidAt + 14 ngày` và xử lý chuyển pending -> available qua settlement service.

### 0.4 Tài liệu Mermaid as-built

- Mermaid code để import vào draw.io đã được tạo tại:
  - `docs/plans/as-built-mermaid-drawio.md`

## 1. Tổng Quan Hệ Thống

### Mục Đích Hệ Thống

Hệ thống là một nền tảng E-Learning phân tán, hỗ trợ tạo khóa học đa phương tiện, tích hợp AI RAG (Retrieval-Augmented Generation) để tạo nội dung thông minh, chấm điểm tự động, cùng hệ thống thanh toán Escrow (giữ tiền 14 ngày) và báo cáo phân tích thời gian thực.

### Kiến Trúc

Kiến trúc Microservices hướng sự kiện (Event-Driven Microservices).

- **Frontend Orchestration:** Frontend đóng vai trò điều phối chính cho các tính năng AI.
- **CQRS Lite:** Tách biệt luồng xử lý nghiệp vụ và luồng báo cáo (Analytics Service).
- **RAG Architecture:** Sử dụng Vector DB (do Common Service quản lý) để cung cấp ngữ cảnh (Context) cho AI.
- **Database per Service:** Dữ liệu phân chia rõ ràng theo domain.

### Thành Phần Cấp Cao

- **Frontend (Next.js):** Client App.
- **API Gateway:** Entry point duy nhất định tuyến request.
- **Backend Services (.NET):** Identity, Catalog, Learning, Assessment, Sales, Common, Analytics.
- **Infras:** PostgreSQL, Redis, AWS S3, RabbitMQ, Vector Database (e.g., Qdrant/Pinecone).

---

## 2. Các Dịch Vụ & Schema Dữ Liệu

### 4.1 Module Users (Identity Service)

**Prefix:** `/api/users`

| Method | Endpoint                                 | Mô tả                                     | Role    |
| :----- | :--------------------------------------- | :---------------------------------------- | :------ |
| POST   | `/auth/register`                         | Đăng ký tài khoản mới (Gửi OTP Email).    | Guest   |
| POST   | `/auth/login`                            | Đăng nhập (Trả về Access/Refresh Token).  | Guest   |
| POST   | `/auth/refresh`                          | Làm mới Token.                            | User    |
| GET    | `/profile`                               | Lấy thông tin cá nhân.                    | User    |
| PUT    | `/profile`                               | Cập nhật thông tin cá nhân (Avatar, Bio). | User    |
| POST   | `/instructor-application`                | Nộp/Cập nhật đơn đăng ký giảng viên.      | Student |
| GET    | `/admin/users`                           | Danh sách người dùng (Search/Filter).     | Admin   |
| PUT    | `/admin/instructor-profiles/{id}/verify` | Duyệt/Từ chối hồ sơ giảng viên.           | Admin   |

### 4.2 Module Catalog (Course Catalog Service)

**Prefix:** `/api/catalog`

| Method | Endpoint                      | Mô tả                                                              | Role       |
| :----- | :---------------------------- | :----------------------------------------------------------------- | :--------- |
| GET    | `/courses`                    | Tìm kiếm & Lọc khóa học (Public).                                  | All        |
| GET    | `/courses/{id}`               | Lấy chi tiết khóa học (Info, Curriculum Outline).                  | All        |
| GET    | `/categories`                 | Lấy cây danh mục.                                                  | All        |
| POST   | `/courses`                    | Tạo khóa học mới (Draft).                                          | Instructor |
| PUT    | `/courses/{id}`               | Cập nhật thông tin cơ bản.                                         | Instructor |
| POST   | `/courses/{id}/sections`      | Thêm chương học.                                                   | Instructor |
| POST   | `/courses/{id}/lessons`       | Thêm bài học vào chương.                                           | Instructor |
| POST   | `/courses/{id}/documents`     | Lưu metadata tài liệu (Link S3). Trigger sự kiện DocumentUploaded. | Instructor |
| POST   | `/courses/{id}/submit`        | Nộp khóa học để Admin duyệt.                                       | Instructor |
| PUT    | `/admin/courses/{id}/publish` | Admin phê duyệt & công bố khóa học.                                | Admin      |
| POST   | `/reviews`                    | Gửi đánh giá cho khóa học.                                         | Student    |

### 4.3 Module Assessment (Assessment Service)

**Prefix:** `/api/assessment`

| Method | Endpoint                  | Mô tả                                                   | Role       |
| :----- | :------------------------ | :------------------------------------------------------ | :--------- |
| POST   | `/questions`              | Tạo mới/Lưu câu hỏi (Dùng cho cả Manual & AI Generate). | Instructor |
| GET    | `/questions`              | Lấy danh sách câu hỏi (Filter theo Tag/Course).         | Instructor |
| POST   | `/quizzes`                | Tạo/Cấu hình Quiz (Time limit, Shuffle).                | Instructor |
| POST   | `/quizzes/{id}/questions` | Link câu hỏi vào Quiz.                                  | Instructor |
| POST   | `/assignments`            | Tạo bài tập tự luận (Assignment) & Rubric.              | Instructor |
| GET    | `/quizzes/{id}/take`      | Lấy đề thi để làm bài (Client side).                    | Student    |
| POST   | `/submissions/quiz`       | Nộp bài Quiz (Chấm điểm tự động).                       | Student    |
| POST   | `/submissions/assignment` | Nộp bài Assignment (File/Text).                         | Student    |
| POST   | `/submissions/{id}/grade` | Giảng viên chấm điểm (hoặc xác nhận điểm AI).           | Instructor |

### 4.4 Module Learning (Learning Service)

**Prefix:** `/api/learning`

| Method | Endpoint                      | Mô tả                                             | Role    |
| :----- | :---------------------------- | :------------------------------------------------ | :------ |
| GET    | `/my-courses`                 | Lấy danh sách khóa học đã đăng ký.                | Student |
| GET    | `/access/{courseId}`          | Kiểm tra quyền truy cập (Guard cho Video Player). | Student |
| POST   | `/progress`                   | Cập nhật tiến độ xem video (Heartbeat 30s).       | Student |
| GET    | `/certificates`               | Lấy danh sách chứng chỉ đã đạt được.              | Student |
| GET    | `/certificates/{id}/download` | Tải file PDF chứng chỉ.                           | Student |

### 4.5 Module Sales (Sales Service)

**Prefix:** `/api/sales`

| Method | Endpoint                      | Mô tả                                    | Role       |
| :----- | :---------------------------- | :--------------------------------------- | :--------- |
| GET    | `/cart`                       | Lấy thông tin giỏ hàng hiện tại.         | Student    |
| POST   | `/cart/items`                 | Thêm khóa học vào giỏ.                   | Student    |
| DELETE | `/cart/items/{id}`            | Xóa khóa học khỏi giỏ.                   | Student    |
| POST   | `/orders/checkout`            | Tạo đơn hàng & Lấy URL thanh toán VNPay. | Student    |
| POST   | `/ipn/vnpay`                  | Webhook nhận kết quả từ VNPay.           | System     |
| POST   | `/refunds`                    | Yêu cầu hoàn tiền.                       | Student    |
| GET    | `/wallet`                     | Xem số dư ví (Available/Pending).        | Instructor |
| GET    | `/transactions`               | Xem lịch sử biến động số dư (Ledger).    | Instructor |
| POST   | `/payouts`                    | Yêu cầu rút tiền về ngân hàng.           | Instructor |
| PUT    | `/admin/refunds/{id}/approve` | Admin duyệt yêu cầu hoàn tiền.           | Admin      |

### 4.6 Module Common (Common Service & AI)

**Prefix:** `/api/common`

| Method | Endpoint                        | Mô tả                                                       | Role       |
| :----- | :------------------------------ | :---------------------------------------------------------- | :--------- |
| GET    | `/upload-url`                   | Lấy Presigned URL để Frontend upload file trực tiếp lên S3. | User       |
| GET    | `/notifications`                | Lấy danh sách thông báo.                                    | User       |
| PUT    | `/notifications/{id}/read`      | Đánh dấu đã đọc thông báo.                                  | User       |
| POST   | `/ai-features/review-profile`   | (AI) Phân tích Bio/Kinh nghiệm giảng viên.                  | Student    |
| POST   | `/ai-features/generate-quiz`    | (AI RAG) Tạo Quiz từ tài liệu S3 (sử dụng Vector Search).   | Instructor |
| POST   | `/ai-features/grade-submission` | (AI) Gợi ý điểm & nhận xét cho bài luận.                    | Instructor |

### 4.7 Module Analytics (Analytics Service)

**Prefix:** `/api/analytics`

| Method | Endpoint                   | Mô tả                                                  | Role       |
| :----- | :------------------------- | :----------------------------------------------------- | :--------- |
| GET    | `/instructor/overview`     | Dashboard tổng quan (Tổng doanh thu, Học viên active). | Instructor |
| GET    | `/instructor/courses/{id}` | Chi tiết hiệu suất khóa học (Retention, Avg Score).    | Instructor |
| GET    | `/instructor/revenue`      | Báo cáo doanh thu & Hoàn tiền theo tháng.              | Instructor |
| GET    | `/admin/overview`          | Dashboard Admin (GMV, Tổng User, System Health).       | Admin      |

---

## 3. Thiết Kế Giao Tiếp (Communication Design)

### 3.1 Giao Tiếp Đồng Bộ – HTTP (REST)

Sử dụng hạn chế để đảm bảo tính sẵn sàng cao, chỉ dùng cho các luồng validation thiết yếu hoặc truy xuất dữ liệu thời gian thực cho UI.

| Caller (Service) | Target (Service) | Endpoint                     | Mục Đích & Lý Do Cần Thiết                                                                                                                    |
| :--------------- | :--------------- | :--------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------- |
| Frontend         | API Gateway      | `/api/{module}/*`            | Routing request đến đúng service.                                                                                                             |
| Sales Service    | Course Catalog   | `/internal/courses/validate` | **Data Integrity:** Khi tạo đơn hàng (liên quan tiền bạc), bắt buộc phải kiểm tra giá và trạng thái (Active/Published) mới nhất của khóa học. |
| Learning Service | Course Catalog   | `/internal/courses/exists`   | **Validation:** Khi Admin thực hiện enroll thủ công, cần kiểm tra khóa học có tồn tại không.                                                  |

### 3.2 Giao Tiếp Bất Đồng Bộ – RabbitMQ (Event-driven)

Hệ thống sử dụng Event Bus để xử lý các tác vụ nền, đồng bộ dữ liệu chéo và tổng hợp báo cáo (Analytics).

#### Nhóm Sự Kiện: User & Identity

| Tên Sự Kiện          | Producer | Consumers                | Hành Động Xử Lý                                                                                   |
| :------------------- | :------- | :----------------------- | :------------------------------------------------------------------------------------------------ |
| `UserRegistered`     | Identity | Sales, Analytics, Common | - **Sales:** Init Cart.<br>- **Analytics:** Tăng Total Users.<br>- **Common:** Gửi Email Welcome. |
| `InstructorApproved` | Identity | Sales, Common            | - **Sales:** Tạo InstructorWallet (Ví tiền).<br>- **Common:** Gửi Email báo kết quả.              |

#### Nhóm Sự Kiện: Content & RAG AI

| Tên Sự Kiện                 | Producer | Consumers         | Hành Động Xử Lý                                                                         |
| :-------------------------- | :------- | :---------------- | :-------------------------------------------------------------------------------------- |
| `DocumentUploaded`          | Catalog  | Common            | - **Common (RAG):** Tải file từ S3 -> Chunking -> Embedding -> Lưu vào Vector DB.       |
| `MediaTranscodingCompleted` | Common   | Catalog           | - **Catalog:** Update Video Status (Ready).                                             |
| `CoursePublished`           | Catalog  | Analytics, Common | - **Analytics:** Tăng Total Courses.<br>- **Common:** Gửi thông báo cho người theo dõi. |
| `ReviewPosted`              | Catalog  | Analytics         | - **Analytics:** Tính lại AvgRating trong AggCourseStats.                               |

#### Nhóm Sự Kiện: Sales & Wallet (Logic 14 ngày)

| Tên Sự Kiện       | Producer | Consumers                            | Hành Động Xử Lý                                                                                                                                                                                                                |
| :---------------- | :------- | :----------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `OrderPaid`       | Sales    | Learning, Catalog, Analytics, Common | - **Learning:** Tạo Enrollment (Active).<br>- **Sales (Internal):** Tạo TransactionLedger (Pending, AvailableAt = Now+14d).<br>- **Analytics:** Tăng TotalRevenue, TotalEnrollments.<br>- **Common:** Email Invoice.           |
| `RefundProcessed` | Sales    | Learning, Analytics, Common          | - **Learning:** Update Enrollment -> Refunded.<br>- **Sales (Internal):** Tạo Ledger Refund (Trừ tiền ví).<br>- **Analytics:** Trừ doanh thu (Net earnings), tăng tỷ lệ hoàn tiền.<br>- **Common:** Email thông báo hoàn tiền. |

#### Nhóm Sự Kiện: Learning & Analytics

| Tên Sự Kiện               | Producer   | Consumers           | Hành Động Xử Lý                                                                                             |
| :------------------------ | :--------- | :------------------ | :---------------------------------------------------------------------------------------------------------- |
| `LessonCompleted`         | Learning   | Analytics           | - **Analytics:** Cập nhật AggLessonPerformance (Tỷ lệ hoàn thành bài học, Drop-off rate).                   |
| `SubmissionGraded`        | Assessment | Learning, Analytics | - **Learning:** Update tiến độ học tập.<br>- **Analytics:** Cập nhật AssessmentAnalytics (Điểm trung bình). |
| `StudentActivityRecorded` | Learning   | Analytics           | - **Analytics:** Cập nhật "Active Students" (truy cập trong 7 ngày).                                        |
| `CourseCompleted`         | Learning   | Common              | - **Common:** Tạo PDF Chứng chỉ -> Gửi Email.                                                               |

---

## 4. Các Luồng Hệ Thống Điển Hình

### Luồng 1: Học Viên Đăng Ký Trở Thành Giảng Viên (AI Support)

**Mục tiêu:** Đảm bảo hồ sơ chất lượng qua AI và khởi tạo tài chính cho giảng viên mới.

1.  **AI Review Hồ Sơ:**
    - Học viên nhập Bio và Kinh nghiệm trên Frontend.
    - Frontend gọi `POST /api/common/ai-features/review-profile` (Common Service).
    - Common Service gửi prompt sang Gemini API để phân tích văn phong, độ tin cậy.
    - Trả về Feedback (JSON) cho Frontend hiển thị gợi ý chỉnh sửa.
2.  **Nộp Đơn Đăng Ký:**
    - Học viên hoàn tất chỉnh sửa và bấm "Gửi Đơn".
    - Frontend gọi `POST /api/users/instructor-application` (Identity Service).
    - Identity Service lưu InstructorProfile với trạng thái `Pending`.
3.  **Phê Duyệt & Khởi Tạo:**
    - Admin xem danh sách và duyệt đơn (`PUT /api/users/admin/instructor-profiles/{id}/verify`).
    - Identity Service cập nhật trạng thái `Verified` và publish event `InstructorApproved`.
    - **Sales Service (Consumer):** Nhận event, tạo InstructorWallet để sẵn sàng nhận doanh thu.
    - **Common Service (Consumer):** Gửi email thông báo cho giảng viên.

### Luồng 2: Tạo Khóa Học & RAG Vectorization

1.  **Upload Tài Liệu:** Giảng viên upload PDF. Catalog lưu metadata, publish `DocumentUploaded`.
2.  **RAG Process (Async):** Common Service nhận event -> Tải file -> Chunking -> Embedding -> Lưu Vector DB.
3.  **Tạo Quiz Thông Minh:**
    - Giảng viên chọn "Generate Quiz". Frontend gọi `POST /api/common/ai-features/generate-quiz`.
    - Common Service: Query Vector DB tìm ngữ cảnh -> Gọi Gemini -> Trả câu hỏi.
4.  **Lưu Trữ:** Giảng viên duyệt -> Frontend gọi Assessment Service để lưu.

### Luồng 3: Thanh Toán & Quản Lý Dòng Tiền (Escrow Logic)

1.  **Thanh toán:** Học viên trả tiền VNPay.
2.  **Ghi nhận (Sync):** Sales Service tạo Order.
3.  **Ledger (Internal):** Sales Service tạo TransactionLedger (Status: Pending, AvailableAt: Now + 14 days).
4.  **Quyết toán (Cronjob):** Sau 14 ngày, Job chuyển trạng thái `Completed` -> Tiền vào Available Balance.
5.  **Rút tiền:** Giảng viên gọi `POST /api/sales/payouts`.

### Luồng 5: Analytics Real-time

1.  **Tác động:** Học viên xem xong video.
2.  **Ghi nhận:** Learning Service update DB, bắn event `LessonCompleted`.
3.  **Tổng hợp:** Analytics Service nhận event -> Cộng dồn vào bảng `AggLessonPerformance`.
4.  **Hiển thị:** Giảng viên mở Dashboard -> Frontend gọi `GET /api/analytics/courses/{id}` -> Dữ liệu hiển thị ngay lập tức.

---

## 5. Sơ Đồ Kiến Trúc Hệ Thống (ASCII)

```
+-----------------------------------------------------------------------+
|                          FRONTEND (Next.js)                           |
+-----------------------------------------------------------------------+
          |                            |                           |
          | HTTP (REST)                | S3 Direct                 | Pay
          v                            v                           v
+--------------------+     +-----------------------+     +----------------+
|    API GATEWAY     |     |  AWS S3 / CLOUDFRONT  |     |     VNPAY      |
|  (/api/{module}/*) |     |  (Video & Documents)  |     |  (Payment Gw)  |
+--------------------+     +-----------------------+     +----------------+
          |                            ^                           |
          | Routing                    | Event                     | Callback
          v                            |                           v
+-----------------------------------------------------------------------+
|                        BACKEND MICROSERVICES                          |
|                                                                       |
| +--------------+    +--------------+    +--------------+              |
| |   IDENTITY   |    |    COMMON    |--->|  AI GATEWAY  |---> Gemini   |
| | /api/users   |    | /api/common  |    |   (Wrapper)  |              |
| | /api/auth    |    |              |<---|  VECTOR DB   | (RAG Context)|
| +--------------+    +--------------+    +--------------+              |
|        ^                   ^                   ^                      |
| +--------------+           |            +--------------+              |
| |    SALES     |           |            |  ASSESSMENT  |              |
| |  /api/sales  |           |            | /api/assess..|              |
| | (Wallet/Ledg)|           |            +--------------+              |
| +--------------+           |                   |                      |
|        | Sync              |            +--------------+              |
|        v                   |            |   LEARNING   |              |
| +--------------+           |            | /api/learning|              |
| |    CATALOG   |           |            +--------------+              |
| | /api/catalog |<----------+                                          |
| +--------------+                                                      |
|                                                                       |
| +------------------------------------------------------------------+  |
| |                       ANALYTICS SERVICE                          |  |
| | /api/analytics (Read-Only Dashboards via Aggregated Data)        |  |
| +------------------------------------------------------------------+  |
+-----------------------------------------------------------------------+
          ^                            ^
          | Pub/Sub (Async)            | Cache (Sync)
          v                            v
+--------------------+      +--------------------+
|      RABBITMQ      |      |       REDIS        |
+--------------------+      +--------------------+
          |
          v
+-----------------------------------------------------------------------+
|                       DATA LAYER (PostgreSQL)                         |
| +----------+ +----------+ +----------+ +----------+ +-------+ +-----+ |
| | Identity | | Catalog  | | Assess   | | Learning | | Sales | |Analy| |
| +----------+ +----------+ +----------+ +----------+ +-------+ +-----+ |
+-----------------------------------------------------------------------+
```
