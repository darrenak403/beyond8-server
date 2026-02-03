# Plan: Learning Service (Theo dõi tiến độ & Enrollments)

## Tổng Quan

Triển khai **Learning Service** theo CONCEPTUAL_DATA_MODEL và requirements MOD-06, MOD-07, MOD-08 — quản lý enrollment, tiến độ học (video/quiz/assignment), ghi chú, đánh giá khóa học và chứng chỉ. Tích hợp chặt chẽ với **Catalog Service** (cấu trúc khóa học), **Assessment Service** (quiz/assignment), và **Sales Service** (order → enrollment).

### Trạng Thái Hiện Tại

- **Learning Domain** đã có: entities `Enrollment`, `LessonProgress`, `SectionProgress`, `LessonNote`, `CourseReview`, `Certificate`; enums `EnrollmentStatus`, `LessonProgressStatus`.
- **Learning Application / Infrastructure / Api**: chưa có logic (chỉ scaffold projects).
- **Catalog Service** đã có: Course, Section, Lesson, API lấy course/sections/lessons.
- **Assessment Service** đã có: Quiz, Assignment, submissions; events `AssignmentSubmittedEvent`, `AiGradingCompletedEvent` (chưa có `QuizAttemptCompletedEvent`).
- **Sales Service**: chưa triển khai (OrderCompletedEvent → tạo Enrollment sẽ làm khi có Sales).

### Phạm Vi Learning Service

Learning Service **sở hữu**:

- **Enrollment**: đăng ký khóa học (tạo từ OrderCompletedEvent hoặc enroll free).
- **LessonProgress**: tiến độ từng bài (video position, quiz summary sync từ Assessment).
- **SectionProgress**: trạng thái nộp/chấm assignment theo section (sync từ Assessment).
- **LessonNote**: ghi chú của học viên theo bài học.
- **CourseReview**: đánh giá khóa học (1 user – 1 course); publish `CourseRatingUpdatedEvent` → Catalog cập nhật Course.AvgRating.
- **Certificate**: cấp chứng chỉ khi hoàn thành khóa học (100% lesson + điều kiện quiz nếu có).

**Không** quản lý: nội dung khóa học (Catalog), câu hỏi/quiz/assignment/submissions (Assessment), order/payment (Sales).

---

## Kiến Trúc & Tích Hợp

### Sơ đồ phụ thuộc

```
                    ┌─────────────────┐
                    │  Sales Service  │  (chưa có)
                    │  OrderCompleted │
                    │  OrderRefunded  │
                    └────────┬────────┘
                             │ events
                             ▼
┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐
│  Catalog        │   │   Learning      │   │  Assessment     │
│  Service        │◀──│   Service       │◀──│  Service        │
│                 │   │                 │   │                 │
│ - Course        │   │ - Enrollment    │   │ - QuizAttempt   │
│ - Section       │   │ - LessonProgress│   │ - Assignment    │
│ - Lesson        │   │ - SectionProgress   │   Submission     │
│ - TotalLessons  │   │ - Review        │   │                 │
│ - AvgRating     │   │ - Certificate   │   │                 │
└────────┬────────┘   └────────┬────────┘   └────────┬────────┘
         │                     │                     │
         │ HTTP (get structure)│                     │ events
         │                     │  CourseRatingUpdated│ QuizAttemptCompleted
         │                     │  EnrollmentCreated  │ AssignmentSubmitted
         │                     │  LessonCompleted   │ AiGradingCompleted
         └─────────────────────┴─────────────────────┘
```

### 1. Learning ← Sales (khi có Sales Service)

| Event                 | Hành động Learning                                                                                                                                           |
| --------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `OrderCompletedEvent` | Tạo `Enrollment` (snapshot course/instructor/price), khởi tạo `TotalLessons` từ Catalog (hoặc từ event nếu Sales gửi kèm). Publish `EnrollmentCreatedEvent`. |
| `OrderRefundedEvent`  | Cập nhật `Enrollment.Status = Refunded`.                                                                                                                     |

