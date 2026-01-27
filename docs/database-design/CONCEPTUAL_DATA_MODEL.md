# Conceptual Data Model - Beyond8 E-Learning Platform

**Architecture:** Microservices with Database-per-Service
**Pattern:** Event-Driven Architecture + CQRS for Analytics
**Last Updated:** January 24, 2026

---

## 🏗️ Architecture Principles

### 1. **Microservices Independence**
- Each service owns its data exclusively
- No direct database access across services
- Communication via Events (RabbitMQ/MassTransit)

### 2. **Data Consistency Strategy**
- **Strong Consistency**: Within service boundaries (ACID)
- **Eventual Consistency**: Across services (Event-driven)
- **Compensating Transactions**: For distributed operations (Saga pattern)

### 3. **Common Patterns Across All Services**

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }  // Soft delete
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? DeletedBy { get; set; }
}
```

**All entities inherit from BaseEntity for:**
- ✅ Audit trail (Created, Updated, Deleted metadata)
- ✅ Soft delete pattern
- ✅ Consistent UUID v7 generation
- ✅ User tracking (CreatedBy, UpdatedBy, DeletedBy)

---

## 📊 Services & Database Boundaries

```
┌─────────────────────────────────────────────────────────────────┐
│                      Beyond8 Platform                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
│  │   Identity   │  │  Integration │  │   Course     │        │
│  │   Service    │  │   Service    │  │   Catalog    │        │
│  │              │  │              │  │   Service    │        │
│  │  ┌────────┐  │  │  ┌────────┐  │  │  ┌────────┐  │        │
│  │  │Identity│  │  │  │Integra-│  │  │  │Courses │  │        │
│  │  │   DB   │  │  │  │tion DB │  │  │  │   DB   │  │        │
│  │  └────────┘  │  │  └────────┘  │  │  └────────┘  │        │
│  └──────────────┘  └──────────────┘  └──────────────┘        │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
│  │  Assessment  │  │   Learning   │  │    Sales     │        │
│  │   Service    │  │   Service    │  │   Service    │        │
│  │              │  │              │  │              │        │
│  │  ┌────────┐  │  │  ┌────────┐  │  │  ┌────────┐  │        │
│  │  │Assessmt│  │  │  │Learning│  │  │  │ Sales  │  │        │
│  │  │   DB   │  │  │  │   DB   │  │  │  │   DB   │  │        │
│  │  └────────┘  │  │  └────────┘  │  │  └────────┘  │        │
│  └──────────────┘  └──────────────┘  └──────────────┘        │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐                           │
│  │  Analytics   │  │    Common    │                           │
│  │   Service    │  │   Service    │                           │
│  │   (CQRS)     │  │              │                           │
│  │  ┌────────┐  │  │  ┌────────┐  │                           │
│  │  │Analytics│  │  │  │Common │  │                           │
│  │  │   DB   │  │  │  │   DB   │  │                           │
│  │  └────────┘  │  │  └────────┘  │                           │
│  └──────────────┘  └──────────────┘                           │
│                                                                 │
│                   ┌──────────────────┐                         │
│                   │   RabbitMQ       │                         │
│                   │   Event Bus      │                         │
│                   └──────────────────┘                         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 1️⃣ Identity Service

**Database:** `Identities`
**Responsibility:** Authentication, Authorization, User & Instructor Management

### **User** ⭐ Core Entity

```csharp
public class User : BaseEntity
{
    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    // Relationships
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual InstructorProfile? InstructorProfile { get; set; }
}

public enum UserStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_user_email ON users(email) WHERE deleted_at IS NULL;
CREATE INDEX idx_user_status ON users(status) WHERE deleted_at IS NULL;
CREATE INDEX idx_user_phone ON users(phone_number) WHERE deleted_at IS NULL;
```

---

### **Role**

```csharp
public class Role : BaseEntity
{
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;  // ROLE_STUDENT, ROLE_INSTRUCTOR, ROLE_ADMIN

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Relationships
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_role_code ON roles(code) WHERE deleted_at IS NULL;
```

---

### **UserRole** (Join Table)

```csharp
public class UserRole
{
    public Guid UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public Guid? AssignedBy { get; set; }
}
```

**Composite PK:**
```sql
ALTER TABLE user_roles ADD PRIMARY KEY (user_id, role_id);
CREATE INDEX idx_user_role_user ON user_roles(user_id) WHERE revoked_at IS NULL;
```

---

### **InstructorProfile** ⭐ Key Entity

```csharp
public class InstructorProfile : BaseEntity
{
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    // Profile Information
    [MaxLength(300)]
    public string? Bio { get; set; }

    [MaxLength(200)]
    public string? Headline { get; set; }

    [MaxLength(50)]
    public string? TaxId { get; set; }

    public List<string> TeachingLanguages { get; set; } = ["vi-VN"];

    public string? IntroVideoUrl { get; set; }

    // Complex JSON Fields (PostgreSQL JSONB)
    [Column(TypeName = "jsonb")]
    public string? ExpertiseAreas { get; set; }  // ["Web Development", "AI"]

    [Column(TypeName = "jsonb")]
    public string? Education { get; set; }  // [{degree, school, year}]

    [Column(TypeName = "jsonb")]
    public string? WorkExperience { get; set; }  // [{title, company, years}]

    [Column(TypeName = "jsonb")]
    public string? SocialLinks { get; set; }  // {linkedin, github, website}

    [Column(TypeName = "jsonb")]
    public string BankInfo { get; set; } = string.Empty;  // {bank_name, account_number}

    [Column(TypeName = "jsonb")]
    public string? IdentityDocuments { get; set; }  // {front_url, back_url}

    [Column(TypeName = "jsonb")]
    public string? Certificates { get; set; }  // [{name, url, issued_date}]

    // Verification
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

    [MaxLength(1000)]
    public string? VerificationNotes { get; set; }

    public Guid? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }

    // Statistics (denormalized for performance)
    public int TotalStudents { get; set; } = 0;
    public int TotalCourses { get; set; } = 0;

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? AvgRating { get; set; }

    public int TotalReviews { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalEarnings { get; set; } = 0;
}

public enum VerificationStatus
{
    Pending = 0,
    Verified = 1,
    RequestUpdate = 2,
    Hidden = 3,
    Recovering = 4
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_instructor_user ON instructor_profiles(user_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_instructor_verification ON instructor_profiles(verification_status) WHERE deleted_at IS NULL;
CREATE INDEX idx_instructor_rating ON instructor_profiles(avg_rating DESC) WHERE deleted_at IS NULL;
CREATE INDEX idx_instructor_expertise ON instructor_profiles USING gin(expertise_areas jsonb_path_ops);
```

---

### **Events Published by Identity Service**

```csharp
// 1. User registration
public record UserRegisteredEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
}

// 2. User profile updated
public record UserProfileUpdatedEvent
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public DateTime UpdatedAt { get; init; }
}

// 3. User suspended
public record UserSuspendedEvent
{
    public Guid UserId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime SuspendedAt { get; init; }
}

// 4. Instructor verification changed
public record InstructorVerificationChangedEvent
{
    public Guid InstructorId { get; init; }
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public VerificationStatus OldStatus { get; init; }
    public VerificationStatus NewStatus { get; init; }
    public decimal? AvgRating { get; init; }
    public DateTime ChangedAt { get; init; }
    public string? Reason { get; init; }
}

// 5. Instructor profile updated
public record InstructorProfileUpdatedEvent
{
    public Guid InstructorId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public decimal? AvgRating { get; init; }
    public string? Headline { get; init; }
    public DateTime UpdatedAt { get; init; }
}
```

---

## 2️⃣ Course Catalog Service

**Database:** `CourseCatalog`
**Responsibility:** Course content management, curriculum structure

### **Category** (Self-referencing Hierarchy)

