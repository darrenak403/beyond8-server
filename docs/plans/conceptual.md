# Conceptual Data Model - Final Optimized (Microservices)

**Project:** E-Learning Platform
**Architecture:** Database per Service (Event-Driven)
**Last Updated:** January 13, 2026

---

## 1. Identity Service

_Responsibility: Authentication & User Management._

### **User**

| Attribute     | Type    | Key    | Description                      |
| :------------ | :------ | :----- | :------------------------------- |
| user_id       | UUID    | PK     |                                  |
| email         | String  | UNIQUE |                                  |
| password_hash | String  |        |                                  |
| role          | ENUM    |        | `student`, `instructor`, `admin` |
| full_name     | String  |        |                                  |
| avatar_url    | String  |        |                                  |
| is_active     | Boolean |        |                                  |

### **InstructorProfile**

| Attribute           | Type | Key | Description                       |
| :------------------ | :--- | :-- | :-------------------------------- |
| profile_id          | UUID | PK  |                                   |
| user_id             | UUID | FK  |                                   |
| bio                 | Text |     |                                   |
| bank_info           | JSON |     | Thông tin nhận tiền               |
| verification_status | ENUM |     | `pending`, `verified`, `rejected` |

---

## 2. Course Catalog Service

_Responsibility: Quản lý nội dung khóa học._

### **Category** (Self-referencing)

| Attribute   | Type    | Key    | Description               |
| :---------- | :------ | :----- | :------------------------ |
| category_id | UUID    | PK     |                           |
| parent_id   | UUID    | FK     | Null nếu là Root Category |
| name        | String  |        |                           |
| slug        | String  | UNIQUE |                           |
| level       | Integer |        |                           |

### **Course**

| Attribute     | Type    | Key | Description                      |
| :------------ | :------ | :-- | :------------------------------- |
| course_id     | UUID    | PK  |                                  |
| instructor_id | UUID    |     | **Logical Ref**                  |
| category_id   | UUID    | FK  |                                  |
| title         | String  |     |                                  |
| description   | Text    |     |                                  |
| price         | Decimal |     |                                  |
| status        | ENUM    |     | `draft`, `published`, `archived` |
| thumbnail_url | String  |     |                                  |
| outcomes      | JSON    |     |                                  |

### **Section**

| Attribute   | Type    | Key | Description |
| :---------- | :------ | :-- | :---------- |
| section_id  | UUID    | PK  |             |
| course_id   | UUID    | FK  |             |
| title       | String  |     |             |
| order_index | Integer |     |             |

### **Lesson** (Media Merged)

| Attribute         | Type    | Key | Description                     |
| :---------------- | :------ | :-- | :------------------------------ |
| lesson_id         | UUID    | PK  |                                 |
| section_id        | UUID    | FK  |                                 |
| title             | String  |     |                                 |
| type              | ENUM    |     | `video`, `quiz`, `text`         |
| is_preview        | Boolean |     |                                 |
| **video_hls_url** | String  |     | URL stream video (nếu là video) |
| **duration**      | Integer |     | Seconds                         |
| order_index       | Integer |     |                                 |

### **CourseDocument**

| Attribute  | Type   | Key | Description                          |
| :--------- | :----- | :-- | :----------------------------------- |
| doc_id     | UUID   | PK  |                                      |
| course_id  | UUID   | FK  |                                      |
| lesson_id  | UUID   | FK  |                                      |
| title      | String |     | Tên hiển thị                         |
| **s3_key** | String |     | Key S3 (VD: `courses/101/slide.pdf`) |
| file_type  | String |     | `pdf`, `docx`                        |
| file_size  | Long   |     | Bytes                                |
| vector_id  | UUID   |     | ID trong hệ thống Vector DB          |

---

## 3. Assessment Service

_Responsibility: Ngân hàng đề & Bài tập._

### **Question** (Tag-based Bank)

| Attribute     | Type  | Key | Description         |
| :------------ | :---- | :-- | :------------------ |
| question_id   | UUID  | PK  |                     |
| instructor_id | UUID  | idx |                     |
| content       | Text  |     |                     |
| type          | ENUM  |     | `mcq`, `essay`      |
| options       | JSON  |     |                     |
| **tags**      | Array | idx | (Thay thế Category) |
| difficulty    | ENUM  |     |                     |

