# Plan: Analytic Service

## 1. Tổng quan

**Analytic Service** là microservice event-driven, tiêu thụ events từ Learning, Sale, Catalog, Assessment để xây dựng bảng aggregation. Cung cấp API cho Admin Dashboard (REQ-09.01, REQ-09.03) và Instructor Dashboard.

**Nguyên tắc:**
- Không gọi HTTP đến service khác — chỉ consume MassTransit events
- Lưu dữ liệu denormalized trong PostgreSQL riêng (`analytic-db`)
- Cache kết quả query bằng Redis
- Hỗ trợ filter theo date range (SnapshotDate)

---

## 2. Sơ đồ Events

```
Catalog ──CoursePublishedEvent──────────────► Analytic
Catalog ──CourseUpdatedMetadataEvent────────► Analytic
Learning ──CourseEnrollmentCountChangedEvent─► Analytic
Learning ──CourseCompletedEvent──────────────► Analytic
Learning ──CourseRatingUpdatedEvent──────────► Analytic
Sale ──OrderItemCompletedEvent (MỚI)────────► Analytic
Assessment ──QuizAttemptCompletedEvent──────► Analytic
```

### Event mới cần tạo

**OrderItemCompletedEvent** (`shared/Beyond8.Common/Events/Sale/OrderItemCompletedEvent.cs`)

```csharp
public record OrderItemCompletedEvent(
    Guid OrderId,
    Guid CourseId,
    string CourseTitle,
    Guid InstructorId,
    string InstructorName,
    decimal LineTotal,           // Doanh thu gộp per-item
    decimal PlatformFeeAmount,   // 30% platform
    decimal InstructorEarnings,  // 70% instructor
    DateTime PaidAt);
```

- Sale Service publish 1 event/OrderItem sau khi payment thành công.

---

## 3. Domain Entities

### 3.1 AggCourseStats (đã có, cần thêm 2 field)

Thêm:
```csharp
public DateOnly SnapshotDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
public bool IsCurrent { get; set; } = true;
```

### 3.2 AggLessonPerformance (đã có, cần thêm 2 field)

Thêm:
```csharp
public DateOnly SnapshotDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
public bool IsCurrent { get; set; } = true;
```

### 3.3 AggInstructorRevenue (CẦN IMPLEMENT - hiện đang rỗng)

```csharp
public class AggInstructorRevenue : BaseEntity
{
    public Guid InstructorId { get; set; }

    [MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    // Khóa học & học viên
    public int TotalCourses { get; set; } = 0;
    public int TotalStudents { get; set; } = 0;

    // Tài chính
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRevenue { get; set; } = 0;          // Doanh thu gộp

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalPlatformFee { get; set; } = 0;      // 30% platform

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalInstructorEarnings { get; set; } = 0; // 70% instructor

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRefundAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalPaidOut { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PendingBalance { get; set; } = 0;         // Earnings - PaidOut

    // Đánh giá
    [Column(TypeName = "decimal(5, 2)")]
    public decimal AvgCourseRating { get; set; } = 0;

    public int TotalReviews { get; set; } = 0;

    // Snapshot
    public DateOnly SnapshotDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsCurrent { get; set; } = true;
}
```

### 3.4 AggSystemOverview (CẦN IMPLEMENT - hiện đang rỗng)

```csharp
public class AggSystemOverview : BaseEntity
{
    // Khóa học
    public int TotalCourses { get; set; } = 0;
    public int TotalPublishedCourses { get; set; } = 0;

    // Enrollment
    public int TotalEnrollments { get; set; } = 0;
    public int TotalCompletedEnrollments { get; set; } = 0;

    // Tài chính
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRevenue { get; set; } = 0;           // GMV

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalPlatformFee { get; set; } = 0;       // Doanh thu sàn

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalInstructorEarnings { get; set; } = 0; // Phần instructor

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRefundAmount { get; set; } = 0;

    // Trung bình
    [Column(TypeName = "decimal(5, 2)")]
    public decimal AvgCourseCompletionRate { get; set; } = 0;

    [Column(TypeName = "decimal(3, 2)")]
    public decimal AvgCourseRating { get; set; } = 0;

    public int TotalReviews { get; set; } = 0;

    // Snapshot
    public DateOnly SnapshotDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsCurrent { get; set; } = true;
}
```

