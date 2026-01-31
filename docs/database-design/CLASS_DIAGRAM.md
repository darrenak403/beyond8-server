# Conceptual Class Diagram - Beyond8 E-Learning Platform

## Ky hieu su dung

```
--------------  Association (lien ket thong thuong)
--------------> Inheritance (ke thua - tam giac rong)
<>------------  Aggregation (tap hop - hinh thoi rong)
<*>-----------  Composition (hop thanh - hinh thoi dac)
- - - - - - ->  Dependency (su dung enum)
```

---

## 1. IDENTITY DOMAIN

```
+-------------------+  +---------------------+  +---------------------+
|     <<enum>>      |  |      <<enum>>       |  |      <<enum>>       |
|    UserStatus     |  | VerificationStatus  |  | SubscriptionStatus  |
+-------------------+  +---------------------+  +---------------------+
| Active            |  | Pending             |  | Active              |
| Inactive          |  | Approved            |  | Expired             |
| Banned            |  | Rejected            |  | Cancelled           |
+-------------------+  | Hidden              |  +---------------------+
        ^              +---------------------+            ^
        |                       ^                         |
        | uses                  | uses                    | uses
        |                       |                         |
        +-----------------------+-------------------------+
                                |
               +----------------+----------------+
               |        <<abstract>>             |
               |          BaseEntity             |
               +---------------------------------+
               | - id: Guid                      |
               | - createdAt: DateTime           |
               | - updatedAt: DateTime           |
               +---------------------------------+
               | + getId(): Guid                 |
               | + markUpdated(): void           |
               +---------------------------------+
                                ^
                                |
    +---------------------------+---------------------------+
    |                           |                           |
    |                           |                           |
+---+---------------+   +-------+-------+   +---------------+---+
|       User        |   |     Role      |   | SubscriptionPlan  |
+-------------------+   +---------------+   +-------------------+
| - email           |   | - code        |   | - code            |
| - fullName        |   | - name        |   | - name            |
| - passwordHash    |   | - description |   | - price           |
| - status          |- -+---------------+   | - durationDays    |
| - avatarUrl       |   | + getUsers()  |   | - maxRequests     |
| - phoneNumber     |   | + isAdmin()   |   | - includes[]      |
| - timezone        |   +-------+-------+   | - isActive        |
| - refreshToken    |           |           +-------------------+
+-------------------+           |           | + isActive(): bool|
| + register()      |           |           | + getPrice()      |
| + login(): Token  |           |           +----------+--------+
| + changePassword()|           |                      |
| + hasRole(): bool |           |                      | 1
| + ban(): void     |           |                      |
+--------+----------+           |                      |
         |                      |                      |
         | *                    | *                    |
         |                      |                      |
         v                      v                      |
+------------------------------------------+           |
|              UserRole                    |           |
|         (Association Class)              |           |
+------------------------------------------+           |
| - userId: Guid                           |           |
| - roleId: Guid                           |           |
| - assignedAt: DateTime                   |           |
| - revokedAt: DateTime?                   |           |
+------------------------------------------+           |
| + isActive(): bool                       |           |
| + revoke(): void                         |           |
+------------------------------------------+           |
                                                       |
         +---------------------------------------------+
         |
         | 1
         |
+--------+--------------------------------+
|          UserSubscription               |
+-----------------------------------------+
| - userId: Guid                          |
| - planId: Guid                          |
| - totalRemainingRequests: int           |
| - remainingRequestsPerWeek: int         |
| - startedAt: DateTime                   |
| - expiresAt: DateTime?                  |
| - status: SubscriptionStatus            |- - -+
+-----------------------------------------+     |
| + isActive(): bool                      |     | uses
| + useRequest(): void                    |     |
| + checkQuota(): int                     |     v
| + renew(): void                         | +---------------------+
+-------------------+---------------------+ |      <<enum>>       |
                    |                       | SubscriptionStatus  |
                    | *                     +---------------------+
                    |
      User <*>------+
      (Composition 1:*)


+-----------------------------------------+
|         InstructorProfile               |
+-----------------------------------------+
| - userId: Guid                          |
| - bio: string                           |
| - headline: string                      |
| - taxId: string                         |
| - teachingLanguages[]                   |
| - introVideoUrl: string                 |
| - expertiseAreas: json                  |
| - education: json                       |
| - workExperience: json                  |
| - socialLinks: json                     |
| - bankInfo: json                        |
| - identityDocuments: json               |
| - certificates: json                    |
| - verificationStatus                    |- - - - - - - - -+
| - verificationNotes                     |                 |
| - totalStudents: int                    |                 | uses
| - totalCourses: int                     |                 |
| - avgRating: decimal                    |                 v
+-----------------------------------------+  +---------------------+
| + apply(): void                         |  |      <<enum>>       |
| + submit(): void                        |  | VerificationStatus  |
| + approve(): void                       |  +---------------------+
| + reject(): void                        |
| + isVerified(): bool                    |
+-------------------+---------------------+
                    |
                    | 0..1
                    |
      User <*>------+
      (Composition 1:0..1)
```