```csharp
public class Category : BaseEntity
{
    // Self-referencing
    public Guid? ParentId { get; set; }
    [ForeignKey(nameof(ParentId))]
    public virtual Category? Parent { get; set; }

    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

    // Basic Info
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Hierarchy
    public int Level { get; set; } = 0;  // 0=root, 1=child, 2=grandchild

    [MaxLength(500)]
    public string? Path { get; set; }  // Materialized path: "/1/5/12"

    // Display
    public string? IconUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Statistics (denormalized)
    public int TotalCourses { get; set; } = 0;
    public int TotalPublishedCourses { get; set; } = 0;

    // Relationships
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_category_slug ON categories(slug) WHERE deleted_at IS NULL;
CREATE INDEX idx_category_parent ON categories(parent_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_category_path ON categories USING gin(string_to_array(path, '/'));
CREATE INDEX idx_category_active ON categories(is_active, level) WHERE deleted_at IS NULL;
```

---

### **Course** ⭐ Core Entity

```csharp
public class Course : BaseEntity
{
    // Logical Reference to Identity Service (NO FK)
    public Guid InstructorId { get; set; }

    // Denormalized instructor data (for performance & resilience)
    [Required, MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    public string? InstructorAvatarUrl { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? InstructorRating { get; set; }

    [MaxLength(200)]
    public string? InstructorHeadline { get; set; }

    // Instructor status cache (synced via events)
    public InstructorVerificationStatus InstructorStatus { get; set; }
        = InstructorVerificationStatus.Active;

    public DateTime? InstructorStatusUpdatedAt { get; set; }

    // Category
    public Guid CategoryId { get; set; }
    [ForeignKey(nameof(CategoryId))]
    public virtual Category Category { get; set; } = null!;

    // Basic Info
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(220)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? ShortDescription { get; set; }

    // Pricing
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? DiscountPrice { get; set; }

    public DateTime? DiscountStartDate { get; set; }
    public DateTime? DiscountEndDate { get; set; }

    // Status & Visibility
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;

    [MaxLength(10)]
    public string Language { get; set; } = "vi-VN";

    // Media
    [Required]
    public string ThumbnailUrl { get; set; } = string.Empty;

    public string? IntroVideoUrl { get; set; }

    // Learning Outcomes (JSONB)
    [Column(TypeName = "jsonb")]
    public string Outcomes { get; set; } = "[]";  // ["outcome 1", "outcome 2"]

    [Column(TypeName = "jsonb")]
    public string? Requirements { get; set; }  // ["requirement 1"]

    [Column(TypeName = "jsonb")]
    public string? TargetAudience { get; set; }  // ["student type 1"]

    // Statistics (denormalized)
    public int TotalStudents { get; set; } = 0;
    public int TotalSections { get; set; } = 0;
    public int TotalLessons { get; set; } = 0;
    public int TotalDurationMinutes { get; set; } = 0;

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? AvgRating { get; set; }

    public int TotalReviews { get; set; } = 0;
    public int TotalRatings { get; set; } = 0;

    // SEO
    [MaxLength(160)]
    public string? MetaDescription { get; set; }

    [Column(TypeName = "jsonb")]
    public string? MetaKeywords { get; set; }

    // Publishing
    public DateTime? PublishedAt { get; set; }
    public DateTime? LastUpdatedContentAt { get; set; }

    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Relationships
    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
    public virtual ICollection<CourseDocument> Documents { get; set; } = new List<CourseDocument>();
}

public enum CourseStatus
{
    Draft = 0,
    UnderReview = 1,
    Published = 2,
    Archived = 3,
    Suspended = 4  // By admin or instructor hidden
}

public enum CourseLevel
{
    Beginner = 0,
    Intermediate = 1,
    Advanced = 2,
    AllLevels = 3
}

public enum InstructorVerificationStatus
{
    Active = 0,
    Pending = 1,
    Hidden = 2,
    Suspended = 3
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_course_slug ON courses(slug) WHERE deleted_at IS NULL;
CREATE INDEX idx_course_instructor ON courses(instructor_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_course_instructor_status ON courses(instructor_id, instructor_status) WHERE deleted_at IS NULL;
CREATE INDEX idx_course_category ON courses(category_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_course_status ON courses(status) WHERE deleted_at IS NULL;
CREATE INDEX idx_course_published ON courses(published_at DESC) WHERE status = 2 AND deleted_at IS NULL;
CREATE INDEX idx_course_price ON courses(price) WHERE status = 2 AND deleted_at IS NULL;
CREATE INDEX idx_course_rating ON courses(avg_rating DESC NULLS LAST) WHERE status = 2 AND deleted_at IS NULL;
CREATE INDEX idx_course_students ON courses(total_students DESC) WHERE status = 2 AND deleted_at IS NULL;
```

---

### **Section**

```csharp
public class Section : BaseEntity
{
    public Guid CourseId { get; set; }
    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int OrderIndex { get; set; }  // 1-based

    public bool IsPublished { get; set; } = true;

    // Statistics (denormalized)
    public int TotalLessons { get; set; } = 0;
    public int TotalDurationMinutes { get; set; } = 0;

    // Relationships
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
```

**Indexes:**
```sql
CREATE INDEX idx_section_course ON sections(course_id, order_index) WHERE deleted_at IS NULL;
```

---

### **Lesson** (Media Merged)

```csharp
public class Lesson : BaseEntity
{
    public Guid SectionId { get; set; }
    [ForeignKey(nameof(SectionId))]
    public virtual Section Section { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public LessonType Type { get; set; } = LessonType.Video;

    public int OrderIndex { get; set; }  // 1-based

    public bool IsPreview { get; set; } = false;
    public bool IsPublished { get; set; } = true;

    // Video-specific fields
    public string? VideoHlsUrl { get; set; }
    public string? VideoS3Key { get; set; }
    public string? VideoThumbnailUrl { get; set; }

    public int? DurationSeconds { get; set; }

    [Column(TypeName = "jsonb")]
    public string? VideoQualities { get; set; }  // {"360p": "url", "720p": "url"}

    [Column(TypeName = "jsonb")]
    public string? VideoSubtitles { get; set; }  // [{"lang": "vi", "url": "..."}]

    // Text/Article content
    public string? TextContent { get; set; }  // HTML/Markdown

    // External content
    public string? ExternalUrl { get; set; }  // YouTube, Vimeo, etc.

    [MaxLength(50)]
    public string? ExternalProvider { get; set; }  // "youtube", "vimeo"

    // Quiz reference (Logical reference to Assessment Service)
    public Guid? QuizId { get; set; }

    // Assignment reference (Logical reference to Assessment Service)
    public Guid? AssignmentId { get; set; }

    // Completion tracking
    public int MinCompletionSeconds { get; set; } = 0;
    public int RequiredScore { get; set; } = 0;  // For quizzes (percentage)

    // Statistics
    public int TotalViews { get; set; } = 0;
    public int TotalCompletions { get; set; } = 0;

    // Relationships
    public virtual ICollection<LessonDocument> Documents { get; set; } = new List<LessonDocument>();
}

public enum LessonType
{
    Video = 0,
    Text = 1,
    Quiz = 2,
    Assignment = 3,
    LiveSession = 4,
    ExternalContent = 5
}
```

**Indexes:**
```sql
CREATE INDEX idx_lesson_section ON lessons(section_id, order_index) WHERE deleted_at IS NULL;
CREATE INDEX idx_lesson_preview ON lessons(is_preview) WHERE is_published = true AND deleted_at IS NULL;
CREATE INDEX idx_lesson_quiz ON lessons(quiz_id) WHERE quiz_id IS NOT NULL AND deleted_at IS NULL;
CREATE INDEX idx_lesson_assignment ON lessons(assignment_id) WHERE assignment_id IS NOT NULL AND deleted_at IS NULL;
```

---

### **CourseDocument**

