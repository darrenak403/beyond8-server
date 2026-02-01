# Plan: Quiz & Assignment Module (Assessment Service)

## Tổng Quan

Triển khai hệ thống Quiz & Assignment theo requirements MOD-04, tích hợp với kiến trúc microservices hiện tại của Beyond8.

### Trạng Thái Hiện Tại

- **Catalog Service** đã có: `Lesson.QuizId`, `Section.AssignmentId`, `LessonType` (Video, Text, Quiz), PATCH lesson với QuizId
- **Integration Service** đã có: `POST /api/v1/ai/quiz/generate` (GenQuizRequest → GenQuizResponse), AI Usage Tracking
- **Beyond8.Common** có: BaseEntity, IGenericRepository, ApiResponse, MassTransit config

### Phạm Vi Triển Khai

Tạo **Assessment Service** mới để quản lý:

- Question Bank (Ngân hàng câu hỏi)
- Quiz Configuration (Cấu hình quiz)
- Assignment Configuration (Cấu hình bài tập)
- Student Submissions (Bài nộp - có thể chuyển sang Learning Service sau)
- AI Grading Integration (Tích hợp chấm điểm AI)

---

## Luồng Ưu Tiên #1: AI → Quiz Bank → Quiz Lesson (Nối Catalog)

**Mục tiêu:** Hoàn thành 1 luồng end-to-end: sinh câu hỏi bằng AI → đưa vào Question Bank → tạo Quiz → gắn Quiz vào Lesson (Catalog) trước khi triển khai Assignment / Gradebook.

### Lesson có trước hay Quiz có trước?

**Lesson có trước.** Tránh chicken-and-egg (Lesson cần QuizId, Quiz cần LessonId) bằng thứ tự sau:

1. **Catalog:** Instructor tạo/sửa **Lesson** trước — lesson đã tồn tại với `LessonId`, có thể set `Type = Quiz` ngay nhưng **`QuizId = null`** (chưa có quiz).
2. **Assessment:** Instructor tạo **Quiz** với **`LessonId = <id của lesson vừa có>`** (và CourseId, Title, questionIds…). Quiz tham chiếu tới lesson đã có.
3. **Assessment → Catalog:** Khi **publish** quiz, Assessment gọi Catalog **PATCH lesson** để set **`Lesson.QuizId = quiz.Id`**. Lúc này hai chiều mới đủ: Lesson biết Quiz, Quiz đã biết Lesson từ lúc tạo.

Tóm lại: **Lesson tồn tại trước (QuizId = null) → tạo Quiz với LessonId → publish → Catalog cập nhật Lesson.QuizId.**

### Sơ đồ luồng

```
Instructor (Course/Lesson)                    Integration          Assessment                Catalog
        |                                          |                     |                        |
        | 1. "Tạo quiz từ AI" (courseId, lessonId)  |                     |                        |
        |----------------------------------------->|                     |                        |
        |                                          | 2. Vector search +  |                        |
        |                                          |    Gemini → GenQuizResponse                  |
        |<-----------------------------------------|                     |                        |
        | 3. GenQuizResponse (Easy/Medium/Hard)    |                     |                        |
        |                                                                  |                        |
        | 4. POST .../questions/import-from-ai     |                     |                        |
        |    (GenQuizResponse, instructorId)       |                     |                        |
        |----------------------------------------------------------------->|                        |
        |                                          |                     | 5. Lưu Question bank   |
        |<-----------------------------------------------------------------|                        |
        | 6. QuestionIds[]                         |                     |                        |
        |                                                                  |                        |
        | 7. POST .../quizzes (title, lessonId, courseId, questionIds…)   |                        |
        |----------------------------------------------------------------->|                        |
        |                                          |                     | 8. Tạo Quiz + QuizQuestion
        |<-----------------------------------------------------------------|                        |
        | 9. QuizId                                |                     |                        |
        |                                                                  |                        |
        | 10. POST .../quizzes/{id}/publish         |                     |                        |
        |----------------------------------------------------------------->|                        |
        |                                          |                     | 11. Publish quiz        |
        |                                          |                     | 12. PATCH /lessons/{id} { QuizId }
        |                                          |                     |------------------------->|
        |                                          |                     | 13. Lesson.QuizId = id  |
        |<-----------------------------------------------------------------|                        |
        | 14. Quiz published, lesson linked        |                     |                        |
```

### Các bước kỹ thuật (luồng hoàn chỉnh)

