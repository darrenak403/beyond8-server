# 📚 Services Documentation - Beyond8 Server

Comprehensive documentation for all microservices in the Beyond8 system.

---

## Identity Service

Handles authentication, user management, instructor profiles, and subscriptions.

### Features

#### Authentication

- User registration with OTP verification
- Login with JWT token generation
- Password management (reset, change, forgot password)
- Refresh token implementation

#### User Management

- User profile management (CRUD operations)
- User status management (Active, Inactive, Banned)
- Avatar and cover image management
- Admin user management

#### Instructor Management

- Instructor profile creation and verification workflow
- Profile submission for approval
- Admin approval/rejection of instructor applications
- Instructor verification status tracking (Pending, Approved, Rejected, Hidden)

#### Subscription Management

- Subscription plan management
- User subscription tracking
- AI usage quota management

### API Endpoints

**Auth Endpoints** (`/api/v1/auth`):

- `POST /register` - Register new user
- `POST /verify-otp` - Verify OTP for registration
- `POST /resend-otp` - Resend OTP
- `POST /login` - User login
- `POST /refresh-token` - Refresh JWT token
- `POST /forgot-password` - Request password reset
- `POST /verify-forgot-password-otp` - Verify forgot password OTP
- `POST /reset-password` - Reset password
- `POST /change-password` - Change password (authenticated)

**User Endpoints** (`/api/v1/users`):

- `GET /me` - Get current user profile
- `PATCH /me` - Update current user profile
- `GET /` - Get all users (Admin only, paginated)
- `GET /{id}` - Get user by ID
- `POST /` - Create user (Admin only)
- `PATCH /{id}` - Update user (Admin only)

**Instructor Endpoints** (`/api/v1/instructors`):

- `GET /check` - Check instructor application status
- `POST /apply` - Apply to become instructor
- `GET /profile` - Get own instructor profile
- `PATCH /profile` - Update instructor profile
- `POST /profile/submit` - Submit profile for approval
- `GET /admin/pending` - Get pending applications (Admin/Staff)
- `GET /admin/all` - Get all instructor profiles (Admin/Staff)
- `POST /admin/{id}/approve` - Approve instructor (Admin/Staff)
- `POST /admin/{id}/reject` - Reject instructor (Admin/Staff)
- `POST /admin/{id}/hide` - Hide instructor (Admin/Staff)

**Subscription Endpoints** (`/api/v1/subscriptions`):

- `GET /my-subscription` - Get current user's subscription
- `GET /plans` - Get all subscription plans

---

## Integration Service

Handles media file uploads, storage management using AWS S3, AI integration, notifications, email, and eKYC.

### Features

#### Media File Management

- **Presigned URL Generation**: Generate secure upload URLs for client-side file uploads
- **File Upload Workflow**: 3-step process (request → upload → confirm)
- **File Management**: Get, list, and delete user files
- **Folder Organization**: Organize files by type (avatars, certificates, identity cards)
- **File Status Tracking**: Track upload status (Pending, Uploaded, Failed, Deleted)

#### AI Integration

- **AI Usage Tracking**: Track AI API calls (tokens, costs, providers)
- **AI Prompt Management**: Store and manage reusable AI prompts
- **Gemini Integration**: Integration with Google's Gemini AI service
- **Quiz Generation**: AI-powered quiz generation from course content
- **Document Embedding**: Vector embeddings for course documents (RAG support)
- **Profile Review**: AI-assisted instructor profile review
- **Usage Analytics**: Statistics and reports on AI usage per user or system-wide

#### Notifications

- **Push Notifications**: Firebase Cloud Messaging (FCM) integration
- **Notification History**: Track and retrieve user notifications
- **Read/Unread Status**: Mark notifications as read

#### Email

- **Transactional Emails**: Send OTP, approval, and notification emails
- **Email Templates**: Pre-defined templates for different email types
- **Event-Driven**: Triggered via MassTransit consumers

#### VNPT eKYC

