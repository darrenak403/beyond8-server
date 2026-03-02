# As-Built Sequence Diagrams (Draw.io) - Simplified

Bản này tối giản để dễ đọc khi vẽ trên draw.io.

> Lưu ý:
>
> - Mỗi lần paste 1 block Mermaid.
> - Paste từ `sequenceDiagram` (không kèm `mermaid ... `).

---

## 1) Instructor Apply -> Approve/Reject

```mermaid
sequenceDiagram
    autonumber
    actor Student
    actor Admin
    participant Identity
    participant MQ
    participant Sale
    participant Integration

    Note over Student, Identity: Giai đoạn Đăng ký
    Student->>+Identity: Apply instructor profile

    %% Kích hoạt MQ bằng dấu +
    Identity->>+MQ: InstructorProfileSubmittedEvent

    %% Hủy kích hoạt MQ bằng dấu -
    MQ-->>-Integration: Notify Admin/Staff
    deactivate Identity

    Note over Admin, Identity: Giai đoạn Phê duyệt
    Admin->>+Identity: Review & Decide (Approve/Reject)

    alt Approved
        Identity->>+MQ: InstructorApprovalEvent
        MQ->>Sale: Create instructor wallet
        MQ-->>-Integration: Send approval email/noti
    else Rejected
        Identity->>+MQ: InstructorRejectedEvent
        MQ-->>-Integration: Send rejection email
        MQ->>Student: Notify to update profile
    end
    deactivate Identity
```

---

## 2) Course Review Flow

```mermaid
sequenceDiagram
    autonumber
    actor Instructor
    actor Admin
    participant Catalog
    participant MQ
    participant Integration

    Note over Instructor, Catalog: Giai đoạn gửi duyệt
    Instructor->>Catalog: Submit course
    Catalog->>Catalog: Status = PendingApproval

    Note over Admin, Catalog: Giai đoạn kiểm duyệt
    alt Approve
        Admin->>Catalog: Approve
        Catalog->>Catalog: Status = Approved
        Catalog->>MQ: CourseApprovedEvent
        MQ->>Integration: Send approved email/noti
    else Reject
        Admin->>Catalog: Reject(reason)
        Catalog->>Catalog: Status = Rejected
        Catalog->>MQ: CourseRejectedEvent
        MQ->>Integration: Send rejected email/noti
    end
```

---

## 3) Free Enrollment

```mermaid
sequenceDiagram
    autonumber
    actor Student
    participant Learning
    participant MQ
    participant Sale

    Note over Student, Learning: Khởi tạo đăng ký
    Student->>+Learning: Enroll free course
    Learning->>Learning: Create Enrollment (Status: Pending)

    Learning->>+MQ: FreeEnrollmentOrderRequestEvent
    MQ->>+Sale: Create free Order (Status: Paid)

    Note over Sale: Xử lý hóa đơn 0đ
    Sale->>+MQ: OrderCompletedEvent

    Note over MQ, Learning: Cập nhật trạng thái học tập
    MQ-->>-Learning: Sync Enrollment Status
    Learning->>Learning: Update Status (Active)

    Learning-->>-Student: Success! Start Learning
    deactivate Sale
```

---

## 4) Paid Checkout + VNPay Callback

```mermaid
sequenceDiagram
    actor Student
    participant Sale
    participant VNPay
    participant MQ
    participant Learning

    Student->>Sale: Checkout / create order (Pending)
    Sale-->>Student: Payment URL
    Student->>VNPay: Pay
    VNPay->>Sale: Callback

    alt Success
        Sale->>Sale: Payment=Completed, Order=Paid
        Sale->>Sale: Set SettlementEligibleAt (+14d)
        Sale->>MQ: OrderCompletedEvent
        MQ->>Learning: Enroll paid courses
    else Failed
        Sale->>Sale: Payment=Failed, Order=Failed
    end
```

---

## 5) Settlement Job (Hourly)

```mermaid
sequenceDiagram
    participant Hangfire
    participant Sale
    participant MQ
    participant Analytic

    Hangfire->>Sale: ProcessPendingSettlements
    Sale->>Sale: Move Pending -> Available
    Sale->>Sale: Mark Order settled
    Sale->>MQ: SettlementCompletedEvent
    MQ->>Analytic: Update revenue aggregate
```

---

## 7) Assignment AI Grading (Async)

```mermaid
sequenceDiagram
    actor Student
    participant Assessment
    participant MQ
    participant Integration
    participant AI

    Student->>Assessment: Submit assignment
    Assessment->>MQ: AssignmentSubmittedEvent
    MQ->>Integration: Start AI grading
    Integration->>AI: Grade submission

    alt Success
        Integration->>MQ: AiGradingCompletedEvent(success)
        MQ->>Assessment: Update status = AiGraded
    else Failure
        Integration->>MQ: AiGradingCompletedEvent(failed)
        MQ->>Assessment: Update status = ManualReview
    end
```

---

## 8) Manual Grading

```mermaid
sequenceDiagram
    actor Instructor
    participant Assessment
    participant MQ
    participant Learning

    Instructor->>Assessment: Grade assignment manually
    Assessment->>MQ: AssignmentGradedEvent
    MQ->>Learning: Update progress/completion
```

---

## 9) Certificate Issuance

```mermaid
sequenceDiagram
    participant Learning
    participant CertificateService
    participant MQ
    participant Integration

    Learning->>CertificateService: Check eligibility
    alt Eligible
        CertificateService->>CertificateService: Create certificate
        CertificateService->>MQ: CourseCompletedEvent
        MQ->>Integration: Send certificate email/noti
    else Not eligible
        CertificateService->>CertificateService: Skip
    end
```

---

## 10) Video Callback

```mermaid
sequenceDiagram
    participant Lambda
    participant Catalog
    participant MQ
    participant Integration

    Lambda->>Catalog: HLS callback
    Catalog->>Catalog: Update video variants
    Catalog->>MQ: TranscodingVideoSuccessEvent
    MQ->>Integration: Notify instructor
```