### **Quiz**

| Attribute     | Type    | Key    | Description               |
| :------------ | :------ | :----- | :------------------------ |
| quiz_id       | UUID    | PK     |                           |
| lesson_id     | UUID    | unique | **Logical Ref** (Catalog) |
| title         | String  |        |                           |
| time_limit    | Integer |        | Minutes                   |
| passing_score | Integer |        |                           |
| settings      | JSON    |        |                           |

### **QuizQuestion**

| Attribute   | Type    | Key | Description |
| :---------- | :------ | :-- | :---------- |
| quiz_id     | UUID    | PK  |             |
| question_id | UUID    | PK  |             |
| points      | Integer |     |             |

### **Assignment**

| Attribute      | Type    | Key | Description     |
| :------------- | :------ | :-- | :-------------- |
| assignment_id  | UUID    | PK  |                 |
| section_id     | UUID    |     | **Logical Ref** |
| title          | String  |     |                 |
| instruction    | Text    |     |                 |
| attachment_url | String  |     |                 |
| max_score      | Integer |     |                 |

---

## 4. Learning Service

_Responsibility: Tiến độ học tập (Transaction)._

### **Enrollment**

| Attribute     | Type     | Key | Description                       |
| :------------ | :------- | :-- | :-------------------------------- |
| enrollment_id | UUID     | PK  |                                   |
| user_id       | UUID     | idx |                                   |
| course_id     | UUID     | idx |                                   |
| status        | ENUM     |     | `active`, `completed`, `refunded` |
| created_at    | DateTime |     |                                   |

### **LessonProgress**

| Attribute     | Type     | Key | Description            |
| :------------ | :------- | :-- | :--------------------- |
| user_id       | UUID     | PK  | Composite PK           |
| lesson_id     | UUID     | PK  |                        |
| course_id     | UUID     | idx |                        |
| status        | ENUM     |     | `started`, `completed` |
| last_position | Integer  |     | Video timestamp        |
| updated_at    | DateTime |     |                        |

### **QuizSubmission**

| Attribute     | Type    | Key | Description |
| :------------ | :------ | :-- | :---------- |
| submission_id | UUID    | PK  |             |
| quiz_id       | UUID    | idx |             |
| user_id       | UUID    | idx |             |
| score         | Decimal |     |             |
| is_passed     | Boolean |     |             |
| answers       | JSON    |     |             |

### **AssignmentSubmission**

| Attribute     | Type    | Key | Description |
| :------------ | :------ | :-- | :---------- |
| submission_id | UUID    | PK  |             |
| assignment_id | UUID    | idx |             |
| user_id       | UUID    | idx |             |
| file_url      | String  |     |             |
| grade         | Decimal |     |             |

---

## 5. Analytics Service (Aggregation Only)

_Responsibility: Dashboard & Báo cáo. Dữ liệu được update qua Event._

### **AggSystemOverview** (Admin Dashboard)

| Attribute      | Type    | Key | Description          |
| :------------- | :------ | :-- | :------------------- |
| id             | UUID    | PK  |                      |
| report_date    | Date    | idx |                      |
| total_users    | Integer |     |                      |
| total_revenue  | Decimal |     | Tổng doanh thu sàn   |
| pending_payout | Decimal |     | Tiền đang chờ trả GV |
| refund_count   | Integer |     |                      |

### **AggInstructorRevenue** (Financial Stats)

| Attribute     | Type    | Key | Description                      |
| :------------ | :------ | :-- | :------------------------------- |
| instructor_id | UUID    | PK  | Composite PK                     |
| month         | String  | PK  | `YYYY-MM`                        |
| total_sales   | Decimal |     | Doanh thu trước phí              |
| total_refunds | Decimal |     |                                  |
| net_earnings  | Decimal |     | Thực nhận (Sales - Refund - Fee) |

### **AggCourseStats** (Course Performance)

| Attribute       | Type    | Key | Description |
| :-------------- | :------ | :-- | :---------- |
| course_id       | UUID    | PK  |             |
| total_students  | Integer |     |             |
| total_completed | Integer |     |             |
| avg_rating      | Decimal |     |             |
| total_revenue   | Decimal |     |             |

### **AggLessonPerformance** (Lesson Quality)