**Lưu ý:** Nếu chưa có Sales, có thể tạm hỗ trợ **enroll free** (API tạo Enrollment trực tiếp khi course free), đồng thời chuẩn bị consumer cho OrderCompletedEvent.

### 2. Learning ← Catalog

- **HTTP (Learning gọi Catalog):**
  - Lấy thông tin khóa học (sections, lessons) để biết `TotalLessons`, thứ tự section/lesson, `TotalDurationSeconds` cho từng lesson (video).
  - Dùng khi: tạo Enrollment (set TotalLessons), tính % hoàn thành, xác định “bài tiếp theo” cho Resume, kiểm tra điều kiện certificate.
- **Events (Catalog → Learning):** (cần bổ sung vào Beyond8.Common nếu chưa có)
  - `CourseLessonsUpdatedEvent` (CourseId, TotalLessons, SectionIds, LessonIds): cập nhật `Enrollment.TotalLessons`, có thể tạo/cập nhật hàng loạt `LessonProgress`/`SectionProgress` cho enrollments đang active.
  - `CourseArchivedEvent` / `CourseUnpublishedEvent`: tùy nghiệp vụ — có thể đánh dấu Enrollment suspended hoặc chỉ ngừng enroll mới.
  - `CourseUpdatedEvent` (CourseId, Title, ThumbnailUrl, …): cập nhật denormalized data trên Enrollment.

**ICatalogClient (Learning):** Cần interface kiểu `GetCourseStructureForLearningAsync(courseId)` trả về sections + lessons (id, order, duration, type) để Learning không lưu nội dung course, chỉ sync TotalLessons và dùng cho progress/resume.

### 3. Learning ← Assessment

- **Events (Assessment → Learning):**
  - **Quiz:** Cần event kiểu `QuizAttemptCompletedEvent` (AttemptId, QuizId, **LessonId**, StudentId, Score, IsPassed, CompletedAt). Learning cập nhật `LessonProgress` tương ứng (QuizAttempts++, QuizBestScore, và nếu IsPassed có thể set Completed).
  - **Assignment:** Đã có `AssignmentSubmittedEvent` — nên bổ sung **SectionId** (Assignment gắn Section) để Learning cập nhật đúng `SectionProgress` (AssignmentSubmitted = true). `AiGradingCompletedEvent` đã có AssignmentId, Learning cần resolve Assignment → SectionId (qua Catalog hoặc event bổ sung SectionId) để set `SectionProgress.AssignmentGrade`, `AssignmentGradedAt`.

**Đề xuất:** Assessment khi publish event gửi kèm **LessonId** (quiz) / **SectionId** (assignment) để Learning không phải gọi ngược Assessment/Catalog.

### 4. Learning → Catalog / Integration / Analytics

- **EnrollmentCreatedEvent:** Catalog tăng Course.TotalStudents; Integration gửi notification.
- **CourseRatingUpdatedEvent:** Catalog cập nhật Course.AvgRating, TotalReviews (sau khi Learning lưu CourseReview và tính aggregate).
- **CourseCompletedEvent / LessonCompletedEvent:** Analytics (khi có) dùng cho thống kê hoàn thành.

---

## Luồng Ưu Tiên #1: Enrollment + Progress Heartbeat + My Learning

**Mục tiêu:** Học viên có thể “vào khóa” (enroll free hoặc qua event), xem “My Learning”, gửi heartbeat video và xem resume.

### Thứ tự thực hiện

1. **Foundation:** DbContext, migrations, repositories (Enrollment, LessonProgress, SectionProgress, LessonNote), Unit of Work. Đăng ký Learning API trong AppHost (learning-db, redis, rabbitmq).
2. **Catalog client:** Learning gọi Catalog lấy cấu trúc khóa (sections/lessons, total lessons, duration). Dùng khi tạo enrollment và khi tính progress.
3. **Enrollment:**
   - **Enroll free:** API `POST /api/v1/enrollments` (courseId) — kiểm tra course tồn tại và free, tạo Enrollment, sync TotalLessons từ Catalog, tạo các bản ghi LessonProgress/SectionProgress “rỗng” cho toàn bộ lesson/section của khóa.
   - **OrderCompletedEvent consumer (khi có Sales):** Tạo Enrollment tương tự, snapshot từ event hoặc Catalog.