### Quan he Identity Domain

| From | To | Relationship | Multiplicity | Y nghia |
|------|-----|--------------|--------------|---------|
| User | Role | Association (via UserRole) | *:* | User co nhieu Role, Role co nhieu User |
| User | UserRole | **Composition** <*> | 1:* | UserRole bi xoa khi User bi xoa |
| Role | UserRole | Association | 1:* | Role duoc tham chieu boi UserRole |
| User | InstructorProfile | **Composition** <*> | 1:0..1 | Profile khong ton tai neu khong co User |
| User | UserSubscription | **Composition** <*> | 1:* | Subscription bi xoa khi User bi xoa |
| UserSubscription | SubscriptionPlan | Association | *:1 | Nhieu Subscription tham chieu 1 Plan |

---

## 2. CATALOG DOMAIN

```
+-------------------+  +-------------------+
|     <<enum>>      |  |     <<enum>>      |
|   CourseStatus    |  |    LessonType     |
+-------------------+  +-------------------+
| Draft             |  | Video             |
| PendingApproval   |  | Text              |
| Approved          |  | Quiz              |
| Published         |  | Document          |
| Rejected          |  +-------------------+
| Unpublished       |           ^
+-------------------+           |
         ^                      | uses
         |                      |
         | uses                 |
         |                      |
+--------+--------+             |
|    Category     |<------------+-------------+
+-----------------+             |             | Association
| - name: string  |-------------+-------------+ (self-ref)
| - parentId: Guid|             |               0..1 : *
| - isActive: bool|             |
+-----------------+             |
| + getChildren() |             |
| + getParent()   |             |
| + toggleStatus()|             |
| + getFullPath() |             |
+--------+--------+             |
         | 0..1                 |
         |                      |
         v *                    |
+-------------------------+     |      +-------------------+
|        Course           |-----+------|       User        |
+-------------------------+     | 1:*  |   (Instructor)    |
| - title: string         |     |      +-------------------+
| - slug: string          |     |creates
| - description: string   |     |
| - price: decimal        |     |
| - status: CourseStatus  |- - -+
| - outcomes: json[]      |
| - totalStudents: int    |
+-------------------------+
| + create(): void        |
| + submitForApproval()   |
| + approve(): void       |
| + reject(reason): void  |
| + publish(): void       |
| + unpublish(): void     |
| + isPublished(): bool   |
+------------+------------+
             |
             | <*> 1
             |
             v *
+-------------------------+
|        Section          |
+-------------------------+
| - title: string         |
| - orderIndex: int       |
+-------------------------+
| + create(): void        |
| + reorder(index): void  |
| + getTotalLessons(): int|
+------------+------------+
             |
             | <*> 1
             |
             v *
+-------------------------+      +---------------------+
|        Lesson           |      |   LessonDocument    |
+-------------------------+      +---------------------+
| - title: string         |      | - title: string     |
| - type: LessonType      |- - - | - fileUrl: string   |
| - content: string       | <>---| - fileSize: long    |
| - videoUrl: string      | 1:*  +---------------------+
| - duration: int         |      | + download(): Stream|
| - orderIndex: int       |      +---------------------+
+-------------------------+
| + create(): void        |
| + reorder(index): void  |
| + isVideo(): bool       |
| + isQuiz(): bool        |
+-------------------------+
```

