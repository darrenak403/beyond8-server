# MOD-01: Quản Lý Người Dùng - Yêu Cầu

**Module:** Quản Lý Người Dùng
**Mức Độ Ưu Tiên:** Quan Trọng
**Phụ Thuộc:** MOD-10 (cho khởi tạo ví)

---

## Yêu Cầu Chức Năng

### REQ-01.00: Truy Cập Người Dùng Khách

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Guest User
**Mô Tả:** Người dùng chưa xác thực có thể duyệt và xem trước nội dung

**Tiêu Chí Chấp Nhận:**

- Xem danh mục khóa học với tất cả các khóa học đã công bố
- Xem chi tiết khóa học
- Xem video xem trước tối đa 3 video mỗi khóa học được đánh dấu là Preview
- Thêm khóa học vào giỏ hàng
- Không thể đăng ký, bình luận hoặc đánh giá khi chưa đăng nhập
- Chuyển hướng đến trang đăng nhập/đăng ký khi thực hiện hành động bị hạn chế
- Giỏ hàng được hợp nhất với giỏ hàng trong DB khi đăng nhập

---

### REQ-01.01: Đăng Ký Người Dùng

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Guest User
**Mô Tả:** Người dùng khách tạo tài khoản mới với xác thực email

**Tiêu Chí Chấp Nhận:**

- Hệ thống chấp nhận email, mật khẩu, họ tên đầy đủ, số điện thoại
- Kiểm tra mật khẩu: tối thiểu 8 ký tự, 1 chữ hoa, 1 số, 1 ký tự đặc biệt
- Gửi mã OTP 6 chữ số đến email trong vòng 1 phút
- OTP hết hạn sau 5 phút
- Tài khoản được tạo nhưng chưa kích hoạt cho đến khi xác thực email
- Email trùng lặp bị từ chối với thông báo lỗi rõ ràng

**Quy Tắc Nghiệp Vụ:** BR-01

---

### REQ-01.02: Xác Thực Email

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Guest User
**Mô Tả:** Người dùng xác thực email qua link để kích hoạt tài khoản

**Tiêu Chí Chấp Nhận:**

- Nhập mã OTP 6 chữ số để kích hoạt tài khoản
- Người dùng tự động đăng nhập sau khi xác thực thành công
- OTP hết hạn sau 5 phút, hiển thị lỗi với tùy chọn gửi lại
- OTP sai hiển thị thông báo lỗi, tối đa 5 lần thử
- Email đã được xác thực hiển thị thông báo phù hợp

**Quy Tắc Nghiệp Vụ:** BR-01

---

### REQ-01.03: Đăng Nhập Người Dùng

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student, Instructor, Admin
**Mô Tả:** Người dùng đã đăng ký đăng nhập vào hệ thống

**Tiêu Chí Chấp Nhận:**

- Đăng nhập bằng email và mật khẩu
- JWT token được cấp có hiệu lực trong 24 giờ
- Thông tin đăng nhập không hợp lệ hiển thị lỗi chung
- Email chưa xác thực không thể đăng nhập
- Tài khoản bị cấm không thể đăng nhập với thông báo cụ thể
- Đăng nhập thành công chuyển hướng đến dashboard phù hợp với vai trò

**Quy Tắc Nghiệp Vụ:** BR-01

---

### REQ-01.04: Đăng Xuất Người Dùng

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student, Instructor, Admin
**Mô Tả:** Người dùng kết thúc phiên làm việc

**Tiêu Chí Chấp Nhận:**

- Nút đăng xuất có thể truy cập từ tất cả các trang
- JWT token bị vô hiệu hóa trên server
- Người dùng được chuyển hướng về trang chủ
- Không yêu cầu xác nhận đăng xuất

---

### REQ-01.05: Đặt Lại Mật Khẩu

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student, Instructor, Admin
**Mô Tả:** Người dùng đặt lại mật khẩu đã quên

**Tiêu Chí Chấp Nhận:**