4. **Progress heartbeat:** API `PUT/ PATCH /api/v1/progress/lesson/{lessonId}/heartbeat` (position seconds, optional markComplete). Cập nhật LessonProgress (LastPositionSeconds, WatchPercent, LastAccessedAt), nếu position >= 95% duration hoặc markComplete thì set Completed, cập nhật Enrollment.CompletedLessons + ProgressPercent + LastAccessedLessonId.
5. **My Learning:** API `GET /api/v1/enrollments/me` (paginated) trả về danh sách enrollment của user với ProgressPercent, LastAccessedAt, LastAccessedLessonId; `GET /api/v1/enrollments/{id}` chi tiết 1 khóa (kèm danh sách section/lesson với trạng thái progress từ Learning + cấu trúc từ Catalog).

### Điều kiện hoàn thành luồng

- [ ] Learning: DbContext, migrations, repositories, UoW.
- [ ] Learning: ICatalogClient (get course structure) và dùng khi tạo enrollment / tính progress.
- [ ] Learning: Enroll free API; (optional) OrderCompletedEvent consumer stub.
- [ ] Learning: Heartbeat API và cập nhật LessonProgress + Enrollment aggregate.
- [ ] Learning: My Learning APIs (list enrollments, enrollment detail với progress).
- [ ] Catalog: API hoặc endpoint cho Learning lấy cấu trúc khóa (sections + lessons, total, duration) — có thể dùng sẵn GET course by id + sections + lessons nếu đủ dữ liệu.

---

## API Endpoints (Learning Service)

### Enrollments (`/api/v1/enrollments`)

| Method | Endpoint       | Mô tả                                     | Auth    |
| ------ | -------------- | ----------------------------------------- | ------- |
| GET    | `/me`          | Danh sách enrollment của user (paginated) | Student |
| GET    | `/{id}`        | Chi tiết enrollment + progress theo bài   | Student |
| POST   | `/`            | Enroll free (courseId)                    | Student |
| GET    | `/{id}/resume` | Bài học cần resume (lessonId + position)  | Student |

_(Tạo enrollment từ order do Sales/consumer xử lý, không cần endpoint public.)_

### Progress (`/api/v1/progress`)

| Method | Endpoint                                       | Mô tả                                     | Auth    |
| ------ | ---------------------------------------------- | ----------------------------------------- | ------- |
| PUT    | `/lesson/{lessonId}/heartbeat`                 | Cập nhật vị trí xem / đánh dấu hoàn thành | Student |
| GET    | `/enrollment/{enrollmentId}/lesson/{lessonId}` | Lấy LessonProgress (resume position)      | Student |

### Notes (`/api/v1/notes`)

| Method | Endpoint                             | Mô tả                      | Auth    |
| ------ | ------------------------------------ | -------------------------- | ------- |
| GET    | `/enrollment/{id}/lesson/{lessonId}` | Danh sách ghi chú theo bài | Student |
| POST   | `/`                                  | Tạo ghi chú                | Student |
| PATCH  | `/{id}`                              | Sửa ghi chú                | Student |
| DELETE | `/{id}`                              | Xóa ghi chú                | Student |

### Reviews (`/api/v1/reviews`)

| Method | Endpoint                      | Mô tả                                            | Auth    |
| ------ | ----------------------------- | ------------------------------------------------ | ------- |
| POST   | `/`                           | Gửi/sửa đánh giá khóa học                        | Student |
| GET    | `/enrollment/{id}/can-review` | Kiểm tra đủ điều kiện review (≥80%, chưa review) | Student |

_(Hiển thị danh sách review công khai có thể do Catalog hoặc Learning cung cấp tùy thiết kế; Catalog cần AvgRating/TotalReviews từ event.)_

