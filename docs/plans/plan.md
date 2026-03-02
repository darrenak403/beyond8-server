# 📘 Kế hoạch Phát triển Nền tảng Học Trực tuyến

**Dự án:** Online Learning Platform (Udemy-like) với AI Support  
**Timeline:** 9 tuần (63 ngày) | **Ngày bắt đầu:** 7/1/2026  
**Team:** 4 người
**Tech Stack:** .NET 9 + Next.js 16 + PostgreSQL + AWS + Gemini AI

---

## 1. Tổng quan Dự án

### Mục tiêu Chính

Xây dựng nền tảng học trực tuyến với:

- **Người dùng:** Guest → Student → Instructor → Admin
- **Nội dung:** Khóa học (Video, Quiz, Assignment, Tài liệu)
- **Thanh toán:** VNPay integration + Refund policy (7 days)
- **AI:** Quiz generation + Smart grading + OCR documents

### Priority Levels

| Level  | Mô tả           | Modules                                |
| ------ | --------------- | -------------------------------------- |
| **P0** | Must-have (MVP) | MOD-01, 02, 03, 04, 06, 07, 10         |
| **P1** | Should-have     | MOD-05 (AI), 08 (Forum), 09 (Rating)   |
| **P2** | Nice-to-have    | Chatbot, Analytics advanced, Community |

---

## 2. 10 Modules - Tóm tắt

### MOD-01: User Management (P0)

- **4 Roles:** Guest, Student, Instructor, Admin (phân quyền theo trang)
- **Xác thực:** Register + Email verification, Login JWT, Password reset
- **Instructor Application:** Submit CV → Admin approve/reject → Auto-upgrade role
- **Quy tắc:** BR-01 (Authentication), BR-02 (Instructor approval)
- **Chi tiết:** Xem `/requirements/01-USER-MANAGEMENT.md`

### MOD-02: Course Management (P0)

- **Danh mục:** Parent Category (Lập trình, Kinh doanh...) → Sub-Category (Web Dev, iOS...)
- **Lifecycle:** Draft → PendingApproval → Published/Rejected
- **Curriculum:** Sections + Lessons (Video, Quiz, Assignment, Document)
- **Preview:** Instructor đánh dấu ≤3 videos preview → Guest có thể xem
- **Catalog:** Search + Filter (category tree, price, difficulty, rating), Sort (popular/new/rating/price)
- **Quy tắc:** BR-03 (Course approval), BR-04 (Access control)
- **Chi tiết:** Xem `/requirements/02-COURSE-MANAGEMENT.md`

### MOD-03: Video Learning (P1)

- **Upload:** 2 cách - YouTube embed hoặc S3 cloud upload
- **S3 Flow:** Upload → AWS Lambda transcode (HLS 360p/720p/1080p) → CloudFront CDN
- **Player:** Adaptive bitrate, speed control, quality selector, fullscreen
- **Tracking:** Polling 30s, auto-complete ≥95%, resume from last position
- **Preview:** Watermark "PREVIEW" cho Guest users
- **Quy tắc:** BR-04 (Access), BR-06 (Video limits: max 30 phút, 3GB, MP4/MOV/AVI)
- **Chi tiết:** Xem `/requirements/03-VIDEO-LEARNING.md`

### MOD-04: Quiz & Assignment (P0)

- **Quiz:** MC + Essay questions, timer, randomization (Fisher-Yates), auto-grade MC
- **Assignment:** File/Text/Code submission, late penalty
- **Grading:** Auto (MC), Manual, AI-assisted (Gemini) với confidence score
- **Gradebook:** Students × Assignments matrix, export CSV/Excel
- **Quy tắc:** BR-09 (Quiz distribution), BR-10 (AI grading)
- **Chi tiết:** Xem `/requirements/04-QUIZ-ASSIGNMENT.md`

### MOD-05: AI Features (P0)

- **Document Upload:** PDF/DOCX/EPUB → OCR → Text extraction → S3
- **Quiz Generation:** Từ video transcript hoặc document → Gemini → Generate MC questions → Instructor preview/edit
- **Smart Grading:** Essay/Assignment → Gemini → Score + feedback + confidence → If confidence <70% flag for manual review
- **Rate Limit:** Max 5 quiz generations/hour per instructor
- **Quy tắc:** BR-07 (Upload limits), BR-10 (AI grading)
- **Chi tiết:** Xem `/requirements/05-AI-FEATURES.md`

### MOD-06: Progress Tracking & Certificate (P0)

- **Auto-tracking:** Video (polling 30s), Quiz/Assignment (on submit)
- **Dashboard:** Course list với progress bar, filter (In Progress/Completed/Not Started), section breakdown
- **Learning Streak:** Track consecutive learning days (≥1 lesson/day)
- **Certificate:** Auto-generate PDF khi 100% completion → S3 → QR verification
- **Analytics Instructor:** Student interactions, lesson performance, quiz/assignment stats
- **Analytics Student:** Performance report, strengths/weaknesses, course recommendations
- **Quy tắc:** BR-04 (Progress), BR-12 (Certificate)
- **Chi tiết:** Xem `/requirements/06-PROGRESS-TRACKING.md`

