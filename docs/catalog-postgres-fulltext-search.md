# PostgreSQL Full-Text Search trong Catalog Service

Tài liệu mô tả cách hoạt động của tìm kiếm full-text (PostgreSQL) cho khóa học trong Catalog Service.

## Tổng quan

- **API**: `GET /api/v1/courses/search`
- **Tham số**: `Keyword`, `PageNumber`, `PageSize` (trong `FullTextSearchRequest`)
- **Luồng**: Client gửi keyword → chuẩn hóa từ khóa (C#) → so khớp với cột `SearchVector` (tsvector) trong PostgreSQL → sắp xếp theo độ liên quan (rank) → trả về danh sách có phân trang.

---

## 1. Cấu trúc dữ liệu

### Bảng `Courses`

- **SearchVector** (kiểu `tsvector`, nullable): vector full-text do PostgreSQL tạo ra từ các trường có thể tìm kiếm. Được cập nhật **trong DB** mỗi khi course được thêm/sửa (gọi function `courses_search_vector_update_for_ids`).
- **Index**: GIN trên `SearchVector` để tìm kiếm nhanh.

### Extension PostgreSQL

- **unaccent**: Bật trong `CatalogDbContext` (`modelBuilder.HasPostgresExtension("unaccent")`). Dùng để bỏ dấu tiếng Việt khi xây SearchVector và giúp tìm "lap trinh" khớp "Lập trình" (tùy phiên bản/cấu hình unaccent).

---

## 2. Cập nhật SearchVector (khi thêm/sửa course)

### Nơi gọi

- **CatalogDbContext.SaveChangesAsync**: Sau khi `base.SaveChangesAsync()` ghi course xuống DB, nếu có course thêm hoặc sửa thì gọi function PostgreSQL:

```csharp
await Database.ExecuteSqlRawAsync(
    "SELECT courses_search_vector_update_for_ids(@p0)",
    new NpgsqlParameter { ..., Value = courseIds.ToArray() });
```

### Function PostgreSQL: `courses_search_vector_update_for_ids(p_ids UUID[])`

- **Input**: Mảng ID các course vừa thêm/sửa.
- **Việc làm**: Với mỗi course trong `p_ids`, tính lại `SearchVector` rồi `UPDATE` vào bảng `Courses`.

**Cách tính SearchVector** (trong migration `RemoveSearchableTextUsePostgresFullText`):

1. Lấy dữ liệu từ `Courses` và `Categories` (join theo `CategoryId`).
2. Với mỗi trường cần search:
   - `unaccent(...)` để bỏ dấu (hỗ trợ tiếng Việt).
   - `to_tsvector('simple', ...)` để chuyển thành tsvector (config `simple` = không stem, tách từ theo khoảng trắng).
   - `setweight(..., 'A'|'B'|'C'|'D')` để gán trọng số (A cao nhất, D thấp nhất).
3. Nối tất cả tsvector đã setweight bằng `||`.

**Trường tham gia và trọng số**:

| Trường / Nguồn           | Trọng số | Ghi chú |
|-------------------------|----------|--------|
| Title                   | A        | Quan trọng nhất |
| Slug, ShortDescription, Category.Name | B | |
| Description, Outcomes, Requirements, TargetAudience, Status, Level, Language | C | JSONB được `regexp_replace` thành text rồi mới unaccent + to_tsvector |
| InstructorName          | D        | |

- **Status / Level**: Chuyển enum sang chuỗi (tiếng Anh + tiếng Việt) rồi mới unaccent + to_tsvector.

---

## 3. Chuẩn hóa từ khóa tìm kiếm (phía C#)

Trước khi đưa keyword vào PostgreSQL, ứng dụng chuẩn hóa qua **StringHelper** (Catalog.Application):

### `FormatSearchTermPlain(keyword)` (dùng cho full-text search)

1. **RemoveDiacritics(keyword)**  
   - Unicode NFD (FormD) → bỏ ký tự NonSpacingMark → NFC (FormC) → lowercase.  
   - Ví dụ: "Lập trình" → "lap trinh".

2. **SanitizeForTsQuery(unsign)**  
   - Thay các ký tự đặc biệt của tsquery (`&`, `|`, `!`, `:`, `*`, `(`, `)`, `\`) bằng khoảng trắng để tránh lỗi cú pháp.

3. Tách theo khoảng trắng, bỏ rỗng, rồi **nối lại bằng một dấu cách** (plain text, không dùng `&` hay `:*`).  
   - Dùng cho `plainto_tsquery('simple', ...)` ở bước truy vấn.

---

## 4. Thực hiện tìm kiếm (Repository)

### CourseRepository.FullTextSearchCoursesAsync

1. **Query cơ bản**:  
   - `Courses` + include `Category`, `Category.Parent`.  
   - Lọc `IsActive == true` và `Status == Published`.

2. **Áp dụng full-text search** (nếu có keyword):
   - `plainText = StringHelper.FormatSearchTermPlain(keyword)`.
   - Lọc: `SearchVector != null && SearchVector.Matches(EF.Functions.PlainToTsQuery("simple", plainText))`.  
   - EF Core dịch sang SQL: `plainto_tsquery('simple', @p)` và toán tử `@@` với tsvector.

3. **Đếm**: `totalCount = await query.CountAsync()`.

4. **Sắp xếp**:
   - Có keyword: sắp theo **rank** giảm dần:  
     `SearchVector.Rank(EF.Functions.PlainToTsQuery("simple", plainText))`  
     → kết quả liên quan hơn lên trước.
   - Không keyword: sắp theo `CreatedAt` giảm dần.

5. **Phân trang**: `Skip((pageNumber - 1) * pageSize).Take(pageSize)`.

---

## 5. Luồng từ API đến DB

```
Client: GET /api/v1/courses/search?Keyword=lap+trinh&PageNumber=1&PageSize=10
    ↓
CourseApis.FullTextSearchCoursesAsync → ICourseService.FullTextSearchCoursesAsync(request)
    ↓
CourseService.FullTextSearchCoursesAsync → unitOfWork.CourseRepository.FullTextSearchCoursesAsync(...)
    ↓
CourseRepository:
  - FormatSearchTermPlain("lap trinh") → "lap trinh" (đã bỏ dấu + sanitize)
  - query.Where(SearchVector.Matches(PlainToTsQuery("simple", "lap trinh")))
  - OrderBySearchRank (rank giảm dần)
  - Skip/Take → ToListAsync()
    ↓
PostgreSQL: thực thi SQL với plainto_tsquery('simple','lap trinh') và @@ trên SearchVector (GIN index)
    ↓
Trả về ApiResponse<List<CourseResponse>> có phân trang.
```

---

## 6. Cập nhật SearchVector khi dữ liệu thay đổi

```
User / System: thêm hoặc sửa Course (Create, Update metadata, v.v.)
    ↓
CatalogDbContext.SaveChangesAsync:
  1. Lấy danh sách course Id (Added/Modified)
  2. base.SaveChangesAsync() → ghi xuống DB
  3. Nếu có course thay đổi → ExecuteSqlRawAsync("SELECT courses_search_vector_update_for_ids(@p0)", courseIds)
    ↓
PostgreSQL: function cập nhật SearchVector cho đúng các bản ghi trong p_ids (dùng unaccent + to_tsvector + setweight như trên).
```

---

## 7. Lưu ý

- **Config tsvector**: Dùng `'simple'` (không stem), phù hợp tiếng Việt và đơn giản hóa so khớp.
- **unaccent**: Cần extension `unaccent` và (tùy môi trường) cấu hình hỗ trợ tiếng Việt để "lap trinh" khớp "Lập trình" ổn định.
- **Keyword rỗng**: Không áp dụng full-text, chỉ lọc Published + Active, sắp theo `CreatedAt`, có phân trang.
- **Bảo mật**: Chuẩn hóa và sanitize keyword tránh lỗi cú pháp tsquery; không dùng raw keyword trong SQL.

---

## 8. Tham chiếu code

| Thành phần | Vị trí |
|------------|--------|
| API search | `Beyond8.Catalog.Api/Apis/CourseApis.cs` – MapGet `/search`, FullTextSearchCoursesAsync |
| Service | `Beyond8.Catalog.Application/Services/Implements/CourseService.cs` – FullTextSearchCoursesAsync |
| Repository | `Beyond8.Catalog.Infrastructure/Repositories/Implements/CourseRepository.cs` – ApplyFullTextSearch, OrderBySearchRank, FullTextSearchCoursesAsync |
| Chuẩn hóa từ khóa | `Beyond8.Catalog.Application/Helpers/StringHelper.cs` – FormatSearchTermPlain, RemoveDiacritics, SanitizeForTsQuery |
| Cập nhật SearchVector | `Beyond8.Catalog.Infrastructure/Data/CatalogDbContext.cs` – SaveChangesAsync gọi courses_search_vector_update_for_ids |
| Function PostgreSQL | Migration `20260202120000_RemoveSearchableTextUsePostgresFullText.cs` – định nghĩa courses_search_vector_update_for_ids |