```csharp
public class CourseDocument : BaseEntity
{
    public Guid CourseId { get; set; }
    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Reference to Media Service (Integration Service)
    // Full CloudFront URL from MediaFile
    [Required]
    public string MediaFileUrl { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    // Statistics
    public int DownloadCount { get; set; } = 0;

    // Vector DB (for AI search/RAG)
    public Guid? VectorDbId { get; set; }
    public bool IsIndexedInVectorDb { get; set; } = false;
}
```

**Note:** File metadata (name, size, type, etc.) is retrieved from Integration Service via API:
- `GET /api/v1/media/file-info?cloudFrontUrl={url}` - Get file metadata
- `GET /api/v1/media/download?cloudFrontUrl={url}` - Generate download URL

### **LessonDocument**

```csharp
public class LessonDocument : BaseEntity
{
    public Guid LessonId { get; set; }
    [ForeignKey(nameof(LessonId))]
    public virtual Lesson Lesson { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Reference to Media Service (Integration Service)
    [Required]
    public string MediaFileUrl { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    // Statistics
    public int DownloadCount { get; set; } = 0;

    // Vector DB (for AI search/RAG)
    public Guid? VectorDbId { get; set; }
    public bool IsIndexedInVectorDb { get; set; } = false;
}
```

**Note:** Same approach as CourseDocument - all file metadata managed by Integration Service.

**Indexes:**
```sql
CREATE INDEX idx_course_doc_course ON course_documents(course_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_course_doc_vector ON course_documents(vector_db_id) WHERE vector_db_id IS NOT NULL;
CREATE INDEX idx_lesson_doc_lesson ON lesson_documents(lesson_id) WHERE deleted_at IS NULL;
```

---

### **Events Published by Course Catalog Service**

```csharp
// 1. Course published
public record CoursePublishedEvent
{
    public Guid CourseId { get; init; }
    public Guid InstructorId { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public DateTime PublishedAt { get; init; }
}

// 2. Course updated
public record CourseUpdatedEvent
{
    public Guid CourseId { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int TotalLessons { get; init; }
    public int TotalDurationMinutes { get; init; }
    public DateTime UpdatedAt { get; init; }
}

// 3. Course archived
public record CourseArchivedEvent
{
    public Guid CourseId { get; init; }
    public Guid InstructorId { get; init; }
    public DateTime ArchivedAt { get; init; }
    public string? Reason { get; init; }
}
```

---

### **Events Consumed by Course Catalog Service**

```csharp
// From Identity Service
- InstructorVerificationChangedEvent
- InstructorProfileUpdatedEvent

// From Sales Service
- OrderCompletedEvent (increment TotalStudents)

// From Learning Service
- CourseRatingUpdatedEvent (update AvgRating, TotalReviews)
```

---

## 3️⃣ Assessment Service

**Database:** `Assessments`
**Responsibility:** Question bank, quizzes, assignments

### **Question** (Tag-based Bank)

```csharp
public class Question : BaseEntity
{
    public Guid InstructorId { get; set; }  // Logical reference

    [Required]
    public string Content { get; set; } = string.Empty;

    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

    // Options for MCQ (JSONB)
    [Column(TypeName = "jsonb")]
    public string? Options { get; set; }  // [{"id":"a","text":"Option 1","isCorrect":true}]

    // Answer for other types
    public string? CorrectAnswer { get; set; }

    [MaxLength(1000)]
    public string? Explanation { get; set; }

    // Tags (replaces Category)
    [Column(TypeName = "jsonb")]
    public List<string> Tags { get; set; } = new List<string>();  // ["javascript", "async"]

    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;

    public int Points { get; set; } = 1;

    // Statistics
    public int TimesUsed { get; set; } = 0;
    public int TimesCorrect { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? SuccessRate { get; set; }

    public bool IsPublic { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

public enum QuestionType
{
    MultipleChoice = 0,
    TrueFalse = 1,
    ShortAnswer = 2,
    Essay = 3,
    Matching = 4,
    Coding = 5
}

public enum DifficultyLevel
{
    Easy = 0,
    Medium = 1,
    Hard = 2
}
```

**Indexes:**
```sql
CREATE INDEX idx_question_instructor ON questions(instructor_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_question_type ON questions(type) WHERE deleted_at IS NULL;
CREATE INDEX idx_question_difficulty ON questions(difficulty) WHERE deleted_at IS NULL;
CREATE INDEX idx_question_tags ON questions USING gin(tags);
CREATE INDEX idx_question_public ON questions(is_public, is_active) WHERE deleted_at IS NULL;
```

---

### **Quiz**

```csharp
public class Quiz : BaseEntity
{
    public Guid LessonId { get; set; }  // Logical reference to Course Catalog

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int TimeLimitMinutes { get; set; } = 30;

    public int PassingScore { get; set; } = 70;  // Percentage

    public int MaxAttempts { get; set; } = 0;  // 0 = unlimited

    public bool ShuffleQuestions { get; set; } = true;
    public bool ShuffleOptions { get; set; } = true;
    public bool ShowCorrectAnswers { get; set; } = true;
    public bool ShowExplanations { get; set; } = true;

    // Settings (JSONB for flexibility)
    [Column(TypeName = "jsonb")]
    public string? Settings { get; set; }  // {"allowReview": true, "showTimer": true}

    public bool IsActive { get; set; } = true;

    // Statistics
    public int TotalAttempts { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? AvgScore { get; set; }

    public int TotalQuestions { get; set; } = 0;

    // Relationships
    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_quiz_lesson ON quizzes(lesson_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_quiz_active ON quizzes(is_active) WHERE deleted_at IS NULL;
```

---

### **QuizQuestion** (Join Table with Order)

```csharp
public class QuizQuestion
{
    public Guid QuizId { get; set; }
    [ForeignKey(nameof(QuizId))]
    public virtual Quiz Quiz { get; set; } = null!;

    public Guid QuestionId { get; set; }
    [ForeignKey(nameof(QuestionId))]
    public virtual Question Question { get; set; } = null!;

    public int OrderIndex { get; set; }
    public int Points { get; set; } = 1;
}
```

**Composite PK:**
```sql
ALTER TABLE quiz_questions ADD PRIMARY KEY (quiz_id, question_id);
CREATE INDEX idx_quiz_question_order ON quiz_questions(quiz_id, order_index);
```

---

### **Assignment**

```csharp
public class Assignment : BaseEntity
{
    public Guid LessonId { get; set; }  // Logical reference to Course Catalog

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Instruction { get; set; } = string.Empty;

    // Attachments
    [Column(TypeName = "jsonb")]
    public string? AttachmentUrls { get; set; }  // ["url1", "url2"]

    public int MaxScore { get; set; } = 100;

    public int DueDays { get; set; } = 7;  // Days after enrollment

    public bool AllowLateSubmission { get; set; } = true;
    public int LatePenaltyPercent { get; set; } = 10;

    // Submission settings
    [Column(TypeName = "jsonb")]
    public string? SubmissionSettings { get; set; }  // {"fileTypes": ["pdf","docx"], "maxFileSize": 10}

    // Rubric (JSONB)
    [Column(TypeName = "jsonb")]
    public string? Rubric { get; set; }  // [{criterion, maxPoints, description}]

    public bool IsActive { get; set; } = true;

    // Statistics
    public int TotalSubmissions { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? AvgScore { get; set; }
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_assignment_lesson ON assignments(lesson_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_assignment_active ON assignments(is_active) WHERE deleted_at IS NULL;
```

---

### **Events Published by Assessment Service**