### Certificates (`/api/v1/certificates`)

| Method | Endpoint         | Mô tả                       | Auth      |
| ------ | ---------------- | --------------------------- | --------- |
| GET    | `/me`            | Danh sách certificate       | Student   |
| GET    | `/{id}`          | Chi tiết + link tải PDF     | Student   |
| GET    | `/verify/{hash}` | Xác thực chứng chỉ (public) | Anonymous |

---

## MassTransit Events

### Events Learning cần consume (bổ sung vào Common nếu chưa có)

- **Sales:** `OrderCompletedEvent`, `OrderRefundedEvent`
- **Catalog:** `CourseLessonsUpdatedEvent`, `CourseArchivedEvent` / `CourseUnpublishedEvent`, `CourseUpdatedEvent`
- **Assessment:** `QuizAttemptCompletedEvent` (LessonId, StudentId, Score, IsPassed), `AssignmentSubmittedEvent` (nên thêm SectionId), `AiGradingCompletedEvent` (nên thêm SectionId hoặc AssignmentId đủ để Learning resolve SectionId)

### Events Learning publish (thêm vào Beyond8.Common/Events/Learning/)

```csharp
public record EnrollmentCreatedEvent(Guid EnrollmentId, Guid UserId, Guid CourseId, Guid InstructorId, decimal PricePaid, DateTime EnrolledAt);
public record CourseCompletedEvent(Guid EnrollmentId, Guid UserId, Guid CourseId, Guid InstructorId, DateTime CompletedAt);
public record CourseReviewedEvent(Guid CourseId, Guid UserId, int Rating, DateTime ReviewedAt);
public record CourseRatingUpdatedEvent(Guid CourseId, decimal AvgRating, int TotalReviews, int TotalRatings);
public record LessonCompletedEvent(Guid LessonId, Guid UserId, Guid CourseId, DateTime CompletedAt);
```

---

## File Structure (Learning Service)

```
src/Services/Learning/
├── Beyond8.Learning.Api/
│   ├── Apis/
│   │   ├── EnrollmentApis.cs
│   │   ├── ProgressApis.cs
│   │   ├── NoteApis.cs
│   │   ├── ReviewApis.cs
│   │   └── CertificateApis.cs
│   ├── Bootstrapping/
│   │   └── ApplicationServiceExtensions.cs
│   ├── Program.cs
│   └── Beyond8.Learning.Api.csproj
│
├── Beyond8.Learning.Application/
│   ├── Consumers/
│   │   ├── OrderCompletedConsumer.cs      # khi có Sales
│   │   ├── OrderRefundedConsumer.cs
│   │   ├── CourseLessonsUpdatedConsumer.cs
│   │   ├── QuizAttemptCompletedConsumer.cs
│   │   ├── AssignmentSubmittedConsumer.cs
│   │   └── AiGradingCompletedConsumer.cs
│   ├── Clients/
│   │   └── Catalog/
│   │       ├── ICatalogClient.cs
│   │       └── CatalogClient.cs
│   ├── Dtos/
│   │   ├── Enrollments/
│   │   ├── Progress/
│   │   ├── Notes/
│   │   ├── Reviews/
│   │   └── Certificates/
│   ├── Mappings/
│   ├── Services/
│   │   ├── Interfaces/
│   │   │   ├── IEnrollmentService.cs
│   │   │   ├── IProgressService.cs
│   │   │   ├── INoteService.cs
│   │   │   ├── IReviewService.cs
│   │   │   └── ICertificateService.cs
│   │   └── Implements/
│   ├── Validators/
│   └── Beyond8.Learning.Application.csproj
│
├── Beyond8.Learning.Domain/
│   ├── Entities/          # đã có
│   ├── Enums/             # đã có
│   ├── Repositories/
│   │   ├── IEnrollmentRepository.cs
│   │   ├── ILessonProgressRepository.cs
│   │   ├── ISectionProgressRepository.cs
│   │   ├── ILessonNoteRepository.cs
│   │   ├── ICourseReviewRepository.cs
│   │   └── ICertificateRepository.cs
│   └── Beyond8.Learning.Domain.csproj
│
└── Beyond8.Learning.Infrastructure/
    ├── Data/
    │   ├── LearningDbContext.cs
    │   └── LearningDbContextFactory.cs
    ├── Migrations/
    ├── Repositories/
    └── Beyond8.Learning.Infrastructure.csproj
```