- **ID Card OCR**: Extract information from Vietnamese ID cards
- **ID Classification**: Classify ID card types
- **Liveness Detection**: Verify user is a real person

### API Endpoints

**Media File Endpoints** (`/api/v1/media`):

- `/avatar/presigned-url` - Upload user avatar (max 5MB, images only)
- `/certificate/presigned-url` - Upload instructor certificates (max 10MB, PDF/images)
- `/identity-card/front/presigned-url` - Upload front side of ID card (max 10MB, PDF/images)
- `/identity-card/back/presigned-url` - Upload back side of ID card (max 10MB, PDF/images)
- `POST /confirm` - Confirm file upload completion
- `GET /{fileId}` - Get file information by ID
- `GET /folder/{folder}` - Get all user files in a specific folder
- `DELETE /{fileId}` - Delete a file (soft delete)

**AI Usage Endpoints** (`/api/v1/ai-usage`):

- `GET /my-usage` - Get current user's AI usage history (paginated)
- `GET /all` - Get all AI usage records (Admin only, paginated)
- `GET /user/{userId}` - Get specific user's AI usage history (Admin only, paginated)
- `GET /statistics` - Get overall AI usage statistics (Admin only)
- `GET /by-date-range` - Get AI usage within date range (Admin only, paginated)

**AI Endpoints** (`/api/v1/ai`):

- `POST /generate-quiz` - Generate quiz from content
- `POST /embed-documents` - Create vector embeddings for documents
- `POST /review-profile` - AI-assisted profile review

**AI Prompt Endpoints** (`/api/v1/ai-prompts`):

- `GET /` - Get all prompts
- `GET /{id}` - Get prompt by ID
- `POST /` - Create new prompt (Admin)
- `PATCH /{id}` - Update prompt (Admin)
- `DELETE /{id}` - Delete prompt (Admin)

**Notification Endpoints** (`/api/v1/notifications`):

- `GET /` - Get user notifications (paginated)
- `GET /status` - Get notification status (unread count)
- `POST /{id}/read` - Mark notification as read
- `POST /read-all` - Mark all notifications as read

**VNPT eKYC Endpoints** (`/api/v1/ekyc`):

- `POST /classify` - Classify ID card type
- `POST /ocr/front` - OCR front side of ID card
- `POST /ocr/back` - OCR back side of ID card
- `POST /liveness` - Liveness detection

### Storage Configuration

- **Provider**: AWS S3
- **File Key Format**: `{folder}/{userId}/{filename}` or `{folder}/{userId}/{subFolder}/{filename}`
- **Presigned URL Expiration**: 15 minutes
- **File Validation**: Type-specific validators for avatars and documents

---

## Catalog Service

Handles course management, categories, sections, and lessons for the e-learning platform.

### Features

#### Category Management

- **Hierarchical Categories**: Support for parent-child category relationships
- **Category Tree**: Get full category tree structure
- **Status Management**: Enable/disable categories

#### Course Management

- **Course Creation**: Instructors can create courses with metadata
- **Course Status Workflow**: Draft → PendingApproval → Approved → Published
- **Course Approval**: Admin/Staff can approve or reject courses
- **Instructor Verification**: Courses require verified instructor status
- **Course Statistics**: Track students, lessons, ratings, and reviews
- **JSONB Fields**: Outcomes, requirements, target audience stored as JSON

#### Section Management

- **Section CRUD**: CRUD operations for course sections
- **Section Ordering**: Reorder sections within a course
- **Ownership Validation**: Only course owner can modify sections

#### Lesson Management

- **Lesson CRUD**: CRUD operations for lessons within sections
- **Lesson Types**: Support for different lesson types (Video, Text, Quiz)
- **Lesson Ordering**: Reorder lessons within a section
- **Video Processing**: HLS callback for video lessons
- **Lesson Documents**: Attach documents to lessons

### API Endpoints

**Category Endpoints** (`/api/v1/categories`):