```csharp
// 1. Quiz completed
public record QuizCompletedEvent
{
    public Guid QuizId { get; init; }
    public Guid LessonId { get; init; }
    public Guid UserId { get; init; }
    public decimal Score { get; init; }
    public bool Passed { get; init; }
    public DateTime CompletedAt { get; init; }
}

// 2. Assignment submitted
public record AssignmentSubmittedEvent
{
    public Guid AssignmentId { get; init; }
    public Guid LessonId { get; init; }
    public Guid UserId { get; init; }
    public DateTime SubmittedAt { get; init; }
}

// 3. Assignment graded
public record AssignmentGradedEvent
{
    public Guid AssignmentId { get; init; }
    public Guid UserId { get; init; }
    public decimal Grade { get; init; }
    public DateTime GradedAt { get; init; }
}
```

---

## 4️⃣ Learning Service

**Database:** `Learning`
**Responsibility:** Student progress tracking, enrollments

### **Enrollment**

```csharp
public class Enrollment : BaseEntity
{
    public Guid UserId { get; set; }  // Logical reference to Identity
    public Guid CourseId { get; set; }  // Logical reference to Course Catalog

    // Denormalized course data (snapshot at enrollment time)
    [Required, MaxLength(200)]
    public string CourseTitle { get; set; } = string.Empty;

    public string? CourseThumbnailUrl { get; set; }

    public Guid InstructorId { get; set; }

    [MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PricePaid { get; set; } = 0;

    // Status
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    // Progress
    [Column(TypeName = "decimal(5, 2)")]
    public decimal ProgressPercent { get; set; } = 0;

    public int CompletedLessons { get; set; } = 0;
    public int TotalLessons { get; set; } = 0;

    // Timestamps
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? CertificateIssuedAt { get; set; }

    public Guid? CertificateId { get; set; }

    // Access control
    public DateTime? ExpiresAt { get; set; }  // For time-limited courses
    public bool HasLifetimeAccess { get; set; } = true;

    // Last activity
    public DateTime? LastAccessedAt { get; set; }
    public Guid? LastAccessedLessonId { get; set; }
}

public enum EnrollmentStatus
{
    Active = 0,
    Completed = 1,
    Refunded = 2,
    Expired = 3,
    Suspended = 4
}
```

**Indexes:**
```sql
CREATE INDEX idx_enrollment_user ON enrollments(user_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_enrollment_course ON enrollments(course_id) WHERE deleted_at IS NULL;
CREATE UNIQUE INDEX idx_enrollment_user_course ON enrollments(user_id, course_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_enrollment_status ON enrollments(status) WHERE deleted_at IS NULL;
CREATE INDEX idx_enrollment_instructor ON enrollments(instructor_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_enrollment_completed ON enrollments(completed_at DESC) WHERE completed_at IS NOT NULL;
```

---

### **LessonProgress**

```csharp
public class LessonProgress : BaseEntity
{
    public Guid UserId { get; set; }  // Logical reference
    public Guid LessonId { get; set; }  // Logical reference
    public Guid CourseId { get; set; }  // For filtering

    public LessonProgressStatus Status { get; set; } = LessonProgressStatus.NotStarted;

    // Video progress
    public int LastPosition { get; set; } = 0;  // Seconds
    public int TotalDuration { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal WatchPercent { get; set; } = 0;

    // Quiz/Assignment results
    public int? QuizAttempts { get; set; }
    public decimal? QuizBestScore { get; set; }

    public bool? AssignmentSubmitted { get; set; }
    public decimal? AssignmentGrade { get; set; }

    // Timestamps
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}

public enum LessonProgressStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2
}
```

**Composite PK:**
```sql
ALTER TABLE lesson_progress ADD PRIMARY KEY (user_id, lesson_id);
CREATE INDEX idx_lesson_progress_course ON lesson_progress(course_id, user_id);
CREATE INDEX idx_lesson_progress_status ON lesson_progress(status);
CREATE INDEX idx_lesson_progress_updated ON lesson_progress(last_accessed_at DESC);
```

---

### **QuizSubmission**

```csharp
public class QuizSubmission : BaseEntity
{
    public Guid QuizId { get; set; }  // Logical reference
    public Guid UserId { get; set; }  // Logical reference
    public Guid EnrollmentId { get; set; }  // For relationship

    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;

    public int AttemptNumber { get; set; } = 1;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal Score { get; set; } = 0;

    public bool IsPassed { get; set; } = false;

    // Answers (JSONB)
    [Column(TypeName = "jsonb")]
    public string Answers { get; set; } = "[]";  // [{questionId, answer, isCorrect}]

    public int TimeSpentSeconds { get; set; } = 0;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
```

**Indexes:**
```sql
CREATE INDEX idx_quiz_submission_quiz ON quiz_submissions(quiz_id, user_id);
CREATE INDEX idx_quiz_submission_user ON quiz_submissions(user_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_quiz_submission_enrollment ON quiz_submissions(enrollment_id) WHERE deleted_at IS NULL;
```

---

### **AssignmentSubmission**

```csharp
public class AssignmentSubmission : BaseEntity
{
    public Guid AssignmentId { get; set; }  // Logical reference
    public Guid UserId { get; set; }  // Logical reference
    public Guid EnrollmentId { get; set; }

    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;

    public int AttemptNumber { get; set; } = 1;

    // Submission files
    [Column(TypeName = "jsonb")]
    public string FileUrls { get; set; } = "[]";  // ["url1", "url2"]

    [MaxLength(2000)]
    public string? TextSubmission { get; set; }

    // Grading
    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Grade { get; set; }

    [MaxLength(2000)]
    public string? Feedback { get; set; }

    public Guid? GradedBy { get; set; }
    public DateTime? GradedAt { get; set; }

    public AssignmentSubmissionStatus Status { get; set; } = AssignmentSubmissionStatus.Submitted;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public bool IsLate { get; set; } = false;
}

public enum AssignmentSubmissionStatus
{
    Submitted = 0,
    Graded = 1,
    Returned = 2,
    Resubmitted = 3
}
```

**Indexes:**
```sql
CREATE INDEX idx_assignment_submission_assignment ON assignment_submissions(assignment_id, user_id);
CREATE INDEX idx_assignment_submission_user ON assignment_submissions(user_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_assignment_submission_enrollment ON assignment_submissions(enrollment_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_assignment_submission_grading ON assignment_submissions(status) WHERE status = 0;
```

---

### **CourseReview**

```csharp
public class CourseReview : BaseEntity
{
    public Guid CourseId { get; set; }  // Logical reference
    public Guid UserId { get; set; }  // Logical reference
    public Guid EnrollmentId { get; set; }

    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;

    public int Rating { get; set; }  // 1-5

    [MaxLength(2000)]
    public string? Review { get; set; }

    // Detailed ratings
    public int? ContentQuality { get; set; }  // 1-5
    public int? InstructorQuality { get; set; }  // 1-5
    public int? ValueForMoney { get; set; }  // 1-5

    public bool IsVerifiedPurchase { get; set; } = true;
    public bool IsPublished { get; set; } = true;

    // Helpfulness
    public int HelpfulCount { get; set; } = 0;
    public int NotHelpfulCount { get; set; } = 0;

    // Moderation
    public bool IsFlagged { get; set; } = false;

    [MaxLength(500)]
    public string? FlagReason { get; set; }

    public Guid? ModeratedBy { get; set; }
    public DateTime? ModeratedAt { get; set; }
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_course_review_user_course ON course_reviews(user_id, course_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_course_review_course ON course_reviews(course_id) WHERE is_published = true AND deleted_at IS NULL;
CREATE INDEX idx_course_review_rating ON course_reviews(rating DESC) WHERE is_published = true AND deleted_at IS NULL;
CREATE INDEX idx_course_review_helpful ON course_reviews(helpful_count DESC) WHERE is_published = true AND deleted_at IS NULL;
```

---

### **Certificate**

