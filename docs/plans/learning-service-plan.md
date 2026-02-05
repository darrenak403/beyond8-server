# Plan: Learning Service

## 1. Tổng quan

**Learning Service** quản lý: enrollment, tiến độ học (video/quiz/assignment), ghi chú, đánh giá khóa học, chứng chỉ. Tích hợp với Catalog (cấu trúc khóa), Assessment (quiz/assignment), Sales (order → enrollment).

---

## 2. Luồng mua khóa học

### Trả phí: Order → Payment → Success → Event → Enroll

```
Order (Sales) → Payment (gateway) → Success → Publish OrderPaidEvent
    → Learning consumer tạo Enrollment (idempotent OrderId+CourseId)
    → Publish CourseEnrollmentCountChangedEvent → Catalog cập nhật TotalStudents
```

- Refund: `OrderRefundedEvent` → Learning set Enrollment.Status = Refunded, publish lại CourseEnrollmentCountChangedEvent.

### Miễn phí: Enroll → Event tạo Order

```
User gọi POST enroll free → Learning tạo Enrollment + LessonProgress/SectionProgress
    → Publish CourseEnrollmentCountChangedEvent (Catalog cập nhật TotalStudents) ✅ đã có
    → Publish FreeEnrollmentOrderRequestEvent → Order service (khi có) tạo order 0đ
```

- //TODO: Định nghĩa FreeEnrollmentOrderRequestEvent trong Common.Events.Learning.
- //TODO: Sau EnrollFreeAsync, publish FreeEnrollmentOrderRequestEvent (UserId, CourseId, EnrollmentId, Amount=0).
- //TODO: Order/Sales service: consumer FreeEnrollmentOrderRequestEvent tạo order + order item giá 0.

---

## 3. Kiến trúc & Events

### Sơ đồ

```
Sales ──OrderPaidEvent / OrderRefundedEvent──► Learning
Catalog ◄──CourseEnrollmentCountChangedEvent── Learning  (✅ đã có)
Catalog ◄──CourseRatingUpdatedEvent────────── Learning
Learning ◄──QuizAttemptCompletedEvent──────── Assessment
Learning ◄──AssignmentSubmittedEvent / AiGradingCompletedEvent ── Assessment
Learning ──HTTP GetCourseStructure──────────► Catalog
```

### Learning consume

| Event                                             | Hành động                                                                       |
| ------------------------------------------------- | ------------------------------------------------------------------------------- |
| OrderPaidEvent / OrderCompletedEvent              | Tạo Enrollment từ order (idempotent), publish CourseEnrollmentCountChangedEvent |
| OrderRefundedEvent                                | Enrollment.Status = Refunded, publish CourseEnrollmentCountChangedEvent         |
| CourseLessonsUpdatedEvent                         | Cập nhật Enrollment.TotalLessons, sync LessonProgress/SectionProgress           |
| QuizAttemptCompletedEvent                         | Cập nhật LessonProgress (QuizAttempts, Score, Completed)                        |
| AssignmentSubmittedEvent, AiGradingCompletedEvent | Cập nhật SectionProgress                                                        |

- //TODO: Common.Events.Sales: OrderPaidEvent (hoặc OrderCompletedEvent), OrderRefundedEvent.
- //TODO: Learning.Application.Consumers: OrderCompletedConsumer, OrderRefundedConsumer.
- //TODO: Assessment publish QuizAttemptCompletedEvent (LessonId, StudentId, Score, IsPassed). Assignment events thêm SectionId.

### Learning publish

| Event                                      | Đã có  | Consumer (service khác)                  |
| ------------------------------------------ | ------ | ---------------------------------------- |
| CourseEnrollmentCountChangedEvent          | ✅     | Catalog cập nhật Course.TotalStudents    |
| FreeEnrollmentOrderRequestEvent            | //TODO | Order tạo order 0đ                       |
| CourseRatingUpdatedEvent                   | //TODO | Catalog cập nhật AvgRating, TotalReviews |
| EnrollmentCreatedEvent                     | //TODO | Integration (notification)               |
| CourseCompletedEvent, LessonCompletedEvent | //TODO | Analytics                                |

---

## 4. API Endpoints (Learning)

| Nhóm         | Method                | Endpoint                             | Mô tả                            |
| ------------ | --------------------- | ------------------------------------ | -------------------------------- |
| Enrollments  | GET                   | `/me`                                | Danh sách enrollment (paginated) |
|              | GET                   | `/{id}`                              | Chi tiết enrollment + progress   |
|              | POST                  | `/`                                  | Enroll free (courseId) ✅        |
|              | GET                   | `/{id}/resume`                       | LessonId + position resume       |
|              | GET                   | `/check?courseId=`                   | Đã enroll chưa ✅                |
| Progress     | PUT                   | `/lesson/{lessonId}/heartbeat`       | Cập nhật vị trí / mark complete  |
|              | GET                   | `/enrollment/{id}/lesson/{lessonId}` | LessonProgress (resume)          |
| Notes        | GET/POST/PATCH/DELETE | Theo enrollment + lesson             | Ghi chú                          |
| Reviews      | POST                  | `/`                                  | Gửi/sửa đánh giá                 |
|              | GET                   | `/enrollment/{id}/can-review`        | Đủ điều kiện review?             |
| Certificates | GET                   | `/me`, `/{id}`, `/verify/{hash}`     | Danh sách, chi tiết, xác thực    |