### Quan he Catalog Domain

| From | To | Relationship | Multiplicity | Y nghia |
|------|-----|--------------|--------------|---------|
| Category | Category | Association -- | 0..1:* | Self-reference (parent-child) |
| Category | Course | Association -- | 0..1:* | Course thuoc Category (optional) |
| User | Course | Association -- | 1:* | Instructor creates courses |
| Course | Section | **Composition** <*> | 1:* | Section bi xoa khi Course bi xoa |
| Section | Lesson | **Composition** <*> | 1:* | Lesson bi xoa khi Section bi xoa |
| Lesson | LessonDocument | **Aggregation** <> | 1:* | Document co the ton tai doc lap |
| Course | CourseDocument | **Aggregation** <> | 1:* | Document co the ton tai doc lap |

---

## 3. ASSESSMENT DOMAIN

```
+-------------------+  +-------------------+  +---------------------+
|     <<enum>>      |  |     <<enum>>      |  |      <<enum>>       |
|   QuestionType    |  |    Difficulty     |  |  QuizAttemptStatus  |
+-------------------+  +-------------------+  +---------------------+
| SingleChoice      |  | Easy              |  | InProgress          |
| MultipleChoice    |  | Medium            |  | Graded              |
| TrueFalse         |  | Hard              |  | Expired             |
| FillInBlank       |  +-------------------+  | Abandoned           |
+-------------------+           ^            +---------------------+
         ^                      |                      ^
         |                      |                      |
         | uses                 | uses                 | uses
         |                      |                      |
         +----------------------+----------------------+
                                |
                   +------------+------------+
                   |          User           |
                   |      (Instructor)       |
                   +------------+------------+
                                | creates
                                | 1 : *
                                v
+------------------------------------------------------------------------------+
|                                                                              |
|  +---------------------------+              +---------------------------+    |
|  |        Question           |              |           Quiz            |    |
|  +---------------------------+              +---------------------------+    |
|  | - content: string         |              | - title: string           |    |
|  | - type: QuestionType      |- - -         | - description: string     |    |
|  | - options: json[]         |<------------>| - timeLimitMinutes: int?  |    |
|  | - explanation: string     |    * : *     | - passScorePercent: dec   |    |
|  | - points: decimal         |   (via       | - maxAttempts: int        |    |
|  | - difficulty: Difficulty  |- QuizQues)   | - shuffleQuestions: bool  |    |
|  | - tags: string[]          |              | - allowReview: bool       |    |
|  +---------------------------+              | - showExplanation: bool   |    |
|  | + create(): void          |              | - totalAttempts: int      |    |
|  | + validate(): bool        |              | - averageScore: decimal?  |    |
|  | + getCorrectAnswers(): [] |              | - passCount: int          |    |
|  | + checkAnswer(): bool     |              | - isActive: bool          |    |
|  | + isMultipleChoice(): bool|              +---------------------------+    |
|  | + calculatePoints(): dec  |              | + create(): void          |    |
|  +-------------+-------------+              | + addQuestion(): void     |    |
|                |                            | + removeQuestion(): void  |    |
|                | 1                          | + linkToLesson(): void    |    |
|                | <> Aggregation             | + publish(): void         |    |
|                | (reusable)                 | + getStatistics(): Stats  |    |
|                v *                          | + hasTimeLimit(): bool    |    |
|  +---------------------------+              +-------------+-------------+    |
|  |      QuizQuestion         |                            |                  |
|  +---------------------------+                            | <*> 1            |
|  | - orderIndex: int         |<---------------------------+ Composition      |
|  | - points: decimal         |              *                                |
|  +---------------------------+                                               |
|  | + reorder(): void         |                                               |
|  | + updatePoints(): void    |                                               |
|  +---------------------------+                                               |
|                                                                              |
+------------------------------------------------------------------------------+
                             |
              +--------------+--------------+---------------------------+
              |              |                                          |
              | Association  | Association                              | Composition
              | (linked to)  | (linked to)                              |
              v 0..1         v 0..1                                     v *
       +------------+  +------------+     +-----------------------------------+
       |   Lesson   |  |   Course   |     |           QuizAttempt             |
       |  (Catalog) |  |  (Catalog) |     +-----------------------------------+
       +------------+  +------------+     | - studentId: Guid                 |
                                          | - attemptNumber: int              |
                                          | - startedAt: DateTime             |
                                          | - submittedAt: DateTime?          |
                                          | - answers: json{}                 |
                                          | - questionOrder: json[]           |
                                          | - optionOrders: json{}            |
                                          | - flaggedQuestions: json[]        |
                                          | - score: decimal?                 |
                                          | - scorePercent: decimal?          |
                                          | - isPassed: bool?                 |
                                          | - timeSpentSeconds: int           |
                                          | - status: QuizAttemptStatus       |- - -
                                          | - shuffleSeed: int                |
                                          +-----------------------------------+
                                          | + start(): void                   |
                                          | + saveAnswer(): void              |
                                          | + flagQuestion(): void            |
                                          | + unflagQuestion(): void          |
                                          | + submit(): Result                |
                                          | + grade(): void                   |
                                          | + getResult(): Result             |
                                          | + isExpired(): bool               |
                                          | + getRemainingTime(): int         |
                                          | + getFlaggedQuestions(): []       |
                                          | + calculateScore(): decimal       |
                                          +-------------------+---------------+
                                                              |
                                                              | Association
                                                              | (attempts)
                                                              v 1
                                          +---------------------------+
                                          |          User             |
                                          |       (Student)           |
                                          +---------------------------+
```