```csharp
public class Certificate : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;

    public Guid UserId { get; set; }  // Logical reference
    public Guid CourseId { get; set; }  // Logical reference

    [Required, MaxLength(100)]
    public string CertificateNumber { get; set; } = string.Empty;  // CERT-2026-XXXXX

    // Denormalized data (for certificate display)
    [Required, MaxLength(200)]
    public string StudentName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string CourseTitle { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    public DateTime CompletionDate { get; set; }
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

    // Certificate file
    public string? CertificatePdfUrl { get; set; }
    public string? CertificateImageUrl { get; set; }

    // Verification
    [Required, MaxLength(64)]
    public string VerificationHash { get; set; } = string.Empty;

    public bool IsValid { get; set; } = true;
    public DateTime? RevokedAt { get; set; }

    [MaxLength(500)]
    public string? RevocationReason { get; set; }
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_certificate_number ON certificates(certificate_number);
CREATE UNIQUE INDEX idx_certificate_enrollment ON certificates(enrollment_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_certificate_user ON certificates(user_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_certificate_course ON certificates(course_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_certificate_verification ON certificates(verification_hash);
CREATE INDEX idx_certificate_issued ON certificates(issued_date DESC) WHERE deleted_at IS NULL;
```

---

### **Events Published by Learning Service**

```csharp
// 1. Enrollment created
public record EnrollmentCreatedEvent
{
    public Guid EnrollmentId { get; init; }
    public Guid UserId { get; init; }
    public Guid CourseId { get; init; }
    public Guid InstructorId { get; init; }
    public decimal PricePaid { get; init; }
    public DateTime EnrolledAt { get; init; }
}

// 2. Course completed
public record CourseCompletedEvent
{
    public Guid EnrollmentId { get; init; }
    public Guid UserId { get; init; }
    public Guid CourseId { get; init; }
    public Guid InstructorId { get; init; }
    public DateTime CompletedAt { get; init; }
}

// 3. Course reviewed
public record CourseReviewedEvent
{
    public Guid CourseId { get; init; }
    public Guid UserId { get; init; }
    public int Rating { get; init; }
    public DateTime ReviewedAt { get; init; }
}

// 4. Course rating updated (aggregate)
public record CourseRatingUpdatedEvent
{
    public Guid CourseId { get; init; }
    public decimal AvgRating { get; init; }
    public int TotalReviews { get; init; }
    public int TotalRatings { get; init; }
}

// 5. Lesson completed
public record LessonCompletedEvent
{
    public Guid LessonId { get; init; }
    public Guid UserId { get; init; }
    public Guid CourseId { get; init; }
    public DateTime CompletedAt { get; init; }
}
```

---

### **Events Consumed by Learning Service**

```csharp
// From Sales Service
- OrderCompletedEvent → Create Enrollment

// From Course Catalog Service
- CourseArchivedEvent → Suspend Enrollments
- CourseLessonsUpdatedEvent → Update TotalLessons

// From Assessment Service
- QuizCompletedEvent → Update LessonProgress
- AssignmentGradedEvent → Update LessonProgress
```

---

## 5️⃣ Sales Service

**Database:** `Sales`
**Responsibility:** Orders, payments, wallet, escrow (14-day logic)

### **Order**

```csharp
public class Order : BaseEntity
{
    public Guid UserId { get; set; }  // Logical reference

    [Required, MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;  // ORD-2026-XXXXX

    // Denormalized user data
    [Required, MaxLength(200)]
    public string UserEmail { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string UserName { get; set; } = string.Empty;

    // Pricing
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Subtotal { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DiscountAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; } = 0;

    [MaxLength(10)]
    public string Currency { get; set; } = "VND";

    // Coupon
    public Guid? CouponId { get; set; }

    [MaxLength(50)]
    public string? CouponCode { get; set; }

    // Status
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Settlement (14-day escrow logic)
    public bool IsSettled { get; set; } = false;
    public DateTime? SettledAt { get; set; }
    public DateTime? SettlementEligibleAt { get; set; }  // CreatedAt + 14 days

    // Payment
    public Guid? PaymentId { get; set; }

    [MaxLength(50)]
    public string? PaymentMethod { get; set; }  // "momo", "vnpay", "stripe"

    [MaxLength(100)]
    public string? PaymentTransactionId { get; set; }

    public DateTime? PaidAt { get; set; }

    // Refund
    public DateTime? RefundRequestedAt { get; set; }
    public DateTime? RefundedAt { get; set; }

    [MaxLength(500)]
    public string? RefundReason { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? RefundAmount { get; set; }

    // Relationships
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3,
    PartiallyRefunded = 4,
    Cancelled = 5
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_order_number ON orders(order_number);
CREATE INDEX idx_order_user ON orders(user_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_order_status ON orders(status) WHERE deleted_at IS NULL;
CREATE INDEX idx_order_paid ON orders(paid_at DESC) WHERE status = 1 AND deleted_at IS NULL;
CREATE INDEX idx_order_settlement ON orders(settlement_eligible_at) WHERE is_settled = false AND status = 1;
CREATE INDEX idx_order_payment_tx ON orders(payment_transaction_id) WHERE payment_transaction_id IS NOT NULL;
```

---

### **OrderItem** (Snapshot)

```csharp
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    public Guid CourseId { get; set; }  // Logical reference (snapshot)
    public Guid InstructorId { get; set; }  // Snapshot (for revenue split)

    // Course snapshot at purchase time
    [Required, MaxLength(200)]
    public string CourseTitle { get; set; } = string.Empty;

    public string? CourseThumbnailUrl { get; set; }

    [Required, MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    // Pricing
    [Column(TypeName = "decimal(18, 2)")]
    public decimal OriginalPrice { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DiscountPrice { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal FinalPrice { get; set; } = 0;

    // Revenue split (snapshot of platform commission)
    [Column(TypeName = "decimal(5, 2)")]
    public decimal PlatformCommissionPercent { get; set; } = 20;  // 20%

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PlatformCommissionAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal InstructorEarnings { get; set; } = 0;
}
```

**Indexes:**
```sql
CREATE INDEX idx_order_item_order ON order_items(order_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_order_item_course ON order_items(course_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_order_item_instructor ON order_items(instructor_id) WHERE deleted_at IS NULL;
```

---

### **InstructorWallet**

```csharp
public class InstructorWallet : BaseEntity
{
    public Guid InstructorId { get; set; }  // Logical reference (UNIQUE)

    // Denormalized instructor info
    [Required, MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string InstructorEmail { get; set; } = string.Empty;

    // Balance
    [Column(TypeName = "decimal(18, 2)")]
    public decimal AvailableBalance { get; set; } = 0;  // Can withdraw now

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PendingBalance { get; set; } = 0;  // Locked (14-day escrow)

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalEarnings { get; set; } = 0;  // Lifetime

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalWithdrawn { get; set; } = 0;  // Lifetime

    [MaxLength(10)]
    public string Currency { get; set; } = "VND";

    // Bank info (JSONB - encrypted in real system)
    [Column(TypeName = "jsonb")]
    public string? BankInfo { get; set; }  // {bank_name, account_number, account_holder}

    public bool IsActive { get; set; } = true;
    public DateTime? SuspendedAt { get; set; }

    [MaxLength(500)]
    public string? SuspensionReason { get; set; }

    // Relationships
    public virtual ICollection<TransactionLedger> Transactions { get; set; } = new List<TransactionLedger>();
    public virtual ICollection<PayoutRequest> PayoutRequests { get; set; } = new List<PayoutRequest>();
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_wallet_instructor ON instructor_wallets(instructor_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_wallet_active ON instructor_wallets(is_active) WHERE deleted_at IS NULL;
CREATE INDEX idx_wallet_available ON instructor_wallets(available_balance DESC) WHERE is_active = true AND deleted_at IS NULL;
```

---

### **TransactionLedger** ⭐ Core Logic (14-day Escrow)

