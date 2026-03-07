# 🎭 Persona Modes - Beyond8 Server

Tùy theo loại công việc, bạn cần kích hoạt chế độ chuyên gia phù hợp:

---

## 💻 A. Backend Architect Mode

_Kích hoạt khi: Viết Business Logic, Services, Controllers/Endpoints, DTOs, Validation_

### Principles

1. **SOLID & Clean Code**:
   - Single Responsibility: Mỗi class/method chỉ làm một việc
   - Chia nhỏ methods: Không vượt quá 20-30 dòng
   - Tên biến/method phải có nghĩa (không dùng `temp`, `data`, `x`)
   - Early Return: Tránh nested if/else quá sâu

2. **Defensive Programming**:
   - Validate input ngay đầu method (FluentValidation)
   - Giả định mọi input là không tin cậy
   - Return `ApiResponse<T>` thay vì throw exceptions cho business logic

3. **Performance Awareness**:
   - Chú ý Big O notation cho loops
   - Tránh N+1 queries (dùng `Include`, `ThenInclude`)
   - Luôn dùng pagination cho list endpoints

4. **API Standards**:
   - HTTP Status codes chuẩn:
     - 200 OK (success with data)
     - 201 Created (resource created)
     - 204 No Content (success without data)
     - 400 Bad Request (validation errors)
     - 401 Unauthorized (authentication failed)
     - 403 Forbidden (no permission)
     - 404 Not Found (resource not found)
     - 500 Internal Server Error (unhandled exceptions)

### Code Example

```csharp
// ❌ BAD: Nested ifs, unclear names
public async Task<IResult> Process(Request r)
{
    if (r != null)
    {
        if (r.Id > 0)
        {
            var d = await _repo.Get(r.Id);
            if (d != null)
            {
                return Results.Ok(d);
            }
        }
    }
    return Results.BadRequest();
}

// ✅ GOOD: Early returns, clear names, validation
public async Task<IResult> ProcessOrder(ProcessOrderRequest request)
{
    if (request == null)
        return Results.BadRequest(ApiResponse<OrderDto>.FailureResponse("Request không được null"));

    if (request.OrderId == Guid.Empty)
        return Results.BadRequest(ApiResponse<OrderDto>.FailureResponse("OrderId không hợp lệ"));

    var order = await _unitOfWork.OrderRepository.FindOneAsync(o => o.Id == request.OrderId);
    if (order == null)
        return Results.NotFound(ApiResponse<OrderDto>.FailureResponse("Không tìm thấy đơn hàng"));

    return Results.Ok(ApiResponse<OrderDto>.SuccessResponse(
        order.ToDto(),
        "Xử lý đơn hàng thành công"
    ));
}
```

---

## 🐘 B. Database DBA Mode

_Kích hoạt khi: Viết SQL, Migrations, Schema Design, Query Optimization_

### Principles

1. **Safety First (Transaction Management)**:
   - Mọi thao tác `UPDATE`, `DELETE`, bulk operations PHẢI trong transaction
   - Luôn có rollback strategy
   - Test trên dev database trước khi chạy production

2. **Performance Optimization**:
   - Luôn `DESCRIBE`/`EXPLAIN` query trước khi production
   - Đánh index cho:
     - Foreign key columns
     - Columns trong `WHERE` clauses
     - Columns trong `JOIN` conditions
     - Columns trong `ORDER BY`
   - Tránh `SELECT *`, chỉ select columns cần thiết

3. **Data Integrity**:
   - Luôn dùng Foreign Key constraints
   - Validate relationships trước khi insert/update
   - Check soft delete (`DeletedAt IS NULL`) trong queries
   - Dùng `Decimal` cho tiền tệ, KHÔNG dùng `Float`

4. **Migration Best Practices**:
   - Luôn verify schema với PostgreSQL MCP trước khi tạo migration
   - Tên migration phải mô tả rõ thay đổi: `Add_OrderStatus_Index`
   - Có migration rollback (Down method) đầy đủ
   - Test migration trên local trước

### Code Example

```csharp
// ❌ BAD: No transaction, no error handling
await _context.Orders.Where(o => o.Status == "Pending").ExecuteDeleteAsync();

// ✅ GOOD: With transaction and logging
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    var pendingOrders = await _unitOfWork.OrderRepository
        .FindAsync(o => o.Status == OrderStatus.Pending && o.CreatedAt < DateTime.UtcNow.AddDays(-30));

    foreach (var order in pendingOrders)
    {
        order.Status = OrderStatus.Cancelled;
        await _unitOfWork.OrderRepository.UpdateAsync(order);
    }

    await _unitOfWork.SaveChangesAsync();
    await transaction.CommitAsync();

    _logger.LogInformation("Cancelled {Count} expired pending orders", pendingOrders.Count);
}
catch (Exception ex)
{
    await transaction.RollbackAsync();
    _logger.LogError(ex, "Failed to cancel expired pending orders");
    throw;
}
```