### Quan he Assessment Domain

| From | To | Relationship | Multiplicity | Y nghia |
|------|-----|--------------|--------------|---------|
| User (Instructor) | Question | Association -- | 1:* | Instructor tao Question |
| User (Instructor) | Quiz | Association -- | 1:* | Instructor tao Quiz |
| Question | QuizQuestion | **Aggregation** <> | 1:* | Question co the reuse nhieu Quiz |
| Quiz | QuizQuestion | **Composition** <*> | 1:* | QuizQuestion thuoc ve Quiz |
| Quiz | QuizAttempt | **Composition** <*> | 1:* | Attempt thuoc ve Quiz |
| User (Student) | QuizAttempt | Association -- | 1:* | Student attempts quiz |
| Quiz | Lesson | Association -- | 0..1:0..1 | Quiz linked to Lesson |
| Quiz | Course | Association -- | 0..1:0..1 | Quiz linked to Course |

---

## 4. INTEGRATION DOMAIN

```
+-------------------+  +---------------------+  +-------------------+
|     <<enum>>      |  |      <<enum>>       |  |     <<enum>>      |
|    FileStatus     |  |  NotificationType   |  |    AIProvider     |
+-------------------+  +---------------------+  +-------------------+
| Pending           |  | System              |  | Gemini            |
| Uploaded          |  | Course              |  | OpenAI            |
| Failed            |  | Quiz                |  | Claude            |
| Deleted           |  | Promotion           |  +-------------------+
+-------------------+  | Reminder            |           ^
         ^             +---------------------+           |
         |                      ^                        |
         |                      |                        |
         | uses                 | uses                   | uses
         |                      |                        |
         +----------------------+------------------------+
                                |
+-------------------+           |         +-------------------+
|     <<enum>>      |           |         |     <<enum>>      |
|    AIFeature      |           |         |    PromptType     |
+-------------------+           |         +-------------------+
| QuizGeneration    |           |         | QuizGeneration    |
| DocumentEmbedding |           |         | ProfileReview     |
| ProfileReview     |           |         | Embedding         |
| ContentSummary    |           |         | ContentSummary    |
+-------------------+           |         +-------------------+
         ^                      |                   ^
         |                      |                   |
         | uses                 |                   | uses
         |                      |                   |
         +----------------------+-------------------+
                                |
                   +------------+------------+
                   |          User           |
                   +------------+------------+
                                |
           +--------------------+--------------------+
           |                    |                    |
           | <*> 1              | <*> 1              | <*> 1
           |                    |                    |
           v *                  v *                  v *
+---------------------+ +---------------------+ +---------------------+
|     MediaFile       | |    Notification     | |      AIUsage        |
+---------------------+ +---------------------+ +---------------------+
| - folder: string    | | - title: string     | | - provider          |
| - fileName: string  | | - content: string   | |   : AIProvider      |- - -
| - fileKey: string   | | - type              | | - feature           |
| - contentType: str  | |   : NotificationType|-|   : AIFeature       |- - -
| - size: long        | | - isRead: bool      | | - tokensUsed: int   |
| - status: FileStatus|-| - metadata: json    | | - promptTokens: int |
+---------------------+ +---------------------+ | - completionTokens  |
| + generatePresigned | | + markAsRead(): void| | - cost: decimal     |
|   Url(): string     | | + isUnread(): bool  | +---------------------+
| + confirmUpload()   | | + send(): void      | | + record(): void    |
| + delete(): void    | | + getTimeAgo(): str | | + calculateCost()   |
| + getPublicUrl()    | +---------------------+ | + getUsageSummary() |
| + isUploaded(): bool|                         +---------------------+
| + validateType(): bo|
+---------------------+


+-------------------------+
|       AIPrompt          |  (Standalone)
+-------------------------+
| - name: string          |
| - template: string      |
| - type: PromptType      |- - - - - - - - - -+
| - variables: string[]   |                   |
| - isActive: bool        |                   | uses
+-------------------------+                   |
| + render(vars): string  |                   v
| + validate(): bool      |         +-------------------+
| + activate(): void      |         |     <<enum>>      |
| + deactivate(): void    |         |    PromptType     |
| + clone(): AIPrompt     |         +-------------------+
+-------------------------+
```