```csharp
public class TransactionLedger : BaseEntity
{
    public Guid WalletId { get; set; }
    [ForeignKey(nameof(WalletId))]
    public virtual InstructorWallet Wallet { get; set; } = null!;

    public Guid ReferenceId { get; set; }  // OrderId, PayoutId, RefundId

    [MaxLength(50)]
    public string ReferenceType { get; set; } = string.Empty;  // "Order", "Payout", "Refund"

    public TransactionType Type { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal BalanceBefore { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal BalanceAfter { get; set; } = 0;

    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    // 14-DAY ESCROW LOGIC
    public DateTime? AvailableAt { get; set; }  // When funds become available (Order Date + 14d)

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? ExternalTransactionId { get; set; }
}

public enum TransactionType
{
    Sale = 0,           // From order
    Refund = 1,         // Deduct from wallet
    Payout = 2,         // Withdrawal
    Settlement = 3,     // Move from pending to available
    PlatformFee = 4,    // Commission deduction
    Adjustment = 5      // Manual correction
}

public enum TransactionStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3
}
```

**Indexes:**
```sql
CREATE INDEX idx_transaction_wallet ON transaction_ledger(wallet_id, created_at DESC);
CREATE INDEX idx_transaction_reference ON transaction_ledger(reference_id, reference_type);
CREATE INDEX idx_transaction_type ON transaction_ledger(type) WHERE deleted_at IS NULL;
CREATE INDEX idx_transaction_status ON transaction_ledger(status) WHERE deleted_at IS NULL;
CREATE INDEX idx_transaction_available ON transaction_ledger(available_at) WHERE status = 0 AND available_at IS NOT NULL;
```

---

### **PayoutRequest**

```csharp
public class PayoutRequest : BaseEntity
{
    public Guid WalletId { get; set; }
    [ForeignKey(nameof(WalletId))]
    public virtual InstructorWallet Wallet { get; set; } = null!;

    [Required, MaxLength(50)]
    public string RequestNumber { get; set; } = string.Empty;  // PAYOUT-2026-XXXXX

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? ProcessingFee { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? NetAmount { get; set; }

    // Bank info snapshot (JSONB)
    [Column(TypeName = "jsonb")]
    public string BankInfo { get; set; } = string.Empty;

    public PayoutStatus Status { get; set; } = PayoutStatus.Requested;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }

    public Guid? ProcessedBy { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? ExternalTransactionId { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }
}

public enum PayoutStatus
{
    Requested = 0,
    Approved = 1,
    Processing = 2,
    Completed = 3,
    Rejected = 4,
    Failed = 5
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_payout_number ON payout_requests(request_number);
CREATE INDEX idx_payout_wallet ON payout_requests(wallet_id, created_at DESC);
CREATE INDEX idx_payout_status ON payout_requests(status) WHERE deleted_at IS NULL;
CREATE INDEX idx_payout_requested ON payout_requests(requested_at DESC) WHERE deleted_at IS NULL;
CREATE INDEX idx_payout_pending ON payout_requests(status) WHERE status IN (0, 1, 2);
```

---

### **Coupon**

```csharp
public class Coupon : BaseEntity
{
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public CouponType Type { get; set; } = CouponType.Percentage;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DiscountValue { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MaxDiscountAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MinOrderAmount { get; set; }

    // Scope
    public Guid? ApplicableInstructorId { get; set; }  // null = all instructors
    public Guid? ApplicableCourseId { get; set; }  // null = all courses

    [Column(TypeName = "jsonb")]
    public string? ApplicableCategoryIds { get; set; }  // ["cat1", "cat2"]

    // Usage limits
    public int? UsageLimit { get; set; }  // null = unlimited
    public int UsageCount { get; set; } = 0;

    public int? UsageLimitPerUser { get; set; }

    // Validity
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; set; }

    public bool IsActive { get; set; } = true;

    // Creator
    public Guid? CreatedByInstructorId { get; set; }  // For instructor-created coupons
    public bool IsPlatformCoupon { get; set; } = false;
}

public enum CouponType
{
    Percentage = 0,
    FixedAmount = 1
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_coupon_code ON coupons(code) WHERE deleted_at IS NULL;
CREATE INDEX idx_coupon_active ON coupons(is_active, valid_from, valid_until) WHERE deleted_at IS NULL;
CREATE INDEX idx_coupon_instructor ON coupons(applicable_instructor_id) WHERE applicable_instructor_id IS NOT NULL;
CREATE INDEX idx_coupon_course ON coupons(applicable_course_id) WHERE applicable_course_id IS NOT NULL;
```

---

### **Events Published by Sales Service**

```csharp
// 1. Order completed (paid)
public record OrderCompletedEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public List<OrderItemData> Items { get; init; } = new();
    public decimal TotalAmount { get; init; }
    public DateTime PaidAt { get; init; }
}

public class OrderItemData
{
    public Guid CourseId { get; init; }
    public Guid InstructorId { get; init; }
    public decimal FinalPrice { get; init; }
    public decimal InstructorEarnings { get; init; }
}

// 2. Order refunded
public record OrderRefundedEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public List<Guid> CourseIds { get; init; } = new();
    public decimal RefundAmount { get; init; }
    public DateTime RefundedAt { get; init; }
    public string? Reason { get; init; }
}

// 3. Funds settled (14 days passed)
public record FundsSettledEvent
{
    public Guid WalletId { get; init; }
    public Guid InstructorId { get; init; }
    public decimal Amount { get; init; }
    public DateTime SettledAt { get; init; }
}

// 4. Payout completed
public record PayoutCompletedEvent
{
    public Guid PayoutId { get; init; }
    public Guid InstructorId { get; init; }
    public decimal Amount { get; init; }
    public DateTime CompletedAt { get; init; }
}
```

---

### **Events Consumed by Sales Service**

```csharp
// From Course Catalog Service
- CoursePublishedEvent → Allow purchasing
- CourseArchivedEvent → Disable purchasing

// From Identity Service
- InstructorVerificationChangedEvent → Update wallet, suspend if hidden
```

---

## 6️⃣ Analytics Service (CQRS - Read Model)

**Database:** `Analytics`
**Responsibility:** Aggregated reports, dashboards (Event-sourced)

### **AggSystemOverview** (Admin Dashboard)

```csharp
public class AggSystemOverview : BaseEntity
{
    public DateTime ReportDate { get; set; }  // Daily snapshot

    // Users
    public int TotalUsers { get; set; } = 0;
    public int TotalActiveUsers { get; set; } = 0;
    public int NewUsersToday { get; set; } = 0;
    public int TotalInstructors { get; set; } = 0;
    public int TotalStudents { get; set; } = 0;

    // Courses
    public int TotalCourses { get; set; } = 0;
    public int TotalPublishedCourses { get; set; } = 0;
    public int NewCoursesToday { get; set; } = 0;

    // Enrollments
    public int TotalEnrollments { get; set; } = 0;
    public int NewEnrollmentsToday { get; set; } = 0;
    public int TotalCompletedEnrollments { get; set; } = 0;

    // Revenue
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRevenue { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRevenueToday { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PendingPayout { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalPayouts { get; set; } = 0;

    // Refunds
    public int TotalRefunds { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRefundAmount { get; set; } = 0;

    // Platform earnings
    [Column(TypeName = "decimal(18, 2)")]
    public decimal PlatformEarnings { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PlatformEarningsToday { get; set; } = 0;
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_system_overview_date ON agg_system_overview(report_date) WHERE deleted_at IS NULL;
CREATE INDEX idx_system_overview_recent ON agg_system_overview(report_date DESC);
```

---

### **AggInstructorRevenue** (Financial Stats)