---

## 4. MassTransit Consumers

| Consumer | Event | Cập nhật |
|----------|-------|----------|
| `CoursePublishedEventConsumer` | CoursePublishedEvent | Tạo AggCourseStats, +TotalCourses trong AggInstructorRevenue & AggSystemOverview |
| `CourseUpdatedMetadataEventConsumer` | CourseUpdatedMetadataEvent | Cập nhật AggCourseStats.CourseTitle |
| `CourseEnrollmentCountChangedEventConsumer` | CourseEnrollmentCountChangedEvent | AggCourseStats.TotalStudents, AggInstructorRevenue.TotalStudents, AggSystemOverview.TotalEnrollments |
| `CourseCompletedEventConsumer` | CourseCompletedEvent | AggCourseStats.TotalCompletedStudents + CompletionRate, AggSystemOverview.TotalCompletedEnrollments |
| `CourseRatingUpdatedEventConsumer` | CourseRatingUpdatedEvent | AggCourseStats.AvgRating + TotalReviews, AggInstructorRevenue.AvgCourseRating, AggSystemOverview.AvgCourseRating |
| `OrderItemCompletedEventConsumer` | OrderItemCompletedEvent (MỚI) | Revenue fields trong AggCourseStats, AggInstructorRevenue, AggSystemOverview |
| `QuizAttemptCompletedEventConsumer` | QuizAttemptCompletedEvent | AggLessonPerformance metrics |

---

## 5. API Endpoints

### 5.1 System Overview — `/api/v1/analytics/system`
| Method | Path | Mô tả | Auth |
|--------|------|-------|------|
| GET | `/overview` | Dashboard tổng quan hệ thống (REQ-09.01) | Admin, Staff |

### 5.2 Course Stats — `/api/v1/analytics/courses`
| Method | Path | Mô tả | Auth |
|--------|------|-------|------|
| GET | `/` | Tất cả course stats (paginated, optional date range) | Admin, Staff |
| GET | `/{courseId}` | Chi tiết stats 1 khóa | Admin, Staff, Instructor (sở hữu) |
| GET | `/top` | Top courses theo students/revenue/rating | Admin, Staff |
| GET | `/instructor` | Course stats của instructor hiện tại | Instructor |

### 5.3 Instructor Revenue — `/api/v1/analytics/instructors`
| Method | Path | Mô tả | Auth |
|--------|------|-------|------|
| GET | `/` | Tất cả instructor revenue (paginated, date range) | Admin, Staff |
| GET | `/me` | Revenue của instructor hiện tại | Instructor |
| GET | `/top` | Top instructors theo revenue/students | Admin, Staff |

### 5.4 Lesson Performance — `/api/v1/analytics/lessons`
| Method | Path | Mô tả | Auth |
|--------|------|-------|------|
| GET | `/course/{courseId}` | Performance các bài học trong course | Instructor (sở hữu), Admin, Staff |

---

## 6. DTOs

### Request DTOs
- `DateRangeAnalyticRequest` : PaginationRequest + StartDate (DateOnly?) + EndDate (DateOnly?)
- `TopAnalyticRequest` : count (int, default 10), sortBy (string: "students"/"revenue"/"rating")

### Response DTOs
- `CourseStatsResponse` — map từ AggCourseStats
- `TopCourseResponse` — CourseId, CourseTitle, InstructorName, TotalStudents, TotalRevenue, AvgRating
- `InstructorRevenueResponse` — map từ AggInstructorRevenue
- `TopInstructorResponse` — InstructorId, InstructorName, TotalStudents, TotalRevenue, AvgCourseRating
- `SystemOverviewResponse` — map từ AggSystemOverview
- `LessonPerformanceResponse` — map từ AggLessonPerformance