- Link Quên Mật Khẩu trên trang đăng nhập
- Hệ thống gửi mã OTP 6 chữ số đến email đã đăng ký
- OTP có hiệu lực trong 5 phút
- Người dùng nhập OTP và đặt mật khẩu mới đáp ứng các yêu cầu
- OTP sai hiển thị thông báo lỗi, tối đa 5 lần thử
- Mật khẩu cũ bị vô hiệu hóa ngay lập tức
- Email xác nhận được gửi sau khi đặt lại thành công

**Quy Tắc Nghiệp Vụ:** BR-01

---

### REQ-01.06: Quản Lý Hồ Sơ (Updated)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student, Instructor, Admin
**Mô Tả:** Người dùng cập nhật thông tin hồ sơ của mình

**Tiêu Chí Chấp Nhận:**

- Chỉnh sửa: họ tên đầy đủ, số điện thoại, tiểu sử, ảnh đại diện
- **Đối với Instructor:** Có thể cập nhật thông tin tài khoản ngân hàng (Tên NH, STK, Tên chủ TK) để nhận thanh toán (cập nhật vào `InstructorProfile`)
- Ảnh đại diện: tối đa 5MB, JPG/PNG, tự động thay đổi kích thước về 300x300
- Không thể thay đổi: email, vai trò
- Thay đổi được lưu ngay lập tức với thông báo thành công
- Hiển thị ảnh hồ sơ trên tất cả các hành động của người dùng

---

### REQ-01.07: Đăng Ký Làm Giảng Viên

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student
**Mô Tả:** Học viên đăng ký trở thành giảng viên

**Tiêu Chí Chấp Nhận:**

- Mẫu đăng ký bao gồm: lĩnh vực chuyên môn, tiểu sử, đề cương khóa học mẫu, xác thực danh tính
- Tải lên tài liệu: tối đa 10MB tổng cộng, PDF/DOCX/JPG
- Trước khi nộp: Hệ thống sử dụng AI (Gemini API) để review hồ sơ
  - AI kiểm tra: khả năng viết, tính hợp lệ của tài liệu, phù hợp tiêu chuẩn
  - Nếu AI phát hiện vấn đề (tin cậy <60%): hiển thị cảnh báo & gợi ý cải tiến
  - Học viên có thể chỉnh sửa lại trước khi nộp hoặc bỏ qua cảnh báo & nộp
- Chỉ nộp một lần; trạng thái hiển thị Đang Chờ Xét Duyệt
- Không thể tạo khóa học cho đến khi được phê duyệt
- Gửi thông báo khi quyết định được đưa ra

**Quy Tắc Nghiệp Vụ:** BR-02

---

### REQ-01.08: Phê Duyệt/Từ Chối Đơn Đăng Ký Giảng Viên (Updated)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Quản trị viên xem xét và quyết định các đơn đăng ký giảng viên

**Tiêu Chí Chấp Nhận:**

- Admin xem danh sách các đơn đăng ký đang chờ xử lý
- Xem chi tiết đơn đăng ký đầy đủ và tài liệu đã tải lên
- Phê duyệt:
  - Vai trò người dùng được bổ sung thêm `Instructor` (hỗ trợ Dual Role)
  - **Hệ thống tự động khởi tạo Ví Giảng Viên (InstructorWallet) rỗng (liên kết MOD-10)**
- Người dùng vẫn giữ quyền truy cập đến các khóa học đã đăng ký trước đó như học viên
- Từ chối: phải cung cấp lý do tối thiểu 20 ký tự
- Người nộp đơn bị từ chối có thể nộp lại sau 7 ngày
- Người nộp đơn nhận thông báo email với quyết định và lý do

**Quy Tắc Nghiệp Vụ:** BR-02, BR-04A, BR-19

---

### REQ-01.09: Quản Lý Vai Trò Người Dùng

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Quản trị viên gán/thu hồi vai trò

**Tiêu Chí Chấp Nhận:**

- Admin có thể nâng cấp Student thành Instructor
- Admin có thể hạ cấp Instructor thành Student
- Không thể xóa vai trò Admin của chính mình
- Thay đổi vai trò được ghi lại với timestamp và ID admin
- Người dùng được thông báo ngay lập tức qua email

