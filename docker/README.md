# Beyond8 Docker

Tổng quan cấu hình Docker cho Beyond8 Server.

## Cấu trúc thư mục

```
docker/
├── .env.example          # Biến môi trường mẫu (copy thành .env)
├── docker-compose-dev.yml   # Dev: build từ Dockerfile, đủ 5 API services
├── docker-compose-prod.yml  # Prod: image pre-built, hiện Identity + Integration
├── README.md             # File này
└── data/
    └── gateway/
        └── etc/nginx/conf.d/
            └── default.conf   # Nginx gateway – route theo service & chức năng
```

## So sánh docker-compose-dev vs docker-compose-prod

| Thành phần | docker-compose-dev | docker-compose-prod |
|------------|--------------------|----------------------|
| **API services** | Identity, Integration, Assessment, Catalog, Learning (build từ Dockerfile) | Chỉ Identity, Integration (image `ngothanhdatak/*`) |
| **Cơ sở hạ tầng** | Postgres, Redis, RabbitMQ, Aspire Dashboard, Nginx Gateway | Giống + Adminer |
| **Gateway depends_on** | Tất cả 5 API services | Chỉ Identity, Integration |
| **Build** | `build: context + dockerfile` | `image: ...` (không build) |

## Port mặc định (host)

| Service | Port (host) | Ghi chú |
|---------|-------------|--------|
| Gateway | 8080 | API công khai qua Nginx |
| Identity | 8081 | |
| Integration | 8082 | |
| Assessment | 8084 | |
| Catalog | 8085 | |
| Learning | 8086 | |
| Postgres | 5432 | |
| Redis | 6379 | |
| RabbitMQ | 5672 (AMQP), 15672 (Management) | |
| Aspire Dashboard | 18888 (UI), 18889 (OTLP) | |
| Adminer (prod) | 8083 | Chỉ trong prod |

Override bằng biến env (ví dụ `IDENTITY_SERVICE_PORT=9081`).

## Database (Postgres, database-per-service)

Tên DB dùng trong connection string (Const.*) và biến env:

| Service | Const key | Biến env (.env) | Ví dụ giá trị |
|---------|-----------|------------------|----------------|
| Identity | identity-db | IDENTITY_SERVICE_DATABASE | Identities |
| Integration | integration-db | INTEGRATION_SERVICE_DATABASE | Integrations |
| Assessment | assessment-db | ASSESSMENT_SERVICE_DATABASE | Assessments |
| Catalog | catalog-db | CATALOG_SERVICE_DATABASE | Catalogs |
| Learning | learning-db | LEARNING_SERVICE_DATABASE | Learnings |
| Sale | sale-db | (chưa trong compose) | Sales |

Cùng một Postgres container; mỗi service một database.

## Client URL (gọi giữa các service trong Docker)

Trong mạng Docker, các service gọi nhau qua tên container `beyond8-*-service:8080`:

| Service gọi | Service được gọi | Biến env (tùy chọn) |
|-------------|-------------------|----------------------|
| Integration | Identity | Clients__Identity__BaseUrl |
| Catalog | Identity, Learning | Clients__Identity__BaseUrl, Clients__Learning__BaseUrl |
| Assessment | Catalog, Learning | Clients__Catalog__BaseUrl, Clients__Learning__BaseUrl |
| Learning | Identity, Catalog | Clients__Identity__BaseUrl, Clients__Catalog__BaseUrl |

Identity không gọi HTTP tới service khác (chỉ MassTransit). Learning không gọi Assessment qua HTTP (chỉ consume event).

## Dockerfile

Có Dockerfile cho 5 API services:

- `src/Services/Identity/Beyond8.Identity.Api/Dockerfile`
- `src/Services/Integration/Beyond8.Integration.Api/Dockerfile`
- `src/Services/Assessment/Beyond8.Assessment.Api/Dockerfile`
- `src/Services/Catalog/Beyond8.Catalog.Api/Dockerfile`
- `src/Services/Learning/Beyond8.Learning.Api/Dockerfile`

**Sale** chưa có Dockerfile và chưa có trong compose.

## Nginx Gateway (default.conf)

Route được gom theo **service** và **chức năng**:

1. **Identity**: auth, users, instructors, subscriptions, identity (catch-all)
2. **Integration**: ai-usage, ai-prompts, ai, media, vnpt-ekyc, notifications, integration (catch-all), SignalR `/hubs/app`
3. **Catalog**: categories, courses, sections, lessons, lesson-documents, course-documents
4. **Assessment**: quizzes, quiz-attempts, questions, assignments, assignment-submissions
5. **Learning**: enrollments, course-reviews, certificates

Rate limit: zone `auth` (10r/m) cho auth/users; `upload` cho media/eKYC; `general` (100r/m) cho còn lại.

## Chạy Dev

```bash
cd docker
cp .env.example .env
# Chỉnh .env: JWT, DB passwords, AWS/Gemini/Resend/VNPT nếu dùng
docker compose -f docker-compose-dev.yml up -d
```

API qua gateway: `http://localhost:8080` (hoặc port đã set trong `.env`).