---

## Phases Triển Khai

### Phase 1: Foundation (Tuần 1)

- [ ] Thêm constant `LearningServiceDatabase` vào Beyond8.Common (nếu chưa có).
- [ ] Cấu hình LearningDbContext, migrations cho toàn bộ entity (Enrollment, LessonProgress, SectionProgress, LessonNote, CourseReview, Certificate).
- [ ] Định nghĩa repository interfaces trong Domain, implement trong Infrastructure, Unit of Work.
- [ ] Đăng ký Learning Service trong AppHost (learning-db, redis, rabbitmq).
- [ ] Cấu hình API cơ bản (auth, rate limiting, global exception, OpenAPI).

### Phase 2: Catalog Client + Enrollment (Tuần 2)

- [ ] ICatalogClient: lấy cấu trúc khóa (sections, lessons, total lessons, duration từng lesson). Có thể gọi Catalog API hiện có (GET course, GET sections by course, GET lessons by section) và aggregate ở Learning.
- [ ] IEnrollmentService: Enroll free (validate course, total lessons từ Catalog, tạo Enrollment + LessonProgress/SectionProgress cho từng lesson/section).
- [ ] Enrollment APIs: POST enroll, GET me, GET {id}, GET {id}/resume.
- [ ] (Optional) Stub consumer OrderCompletedEvent: log và tạo Enrollment khi event có CourseId/UserId/PricePaid (khi Sales chưa có có thể test bằng manual publish).

### Phase 3: Progress Heartbeat + My Learning (Tuần 3)

- [ ] IProgressService: heartbeat (cập nhật LessonProgress, tính Completed nếu đủ 95% hoặc mark complete), recalc Enrollment.CompletedLessons + ProgressPercent + LastAccessedLessonId.
- [ ] Progress APIs: PUT lesson heartbeat, GET lesson progress (resume).
- [ ] My Learning: GET enrollments/me với sort theo LastAccessedAt; GET enrollment detail kèm progress từng lesson/section (merge với cấu trúc từ Catalog).

### Phase 4: Assessment Events → Progress (Tuần 4)

- [ ] Định nghĩa/bổ sung event trong Common: `QuizAttemptCompletedEvent` (LessonId, StudentId, Score, IsPassed). Bổ sung SectionId vào `AssignmentSubmittedEvent` / `AiGradingCompletedEvent` (hoặc resolve qua Catalog/Assessment API).
- [ ] QuizAttemptCompletedConsumer: tìm LessonProgress theo UserId + LessonId, cập nhật QuizAttempts, QuizBestScore; nếu IsPassed set Completed, đồng bộ Enrollment.
- [ ] AssignmentSubmittedConsumer: cập nhật SectionProgress (AssignmentSubmitted = true). AiGradingCompletedConsumer: cập nhật SectionProgress (AssignmentGrade, AssignmentGradedAt). Cần SectionId từ event hoặc từ Assignment (Assessment publish kèm SectionId).

### Phase 5: Notes + Reviews (Tuần 5)

- [ ] INoteService: CRUD LessonNote (theo enrollment + lesson, check user sở hữu enrollment).
- [ ] Note APIs: GET by enrollment/lesson, POST, PATCH, DELETE.
- [ ] IReviewService: submit/update CourseReview (điều kiện: enrolled, ≥80% progress, 1 review/course), tính AvgRating/TotalReviews, publish CourseRatingUpdatedEvent.
- [ ] Review APIs: POST review, GET can-review. (Catalog hoặc gateway sẽ gọi Learning/Catalog để hiển thị list review và Course.AvgRating.)