### Quan he Integration Domain

| From | To | Relationship | Multiplicity | Y nghia |
|------|-----|--------------|--------------|---------|
| User | MediaFile | **Composition** <*> | 1:* | File thuoc User |
| User | Notification | **Composition** <*> | 1:* | Notification thuoc User |
| User | AIUsage | **Composition** <*> | 1:* | Usage record thuoc User |
| AIPrompt | - | Standalone | - | Khong co quan he voi entity khac |

---

## 5. CROSS-DOMAIN RELATIONSHIPS

```
+-----------------------------------------------------------------------------+
|                         BEYOND8 SYSTEM OVERVIEW                              |
+-----------------------------------------------------------------------------+

    IDENTITY DOMAIN              CATALOG DOMAIN              ASSESSMENT DOMAIN
    ---------------              --------------              -----------------

    +----------+                 +----------+                +----------+
    |   User   |-----creates---->|  Course  |<---linked to---|   Quiz   |
    |          |                 +----+-----+                +----+-----+
    |    |     |                      |                           |
    |    |     |                      | <*>                       | <*>
    |    v     |                      |                           |
    | +------+ |                      v                           v
    | | Role | |                 +----------+                +----------+
    | +------+ |                 |  Lesson  |<---linked to---|QuizQues. |
    |  (via    |                 +----------+                +----------+
    | UserRole)|                                                  |
    |          |                                                  | <>
    |          |                                                  |
    |          |-----attempts---------------------------------+---+------+
    |          |                                              |QuizAttempt|
    +----------+                                              +-----------+


    INTEGRATION DOMAIN
    ------------------

    +----------+
    |   User   |
    +----+-----+
         |
    +----+------------+
    |<*> |<*>         |<*>
    |    |            |
    v    v            v
+-------+ +--------+ +-------+
|Media  | |Notifi- | |AIUsage|
|File   | |cation  | |       |
+-------+ +--------+ +-------+
```

