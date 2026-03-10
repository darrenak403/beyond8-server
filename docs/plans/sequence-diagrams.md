# Sequence Diagrams (Mermaid)

Controllers and EmailService (no consumers). Paste each block from `sequenceDiagram`.

---

## 1. Instructor registration and approval

User apply → AI profile review → Admin approve or reject → EmailService.

```mermaid
sequenceDiagram
    autonumber
    actor User
    actor Admin
    participant "InstructorController" as :InstructorController
    participant "AiController" as :AiController
    participant "EmailService" as :EmailService

    rect rgb(230, 245, 255)
        User->>+:InstructorController: POST /api/v1/instructors/apply (CreateInstructorProfileRequest)
        :InstructorController->>:InstructorController: ValidateRequest (validator)
        :InstructorController->>:InstructorController: GetUser, check existing profile (reject if Pending/Verified/RequestUpdate)
        :InstructorController->>:InstructorController: Create InstructorProfile, AddAsync, SaveChanges
        :InstructorController->>:InstructorController: Publish InstructorProfileSubmittedEvent
        :InstructorController-->>-User: 200 OK – Application submitted successfully
    end

    rect rgb(255, 248, 220)
        Admin->>+:AiController: POST /api/v1/ai/profile-review (ProfileReviewRequest)
        :AiController->>:AiController: GetPrompt, build prompt, GenerateContentAsync (Gemini), parse AiProfileReviewResponse
        :AiController-->>-Admin: 200 OK – AiProfileReviewResponse (IsAccepted, TotalScore, Details)
    end

    rect rgb(220, 255, 220)
        alt Approved
            Admin->>+:InstructorController: POST /api/v1/instructors/{id}/approve
            :InstructorController->>:InstructorController: ValidateProfileForReviewAsync, get ROLE_INSTRUCTOR, add UserRole, set profile Verified/VerifiedAt/VerifiedBy, SaveChanges
            :InstructorController->>:InstructorController: Publish InstructorApprovalEvent
            :InstructorController->>:EmailService: SendInstructorApprovalEmailAsync
            :InstructorController-->>-Admin: 200 OK

        else Reject or request update
            Admin->>+:InstructorController: POST /api/v1/instructors/{id}/not-approve (NotApproveInstructorProfileRequest)
            :InstructorController->>:InstructorController: ValidateProfileForReviewAsync, set VerificationStatus + VerificationNotes, SaveChanges
            :InstructorController->>:InstructorController: Publish InstructorUpdateRequestEvent
            :InstructorController->>:EmailService: SendInstructorUpdateRequestEmailAsync
            :InstructorController-->>-Admin: 200 OK
        end
    end
```

---

## 2. Course lifecycle: create, submit, approve, publish

Instructor: create course → section → lesson → submit for approval. Admin: approve. Instructor: publish.

```mermaid
sequenceDiagram
    autonumber
    actor Instructor
    actor Admin
    participant "CourseController" as :CourseController
    participant "SectionController" as :SectionController
    participant "LessonController" as :LessonController

    rect rgb(230, 245, 255)
        Instructor->>+:CourseController: POST /api/v1/courses (CreateCourseRequest)
        :CourseController->>:CourseController: Validate category, get instructor, ToEntity, AddAsync Course, SaveChanges
        :CourseController->>:CourseController: Publish CourseCreatedEvent
        :CourseController-->>-Instructor: 200 OK – CourseResponse

        Instructor->>+:SectionController: POST /api/v1/sections (CreateSectionRequest, courseId)
        :SectionController->>:SectionController: CheckCourseOwnership, max OrderIndex, ToEntity, AddAsync Section, SaveChanges
        :SectionController-->>-Instructor: 200 OK – SectionResponse

        Instructor->>+:LessonController: POST /api/v1/lessons/video | /text | /quiz (Create…LessonRequest, sectionId)
        :LessonController->>:LessonController: CheckSectionOwnership, OrderIndex, AddAsync Lesson + type, UpdateSectionStatistics, SaveChanges
        :LessonController-->>-Instructor: 200 OK – LessonResponse
    end

    rect rgb(255, 248, 220)
        Instructor->>+:CourseController: POST /api/v1/courses/{id}/submit-approval
        :CourseController->>:CourseController: Validate Draft, min 1 section, min 3 lessons, all videos HLS, Status = PendingApproval, SaveChanges
        :CourseController->>:CourseController: Publish CourseSubmittedForApprovalEvent
        :CourseController-->>-Instructor: 200 OK
    end

    rect rgb(220, 255, 220)
        Admin->>+:CourseController: POST /api/v1/courses/{id}/approve (ApproveCourseRequest)
        :CourseController->>:CourseController: Validate PendingApproval, Status = Approved, ApprovalNotes, SaveChanges
        :CourseController->>:CourseController: Publish CourseApprovedEvent
        :CourseController-->>-Admin: 200 OK
    end

    rect rgb(230, 230, 255)
        Instructor->>+:CourseController: POST /api/v1/courses/{id}/publish
        :CourseController->>:CourseController: Validate Approved, Status = Published, SaveChanges
        :CourseController->>:CourseController: Publish CoursePublishedEvent
        :CourseController-->>-Instructor: 200 OK
    end
```