---

## 7. Cấu trúc file

```
Beyond8.Analytic.Domain/
├── Entities/
│   ├── AggCourseStats.cs          (SỬA: thêm SnapshotDate, IsCurrent)
│   ├── AggLessonPerformance.cs    (SỬA: thêm SnapshotDate, IsCurrent)
│   ├── AggInstructorRevenue.cs    (SỬA: implement đầy đủ)
│   └── AggSystemOverview.cs       (SỬA: implement đầy đủ)
└── Repositories/Interfaces/
    ├── IAggCourseStatsRepository.cs
    ├── IAggLessonPerformanceRepository.cs
    ├── IAggInstructorRevenueRepository.cs
    ├── IAggSystemOverviewRepository.cs
    └── IUnitOfWork.cs

Beyond8.Analytic.Application/
├── Dtos/
│   ├── Common/DateRangeAnalyticRequest.cs
│   ├── CourseStats/CourseStatsResponse.cs
│   ├── CourseStats/TopCourseResponse.cs
│   ├── InstructorRevenue/InstructorRevenueResponse.cs
│   ├── InstructorRevenue/TopInstructorResponse.cs
│   ├── SystemOverview/SystemOverviewResponse.cs
│   └── LessonPerformance/LessonPerformanceResponse.cs
├── Mappings/
│   ├── CourseStatsMappings.cs
│   ├── InstructorRevenueMappings.cs
│   ├── SystemOverviewMappings.cs
│   └── LessonPerformanceMappings.cs
├── Services/
│   ├── Interfaces/
│   │   ├── ICourseStatsService.cs
│   │   ├── IInstructorRevenueService.cs
│   │   ├── ISystemOverviewService.cs
│   │   └── ILessonPerformanceService.cs
│   └── Implements/
│       ├── CourseStatsService.cs
│       ├── InstructorRevenueService.cs
│       ├── SystemOverviewService.cs
│       └── LessonPerformanceService.cs
└── Consumers/
    ├── Learning/
    │   ├── CourseEnrollmentCountChangedEventConsumer.cs
    │   ├── CourseCompletedEventConsumer.cs
    │   └── CourseRatingUpdatedEventConsumer.cs
    ├── Catalog/
    │   ├── CoursePublishedEventConsumer.cs
    │   └── CourseUpdatedMetadataEventConsumer.cs
    ├── Sale/
    │   └── OrderItemCompletedEventConsumer.cs
    └── Assessment/
        └── QuizAttemptCompletedEventConsumer.cs

Beyond8.Analytic.Infrastructure/
├── Data/
│   ├── AnalyticDbContext.cs
│   └── AnalyticDbContextFactory.cs
├── Repositories/Implements/
│   ├── AggCourseStatsRepository.cs
│   ├── AggLessonPerformanceRepository.cs
│   ├── AggInstructorRevenueRepository.cs
│   ├── AggSystemOverviewRepository.cs
│   └── UnitOfWork.cs
└── Migrations/ (generated)

Beyond8.Analytic.Api/
├── Bootstrapping/
│   └── ApplicationServiceExtensions.cs
├── Apis/
│   ├── SystemOverviewApis.cs
│   ├── CourseStatsApis.cs
│   ├── InstructorAnalyticsApis.cs
│   └── LessonPerformanceApis.cs
└── Program.cs (SỬA: thay weather forecast)
```

---

## 8. Database Design

### Indexes

| Entity | Unique Index | Non-Unique Index |
|--------|-------------|-----------------|
| AggCourseStats | (CourseId) WHERE IsCurrent=true | InstructorId, SnapshotDate |
| AggLessonPerformance | (LessonId) WHERE IsCurrent=true | CourseId, InstructorId, SnapshotDate |
| AggInstructorRevenue | (InstructorId) WHERE IsCurrent=true | SnapshotDate |
| AggSystemOverview | — WHERE IsCurrent=true | SnapshotDate |

