# Sequence Diagrams (Mermaid)

Tham chiếu: [UML Sequence Diagram – Visual Paradigm](https://www.visual-paradigm.com/guide/uml-unified-modeling-language/what-is-sequence-diagram/).  
Quy ước: **Actor**, **Lifeline** (participant), **Call** (->>), **Return** (-->>). Paste từng block vào `sequenceDiagram`.

---

## 1. Instructor registration and approval

User nộp hồ sơ → Admin AI review → Admin duyệt/từ chối → EmailService gửi mail.

```mermaid
sequenceDiagram
    autonumber
    actor User
    actor Admin
    participant InstructorController
    participant AiController
    participant EmailService

    User->>InstructorController: apply
    InstructorController->>InstructorController: validate, create profile, Publish(Submitted)
    InstructorController-->>User: 200 OK

    Admin->>AiController: profile-review
    AiController->>AiController: GenerateContent, parse response
    AiController-->>Admin: AiProfileReviewResponse

    alt Approved
        Admin->>InstructorController: approve(id)
        InstructorController->>InstructorController: add role, Verified, Publish(Approval)
        InstructorController->>EmailService: SendApprovalEmail
        InstructorController-->>Admin: 200 OK
    else Reject / Request update
        Admin->>InstructorController: not-approve(id, notes)
        InstructorController->>InstructorController: set status, Publish(UpdateRequest)
        InstructorController->>EmailService: SendUpdateRequestEmail
        InstructorController-->>Admin: 200 OK
    end
```

---

## 2. Course lifecycle: create, submit, approve, publish

Instructor tạo course → section → lesson → submit. Admin approve. Instructor publish.

```mermaid
sequenceDiagram
    autonumber
    actor Instructor
    actor Admin
    participant CourseController
    participant SectionController
    participant LessonController

    Instructor->>CourseController: create course
    CourseController-->>Instructor: CourseResponse

    Instructor->>SectionController: create section
    SectionController-->>Instructor: SectionResponse

    Instructor->>LessonController: create lesson
    LessonController-->>Instructor: LessonResponse

    Instructor->>CourseController: submit-approval
    CourseController->>CourseController: validate, Publish(SubmittedForApproval)
    CourseController-->>Instructor: 200 OK

    Admin->>CourseController: approve
    CourseController->>CourseController: set Approved, Publish(Approved)
    CourseController-->>Admin: 200 OK

    Instructor->>CourseController: publish
    CourseController->>CourseController: set Published, Publish(Published)
    CourseController-->>Instructor: 200 OK
```

---

## 3. Student purchases course

Student tạo order → thanh toán → callback: Order=Paid, Publish(OrderCompletedEvent).

```mermaid
sequenceDiagram
    autonumber
    actor Student
    participant OrderController
    participant PaymentController

    Student->>OrderController: buy-now / checkout
    OrderController->>OrderController: create Order
    OrderController-->>Student: OrderResponse

    Student->>PaymentController: process
    PaymentController->>PaymentController: create Payment, VNPay URL
    PaymentController-->>Student: PaymentUrlResponse

    Note over PaymentController: Callback: Order=Paid, Publish(OrderCompletedEvent)
```

---

## 4. Student learns course and certificate

Student: enrollments, curriculum progress, course/lesson, heartbeat. Hệ thống: thử cấp certificate. Student: lấy certificates.

```mermaid
sequenceDiagram
    autonumber
    actor Student
    participant EnrollmentController
    participant CourseController
    participant LessonController
    participant CertificateController

    Student->>EnrollmentController: GET enrollments/me
    EnrollmentController-->>Student: Enrollment list

    Student->>EnrollmentController: GET curriculum-progress
    EnrollmentController-->>Student: CurriculumProgressResponse

    Student->>CourseController: GET course details
    CourseController-->>Student: CourseDetailResponse

    Student->>LessonController: GET lesson / video
    LessonController-->>Student: LessonResponse

    Student->>EnrollmentController: PUT lesson heartbeat
    EnrollmentController->>EnrollmentController: update progress
    EnrollmentController->>CertificateController: TryIssueCertificateIfEligible
    CertificateController->>CertificateController: create Certificate, Publish(CourseCompleted)
    EnrollmentController-->>Student: 200 OK

    Student->>CertificateController: GET certificates/me
    CertificateController-->>Student: Certificate list
```
