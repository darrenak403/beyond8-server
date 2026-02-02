# Đặc Tả Yêu Cầu - Nền Tảng Học Trực Tuyến

**Dự án:** Nền tảng Học trực tuyến với Hỗ trợ AI
**Phiên bản:** 1.2 (Removed AI Chat, Notify & Session Limits)
**Ngày:** 13 tháng 1, 2026
**Quy mô Team:** 4 người
**Timeline:** 9 tuần

---

## 1. Actors

### AC-01: Admin

**Mô tả:** Quản trị viên hệ thống với quyền kiểm soát đầy đủ
**Trách nhiệm:**

- Quản lý tất cả người dùng
- Xem xét và phê duyệt đơn xin trở thành giảng viên
- Xem xét và phê duyệt/từ chối khóa học trước khi xuất bản
- Giám sát hệ thống và doanh thu (Dashboard tổng quan)
- Cấu hình thiết lập hệ thống và duyệt yêu cầu rút tiền (Payout)

**Cấp độ truy cập:** Quyền truy cập toàn hệ thống

### AC-02: Instructor

**Mô tả:** Người tạo nội dung và xuất bản khóa học. Giảng viên cũng có thể học các khóa học của giảng viên khác như một học viên bình thường.
**Trách nhiệm:**

- Tạo và quản lý khóa học
- Tạo quiz và assignment (có thể sử dụng AI để hỗ trợ)
- Chấm bài nộp của học viên (đã có hỗ trợ AI grading)
- Theo dõi phân tích khóa học và doanh thu
- Kiểm duyệt thảo luận khóa học
- Trả lời câu hỏi của học viên
- Quản lý ví tiền và yêu cầu rút tiền

**Cấp độ truy cập:** Khóa học của mình + dữ liệu học viên trong khóa học của mình + các khóa học đã đăng ký

### AC-03: Student

**Mô tả:** Người học đăng ký tham gia khóa học
**Trách nhiệm:**

- Duyệt và tìm kiếm khóa học
- Đăng ký khóa học qua thanh toán
- Xem các bài học video
- Hoàn thành quiz và assignment
- Theo dõi tiến độ của bản thân
- Tham gia thảo luận, đánh giá và review khóa học

**Cấp độ truy cập:** Các khóa học đã đăng ký + hồ sơ cá nhân

### AC-04: Guest User

**Mô tả:** Người truy cập chưa xác thực
**Trách nhiệm:**

- Duyệt danh mục khóa học
- Xem chi tiết khóa học và video preview
- Tìm kiếm khóa học
- Đăng ký tài khoản

**Cấp độ truy cập:** Chỉ nội dung công khai

---

## 2. Các Quy Tắc Kinh Doanh Toàn Cục

### BR-01: Xác Thực Người Dùng

- Bắt buộc xác minh email cho tài khoản mới qua OTP 6 chữ số
- OTP hết hạn sau 5 phút, tối đa 5 lần thử
- JWT token hết hạn sau 24 giờ (access token)
- Refresh token hết hạn sau 7 ngày
- Mật khẩu tối thiểu 8 ký tự: 1 chữ hoa, 1 số, 1 ký tự đặc biệt
- Tài khoản bị cấm không thể đăng nhập
- Tối đa 5 lần đăng nhập sai trong 15 phút → khóa tài khoản tạm thời 30 phút

### BR-02: Phê Duyệt Giảng Viên

- Phải nộp đơn và chờ admin phê duyệt trước khi tạo khóa học
- Đơn bị từ chối có thể nộp lại sau 7 ngày
- AI (Gemini) pre-review hồ sơ với confidence score trước khi nộp
- Cảnh báo nếu AI confidence <60%, instructor có thể chỉnh sửa hoặc bỏ qua
- Admin phải cung cấp rejection_reason khi từ chối đơn
- Student chỉ có thể nộp 1 đơn tại một thời điểm

### BR-03: Xuất Bản & Duyệt Khóa Học

- Khóa học phải được admin phê duyệt trước khi công khai
- Tối đa 5 lần từ chối/tuần per instructor
- Admin phải cung cấp rejection_reason khi từ chối
- Instructor nhận notification ngay lập tức (email + in-app)
- Khóa học Draft hoặc Rejected có thể chỉnh sửa
- Khóa học Published hoặc PendingApproval không thể chỉnh sửa
- Không thể xóa khóa học đã có enrollment

### BR-04: Truy Cập & Enrollment

- Học viên phải hoàn tất thanh toán trước khi truy cập nội dung khóa học trả phí
- Enrollment là vĩnh viễn trừ khi hoàn tiền
- Khóa học miễn phí (price = 0): enroll ngay lập tức không cần thanh toán
- Giảng viên cũng phải thanh toán để đăng ký khóa học của giảng viên khác
- Giảng viên không thể enroll vào khóa học của chính mình
- Ngăn chặn enrollment trùng lặp: kiểm tra enrollment active trước khi tạo mới

### BR-04A: Vai Trò Kép của Giảng Viên