### Query filter: `e.DeletedAt == null`

---

## 9. Thay đổi ngoài Analytic Service

### 9.1 Shared (Beyond8.Common)
- Thêm `AnalyticServiceDatabase = "analytic-db"` vào `Const.cs`
- Tạo `Events/Sale/OrderItemCompletedEvent.cs`

### 9.2 AppHost
- `ExternalServiceRegistrationExtensions.cs`: thêm analyticDb, Analytic-Service project, YARP route `/api/v1/analytics/{**catch-all}`, Scalar reference
- `Beyond8.AppHost.csproj`: thêm ProjectReference đến Beyond8.Analytic.Api

### 9.3 Sale Service
- Sau khi payment thành công, publish `OrderItemCompletedEvent` cho mỗi OrderItem

### 9.4 csproj updates
- `Beyond8.Analytic.Application.csproj`: thêm ref Beyond8.Common, thêm MassTransit.RabbitMQ package
- `Beyond8.Analytic.Infrastructure.csproj`: thêm ref Beyond8.Common

---

## 10. Caching Strategy

| Cache Key | TTL | Invalidate khi |
|-----------|-----|----------------|
| `analytic:system-overview` | 60s | Bất kỳ event nào update AggSystemOverview |
| `analytic:top-courses:{sortBy}:{count}` | 5 min | Event update AggCourseStats |
| `analytic:top-instructors:{sortBy}:{count}` | 5 min | Event update AggInstructorRevenue |
| `analytic:course-stats:{courseId}` | 60s | Event update course cụ thể |
| `analytic:instructor-revenue:{instructorId}` | 60s | Event update instructor cụ thể |

---

## 11. Thứ tự implement

### Phase 1: Foundation (Domain + Infrastructure)
1. Thêm `AnalyticServiceDatabase` vào Const.cs
2. Implement đầy đủ 4 entities (AggInstructorRevenue, AggSystemOverview, thêm SnapshotDate/IsCurrent cho 2 entity cũ)
3. Tạo repository interfaces
4. Tạo AnalyticDbContext + AnalyticDbContextFactory
5. Tạo repository implementations + UnitOfWork
6. Update csproj references
7. Tạo initial migration

### Phase 2: Application Layer
8. Tạo OrderItemCompletedEvent trong Common
9. Tạo DTOs + Mappings
10. Tạo service interfaces + implementations
11. Tạo 7 MassTransit consumers
12. Xóa Class1.cs placeholders

### Phase 3: API Layer
13. Tạo ApplicationServiceExtensions.cs (bootstrap)
14. Thay Program.cs
15. Tạo 4 API endpoint files

### Phase 4: Orchestration
16. Register trong AppHost (csproj + ExternalServiceRegistrationExtensions.cs)

### Phase 5: Sale Service Enhancement
17. Publish OrderItemCompletedEvent trong Sale service sau payment success

---

## 12. Date Range Strategy

**MVP approach:**
- Mỗi entity có `IsCurrent = true` (row hiện tại, luôn cập nhật real-time) và `SnapshotDate`
- Date range query: lọc theo `UpdatedAt` (BaseEntity field) cho row hiện tại
- Tương lai: thêm daily snapshot job (Hangfire) clone current rows → `IsCurrent = false` với `SnapshotDate` cụ thể
- API nhận optional `StartDate`/`EndDate` query params (DateOnly?)

---

## 13. Verification

1. `dotnet build beyond8-server.sln` — compile thành công
2. Chạy AppHost → Analytic service start, connect PostgreSQL + Redis + RabbitMQ
3. Kiểm tra migration tạo đúng 4 bảng
4. Test endpoint GET `/api/v1/analytics/system/overview` → trả về data rỗng/mặc định
5. Publish test `CoursePublishedEvent` qua RabbitMQ → verify row AggCourseStats được tạo