---

## 6. TAT CA ENUMERATIONS

### Identity Domain Enums

| Enum | Values | Su dung boi |
|------|--------|-------------|
| `UserStatus` | Active, Inactive, Banned | User.status |
| `VerificationStatus` | Pending, Approved, Rejected, Hidden | InstructorProfile.verificationStatus |
| `SubscriptionStatus` | Active, Expired, Cancelled | UserSubscription.status |

> **Note:** `Role` KHONG phai enum ma la mot entity rieng biet voi quan he many-to-many voi User thong qua bang UserRole.

### Catalog Domain Enums

| Enum | Values | Su dung boi |
|------|--------|-------------|
| `CourseStatus` | Draft, PendingApproval, Approved, Published, Rejected, Unpublished | Course.status |
| `LessonType` | Video, Text, Quiz, Document | Lesson.type |

### Assessment Domain Enums

| Enum | Values | Su dung boi |
|------|--------|-------------|
| `QuestionType` | SingleChoice, MultipleChoice, TrueFalse, FillInBlank | Question.type |
| `Difficulty` | Easy, Medium, Hard | Question.difficulty |
| `QuizAttemptStatus` | InProgress, Graded, Expired, Abandoned | QuizAttempt.status |

### Integration Domain Enums

| Enum | Values | Su dung boi |
|------|--------|-------------|
| `FileStatus` | Pending, Uploaded, Failed, Deleted | MediaFile.status |
| `NotificationType` | System, Course, Quiz, Promotion, Reminder | Notification.type |
| `AIProvider` | Gemini, OpenAI, Claude | AIUsage.provider |
| `AIFeature` | QuizGeneration, DocumentEmbedding, ProfileReview, ContentSummary | AIUsage.feature |
| `PromptType` | QuizGeneration, ProfileReview, Embedding, ContentSummary | AIPrompt.type |

---

## 7. BANG TOM TAT METHODS THEO ENTITY

| Entity | Key Methods | Mo ta |
|--------|-------------|-------|
| **User** | `register()`, `login()`, `changePassword()`, `ban()`, `hasRole()` | Quan ly tai khoan & xac thuc |
| **Role** | `getUsers()`, `isAdmin()` | Quan ly phan quyen |
| **UserRole** | `isActive()`, `revoke()` | Gan/thu hoi quyen |
| **InstructorProfile** | `apply()`, `submit()`, `approve()`, `reject()`, `isVerified()` | Workflow xac minh giang vien |
| **UserSubscription** | `isActive()`, `renew()`, `checkQuota()`, `useRequest()` | Quan ly goi dang ky |
| **SubscriptionPlan** | `isActive()`, `getPrice()` | Quan ly plan |
| **Category** | `getChildren()`, `getParent()`, `toggleStatus()`, `getFullPath()` | Phan cap danh muc |
| **Course** | `submitForApproval()`, `approve()`, `reject()`, `publish()`, `unpublish()` | Workflow xuat ban khoa hoc |
| **Section** | `create()`, `update()`, `reorder()`, `getTotalLessons()` | Quan ly chuong hoc |
| **Lesson** | `create()`, `update()`, `reorder()`, `isVideo()`, `isQuiz()` | Quan ly bai hoc |
| **Question** | `create()`, `validate()`, `getCorrectAnswers()`, `checkAnswer()` | Quan ly cau hoi |
| **Quiz** | `addQuestion()`, `removeQuestion()`, `linkToLesson()`, `getStatistics()` | Quan ly bai kiem tra |
| **QuizAttempt** | `start()`, `saveAnswer()`, `flagQuestion()`, `submit()`, `grade()`, `getResult()` | Lam bai & cham diem |
| **MediaFile** | `generatePresignedUrl()`, `confirmUpload()`, `delete()`, `getPublicUrl()` | Upload & quan ly file |
| **Notification** | `markAsRead()`, `send()`, `isUnread()` | Thong bao |
| **AIUsage** | `record()`, `calculateCost()`, `getUsageSummary()` | Theo doi su dung AI |
| **AIPrompt** | `render()`, `validate()`, `activate()`, `clone()` | Quan ly prompt AI |