| Bước | Service             | Hành động                                                                                                                                                                                                              |
| ---- | ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 0    | Catalog             | **Lesson đã tồn tại** (instructor tạo trước qua Catalog). Lesson có `LessonId`, `Type = Quiz` (hoặc chưa), **`QuizId = null`**.                                                                                        |
| 1    | Frontend/Instructor | Gọi Integration `POST /api/v1/ai/quiz/generate` với `CourseId`, **`LessonId`** (của lesson bước 0), `TotalCount`, `Distribution`.                                                                                      |
| 2    | Integration         | Vector search tài liệu khóa học, gọi Gemini với prompt "Quiz Generation", trả về `GenQuizResponse` (Easy/Medium/Hard list of `QuizQuestionDto`).                                                                       |
| 3    | Frontend            | Nhận `GenQuizResponse`; có thể chỉnh sửa câu hỏi (optional).                                                                                                                                                           |
| 4    | Assessment          | `POST /api/v1/questions/import-from-ai`: body chứa payload tương thích GenQuizResponse + InstructorId; lưu vào Question bank; trả về danh sách `QuestionId` (và optional tags).                                        |
| 5    | Assessment          | `POST /api/v1/quizzes`: tạo Quiz với `CourseId`, `LessonId`, `Title`, list `QuestionId` (từ bước 4); tạo các QuizQuestion với OrderIndex.                                                                              |
| 6    | Assessment          | `POST /api/v1/quizzes/{id}/publish`: đánh dấu quiz published. **Nếu có LessonId:** gọi Catalog (HTTP client) `PATCH /api/v1/lessons/{lessonId}` với `{ "quizId": "<quizId>", "type": "Quiz" }` để gắn quiz vào lesson. |
| 7    | Catalog             | Cập nhật `Lesson.QuizId`, `Lesson.Type = Quiz` (nếu cần).                                                                                                                                                              |

### Tích hợp Catalog (Assessment → Catalog)

- **Cách làm:** Assessment Service gọi HTTP tới Catalog API khi publish quiz có `LessonId`.
- **Catalog API sẵn có:** `PATCH /api/v1/lessons/{id}` nhận `UpdateLessonRequest` (có `QuizId`, `Type`).
- **Assessment cần:** `ICatalogClient` (trong Beyond8.Common hoặc Assessment.Application) với method ví dụ: `Task<ApiResponse<LessonResponse>> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request)`.
- **Khi nào gọi:** Trong `QuizService.PublishQuizAsync(quizId)`: nếu `quiz.LessonId != null` thì gọi `_catalogClient.UpdateLessonAsync(quiz.LessonId, new { QuizId = quiz.Id, Type = LessonType.Quiz })`.
- **Quyền:** Catalog endpoint đã bảo vệ theo instructor/owner; Assessment gọi với context user hoặc service token (theo quy ước hiện tại của project).

### Contract Integration ↔ Assessment (Import from AI)

- **Integration trả về:** `GenQuizResponse` với `Easy`, `Medium`, `Hard` là `List<QuizQuestionDto>`; mỗi item có `Content`, `Type`, `Options`, `Explanation`, `Tags`, `Difficulty`, `Points`.
- **Assessment import-from-ai request:** DTO nhận tương thích (ví dụ `ImportQuestionsFromAiRequest`) chứa cùng cấu trúc câu hỏi + `InstructorId` (hoặc lấy từ JWT), optional `DefaultTags`, `CourseId`/`LessonId` để gắn tag.
- **Assessment import-from-ai response:** Danh sách `QuestionId` (và có thể trả thêm câu hỏi đã lưu) để frontend dùng ngay cho bước "Create Quiz" với list questionIds.

### Điều kiện hoàn thành luồng

- [ ] Integration `POST /api/v1/ai/quiz/generate` hoạt động (đã có).
- [ ] Assessment: DbContext, repositories, AppHost đăng ký.
- [ ] Assessment: Question CRUD + **POST /questions/import-from-ai** (map GenQuizResponse → Question entities, lưu vào bank).
- [ ] Assessment: Quiz CRUD + QuizQuestion (thêm/xóa/reorder) + **POST /quizzes/{id}/publish**.
- [ ] Assessment: **ICatalogClient** và gọi PATCH lesson khi publish quiz có LessonId.
- [ ] Catalog: PATCH lesson với QuizId đã có; không cần sửa nếu đã hỗ trợ.
- [ ] Frontend (ngoài phạm vi plan): Màn "Tạo quiz từ AI" → gọi Integration generate → gọi Assessment import-from-ai → create quiz với questionIds → publish → lesson tự gắn QuizId.

---