### Phase 6: Certificates (Tuần 6)

- [ ] Logic cấp chứng chỉ: sau mỗi lần cập nhật LessonProgress/SectionProgress completed, kiểm tra 100% lesson hoàn thành + (nếu có quiz) điều kiện điểm (vd >= 70%). Nếu đủ: tạo Certificate, upload PDF (Integration/S3), lưu URL, cập nhật Enrollment.CertificateId, CertificateIssuedAt, publish CourseCompletedEvent.
- [ ] ICertificateService: generate PDF (template), VerificationHash, CertificateNumber.
- [ ] Certificate APIs: GET me, GET {id}, GET verify/{hash} (public).
- [ ] Integration: gọi Integration Service upload file certificate (presigned URL hoặc server-side upload).

### Phase 7: Catalog Events + Sales Consumers (Tuần 7)

- [ ] CourseLessonsUpdatedEvent consumer: cập nhật Enrollment.TotalLessons, đồng bộ LessonProgress/SectionProgress (thêm lesson/section mới nếu có).
- [ ] CourseArchivedEvent / CourseUnpublishedEvent: cập nhật Enrollment status nếu nghiệp vụ yêu cầu.
- [ ] CourseUpdatedEvent: cập nhật Enrollment snapshot (CourseTitle, CourseThumbnailUrl).
- [ ] OrderCompletedConsumer (khi Sales sẵn sàng): tạo Enrollment đầy đủ từ order.
- [ ] OrderRefundedConsumer: Enrollment status = Refunded.
- [ ] Testing E2E, tài liệu API, cập nhật CLAUDE.md.

---

## Cập Nhật Service Khác

### Catalog Service

- Cung cấp API hoặc endpoint để Learning lấy “course structure for learning” (sections + lessons + total + duration) — có thể là GET course by id với include sections/lessons nếu đã có.
- Consumer `CourseRatingUpdatedEvent`: cập nhật Course.AvgRating, TotalReviews khi Learning publish.
- Consumer `EnrollmentCreatedEvent`: tăng Course.TotalStudents.

### Assessment Service

- Publish `QuizAttemptCompletedEvent` với LessonId khi học viên hoàn thành quiz.
- Bổ sung SectionId vào `AssignmentSubmittedEvent` và `AiGradingCompletedEvent` (hoặc document cách Learning resolve SectionId từ AssignmentId).

### Beyond8.Common

- Thêm `Const.LearningServiceDatabase` (nếu chưa có).
- Thêm folder `Events/Learning/` với các event Learning publish.
- Thêm/bổ sung `Events/Sales/` (OrderCompletedEvent, OrderRefundedEvent), `Events/Catalog/` (CourseLessonsUpdatedEvent, …) khi các service đồng ý contract.
- Thêm `QuizAttemptCompletedEvent` trong `Events/Assessment/`.

### AppHost

- Thêm learning-db (PostgreSQL), đăng ký Learning API với reference: learning-db, redis, rabbitmq, **catalog-api** (cho ICatalogClient). Khi có Sales: thêm reference sales-api nếu cần gọi HTTP; events qua RabbitMQ.

---

## Tóm Tắt Ưu Tiên

| Ưu tiên | Nội dung                                             |
| ------- | ---------------------------------------------------- |
| 1       | Foundation (DB, repos, UoW, API bootstrap)           |
| 2       | Catalog client + Enroll free + My Learning APIs      |
| 3       | Progress heartbeat + resume                          |
| 4       | Assessment events → LessonProgress / SectionProgress |
| 5       | Notes + Reviews + CourseRatingUpdatedEvent           |
| 6       | Certificates (logic + PDF + verify)                  |
| 7       | Catalog/Sales event consumers + E2E                  |

Phát triển theo thứ tự trên sẽ cho luồng “enroll → xem bài → heartbeat → resume” hoạt động trước, sau đó bổ sung quiz/assignment sync, review và chứng chỉ, cuối cùng gắn đầy đủ với Catalog và Sales qua events.