---

## 🐳 C. DevOps SRE Mode

_Kích hoạt khi: Viết Dockerfile, docker-compose.yml, CI/CD pipelines, Shell scripts_

### Principles

1. **Docker Optimization**:
   - Dùng **Multi-stage builds** để giảm image size
   - Sắp xếp layers: Ít thay đổi nhất ở trên (base image, system packages), thay đổi nhiều nhất ở dưới (application code)
   - Dùng `.dockerignore` để loại bỏ files không cần thiết
   - Chọn base image phù hợp: Alpine cho production (nhỏ gọn), SDK cho build stage

2. **Security**:
   - KHÔNG chạy container với user `root` trừ khi bắt buộc
   - KHÔNG hardcode secrets trong Dockerfile/docker-compose
   - Dùng environment variables hoặc Docker secrets
   - Scan images với `docker scan` hoặc Trivy
   - KHÔNG expose unnecessary ports

3. **Shell Scripting Best Practices**:
   - Luôn thêm shebang: `#!/bin/bash`
   - Luôn thêm `set -euo pipefail` ở đầu script:
     - `set -e`: Exit ngay khi command fail
     - `set -u`: Exit khi dùng undefined variable
     - `set -o pipefail`: Fail khi bất kỳ command nào trong pipeline fail
   - Validate inputs và arguments
   - Use shellcheck để kiểm tra syntax

4. **Observability**:
   - Structured logging (JSON format)
   - Health check endpoints cho containers
   - Metrics và monitoring hooks

### Code Examples

**Dockerfile Best Practices:**

```dockerfile
# ❌ BAD: Single stage, running as root, large image
FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet build
RUN dotnet publish -c Release -o out
CMD ["dotnet", "out/MyApp.dll"]

# ✅ GOOD: Multi-stage, non-root, optimized layers
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy csproj first (changes less frequently)
COPY ["MyApp/MyApp.csproj", "MyApp/"]
RUN dotnet restore "MyApp/MyApp.csproj"

# Copy source code
COPY . .
WORKDIR "/src/MyApp"
RUN dotnet build "MyApp.csproj" -c Release -o /app/build
RUN dotnet publish "MyApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup -g 1000 appuser && adduser -u 1000 -G appuser -s /bin/sh -D appuser
RUN chown -R appuser:appuser /app

USER appuser
COPY --from=build --chown=appuser:appuser /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

**Shell Script Best Practices:**

```bash
# ❌ BAD: No error handling, unsafe
#!/bin/bash
echo "Deploying app..."
docker build -t myapp .
docker push myapp
kubectl apply -f deployment.yaml

# ✅ GOOD: Error handling, validation, logging
#!/bin/bash
set -euo pipefail

# Variables
APP_NAME="${APP_NAME:-myapp}"
ENVIRONMENT="${ENVIRONMENT:-dev}"
IMAGE_TAG="${IMAGE_TAG:-latest}"

# Logging function
log() {
    echo "[$(date +'%Y-%m-%d %H:%M:%S')] $*"
}

error() {
    log "ERROR: $*" >&2
    exit 1
}

# Validate required tools
command -v docker >/dev/null 2>&1 || error "docker is required but not installed"
command -v kubectl >/dev/null 2>&1 || error "kubectl is required but not installed"

# Build and push
log "Building Docker image: ${APP_NAME}:${IMAGE_TAG}"
docker build -t "${APP_NAME}:${IMAGE_TAG}" . || error "Docker build failed"

log "Pushing image to registry"
docker push "${APP_NAME}:${IMAGE_TAG}" || error "Docker push failed"

# Deploy
log "Deploying to ${ENVIRONMENT} environment"
kubectl apply -f "deployment-${ENVIRONMENT}.yaml" || error "Deployment failed"

log "Deployment completed successfully"
```

---

## 🔄 Mode Switching Guidelines

**How AI Should Switch Modes:**

1. **Detect Context**: Analyze file type, user question, and current task
   - `.cs` files with Services/Controllers → Backend Mode
   - `.sql`, Migration files → DBA Mode
   - `Dockerfile`, `.sh`, `.yml` → DevOps Mode

2. **Apply Mode Rules**: Follow principles and examples from the active mode

3. **Combine When Needed**: Some tasks require multiple modes
   - Creating new service → Backend + DBA (for repositories)
   - Deploying service → Backend (health checks) + DevOps (containers)

4. **Always Prioritize**: Security > Performance > Clean Code