## Kiến Trúc Service

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Catalog        │     │  Assessment     │     │  Integration    │
│  Service        │────▶│  Service        │────▶│  Service        │
│                 │     │                 │     │                 │
│ - Lesson.QuizId │     │ - Question Bank │     │ - AI Grading    │
│ - Section.      │     │ - Quiz Config   │     │ - Notifications │
│   AssignmentId  │     │ - Assignment    │     │ - S3 Upload     │
└─────────────────┘     │ - Submissions   │     └─────────────────┘
                        └─────────────────┘
                               │
                               ▼
                        ┌─────────────────┐
                        │  assessment-db  │
                        │  (PostgreSQL)   │
                        └─────────────────┘
```

---

## API Endpoints

### Question Bank (`/api/v1/questions`)

| Method | Endpoint          | Mô tả                                 | Auth       |
| ------ | ----------------- | ------------------------------------- | ---------- |
| GET    | `/`               | Danh sách câu hỏi (paginated, filter) | Instructor |
| GET    | `/{id}`           | Chi tiết câu hỏi                      | Instructor |
| POST   | `/`               | Tạo câu hỏi                           | Instructor |
| PATCH  | `/{id}`           | Cập nhật câu hỏi                      | Instructor |
| DELETE | `/{id}`           | Xóa câu hỏi                           | Instructor |
| POST   | `/bulk`           | Tạo nhiều câu hỏi                     | Instructor |
| GET    | `/tags`           | Lấy danh sách tags                    | Instructor |
| POST   | `/import-from-ai` | Import từ AI generated                | Instructor |

### Quiz (`/api/v1/quizzes`)

| Method | Endpoint                  | Mô tả                        | Auth       |
| ------ | ------------------------- | ---------------------------- | ---------- |
| GET    | `/`                       | Danh sách quiz               | Instructor |
| GET    | `/{id}`                   | Chi tiết quiz (có questions) | Instructor |
| POST   | `/`                       | Tạo quiz                     | Instructor |
| PATCH  | `/{id}`                   | Cập nhật config              | Instructor |
| DELETE | `/{id}`                   | Xóa quiz                     | Instructor |
| POST   | `/{id}/questions`         | Thêm câu hỏi vào quiz        | Instructor |
| DELETE | `/{id}/questions/{qId}`   | Xóa câu hỏi khỏi quiz        | Instructor |
| POST   | `/{id}/questions/reorder` | Sắp xếp lại câu hỏi          | Instructor |
| POST   | `/{id}/publish`           | Publish quiz                 | Instructor |
| POST   | `/{id}/generate-from-ai`  | Generate câu hỏi từ AI       | Instructor |

### Quiz Student (`/api/v1/quizzes` - Student)

| Method | Endpoint                   | Mô tả                              | Auth    |
| ------ | -------------------------- | ---------------------------------- | ------- |
| GET    | `/{id}/start`              | Bắt đầu làm quiz (không có đáp án) | Student |
| POST   | `/{id}/submit`             | Nộp bài                            | Student |
| POST   | `/{id}/auto-save`          | Auto-save tiến độ                  | Student |
| GET    | `/{id}/result/{attemptId}` | Xem kết quả                        | Student |

### Assignment (`/api/v1/assignments`)

| Method | Endpoint        | Mô tả                | Auth       |
| ------ | --------------- | -------------------- | ---------- |
| GET    | `/`             | Danh sách assignment | Instructor |
| GET    | `/{id}`         | Chi tiết assignment  | Instructor |
| POST   | `/`             | Tạo assignment       | Instructor |
| PATCH  | `/{id}`         | Cập nhật assignment  | Instructor |
| DELETE | `/{id}`         | Xóa assignment       | Instructor |
| POST   | `/{id}/publish` | Publish assignment   | Instructor |

### Assignment Student (`/api/v1/assignments` - Student)

| Method | Endpoint              | Mô tả                | Auth    |
| ------ | --------------------- | -------------------- | ------- |
| GET    | `/{id}/student`       | Xem đề bài           | Student |
| POST   | `/{id}/submit`        | Nộp bài              | Student |
| GET    | `/{id}/my-submission` | Xem bài nộp của mình | Student |

### Gradebook (`/api/v1/gradebook`)

| Method | Endpoint                       | Mô tả                   | Auth       |
| ------ | ------------------------------ | ----------------------- | ---------- |
| GET    | `/quiz/{quizId}/submissions`   | DS bài nộp quiz         | Instructor |
| GET    | `/assignment/{id}/submissions` | DS bài nộp assignment   | Instructor |
| GET    | `/submission/{id}`             | Chi tiết bài nộp        | Instructor |
| POST   | `/submission/{id}/grade`       | Chấm điểm               | Instructor |
| POST   | `/submission/{id}/accept-ai`   | Chấp nhận điểm AI       | Instructor |
| POST   | `/submission/{id}/return`      | Trả bài yêu cầu làm lại | Instructor |

---

## MassTransit Events

### Events mới (thêm vào `Beyond8.Common/Events/Assessment/`)

```csharp
// Quiz Events
public record QuizPublishedEvent(Guid QuizId, Guid InstructorId, Guid? LessonId, string Title, DateTime Timestamp);
public record QuizAttemptSubmittedEvent(Guid AttemptId, Guid QuizId, Guid StudentId, decimal Score, bool IsPassed, DateTime SubmittedAt);

