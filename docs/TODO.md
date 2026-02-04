# TODO – Beyond8 Server

Danh sách việc cần làm / ghi chú cho từng module.

---

## 1. Enroll khóa học miễn phí + Order (REQ-07.01)

### Yêu cầu (07-PAYMENT-ENROLLMENT.md)

- **Xử lý enroll free**: Tạo bản ghi `Enrollment` (đã có) **+ Tạo `Order` giá trị 0đ để lưu lịch sử + Snapshot dữ liệu khóa học vào `OrderItem`**.

### TODO

- **Bổ sung Order vào luồng enroll free**:
  - Tạo bản ghi **Order** (amount = 0, status phù hợp, vd. Paid/Completed) khi user enroll khóa học miễn phí.
  - Tạo **OrderItem** (snapshot: tên khóa học, tên giảng viên, courseId, giá 0, v.v.) gắn với Order đó.
  - Gắn **Enrollment** với **Order** (vd. `Enrollment.OrderId`) nếu thiết kế có liên kết.
- **Vị trí thực hiện**: Order/OrderItem có thể nằm ở **Sales Service** (khi có) hoặc **Learning Service** (nếu gộp lịch sử giao dịch vào Learning). Nếu Sales chưa tồn tại: có thể thêm entity Order, OrderItem vào Learning và tạo trong `EnrollmentService.EnrollFreeAsync` sau khi tạo Enrollment.

---

## 2. Get khóa học kèm coupon (giá + discount) – đã bổ sung cấu trúc

### Đã triển khai

- **CourseResponse** và **CourseSimpleResponse** (Catalog) đã có thêm:
  - **OriginalPrice**: giá ban đầu (giá gốc khóa học).
  - **DiscountPercent** (nullable): phần trăm giảm giá từ coupon.
  - **DiscountAmount** (nullable): số tiền giảm từ coupon.
  - **FinalPrice**: giá sau khi áp dụng discount (hiển thị cho người dùng).
  - **CouponCode**, **CouponDescription** (nullable): mã và mô tả coupon nếu có.
- Hiện tại mapping: `OriginalPrice = Price`, `FinalPrice = Price`, discount = null (chưa có Sales/Coupon).

### TODO (khi có Sales / Coupon service)

- Endpoint get course (summary/details/list) nhận tham số **couponCode** (optional).
- Gọi Sales/Coupon service validate coupon cho courseId → lấy DiscountPercent hoặc DiscountAmount.
- Tính **FinalPrice** và điền **DiscountPercent**, **DiscountAmount**, **CouponCode**, **CouponDescription** vào response.

---

## 3. Khác (ghi chú nhanh)

- **Coupon (REQ-07.01)**: Lấy discount từ bảng coupon (Sales Service) để cho phép enroll free khi có coupon 100% dù course có giá.
- **Learning – Check enrollment**: API `GET /api/v1/enrollments/check?courseId=...` đã có, dùng cho Assessment.

---

_Cập nhật lần cuối: ghi chú TODO tổng hợp._