### MOD-07: Payment & Enrollment (P0)

- **Khóa học miễn phí:** Enroll Free → Ghi danh ngay
- **Khóa học trả phí:** VNPay payment → Callback validation → Ghi danh + access granted
- **Refund:** 7 days, <10% progress, no quiz/assignment completed → Admin approve/reject
- **Instructor Revenue:** Total revenue, revenue by course, transaction list, export
- **Admin Revenue:** Platform analytics, top courses/instructors, transaction logs
- **Quy tắc:** BR-04 (Enrollment), BR-05 (Refund policy), BR-11 (Payment)
- **Chi tiết:** Xem `/requirements/07-PAYMENT-ENROLLMENT.md`

### MOD-09: Rating & Review (P2)

- **Submit:** 1-5 stars (required), text (50-2000 chars, after ≥50% completion)
- **Display:** Avg rating, distribution chart, sort (helpful/newest/rating), filter (by stars)
- **Helpful vote:** +1 per user per review
- **Moderation:** Report → Admin hide/delete
- **Quy tắc:** BR-12 (Rating calculation)
- **Chi tiết:** Xem `/requirements/08-RATING-REVIEW.md`

### MOD-10: Admin Panel (P0)

- **Dashboard:** Metrics (users, courses, revenue), charts (revenue trend, new users, top courses)
- **User Management:** Search, filter (role, status), ban/unban, change role, export CSV
- **Course Approval:** Queue, preview course, approve/reject + feedback → Publish/Draft
- **Instructor Applications:** List pending, approve (grant role)/reject (send reason)
- **Refund Management:** Eligibility check, approve/reject, VNPay refund API, logs
- **System Settings:** Revenue share, refund policy, VNPay credentials, email templates
- **Activity Logs:** Audit trail all admin actions, export CSV
- **Quy tắc:** BR-01, BR-02, BR-03
- **Chi tiết:** Xem `/requirements/09-ADMIN-PANEL.md`

## 3. Kiến trúc Hệ thống

---

## 3. Kiến trúc Hệ thống

### Tech Stack

| Layer             | Technology                                                    |
| ----------------- | ------------------------------------------------------------- |
| **Frontend**      | Next.js 16 + React 19 + TypeScript + Tailwind CSS + shadcn/ui |
| **Backend**       | .NET 9 + ASP.NET Core + Entity Framework Core                 |
| **Database**      | PostgreSQL (relational) + MongoDB (logs) + Redis (cache)      |
| **Message Queue** | RabbitMQ (async tasks)                                        |
| **Search**        | Elasticsearch (course catalog)                                |
| **Storage**       | AWS S3 (videos, docs, images) + CloudFront CDN                |
| **Video**         | AWS Lambda + MediaConvert + Transcribe                        |
| **AI**            | Gemini Flash 2.5 API                                          |
| **Payment**       | VNPay (callback webhooks)                                     |
| **Container**     | Docker + Docker Compose                                       |
| **Monitoring**    | Serilog + Seq (centralized logging)                           |

### System Architecture

```
┌─────────────────────────────────────────┐
│     Next.js Frontend (Web + Mobile)     │
└──────────────┬──────────────────────────┘
               │ HTTPS / WebSocket
┌──────────────▼──────────────────────────┐
│     ASP.NET Core API Gateway            │
│   (Rate Limiting, Authentication)       │
└──────────────┬──────────────────────────┘
               │
    ┌──────────┼──────────┬──────────┐
    │          │          │          │
┌───▼──┐  ┌───▼──┐  ┌───▼──┐  ┌───▼──┐
│Auth  │  │Course│  │Quiz  │  │Video │
│API   │  │API   │  │API   │  │API   │
└───┬──┘  └───┬──┘  └───┬──┘  └───┬──┘
    │         │         │         │
    └─────────┼─────────┼─────────┘
              │
    ┌─────────▼─────────┐
    │  PostgreSQL DB    │
    │ (Primary Data)    │
    └───────────────────┘

┌────────────┐ ┌────────────┐ ┌─────────────┐
│ AWS S3 +   │ │ Gemini API │ │ RabbitMQ +  │
│ CloudFront │ │ (AI)       │ │ Background  │
│ + Lambda   │ │            │ │ Jobs        │
└────────────┘ └────────────┘ └─────────────┘

┌────────────┐ ┌────────────┐ ┌─────────────┐
│ MongoDB    │ │ Elastic    │ │ Redis       │
│ (Logs)     │ │ (Search)   │ │ (Cache)     │
└────────────┘ └────────────┘ └─────────────┘
```

---