// Assignment Events
public record AssignmentPublishedEvent(Guid AssignmentId, Guid InstructorId, Guid? SectionId, string Title, DateTime? DueDate, DateTime Timestamp);
public record AssignmentSubmittedEvent(Guid SubmissionId, Guid AssignmentId, Guid StudentId, GradingMode GradingMode, DateTime SubmittedAt);

// AI Grading Events
public record AiGradingRequestedEvent(Guid SubmissionId, Guid AssignmentId, Guid StudentId, string Rubric, string SubmissionContent, DateTime RequestedAt);
public record AiGradingCompletedEvent(Guid SubmissionId, decimal AiScore, string AiFeedback, decimal Confidence, DateTime CompletedAt);
public record AiGradingFailedEvent(Guid SubmissionId, string ErrorMessage, int RetryCount, DateTime FailedAt);

// Notification Events
public record GradingCompletedNotificationEvent(Guid StudentId, Guid InstructorId, Guid SubmissionId, string AssignmentTitle, decimal FinalScore, DateTime Timestamp);
```

---

## File Structure

```
src/Services/Assessment/
├── Beyond8.Assessment.Api/
│   ├── Apis/
│   │   ├── QuestionApis.cs
│   │   ├── QuizApis.cs
│   │   ├── AssignmentApis.cs
│   │   └── GradebookApis.cs
│   ├── Bootstrapping/
│   │   └── ApplicationServiceExtensions.cs
│   ├── Program.cs
│   └── Beyond8.Assessment.Api.csproj
│
├── Beyond8.Assessment.Application/
│   ├── Consumers/
│   │   ├── AiGradingCompletedConsumer.cs
│   │   └── AiGradingFailedConsumer.cs
│   ├── Dtos/
│   │   ├── Questions/
│   │   ├── Quizzes/
│   │   ├── Assignments/
│   │   └── Gradebook/
│   ├── Helpers/
│   │   ├── FisherYatesShuffler.cs
│   │   └── QuizGradingHelper.cs
│   ├── Mappings/
│   ├── Services/
│   │   ├── Interfaces/
│   │   └── Implements/
│   ├── Validators/
│   └── Beyond8.Assessment.Application.csproj
│
├── Beyond8.Assessment.Domain/
│   ├── Entities/
│   ├── Enums/
│   ├── ValueObjects/
│   ├── Repositories/
│   └── Beyond8.Assessment.Domain.csproj
│
└── Beyond8.Assessment.Infrastructure/
    ├── Data/
    │   ├── AssessmentDbContext.cs
    │   └── AssessmentDbContextFactory.cs
    ├── Migrations/
    ├── Repositories/
    └── Beyond8.Assessment.Infrastructure.csproj