- Giảng viên có thể đăng ký và học các khóa học khác như một học viên
- Giảng viên không thể tự đăng ký vào khóa học của chính mình
- Khi học khóa học của người khác, giảng viên có đầy đủ quyền và trách nhiệm như học viên
- Role được quản lý dưới dạng List<ENUM> trong User entity

### BR-05: Chính Sách Hoàn Tiền

- **Thời gian:** Hoàn tiền trong vòng **14 ngày** kể từ khi enrollment.
- Điều kiện hoàn tiền: tiến độ <10% và chưa hoàn thành quiz/assignment nào
- Không hoàn tiền nếu đã hoàn thành >10% khóa học
- Hoàn tiền được xử lý qua VNPay API (Refund Transaction)
- Admin xem xét và phê duyệt/từ chối refund request
- Status: pending → approved/rejected
- Enrollment status chuyển sang `refunded` khi được phê duyệt

### BR-06: Giới Hạn Video

- Chiều dài tối đa: 30 phút per video
- Kích thước file tối đa: 2GB per video
- Định dạng hỗ trợ upload: MP4, WebM, MOV (H.264 video codec, AAC audio)
- Tự động chuyển đổi sang HLS với adaptive bitrate qua AWS MediaConvert
- Hỗ trợ đến độ phân giải 4K
- Thumbnail tự động extract từ frame đầu tiên hoặc giữa video
- Signed URL (CloudFront) hết hạn sau 24 giờ cho enrolled students
- Public URL cho preview videos (tối đa 3 per course)
- Watermark "PREVIEW" overlay cho guest users

### BR-07: Upload Tài Liệu

- Kích thước tối đa: 50MB per document
- Định dạng hỗ trợ: PDF (with text layer), DOCX
- Tài liệu có thể upload ở 3 levels: Course, Section, Lesson
- Lưu trữ trên S3, Catalog Service chỉ lưu metadata và S3 Key

### BR-08: Ngẫu Nhiên Hóa Quiz

- Câu hỏi được xáo trộn bằng thuật toán Fisher-Yates
- Các đáp án được xáo trộn trong mỗi câu hỏi MCQ
- Mỗi học viên nhận thứ tự câu hỏi duy nhất
- Shuffle chỉ áp dụng nếu instructor bật shuffle_questions và shuffle_options

### BR-09: Phân Bố Độ Khó Quiz

- Tỷ lệ mặc định - Dễ:Trung bình:Khó = 30:50:20
- Instructor có thể tùy chỉnh tỷ lệ per quiz
- AI tạo quiz tôn trọng tỷ lệ này (±5% tolerance)
- Validation: nếu không đạt ratio → regenerate (tối đa 3 lần retry)

### BR-10: Chấm Điểm Có Hỗ Trợ AI

- Assignment có AI grading bắt buộc phải có Rubric
- AI grading chỉ áp dụng cho Essay Assignment
- Hệ thống lưu cả ai_score và score (final score từ instructor)
- ai_score tự động tính khi student submit
- AI confidence <70% → flag `needs_manual_review`, gửi notification cho instructor
- Instructor có thể xem ai_score, ai_feedback, ai_confidence trong grading interface

### BR-11: Thanh Toán

- Cổng thanh toán hỗ trợ: VNPay (ATM, thẻ tín dụng, QR code, ví điện tử)
- Mã giảm giá (Coupon) có ngày hết hạn và giới hạn số lần sử dụng
- Giá chỉ bằng VND
- Transaction data mã hóa AES-256
- Payment status: pending → paid/failed/cancelled/refunded
- Callback VNPay cập nhật order status
- Invoice PDF tự động tạo cho paid orders

### BR-12: Đánh Giá & Review

- Student chỉ có thể review sau khi: enrolled + hoàn thành ≥80% khóa học
- Mỗi student chỉ review 1 lần per course
- Average rating tự động tính và cache trong Course entity (hoặc Aggregate Table)
- Review có thể bị report: reason (spam, offensive, fake)
- Admin xem xét report và có thể xóa review

### BR-13: Chứng Chỉ

- Tự động tạo khi: 100% hoàn thành tất cả lessons + điểm trung bình quiz ≥70% + điểm trung bình assignment ≥70%
- Bao gồm: tên học viên, tiêu đề khóa học, tên instructor, ngày hoàn thành, certificate ID duy nhất
- URL xác minh công khai với verify_code (QR code)

### BR-14: Tiến Độ Học Tập

- Video progress tự động lưu mỗi 30 giây khi đang phát
- Đánh dấu video completed nếu: watched ≥95% duration
- Quiz allow multiple attempts: chỉ tính điểm cao nhất
- Lesson có sequential unlock: phải hoàn thành lesson trước mới mở lesson tiếp theo
- Course progress % = (completed lessons / total lessons) × 100

### BR-15: AI Question Generation

- AI tạo quiz từ document (PDF, DOCX) qua Gemini API
- Số câu hỏi: 5-50, default 10
- Generated questions có metadata: `source = AI`
- Instructor có thể edit inline, delete, regenerate all
- AI generation timeout: 30 giây cho 10 câu hỏi