---

## 3. Student purchases course

Student: create order → process payment. Callback: Order=Paid, Publish OrderCompletedEvent.

```mermaid
sequenceDiagram
    autonumber
    actor Student
    participant "OrderController" as :OrderController
    participant "PaymentController" as :PaymentController

    rect rgb(230, 245, 255)
        Student->>+:OrderController: POST /api/v1/orders/buy-now or POST /api/v1/cart/checkout
        :OrderController->>:OrderController: Validate, get course price (Catalog), create Order (Pending), SaveChanges
        :OrderController-->>-Student: 200 OK – OrderResponse
    end

    rect rgb(255, 248, 220)
        Student->>+:PaymentController: POST /api/v1/payments/process (OrderId, callbackUrl)
        :PaymentController->>:PaymentController: Validate Order Pending, TotalAmount > 0, create Payment (Pending), GenerateVNPayUrl, SaveChanges
        :PaymentController-->>-Student: 200 OK – PaymentUrlResponse
    end

    rect rgb(220, 255, 220)
        :PaymentController->>:PaymentController: GET callback: ValidateCallback, Payment=Completed, Order=Paid, SaveChanges, Publish OrderCompletedEvent, redirect frontend
    end
```

---

## 4. Student learns course and certificate issuance

Student: enrollments → curriculum progress → course/lesson details → heartbeat. System: try issue certificate. Student: get certificates.

```mermaid
sequenceDiagram
    autonumber
    actor Student
    participant "EnrollmentController" as :EnrollmentController
    participant "CourseController" as :CourseController
    participant "LessonController" as :LessonController
    participant "CertificateController" as :CertificateController

    rect rgb(230, 245, 255)
        Student->>+:EnrollmentController: GET /api/v1/enrollments/me or GET /api/v1/enrollments/{id}
        :EnrollmentController->>:EnrollmentController: Get enrolled courses / enrollment by id
        :EnrollmentController-->>-Student: 200 OK – Enrollment list / EnrollmentResponse

        Student->>+:EnrollmentController: GET /api/v1/enrollments/{id}/curriculum-progress
        :EnrollmentController->>:EnrollmentController: Get sections, lessons, section progress (LessonProgress, quiz/assignment state)
        :EnrollmentController-->>-Student: 200 OK – CurriculumProgressResponse
    end

    rect rgb(255, 248, 220)
        Student->>+:CourseController: GET /api/v1/courses/{id}/details
        :CourseController->>:CourseController: Validate enrollment (Learning), return course details for student
        :CourseController-->>-Student: 200 OK – CourseDetailResponse

        Student->>+:LessonController: GET /api/v1/lessons/{id} or GET /api/v1/lessons/{lessonId}/video
        :LessonController->>:LessonController: Validate enrollment, return lesson content / video URL
        :LessonController-->>-Student: 200 OK – LessonResponse / LessonVideoResponse
    end

    rect rgb(220, 255, 220)
        Student->>+:EnrollmentController: PUT /api/v1/enrollments/lesson/{lessonId}/heartbeat (position, completed)
        :EnrollmentController->>:EnrollmentController: Update LessonProgress, SectionProgress, SaveChanges
        :EnrollmentController->>:CertificateController: TryIssueCertificateIfEligibleAsync(enrollmentId)
        :CertificateController->>:CertificateController: Check all lessons completed, quiz/assignment config, create Certificate, Publish CourseCompletedEvent
        :EnrollmentController-->>-Student: 200 OK – LessonProgressResponse
    end

    rect rgb(230, 230, 255)
        Student->>+:CertificateController: GET /api/v1/certificates/me or GET /api/v1/certificates/{id}
        :CertificateController->>:CertificateController: Get my certificates / certificate by id
        :CertificateController-->>-Student: 200 OK – Certificate list / CertificateDetailResponse
    end
```