```

---

## Phases Triển Khai

**Luồng hoàn chỉnh ưu tiên:** Phase 1 + Phase 2 + Phase 3 = **AI → Quiz Bank → Quiz Lesson (nối Catalog)**. Hoàn thành 3 phase này trước khi đẩy mạnh Quiz Taking / Assignment / Gradebook.

---

### Phase 1: Foundation (Tuần 1) ✅ IN PROGRESS

- [x] Tạo 4 projects Assessment Service
- [x] Thêm vào solution file
- [x] Tạo domain entities (Question, Quiz, QuizQuestion, Assignment, QuizAttempt, AssignmentSubmission)
- [x] Tạo enums (QuestionType, DifficultyLevel, GradingMode, AssignmentSubmissionType, QuizAttemptStatus, SubmissionStatus)
- [x] Thêm constants vào Beyond8.Common (`AssessmentServiceDatabase`)
- [ ] Setup DbContext và migrations
- [ ] Implement repositories (IQuestionRepository, IQuizRepository, IQuizQuestionRepository, …)
- [ ] Đăng ký Assessment Service trong AppHost (assessment-db, redis, rabbitmq)

### Phase 2: Question Bank + Import from AI (Tuần 2) — Luồng AI → Bank

- [ ] Implement IQuestionService (CRUD)
- [ ] DTOs và Validators cho Question (Create/Update/Filter, PaginationRequest kế thừa)
- [ ] Question APIs: GET/POST/PATCH/DELETE, GET /tags, filter by tags/difficulty
- [ ] **POST /questions/import-from-ai**: nhận payload tương thích Integration `GenQuizResponse` (Easy/Medium/Hard), map sang entity Question, lưu vào bank, trả về danh sách `QuestionId` (phục vụ bước tạo Quiz)
- [ ] Optional: POST /questions/bulk (tạo nhiều câu từ form thủ công)

### Phase 3: Quiz Management + Link to Catalog (Tuần 3) — Luồng Bank → Quiz → Lesson

- [ ] Implement IQuizService (CRUD, add/remove/reorder questions, publish)
- [ ] Quiz DTOs và Validators (CreateQuizRequest có CourseId, LessonId, Title, list QuestionIds; PublishQuizRequest)
- [ ] Quiz APIs: GET/POST/PATCH/DELETE, POST /{id}/questions, DELETE /{id}/questions/{qId}, POST /{id}/questions/reorder
- [ ] **POST /quizzes/{id}/publish**: đánh dấu quiz published; **nếu Quiz.LessonId != null**: gọi **ICatalogClient** PATCH `/api/v1/lessons/{lessonId}` với `{ QuizId = id, Type = Quiz }` để gắn quiz vào lesson
- [ ] Thêm ICatalogClient (Beyond8.Common hoặc Assessment.Application), cấu hình HttpClient trong Assessment API
- [ ] Fisher-Yates shuffle (chuẩn bị cho Phase 4, có thể stub)
- [ ] Optional: POST /quizzes/{id}/generate-from-ai (gọi Integration generate-quiz + import-from-ai + tạo quiz trong 1 flow — có thể làm sau khi Phase 2–3 ổn định)

### Phase 4: Quiz Taking (Tuần 4)

- [ ] QuizAttempt entity và logic
- [ ] Student quiz APIs (start, submit, auto-save)
- [ ] MCQ auto-grading
- [ ] Timer enforcement
- [ ] Attempt history

### Phase 5: Assignment Module (Tuần 5)

- [ ] Implement IAssignmentService
- [ ] Assignment DTOs và Validators
- [ ] Assignment APIs
- [ ] Rubric management
- [ ] File submission (S3 presigned URL)

### Phase 6: AI Grading (Tuần 6)

- [ ] Thêm Assessment events vào Beyond8.Common
- [ ] Implement AiGradingRequestedConsumer (Integration Service)
- [ ] Create AI grading prompt template
- [ ] Implement AiGradingCompletedConsumer (Assessment Service)
- [ ] Track AI usage

### Phase 7: Gradebook (Tuần 7)

- [ ] Implement IGradebookService
- [ ] Gradebook APIs
- [ ] Instructor review workflow
- [ ] Notification events
- [ ] Testing và documentation

---

## Cập Nhật Các Service Hiện Có (Luồng AI → Quiz Lesson)

### Beyond8.Common

- `Const.AssessmentServiceDatabase` ✅ đã có
- Thêm folder `Events/Assessment/` khi triển khai Phase 6 (AI Grading)

### AppHost (Luồng Quiz Lesson)

- Thêm `assessment-db`, đăng ký Assessment API với reference: assessment-db, redis, rabbitmq, **catalog-api** (để Assessment gọi PATCH lesson).
- Assessment Service cần `WithReference(catalogApi)` để resolve URL Catalog khi gọi ICatalogClient.

### Assessment Service → Catalog

- **ICatalogClient**: interface (Beyond8.Common hoặc Assessment.Application) với `UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request)`.
- **Khi nào gọi:** Trong `QuizService.PublishQuizAsync(quizId)` nếu `quiz.LessonId != null` → gọi `_catalogClient.UpdateLessonAsync(quiz.LessonId, new UpdateLessonRequest { QuizId = quiz.Id, Type = LessonType.Quiz, ... })`.
- Catalog API đã có: `PATCH /api/v1/lessons/{id}` nhận `UpdateLessonRequest` (QuizId, Type).

### Integration Service (Luồng AI → Bank)

- Không cần sửa: `POST /api/v1/ai/quiz/generate` đã trả về `GenQuizResponse` (Easy/Medium/Hard). Assessment chỉ cần implement endpoint `POST /questions/import-from-ai` nhận payload tương thích và lưu vào Question bank.

---
