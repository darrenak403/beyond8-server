# MOD-03: Học Qua Video - Yêu Cầu

**Module:** Học Qua Video
**Mức Độ Ưu Tiên:** Cao
**Phụ Thuộc:** MOD-02 (Quản Lý Khóa Học), MOD-06 (Theo Dõi Tiến Độ)

---

## Yêu Cầu Chức Năng

### REQ-03.01: Upload & Transcode Video (Updated)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Instructor
**Mô Tả:** Giảng viên upload video, hệ thống tự động chuyển đổi sang định dạng streaming (HLS)

**Tiêu Chí Chấp Nhận:**

- **Luồng Upload (S3 Direct):**
  - Instructor request Presigned URL từ Backend (`Catalog Service`).
  - Browser upload file trực tiếp lên AWS S3 (Bucket Raw).
  - Hỗ trợ Multipart upload cho file lớn (>100MB).
  - Định dạng đầu vào: MP4, MOV, WebM (Max 2GB).
- **Luồng Xử Lý (Transcoding):**
  - Sau khi upload xong, trigger AWS MediaConvert.
  - Chuyển đổi video sang định dạng **HLS (.m3u8)** với các mức chất lượng (Adaptive Bitrate: 360p, 480p, 720p, 1080p).
  - Tự động extract duration và tạo thumbnail.
- **Lưu Trữ:**
  - Cập nhật vào bảng `Lesson`:
    - `video_hls_url`: URL file .m3u8 trên CloudFront.
    - `duration`: Thời lượng (giây).
    - `status`: `Processing` -> `Ready` (hoặc `Failed`).
  - Gửi thông báo cho Instructor khi xử lý xong.

**Quy Tắc Nghiệp Vụ:** BR-06

---

### REQ-03.02: Guest Preview Mode

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Guest User
**Mô Tả:** Guest có thể xem tối đa 3 preview videos per course

**Tiêu Chí Chấp Nhận:**

- Chỉ áp dụng cho các Lesson có `is_preview = true`.
- Player load video từ CloudFront (Public URL).
- **Watermark:** Hiển thị lớp phủ (Overlay CSS) chữ "PREVIEW" trên video.
- **Call to Action:** Sau khi hết video (hoặc pause), hiển thị banner mời "Mua khóa học ngay".
- Chặn các thao tác: Seek (tua) qua vùng chưa load, Download.

**Quy Tắc Nghiệp Vụ:** BR-04

---

### REQ-03.03: Student Watch Mode (Updated)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student
**Mô Tả:** Học viên đã đăng ký xem video bài học

**Tiêu Chí Chấp Nhận:**

- **Quyền truy cập:**
  - Kiểm tra `Enrollment` status (`active`) từ `Learning Service`.
  - Nếu hợp lệ, trả về **Signed URL/Cookie** của CloudFront (hết hạn sau 24h) để load file HLS.
- **Player Features:**
  - Tự động chọn độ phân giải dựa trên băng thông mạng (Auto).
  - Cho phép chọn thủ công (360p - 1080p).
  - Tốc độ phát: 0.5x, 1.0x, 1.25x, 1.5x, 2.0x.
  - Ghi nhớ vị trí xem lần cuối (`last_position`) để resume.
- **Sequential Unlock (Mở khóa tuần tự):**
  - Nếu khóa học bật chế độ tuân thủ tuần tự: User không thể xem video tiếp theo nếu video hiện tại chưa hoàn thành (`status != completed`).
  - Bài học bị khóa hiển thị icon ổ khóa.

**Quy Tắc Nghiệp Vụ:** BR-04, BR-14

---

### REQ-03.04: Tracking Tiến Độ (Heartbeat)

**Mức Độ Ưu Tiên:** Bắt Buộc Có
**Tác Nhân:** Student (System)
**Mô Tả:** Tự động ghi nhận tiến độ xem video

**Tiêu Chí Chấp Nhận:**

- **Heartbeat:** Player gửi event về `Learning Service` mỗi 30 giây:
  - Data: `lesson_id`, `current_time` (giây).
  - Service cập nhật `last_position` vào bảng `LessonProgress`.
- **Hoàn thành (Completion):**
  - Tự động đánh dấu `completed` khi user xem > 95% thời lượng video.
  - Hoặc User bấm nút "Mark as Complete" thủ công.
  - Khi hoàn thành: Update status, mở khóa bài tiếp theo (nếu có Sequential Unlock).

**Quy Tắc Nghiệp Vụ:** BR-14

---

## Yêu Cầu Phi Chức Năng

### NREQ-03.01: Hiệu Năng Streaming

- **Start-up time:** Video bắt đầu phát < 2 giây.
- **Buffering:** Không xảy ra buffering quá 1 lần/phút với mạng 4G ổn định.
- **Latency:** Độ trễ cập nhật tiến độ (Heartbeat) không làm lag UI.

### NREQ-03.02: Bảo Mật Nội Dung

- **CloudFront Signed URL/Cookie:** URL video chỉ có hiệu lực cho IP người dùng trong thời gian ngắn.
- **CORS Policy:** Chặn nhúng video trên các domain lạ.
- **DRM (Optional):** Chưa yêu cầu DRM cứng (Widevine/FairPlay) trong giai đoạn này (chỉ dùng HLS Encryption cơ bản nếu cần).

### NREQ-03.03: Khả Năng Tương Thích

- Player hoạt động tốt trên Mobile Web (iOS/Android) và Desktop.
- Hỗ trợ Picture-in-Picture mode.

---

## Ma Trận Truy Xuất

| Requirement ID | Use Case ID | Business Rule | Module Dependency        |
| :------------- | :---------- | :------------ | :----------------------- |
| REQ-03.01      | UC-03.01    | BR-06         | MOD-02, Common Service   |
| REQ-03.02      | UC-03.02    | BR-04         | MOD-02                   |
| REQ-03.03      | UC-03.03    | BR-04, BR-14  | MOD-06, Learning Service |
| REQ-03.04      | UC-03.04    | BR-14         | MOD-06                   |
