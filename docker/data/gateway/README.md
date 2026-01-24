# Beyond8 Nginx Configuration

Nginx reverse proxy configuration cho Beyond8 microservices architecture.

## Cấu trúc

```
nginx/
├── default.conf          # Nginx configuration file
├── docker-compose.yml    # Docker setup (optional)
├── ssl/                  # SSL certificates folder (create manually)
│   ├── beyond8.crt
│   └── beyond8.key
└── README.md
```

## Services & Ports

| Service | Port | Endpoints |
|---------|------|-----------|
| **Identity Service** | 5291 | `/api/v1/auth/*`, `/api/v1/users/*`, `/api/v1/instructors/*` |
| **Integration Service** | 5234 | `/api/v1/media/*`, `/api/v1/ai/*`, `/api/v1/notifications/*`, `/hubs/app` |
| **Nginx (Production)** | 80, 443 | All traffic |
| **Nginx (Development)** | 8080 | All traffic (HTTP only) |

## Tính năng

### 1. **Reverse Proxy**
- Route requests đến đúng microservice
- Load balancing với keepalive connections
- WebSocket support cho SignalR

### 2. **Security**
- SSL/TLS termination
- Security headers (HSTS, X-Frame-Options, etc.)
- CORS configuration
- Rate limiting:
  - General: 100 requests/minute
  - Auth: 10 requests/minute
  - Upload: 20 requests/minute

### 3. **Performance**
- HTTP/2 support
- Response caching (AI usage statistics, OpenAPI docs)
- Connection keepalive
- Gzip compression (nginx default)

### 4. **Monitoring**
- Health check endpoint: `/health`
- Request ID tracking
- Access & error logs
- Cache status headers

## Setup

### Option 1: Direct Nginx Installation

#### 1. Install Nginx

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install nginx
```

**Windows:**
Download từ: https://nginx.org/en/download.html

#### 2. Copy Configuration

**Linux:**
```bash
sudo cp default.conf /etc/nginx/sites-available/beyond8
sudo ln -s /etc/nginx/sites-available/beyond8 /etc/nginx/sites-enabled/
sudo rm /etc/nginx/sites-enabled/default  # Remove default config
```

**Windows:**
```bash
copy default.conf C:\nginx\conf\nginx.conf
```

#### 3. Setup SSL Certificates

**Create self-signed certificate (Development):**
```bash
mkdir ssl
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ssl/beyond8.key \
  -out ssl/beyond8.crt \
  -subj "/CN=api.beyond8.io.vn"
```

**Production:** Sử dụng Let's Encrypt
```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d api.beyond8.io.vn
```

#### 4. Update Configuration

Edit `default.conf` và update:
- `server_name`: Your domain
- SSL certificate paths
- CORS `Access-Control-Allow-Origin`: Your frontend domain

#### 5. Test & Reload

```bash
# Test configuration
sudo nginx -t

# Reload Nginx
sudo systemctl reload nginx

# Check status
sudo systemctl status nginx
```

### Option 2: Docker Setup

#### 1. Create SSL Folder
```bash
mkdir ssl
# Add your SSL certificates to ssl/ folder
```

#### 2. Build Service Images (if not built)
```bash
# Build Identity Service
docker build -t beyond8-identity-api:latest \
  -f src/Services/Identity/Beyond8.Identity.Api/Dockerfile .

# Build Integration Service
docker build -t beyond8-integration-api:latest \
  -f src/Services/Integration/Beyond8.Integration.Api/Dockerfile .