- `GET /tree` - Get category tree (public)
- `GET /` - Get all categories (public, paginated)
- `GET /{id}` - Get category by ID (public)
- `GET /parent/{parentId}` - Get child categories (public)
- `POST /` - Create category (Admin/Staff)
- `PUT /{id}` - Update category (Admin/Staff)
- `DELETE /{id}` - Delete category (Admin/Staff)
- `PATCH /{id}/toggle-status` - Toggle category status (Admin/Staff)

**Course Endpoints** (`/api/v1/courses`):

- `GET /` - Get all published courses (public, paginated with search)
- `GET /{id}` - Get course by ID (Instructor)
- `POST /` - Create course (Instructor, verified only)
- `DELETE /{id}` - Delete course (Instructor, owner only)
- `GET /instructor` - Get instructor's courses (Instructor)
- `GET /instructor/stats` - Get instructor's course statistics
- `POST /{id}/submit-approval` - Submit for approval (Instructor)
- `PATCH /{id}/metadata` - Update course metadata (Instructor)
- `GET /admin` - Get all courses for admin (Admin/Staff)
- `POST /{id}/approve` - Approve course (Admin/Staff)
- `POST /{id}/reject` - Reject course (Admin/Staff)
- `POST /{id}/publish` - Publish approved course (Instructor)
- `POST /{id}/unpublish` - Unpublish course (Instructor)

**Section Endpoints** (`/api/v1/sections`):

- `GET /course/{courseId}` - Get sections by course (Instructor)
- `GET /{id}` - Get section by ID (Instructor)
- `POST /` - Create section (Instructor)
- `PATCH /{id}` - Update section (Instructor)
- `DELETE /{id}` - Delete section (Instructor)
- `POST /reorder` - Reorder sections (Instructor)

**Lesson Endpoints** (`/api/v1/lessons`):

- `GET /section/{sectionId}` - Get lessons by section (Instructor)
- `GET /{id}` - Get lesson by ID (Instructor)
- `POST /` - Create lesson (Instructor)
- `PATCH /{id}` - Update lesson (Instructor)
- `DELETE /{id}` - Delete lesson (Instructor)
- `POST /reorder` - Reorder lessons (Instructor)
- `POST /video/callback` - HLS video callback (internal)

### Course Status Workflow

```
Draft → PendingApproval → Approved → Published
                       ↘ Rejected → Draft (can resubmit)
Published → Unpublished (Hidden) → Published
```

### Domain Entities

- **Category**: Hierarchical course categories
- **Course**: Main course entity with metadata and statistics
- **Section**: Course sections (chapters)
- **Lesson**: Individual lessons within sections
- **CourseDocument**: Attached documents for courses
- **LessonDocument**: Attached documents for lessons

---

## Sale Service

_(Coming soon - Order management, Payments, Coupons, Instructor Payouts)_

---

## Learning Service

_(Coming soon - Enrollments, Progress tracking, Course consumption)_

---

## Assessment Service

_(Coming soon - Quizzes, Assignments, Grading)_

---

## Event-Driven Communication

### MassTransit Events

**Event Types** (defined in `Beyond8.Common.Events`):

- `OtpEmailEvent` - Trigger OTP email sending
- `InstructorProfileSubmittedEvent` - Instructor submits profile for review
- `InstructorApprovalEvent` - Instructor profile approved
- `InstructorHiddenEvent` - Instructor profile hidden
- `InstructorUpdateRequestEvent` - Instructor requests profile update

### Inter-Service Communication

Services communicate via:

1. **HTTP Clients**: For synchronous requests (e.g., `IIdentityClient`)
2. **MassTransit Events**: For asynchronous operations (emails, notifications)

**Example HTTP Client:**

```csharp
public interface IIdentityClient : IBaseClient
{
    Task<ApiResponse<bool>> CheckInstructorProfileVerifiedAsync(Guid userId);
    Task<ApiResponse<SubscriptionResponse>> GetUserSubscriptionAsync(Guid userId);
}
```

**Retry Policy:**

- Exponential backoff: 5 retries
- Min interval: 2 seconds
- Max interval: 30 seconds