---

### REQ-01.10: Cấm/Bỏ Cấm Người Dùng

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Admin
**Mô Tả:** Quản trị viên tạm ngừng hoặc khôi phục tài khoản người dùng

**Tiêu Chí Chấp Nhận:**

- Hành động cấm yêu cầu lý do tối thiểu 20 ký tự
- Người dùng bị cấm không thể đăng nhập
- Các khóa học của giảng viên bị cấm bị ẩn khỏi danh mục
- Học viên đã đăng ký vào khóa học của giảng viên bị cấm vẫn giữ quyền truy cập
- Bỏ cấm khôi phục toàn bộ quyền truy cập
- Cấm/bỏ cấm được ghi lại với timestamp và lý do

---

### REQ-01.11: Xem Danh Sách Người Dùng

**Mức Độ Ưu Tiên:** Nên Có
**Tác Nhân:** Admin
**Mô Tả:** Quản trị viên xem tất cả người dùng đã đăng ký với bộ lọc

**Tiêu Chí Chấp Nhận:**

- Danh sách phân trang 50 người dùng/trang
- Lọc theo: vai trò, trạng thái, ngày đăng ký
- Tìm kiếm theo: email, tên
- Sắp xếp theo: ngày đăng ký, lần đăng nhập cuối
- Hành động nhanh: xem hồ sơ, cấm, thay đổi vai trò

---

### REQ-01.12: Hiển Thị Kỹ Năng/Chứng Chỉ Người Dùng

**Mức Độ Ưu Tiên:** Nên Có
**Tác Nhân:** Student, Instructor
**Mô Tả:** Người dùng thể hiện các chứng chỉ và kỹ năng đã đạt được trên hồ sơ

**Tiêu Chí Chấp Nhận:**

- Học viên có thể thêm kỹ năng thủ công tối đa 20 kỹ năng
- Chứng chỉ khóa học đã hoàn thành tự động được thêm vào hồ sơ
- Hồ sơ công khai hiển thị: tiểu sử, kỹ năng, chứng chỉ
- Hồ sơ giảng viên hiển thị trên các trang khóa học

---

## Yêu Cầu Phi Chức Năng

### NREQ-01.01: Bảo Mật

- Mật khẩu được băm sử dụng bcrypt
- Không lưu trữ mật khẩu dạng văn bản thuần túy
- Giới hạn tốc độ: 5 lần đăng nhập thất bại thì khóa 15 phút

### NREQ-01.02: Hiệu Năng

- Thời gian phản hồi đăng nhập < 300ms
- Cập nhật hồ sơ < 200ms
- Tìm kiếm người dùng < 500ms

### NREQ-01.03: Khả Năng Mở Rộng

- Hỗ trợ 10,000 người dùng đã đăng ký
- Xác thực JWT không trạng thái

---

## Ma Trận Truy Xuất

| Yêu Cầu   | Use Case(s)        | Mức Độ Ưu Tiên |
| :-------- | :----------------- | :------------- |
| REQ-01.00 | UC-02.12, UC-03.03 | Bắt Buộc Có    |
| REQ-01.01 | UC-01.01           | Bắt Buộc Có    |
| REQ-01.02 | UC-01.02           | Bắt Buộc Có    |
| REQ-01.03 | UC-01.03           | Bắt Buộc Có    |
| REQ-01.04 | UC-01.04           | Bắt Buộc Có    |
| REQ-01.05 | UC-01.05           | Bắt Buộc Có    |
| REQ-01.06 | UC-01.06           | Bắt Buộc Có    |
| REQ-01.07 | UC-01.07           | Bắt Buộc Có    |
| REQ-01.08 | UC-01.08           | Bắt Buộc Có    |
| REQ-01.09 | UC-01.09           | Bắt Buộc Có    |
| REQ-01.10 | UC-01.10           | Bắt Buộc Có    |
| REQ-01.11 | UC-01.11           | Nên Có         |
| REQ-01.12 | UC-01.12           | Nên Có         |