```

#### 3. Start Services
```bash
docker-compose up -d
```

#### 4. View Logs
```bash
docker-compose logs -f nginx
```

#### 5. Stop Services
```bash
docker-compose down
```

## Routing Logic

### Identity Service
- `POST /api/v1/auth/login` → Auth login
- `POST /api/v1/auth/register` → User registration
- `GET /api/v1/users/*` → User management
- `GET /api/v1/instructors/*` → Instructor management

### Integration Service
- `POST /api/v1/media/*` → File uploads (max 20MB)
- `POST /api/v1/ai/*` → AI operations (timeout 300s)
- `GET /api/v1/ai-usage/*` → AI statistics (cached 5min)
- `POST /api/v1/vnpt-ekyc/*` → KYC verification
- `GET /api/v1/notifications/*` → Notifications
- `WS /hubs/app` → SignalR WebSocket

## Rate Limiting

| Zone | Rate | Burst | Endpoints |
|------|------|-------|-----------|
| `general` | 100/min | 20 | Most endpoints |
| `auth` | 10/min | 5 | Login, register |
| `upload` | 20/min | 10 | Media uploads, KYC |

**Example Response (429):**
```json
{
  "error": "Too Many Requests",
  "message": "Rate limit exceeded. Please try again later"
}
```

## Caching Strategy

| Resource | Cache Time | Zone |
|----------|------------|------|
| AI Usage Stats | 5 minutes | `beyond8_cache` |
| OpenAPI Docs | 1 hour | `beyond8_cache` |
| Media Uploads | No cache | - |
| Notifications | No cache | - |

**Cache Headers:**
```
X-Cache-Status: HIT     # Served from cache
X-Cache-Status: MISS    # Not in cache
X-Cache-Status: BYPASS  # Cache bypassed
```

## Environment-Specific Settings

### Development (Port 8080)
- HTTP only (no SSL)
- CORS: Allow all origins (`*`)
- Max upload: 50MB
- No rate limiting
- Access: `http://localhost:8080`

### Production (Ports 80/443)
- HTTPS required (HTTP → HTTPS redirect)
- CORS: Specific origin only
- Max upload: 20MB
- Rate limiting enabled
- Access: `https://api.beyond8.io.vn`

## Testing

### 1. Health Check
```bash
curl http://localhost:8080/health
# Response: healthy
```

### 2. Test Identity Service
```bash
curl http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password"}'
```

### 3. Test Integration Service
```bash
curl http://localhost:8080/api/v1/notifications/my-notifications \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 4. Test SignalR Connection
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:8080/hubs/app")
  .build();

await connection.start();
```

### 5. Check Rate Limiting
```bash
# Send 15 requests quickly (should get 429 after 10th request)
for i in {1..15}; do
  curl http://localhost:8080/api/v1/auth/login -X POST
done
```

## Monitoring & Logs

### View Access Logs
```bash
# Docker
docker-compose exec nginx tail -f /var/log/nginx/beyond8_access.log

# Direct install
sudo tail -f /var/log/nginx/beyond8_access.log
```

### View Error Logs
```bash
# Docker
docker-compose exec nginx tail -f /var/log/nginx/beyond8_error.log

# Direct install
sudo tail -f /var/log/nginx/beyond8_error.log
```

### Cache Statistics
```bash
# Check cache directory size
du -sh /var/cache/nginx/beyond8

# Clear cache
sudo rm -rf /var/cache/nginx/beyond8/*
sudo nginx -s reload
```

## Troubleshooting

### Issue: 502 Bad Gateway
**Cause:** Backend services not running
**Solution:**
```bash
# Check if services are running
curl http://localhost:5291/health  # Identity
curl http://localhost:5234/health  # Integration
```

### Issue: 413 Request Entity Too Large
**Cause:** File upload exceeds `client_max_body_size`
**Solution:** Update in `default.conf`:
```nginx
client_max_body_size 50M;  # Increase limit
```

### Issue: SignalR connection fails
**Cause:** WebSocket headers not properly configured
**Solution:** Verify in `/hubs/app` location:
```nginx
proxy_http_version 1.1;
proxy_set_header Upgrade $http_upgrade;
proxy_set_header Connection "upgrade";
```

### Issue: CORS errors
**Cause:** Frontend origin not allowed
**Solution:** Update CORS headers:
```nginx
add_header Access-Control-Allow-Origin "https://your-frontend.com" always;
```

## Production Checklist

- [ ] SSL certificates installed and valid
- [ ] `server_name` updated to your domain
- [ ] CORS origins configured correctly
- [ ] Backend service URLs correct
- [ ] Rate limits adjusted for production load
- [ ] Log rotation configured
- [ ] Monitoring/alerting setup
- [ ] Firewall rules configured (allow 80, 443)
- [ ] DNS records pointing to server

## Security Best Practices

1. **Always use HTTPS in production**
2. **Keep SSL certificates up to date**
3. **Set restrictive CORS origins**
4. **Enable rate limiting**
5. **Regular log monitoring**
6. **Update Nginx regularly**
7. **Use strong SSL ciphers only**
8. **Enable security headers**

## Performance Tuning

### For High Traffic
```nginx
# In http block
worker_processes auto;
worker_connections 4096;

# In upstream blocks
keepalive 64;  # Increase keepalive connections

# Adjust rate limits
limit_req_zone ... rate=1000r/m;  # Increase rate
```

### For Large Files
```nginx
client_max_body_size 100M;
client_body_buffer_size 256k;
proxy_buffering on;
proxy_buffer_size 4k;
proxy_buffers 8 4k;
```

## Additional Resources

- [Nginx Documentation](https://nginx.org/en/docs/)
- [Let's Encrypt](https://letsencrypt.org/)
- [SignalR with Nginx](https://docs.microsoft.com/en-us/aspnet/core/signalr/scale)