| Attribute       | Type    | Key | Description |
| :-------------- | :------ | :-- | :---------- |
| lesson_id       | UUID    | PK  |             |
| total_views     | Integer |     |             |
| completion_rate | Decimal |     |             |

---

## 6. Sales Service (Wallet & Escrow)

_Responsibility: Quản lý Tiền & Đơn hàng (Logic 14 ngày)._

### **Order**

| Attribute      | Type     | Key | Description                             |
| :------------- | :------- | :-- | :-------------------------------------- |
| order_id       | UUID     | PK  |                                         |
| user_id        | UUID     | idx |                                         |
| total_amount   | Decimal  |     |                                         |
| status         | ENUM     |     | `pending`, `paid`, `failed`, `refunded` |
| **is_settled** | Boolean  |     | `true` nếu đã qua 14 ngày an toàn       |
| settled_at     | DateTime |     |                                         |

### **OrderItem** (Snapshot)

| Attribute         | Type    | Key | Description                      |
| :---------------- | :------ | :-- | :------------------------------- |
| item_id           | UUID    | PK  |                                  |
| order_id          | UUID    | FK  |                                  |
| course_id         | UUID    |     | **Logical Ref**                  |
| price             | Decimal |     |                                  |
| **course_title**  | String  |     | Snapshot                         |
| **instructor_id** | UUID    |     | Snapshot (để biết tiền về ví ai) |

### **InstructorWallet**

| Attribute             | Type    | Key    | Description                      |
| :-------------------- | :------ | :----- | :------------------------------- |
| wallet_id             | UUID    | PK     |                                  |
| instructor_id         | UUID    | UNIQUE |                                  |
| **available_balance** | Decimal |        | Tiền có thể rút ngay             |
| **pending_balance**   | Decimal |        | Tiền đang bị giữ (trong 14 ngày) |
| currency              | String  |        |                                  |

### **TransactionLedger** (Core Logic 14 ngày)

| Attribute        | Type     | Key | Description                                    |
| :--------------- | :------- | :-- | :--------------------------------------------- |
| transaction_id   | UUID     | PK  |                                                |
| wallet_id        | UUID     | FK  |                                                |
| ref_id           | UUID     | idx | `order_id` / `payout_id`                       |
| type             | ENUM     |     | `sale`, `refund`, `payout`, `settlement`       |
| amount           | Decimal  |     |                                                |
| status           | ENUM     |     | `pending`, `completed`                         |
| created_at       | DateTime |     |                                                |
| **available_at** | DateTime |     | Thời điểm tiền được mở khóa (Order Date + 14d) |

### **PayoutRequest**

| Attribute  | Type    | Key | Description                          |
| :--------- | :------ | :-- | :----------------------------------- |
| request_id | UUID    | PK  |                                      |
| wallet_id  | UUID    | FK  |                                      |
| amount     | Decimal |     |                                      |
| bank_info  | JSON    |     | Snapshot STK                         |
| status     | ENUM    |     | `requested`, `processed`, `rejected` |

### **Coupon**

| Attribute      | Type    | Key    | Description |
| :------------- | :------ | :----- | :---------- |
| coupon_id      | UUID    | PK     |             |
| code           | String  | UNIQUE |             |
| discount_value | Decimal |        |             |
| usage_limit    | Integer |        |             |

---

## 7. Common Service

_Responsibility: Hạ tầng chung._

### **Notification**

| Attribute | Type    | Key | Description |
| :-------- | :------ | :-- | :---------- |
| notif_id  | UUID    | PK  |             |
| user_id   | UUID    | idx |             |
| title     | String  |     |             |
| message   | Text    |     |             |
| is_read   | Boolean |     |             |

### **EmailQueue**

| Attribute  | Type   | Key | Description      |
| :--------- | :----- | :-- | :--------------- |
| email_id   | UUID   | PK  |                  |
| to_address | String |     |                  |
| subject    | String |     |                  |
| body       | Text   |     |                  |
| status     | ENUM   |     | `queued`, `sent` |

### **AI Usage**

| Attribute   | Type    | Key | Description |
| :---------- | :------ | :-- | :---------- |
| usage_id    | UUID    | PK  |             |
| user_id     | UUID    | idx |             |
| model_name  | String  |     |             |
| tokens_used | Integer |     |             |
| usage_date  | Date    |     |             |

---