### BR-16: Assessment Organization (Simplified)

- Sử dụng Tags (Nhãn) để quản lý câu hỏi thay vì Category Tree phức tạp
- QuestionBank lưu trữ phẳng, Instructor lọc câu hỏi theo Tag
- Một question có thể có nhiều tags (VD: "Java", "Difficult", "Chapter 1")

### BR-17: Data Retention & Privacy

- Soft delete cho User, Course, Review
- Enrollment history giữ vĩnh viễn cho reporting
- Log retention: 1 năm cho system logs
- Personal data export: student có thể request data export

### BR-18: Rate Limiting & Security

- API rate limit: 100 requests/phút per user
- SQL injection prevention: parameterized queries
- XSS protection: input sanitization

### BR-19: Phân Chia Doanh Thu & Rút Tiền

- **Tỷ lệ chia sẻ:** Mặc định Giảng viên 70% - Nền tảng 30% trên doanh thu thực nhận (sau khi trừ phí VNPay).
- **Cơ chế Escrow:** Tiền bán khóa học sẽ nằm ở trạng thái `Pending` trong ví giảng viên (Instructor Wallet) trong **14 ngày** (trùng thời gian refund BR-05).
- **Settlement:** Sau 14 ngày, hệ thống tự động chuyển tiền từ `Pending` sang `Available`.
- **Rút tiền:** Giảng viên chỉ được rút từ số dư `Available`. Hạn mức tối thiểu 500.000 VNĐ.
- **Duyệt chi:** Admin phê duyệt yêu cầu rút tiền trong 1-3 ngày làm việc.

---

## 3. Tổng Quan Các Module

| Module ID | Tên Module                   | Ưu Tiên    | Phụ Thuộc      |
| :-------- | :--------------------------- | :--------- | :------------- |
| MOD-01    | Quản Lý Người Dùng           | Quan Trọng | Không có       |
| MOD-02    | Quản Lý Khóa Học             | Quan Trọng | MOD-01         |
| MOD-03    | Học Video                    | Cao        | MOD-02         |
| MOD-04    | Hệ Thống Quiz & Assignment   | Cao        | MOD-02         |
| MOD-05    | Tính Năng AI (Grading & Gen) | Cao        | MOD-03, MOD-04 |
| MOD-06    | Theo Dõi Tiến Độ             | Trung Bình | MOD-03, MOD-04 |
| MOD-07    | Thanh Toán & Đăng Ký         | Quan Trọng | MOD-01, MOD-02 |
| MOD-08    | Đánh Giá & Review            | Thấp       | MOD-02, MOD-07 |
| MOD-09    | Trang Quản Trị (Admin)       | Quan Trọng | Tất cả modules |
| MOD-10    | Quản Lý Doanh Thu & Ví       | Cao        | MOD-01, MOD-07 |

---

## 4. Thuật Ngữ

| Thuật ngữ        | Định nghĩa                                                             |
| :--------------- | :--------------------------------------------------------------------- |
| Course           | Chương trình học tập đầy đủ chứa các sections và lessons               |
| Section          | Nhóm logic các bài học trong một khóa học                              |
| Lesson           | Một đơn vị học tập thường là 1 video + tài liệu tùy chọn               |
| Enrollment       | Việc đăng ký tham gia khóa học của học viên sau thanh toán             |
| Transcript       | Phiên bản text của âm thanh video để xử lý AI                          |
| Adaptive Bitrate | Streaming video tự động điều chỉnh chất lượng theo tốc độ mạng         |
| HLS              | HTTP Live Streaming - Giao thức truyền tải video                       |
| Escrow           | Cơ chế giữ tiền trung gian (tạm giữ tiền 14 ngày trước khi trả cho GV) |

---

## 5. Yêu Cầu Phi Chức Năng

### NFR-01: Hiệu Năng

- Độ trễ streaming video < 2 giây
- Thời gian load trang < 3 giây
- Thời gian phản hồi API < 500ms
- Hỗ trợ 1000 người dùng đồng thời

### NFR-02: Bảo Mật

- Chỉ HTTPS
- Ngăn chặn SQL injection, XSS, CSRF
- Giới hạn tốc độ 100 requests/phút per user

### NFR-03: Khả Năng Mở Rộng

- Horizontal scaling cho các dịch vụ API
- CDN cho phân phối video
- Database read replicas cho analytics

### NFR-04: Khả Dụng

- 99.5% uptime SLA
- Kiểm tra sức khỏe tự động
- Sao lưu database mỗi 24 giờ

### NFR-05: Tính Dễ Sử Dụng

- Thiết kế responsive: mobile, tablet, desktop
- Tuân thủ WCAG 2.1 AA
- Hỗ trợ đa ngôn ngữ: Tiếng Việt, English

### NFR-06: Khả Năng Bảo Trì

- Code coverage > 70%
- Tài liệu API (Swagger/OpenAPI)
- Ghi log tất cả các thao tác quan trọng