```csharp
public class AggInstructorRevenue : BaseEntity
{
    public Guid InstructorId { get; set; }  // Logical reference

    [Required, MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    [Required, MaxLength(7)]
    public string Month { get; set; } = string.Empty;  // "YYYY-MM"

    // Sales
    public int TotalOrders { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalSales { get; set; } = 0;

    // Refunds
    public int TotalRefunds { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRefundAmount { get; set; } = 0;

    // Platform commission
    [Column(TypeName = "decimal(18, 2)")]
    public decimal PlatformCommission { get; set; } = 0;

    // Net earnings
    [Column(TypeName = "decimal(18, 2)")]
    public decimal NetEarnings { get; set; } = 0;  // Sales - Refunds - Commission

    // Payouts
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalPayouts { get; set; } = 0;

    // Balance
    [Column(TypeName = "decimal(18, 2)")]
    public decimal PendingBalance { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal AvailableBalance { get; set; } = 0;
}
```

**Composite UNIQUE:**
```sql
CREATE UNIQUE INDEX idx_instructor_revenue_month ON agg_instructor_revenue(instructor_id, month) WHERE deleted_at IS NULL;
CREATE INDEX idx_instructor_revenue_earnings ON agg_instructor_revenue(net_earnings DESC);
CREATE INDEX idx_instructor_revenue_recent ON agg_instructor_revenue(month DESC);
```

---

### **AggCourseStats** (Course Performance)

```csharp
public class AggCourseStats : BaseEntity
{
    public Guid CourseId { get; set; }  // Logical reference

    [Required, MaxLength(200)]
    public string CourseTitle { get; set; } = string.Empty;

    public Guid InstructorId { get; set; }

    [MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    // Enrollments
    public int TotalStudents { get; set; } = 0;
    public int TotalCompletedStudents { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal CompletionRate { get; set; } = 0;

    // Ratings
    [Column(TypeName = "decimal(3, 2)")]
    public decimal? AvgRating { get; set; }

    public int TotalReviews { get; set; } = 0;
    public int TotalRatings { get; set; } = 0;

    // Revenue
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRevenue { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRefundAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal NetRevenue { get; set; } = 0;

    // Engagement
    public int TotalViews { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal AvgWatchTime { get; set; } = 0;  // Percentage

    // Last updated
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_course_stats_course ON agg_course_stats(course_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_course_stats_instructor ON agg_course_stats(instructor_id);
CREATE INDEX idx_course_stats_students ON agg_course_stats(total_students DESC);
CREATE INDEX idx_course_stats_revenue ON agg_course_stats(net_revenue DESC);
CREATE INDEX idx_course_stats_rating ON agg_course_stats(avg_rating DESC NULLS LAST);
```

---

### **AggLessonPerformance** (Lesson Quality)

```csharp
public class AggLessonPerformance : BaseEntity
{
    public Guid LessonId { get; set; }  // Logical reference

    [Required, MaxLength(200)]
    public string LessonTitle { get; set; } = string.Empty;

    public Guid CourseId { get; set; }
    public Guid InstructorId { get; set; }

    // Views
    public int TotalViews { get; set; } = 0;
    public int UniqueViewers { get; set; } = 0;

    // Completion
    public int TotalCompletions { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal CompletionRate { get; set; } = 0;

    // Watch time
    [Column(TypeName = "decimal(5, 2)")]
    public decimal AvgWatchPercent { get; set; } = 0;

    public int AvgWatchTimeSeconds { get; set; } = 0;

    // Drop-off
    [Column(TypeName = "jsonb")]
    public string? DropOffPoints { get; set; }  // [{timestamp, dropOffPercent}]

    // Last updated
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
```

**Indexes:**
```sql
CREATE UNIQUE INDEX idx_lesson_perf_lesson ON agg_lesson_performance(lesson_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_lesson_perf_course ON agg_lesson_performance(course_id);
CREATE INDEX idx_lesson_perf_completion ON agg_lesson_performance(completion_rate DESC);
CREATE INDEX idx_lesson_perf_watch ON agg_lesson_performance(avg_watch_percent DESC);
```

---

### **Events Consumed by Analytics Service (CQRS)**

Analytics service consumes ALL events from other services to build aggregates:

```csharp
// From Identity Service
- UserRegisteredEvent
- InstructorVerificationChangedEvent

// From Course Catalog Service
- CoursePublishedEvent
- CourseUpdatedEvent
- CourseArchivedEvent

// From Learning Service
- EnrollmentCreatedEvent
- CourseCompletedEvent
- CourseReviewedEvent
- LessonCompletedEvent

// From Sales Service
- OrderCompletedEvent
- OrderRefundedEvent
- FundsSettledEvent
- PayoutCompletedEvent

// From Assessment Service
- QuizCompletedEvent
- AssignmentSubmittedEvent
```

---

## 7️⃣ Integration Service (Existing - Common Infrastructure)

**Database:** `Integrations`
**Responsibility:** Notifications, media storage, AI usage, eKYC

### **Notification**

```csharp
public class Notification : BaseEntity
{
    public Guid? UserId { get; set; }  // null for broadcast

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public NotificationTarget Target { get; set; } = NotificationTarget.User;

    public List<NotificationChannel> Channels { get; set; } = new();

    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    [Column(TypeName = "jsonb")]
    public string? Data { get; set; }  // Additional payload

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public DateTime? SentAt { get; set; }
}

public enum NotificationTarget
{
    User = 0,
    AllUser = 1,
    AllInstructor = 2,
    AllAdmin = 3,
    AllStaff = 4
}

public enum NotificationChannel
{
    App = 0,
    Email = 1,
    Sms = 2,
    Push = 3
}

public enum NotificationStatus
{
    Pending = 0,
    Delivered = 1,
    Failed = 2
}
```

---

### **MediaFile**

```csharp
public class MediaFile : BaseEntity
{
    public Guid UserId { get; set; }  // Uploader

    [Required, MaxLength(50)]
    public string Folder { get; set; } = string.Empty;  // "avatars", "certificates", "documents"

    [MaxLength(100)]
    public string? SubFolder { get; set; }

    [Required, MaxLength(100)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string S3Key { get; set; } = string.Empty;

    public string? CloudFrontUrl { get; set; }

    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; } = 0;

    public MediaFileStatus Status { get; set; } = MediaFileStatus.Pending;

    public DateTime? UploadedAt { get; set; }
}

public enum MediaFileStatus
{
    Pending = 0,
    Uploaded = 1,
    Failed = 2,
    Deleted = 3
}
```

---

### **AiUsage**

```csharp
public class AiUsage : BaseEntity
{
    public Guid UserId { get; set; }  // Logical reference

    [Required, MaxLength(100)]
    public string Provider { get; set; } = string.Empty;  // "Gemini", "Bedrock"

    [Required, MaxLength(100)]
    public string ModelName { get; set; } = string.Empty;

    public AiOperation Operation { get; set; }

    public Guid? PromptId { get; set; }

    public int InputTokens { get; set; } = 0;
    public int OutputTokens { get; set; } = 0;
    public int TotalTokens { get; set; } = 0;

    [Column(TypeName = "decimal(18, 6)")]
    public decimal InputCost { get; set; } = 0;

    [Column(TypeName = "decimal(18, 6)")]
    public decimal OutputCost { get; set; } = 0;

    [Column(TypeName = "decimal(18, 6)")]
    public decimal TotalCost { get; set; } = 0;

    public int ResponseTimeMs { get; set; } = 0;

    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
}

public enum AiOperation
{
    InstructorProfileReview = 0,
    ContentModeration = 1,
    QuestionGeneration = 2,
    SummaryGeneration = 3,
    Other = 99
}
```

---

## 🔄 Cross-Service Event Flow Examples

### **Example 1: Student Purchases Course**