---

## 5. Phases triển khai (TODO trong code)

### Phase 1: Foundation ✅ (đã xong phần cơ bản)

- //TODO: Const.LearningServiceDatabase trong Common (nếu chưa có).
- //TODO: LearningDbContext, migrations đủ entities.
- //TODO: Repositories + UoW.
- //TODO: AppHost đăng ký learning-db, redis, rabbitmq, catalog-api.

### Phase 2: Catalog client + Enroll free ✅

- ICatalogClient GetCourseStructureAsync ✅
- Enroll free API ✅
- Publish CourseEnrollmentCountChangedEvent sau enroll ✅
- //TODO: Publish FreeEnrollmentOrderRequestEvent sau enroll free.
- //TODO: GET enrollments/me, GET {id}, GET {id}/resume.

### Phase 3: Progress + My Learning

- //TODO: IProgressService heartbeat (LessonProgress, Enrollment.CompletedLessons, ProgressPercent, LastAccessedLessonId).
- //TODO: PUT lesson heartbeat, GET lesson progress.
- //TODO: GET enrollments/me, GET enrollment detail (merge cấu trúc Catalog + progress Learning).

### Phase 4: Assessment events → Progress

- //TODO: Common: QuizAttemptCompletedEvent (LessonId, StudentId, Score, IsPassed). Assignment events thêm SectionId.
- //TODO: QuizAttemptCompletedConsumer → LessonProgress.
- //TODO: AssignmentSubmittedConsumer, AiGradingCompletedConsumer → SectionProgress.

### Phase 5: Notes + Reviews

- //TODO: INoteService CRUD LessonNote.
- //TODO: Note APIs.
- //TODO: IReviewService (≥80% progress, 1 review/course), publish CourseRatingUpdatedEvent.
- //TODO: Review APIs. Catalog consumer CourseRatingUpdatedEvent.

### Phase 6: Certificates

- //TODO: Logic 100% lesson + điều kiện quiz → Certificate, CourseCompletedEvent.
- //TODO: ICertificateService (PDF, VerificationHash).
- //TODO: Certificate APIs. Integration upload PDF.

### Phase 7: Sales + Catalog consumers

- //TODO: OrderCompletedConsumer (tạo Enrollment từ order).
- //TODO: OrderRefundedConsumer (Enrollment.Status = Refunded, publish CourseEnrollmentCountChangedEvent).
- //TODO: CourseLessonsUpdatedConsumer, CourseUnpublishedConsumer, CourseUpdatedConsumer.

---

## 6. Cập nhật service khác (TODO)

### Catalog

- //TODO: Consumer CourseRatingUpdatedEvent → Course.AvgRating, TotalReviews.
- Consumer CourseEnrollmentCountChangedEvent → Course.TotalStudents ✅

### Assessment

- //TODO: Publish QuizAttemptCompletedEvent (LessonId, StudentId, Score, IsPassed).
- //TODO: AssignmentSubmittedEvent, AiGradingCompletedEvent gửi kèm SectionId.

### Common

- //TODO: Events/Sales (OrderPaidEvent, OrderRefundedEvent).
- //TODO: Events/Learning (FreeEnrollmentOrderRequestEvent; CourseRatingUpdatedEvent, … đã liệt kê trên).
- //TODO: Events/Assessment (QuizAttemptCompletedEvent).

### AppHost

- //TODO: Learning API reference: learning-db, redis, rabbitmq, catalog-api. (Khi có Sales: sales-api.)

---

## 7. Tóm tắt ưu tiên

| Ưu tiên | Nội dung                                                                    |
| ------- | --------------------------------------------------------------------------- |
| 1       | Foundation ✅                                                               |
| 2       | Catalog client + Enroll free + CourseEnrollmentCountChangedEvent ✅         |
| 3       | Progress heartbeat + My Learning APIs                                       |
| 4       | Assessment events → LessonProgress / SectionProgress                        |
| 5       | Notes + Reviews + CourseRatingUpdatedEvent                                  |
| 6       | Certificates                                                                |
| 7       | Sales consumers + FreeEnrollmentOrderRequestEvent + Catalog event consumers |

Trong code, tìm `//TODO` hoặc "TODO" trong plan này để biết vị trí cần implement.