---

## 8. ENTITY-ENUM DEPENDENCY MATRIX

```
                    +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
                    |User |Veri |Subs |Cours|Less |Ques |Diff |Quiz |File |Noti |AIPr |AIFe |
                    |Stat |fySt |Stat |eSta |Type |Type |icul |Attem|Stat |Type |ovid |atur |
                    |     |     |     |     |     |     |ty   |Stat |     |     |er   |e    |
+-------------------+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
| User              |  *  |     |     |     |     |     |     |     |     |     |     |     |
| InstructorProfile |     |  *  |     |     |     |     |     |     |     |     |     |     |
| UserSubscription  |     |     |  *  |     |     |     |     |     |     |     |     |     |
| Course            |     |     |     |  *  |     |     |     |     |     |     |     |     |
| Lesson            |     |     |     |     |  *  |     |     |     |     |     |     |     |
| Question          |     |     |     |     |     |  *  |  *  |     |     |     |     |     |
| QuizAttempt       |     |     |     |     |     |     |     |  *  |     |     |     |     |
| MediaFile         |     |     |     |     |     |     |     |     |  *  |     |     |     |
| Notification      |     |     |     |     |     |     |     |     |     |  *  |     |     |
| AIUsage           |     |     |     |     |     |     |     |     |     |     |  *  |  *  |
| AIPrompt          |     |     |     |     |     |     |     |     |     |     |     |     |
+-------------------+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+

* = Entity su dung Enum nay
```

---

## 9. NGUYEN TAC PHAN BIET QUAN HE

| Quan he | Ky hieu | Khi nao dung | Vi du |
|---------|---------|--------------|-------|
| **Association** | ------ | Hai entity lien quan nhung ton tai doc lap | User -> Course (instructor) |
| **Inheritance** | ------> | Entity con ke thua tu entity cha | Tat ca entity -> BaseEntity |
| **Aggregation** | <>----- | "Has-a" - phan co the ton tai doc lap | Question -> QuizQuestion (reuse) |
| **Composition** | <*>---- | "Part-of" - phan khong ton tai neu khong co whole | Course -> Section (cascade delete) |

---

## 10. TONG HOP QUAN HE TOAN HE THONG

| # | From | To | Relationship | Multiplicity |
|---|------|-----|--------------|--------------|
| 1 | User | Role | Association (via UserRole) | *:* |
| 2 | User | UserRole | Composition <*> | 1:* |
| 3 | Role | UserRole | Association | 1:* |
| 4 | User | InstructorProfile | Composition <*> | 1:0..1 |
| 5 | User | UserSubscription | Composition <*> | 1:* |
| 6 | UserSubscription | SubscriptionPlan | Association | *:1 |
| 7 | Category | Category | Association | 0..1:* |
| 8 | Category | Course | Association | 0..1:* |
| 9 | User | Course | Association | 1:* |
| 10 | Course | Section | Composition <*> | 1:* |
| 11 | Section | Lesson | Composition <*> | 1:* |
| 12 | Lesson | LessonDocument | Aggregation <> | 1:* |
| 13 | Course | CourseDocument | Aggregation <> | 1:* |
| 14 | User | Question | Association | 1:* |
| 15 | User | Quiz | Association | 1:* |
| 16 | Quiz | QuizQuestion | Composition <*> | 1:* |
| 17 | Question | QuizQuestion | Aggregation <> | 1:* |
| 18 | Quiz | QuizAttempt | Composition <*> | 1:* |
| 19 | User | QuizAttempt | Association | 1:* |
| 20 | Quiz | Lesson | Association | 0..1:0..1 |
| 21 | Quiz | Course | Association | 0..1:0..1 |
| 22 | User | MediaFile | Composition <*> | 1:* |
| 23 | User | Notification | Composition <*> | 1:* |
| 24 | User | AIUsage | Composition <*> | 1:* |