```
1. User adds course to cart → Sales Service
   ├─► Creates Order (status: Pending)
   └─► Returns order_id

2. User completes payment → Sales Service
   ├─► Updates Order (status: Paid, paid_at: now)
   ├─► Creates OrderItems (snapshot course & instructor data)
   ├─► Creates TransactionLedger (type: Sale, status: Pending)
   │    - amount: instructor_earnings
   │    - available_at: now + 14 days
   ├─► Updates InstructorWallet (pending_balance += amount)
   │
   └─► Publishes: OrderCompletedEvent
        {
          order_id, user_id,
          items: [{course_id, instructor_id, final_price, instructor_earnings}],
          total_amount, paid_at
        }

3. Learning Service consumes OrderCompletedEvent
   ├─► Creates Enrollment (status: Active)
   │    - Snapshot: course_title, thumbnail, instructor_name, price_paid
   │    - Sets: total_lessons (from course data)
   │
   └─► Publishes: EnrollmentCreatedEvent

4. Course Catalog Service consumes EnrollmentCreatedEvent
   └─► Updates Course.TotalStudents += 1

5. Analytics Service consumes OrderCompletedEvent
   ├─► Updates AggSystemOverview (total_revenue, new_enrollments_today)
   ├─► Updates AggInstructorRevenue (total_sales, pending_balance)
   └─► Updates AggCourseStats (total_students, total_revenue)

6. Integration Service consumes EnrollmentCreatedEvent
   └─► Sends Notification to User (type: EnrollmentSuccess)
```

---

### **Example 2: Instructor Profile Hidden by Admin**

```
1. Admin hides instructor → Identity Service
   ├─► Updates InstructorProfile (verification_status: Hidden)
   │
   └─► Publishes: InstructorVerificationChangedEvent
        {
          instructor_id, user_id, full_name,
          old_status: Verified, new_status: Hidden,
          reason: "Policy violation"
        }

2. Course Catalog Service consumes InstructorVerificationChangedEvent
   ├─► Finds all courses where instructor_id = instructor_id
   ├─► Updates each course:
   │    - instructor_status = Hidden
   │    - status = Suspended (if was Published)
   │    - instructor_status_updated_at = now
   │
   ├─► Updates Redis cache: instructor_status:{instructor_id} = "Hidden"
   │
   └─► Publishes: CourseArchivedEvent (for each course)

3. Sales Service consumes InstructorVerificationChangedEvent
   ├─► Finds InstructorWallet by instructor_id
   └─► Updates wallet:
        - is_active = false
        - suspended_at = now
        - suspension_reason = event.reason

4. Learning Service consumes CourseArchivedEvent
   └─► No action needed (students retain access to purchased courses)

5. Integration Service consumes InstructorVerificationChangedEvent
   └─► Sends Notification to Instructor (type: ProfileHidden)
```

---

### **Example 3: Funds Settlement (14 Days Passed)**

```
1. Background Job runs daily → Sales Service
   ├─► Query TransactionLedger:
   │    WHERE type = 'Sale'
   │    AND status = 'Pending'
   │    AND available_at <= NOW()
   │
   ├─► For each transaction:
   │    ├─► Update transaction (status: Completed)
   │    ├─► Update InstructorWallet:
   │    │    - pending_balance -= amount
   │    │    - available_balance += amount
   │    │
   │    ├─► Create new TransactionLedger (type: Settlement)
   │    │
   │    └─► Publish: FundsSettledEvent
   │         {wallet_id, instructor_id, amount, settled_at}
   │
   └─► Update Order (is_settled: true, settled_at: now)

2. Analytics Service consumes FundsSettledEvent
   └─► Updates AggInstructorRevenue:
        - pending_balance -= amount
        - available_balance += amount

3. Integration Service consumes FundsSettledEvent
   └─► Sends Notification to Instructor (type: FundsAvailable)
```

---

## 📈 Database Statistics & Sizing Estimates

### **Expected Record Counts (After 1 Year)**

| Service | Entity | Estimated Records |
|---------|--------|-------------------|
| Identity | User | 100,000 |
| Identity | InstructorProfile | 1,000 |
| Course Catalog | Category | 100 |
| Course Catalog | Course | 5,000 |
| Course Catalog | Section | 25,000 |
| Course Catalog | Lesson | 125,000 |
| Assessment | Question | 50,000 |
| Assessment | Quiz | 100,000 |
| Assessment | Assignment | 25,000 |
| Learning | Enrollment | 200,000 |
| Learning | LessonProgress | 5,000,000 |
| Learning | QuizSubmission | 500,000 |
| Learning | CourseReview | 50,000 |
| Sales | Order | 150,000 |
| Sales | TransactionLedger | 500,000 |
| Sales | PayoutRequest | 10,000 |
| Analytics | AggCourseStats | 5,000 |

---

## 🔒 Security Considerations

### **1. Sensitive Data Encryption**

Fields requiring encryption:
- `InstructorProfile.BankInfo` (JSONB)
- `InstructorWallet.BankInfo` (JSONB)
- `PayoutRequest.BankInfo` (JSONB)
- User passwords (hashed with PasswordHasher<User>)

**Implementation:**
```csharp
// Use column-level encryption in EF Core
[EncryptedColumn]
public string BankInfo { get; set; } = string.Empty;
```

---

### **2. PII Data Protection**

Apply GDPR-compliant soft delete:
- All entities inherit `BaseEntity` with `DeletedAt`
- Implement "Right to be Forgotten" via cascade soft delete
- Anonymize data instead of hard delete where audit trail required

---

### **3. Row-Level Security (PostgreSQL)**

```sql
-- Example: Instructors can only see their own wallet
CREATE POLICY instructor_wallet_policy ON instructor_wallets
  FOR SELECT
  USING (instructor_id = current_user_id());
```

---

## 🚀 Performance Optimization

### **1. Database Indexes**

All tables have comprehensive indexes (see entity definitions above).

### **2. Denormalization Strategy**

Denormalized fields for performance:
- Course → Instructor data (name, avatar, rating, status)
- Enrollment → Course data (title, thumbnail, instructor)
- Order → Course & instructor snapshots
- Statistics fields (total_students, total_lessons, avg_rating)

### **3. Caching Strategy**

**Redis Cache Keys:**
```
instructor_status:{instructor_id}      TTL: 24h
course_detail:{course_slug}            TTL: 1h
course_list:{category}:{page}          TTL: 10m
enrollment:{user_id}:{course_id}       TTL: 1h
```

---

## 📝 Migration Strategy

### **Phase 1: Core Services (MVP)**
1. Identity Service
2. Course Catalog Service
3. Integration Service (existing)

### **Phase 2: Learning & Sales**
4. Learning Service
5. Sales Service (basic order + payment)

### **Phase 3: Assessment & Analytics**
6. Assessment Service
7. Analytics Service (CQRS read models)

### **Phase 4: Advanced Features**
8. Sales Service (14-day escrow + wallet)
9. Certificate generation
10. Advanced analytics & reporting

---

## 🎯 Key Design Decisions Summary

| Decision | Rationale |
|----------|-----------|
| **Database per Service** | Microservices independence, loose coupling |
| **Logical References (no FK)** | Avoid cross-service joins, resilience |
| **Denormalized Data** | Performance, reduced cross-service calls |
| **Event-Driven Sync** | Eventual consistency, scalability |
| **Soft Delete Pattern** | Audit trail, GDPR compliance |
| **UUID v7** | Distributed system friendly, time-sortable |
| **JSONB for Flexibility** | Schema evolution, complex nested data |
| **CQRS for Analytics** | Optimized read models, performance |
| **14-Day Escrow** | Refund protection, instructor trust |
| **Snapshot Pattern** | Historical data integrity (orders, enrollments) |

---

## 📚 Next Steps

1. **Generate EF Core Migrations** for each service
2. **Implement Event Contracts** (shared library)
3. **Setup RabbitMQ Exchanges & Queues**
4. **Implement Consumers** for each service
5. **Add Integration Tests** for event flows
6. **Setup Monitoring** (OpenTelemetry, Aspire Dashboard)
7. **Document API Contracts** (OpenAPI/Swagger)
8. **Implement Saga Pattern** for distributed transactions (order → enrollment)

---

**End of Document**
