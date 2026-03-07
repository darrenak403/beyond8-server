# 🛠️ MCP Tools Guide - Beyond8 Server

This project is equipped with **Model Context Protocol (MCP)** servers to provide real-time access to system resources. **ALWAYS use these tools FIRST** before making assumptions about the current state of the system.

---

## 🐘 PostgreSQL MCP

**Connection:** `postgresql://postgres:postgres@localhost:5432/beyond8_identity`

### When to use

- Query actual database schema before creating/modifying entities
- Verify table structures, columns, constraints, and indexes
- Check existing data before writing seed scripts
- Validate foreign key relationships
- Inspect migration history in `__EFMigrationsHistory` table

### Example queries

```sql
-- Check Users table schema
SELECT column_name, data_type, character_maximum_length, is_nullable
FROM information_schema.columns
WHERE table_name = 'Users';

-- Verify relationships
SELECT tc.constraint_name, tc.table_name, kcu.column_name,
       ccu.table_name AS foreign_table_name, ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage ccu ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY';

-- Check migration status
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId" DESC LIMIT 5;

-- Check indexes on a table
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'Orders';
```

---

## 🐳 Docker MCP

### When to use

- Verify service health before debugging connection issues
- Check which containers are running
- Inspect container logs for errors
- Validate port mappings and network configurations
- Monitor resource usage

### Common checks

```bash
# List running containers
docker ps

# Check specific service
docker ps --filter "name=beyond8"

# View logs
docker logs <container-id> --tail 50

# Inspect network
docker network inspect bridge

# Check container stats
docker stats --no-stream
```

---

## 📁 Filesystem MCP

**Workspace:** `${workspaceFolder}`

### When to use

- Navigate project structure efficiently
- Find configuration files (appsettings.json, etc.)
- Locate specific entity, DTO, or service files
- Search for code patterns across multiple files
- Verify file existence before creating new ones

### Efficient search patterns

- Find all DbContext: `**/Data/*DbContext.cs`
- Find all entities: `**/Domain/Entities/*.cs`
- Find all DTOs: `**/Application/Dtos/**/*.cs`
- Find config files: `**/appsettings*.json`
- Find all controllers: `**/*Controller.cs`
- Find validators: `**/Validators/**/*.cs`

---

## 🔀 Git MCP

**Repository:** `${workspaceFolder}`

### When to use

- Review recent commits before making changes
- Check uncommitted changes and staged files
- View file history to understand evolution
- Identify who last modified a file
- Check current branch and status

### Useful commands

```bash
# Recent commits
git log --oneline -10

# Current status
git status

# File history
git log --follow <file-path>

# Show changes
git diff HEAD~1

# Check who modified
git blame <file-path>

# Search in commit messages
git log --grep="order" --oneline
```

---

## 🌐 Brave Search MCP

**API:** Brave Search API (requires free API key)

### When to use

- Find latest documentation for new libraries/frameworks
- Search for recent solutions to specific errors
- Discover best practices from current resources
- Find updated API references (AWS SDK, EF Core, etc.)

**Get API Key:** https://brave.com/search/api/ (2,000 requests/month free)

### Search strategies

```
"ASP.NET Core 9 new features"
".NET 9 performance improvements Entity Framework"
"PostgreSQL JSONB query best practices"
"Docker multi-stage build .NET"
"MassTransit RabbitMQ retry policy"
```

---

## 📄 Fetch MCP

**Purpose:** Read web page content after finding it via Brave Search

**Workflow:** Brave Search → Find docs → Fetch → Read content → Implement

---

## 🧠 Sequential Thinking MCP

**Purpose:** Break complex problems into logical steps

### Use for

- Architecture design
- Migrations
- Refactoring
- Complex debugging
- Multi-step workflows

### Example prompt

```
"Design the Order service architecture considering:
- Payment integration with VNPay
- Enrollment creation after payment
- Instructor payout with 14-day escrow
- Refund workflow"
```

---

## 💾 Memory MCP

**Purpose:** Remember decisions and patterns across sessions

### Stores

- Architectural choices
- Known bugs and workarounds
- Coding conventions
- User preferences

### Usage

```
Store: "Use OrderStatus enum instead of string for Order.Status"
Store: "Always use ApiResponse<T> wrapper for service methods"
Recall: "What was the decision about pagination?"
```

---

## 🎯 MCP-Driven Development Workflow

### Before Creating/Modifying Entities

```
1. Query PostgreSQL MCP → Check existing schema
2. Review Git MCP → See related recent changes
3. Search Filesystem MCP → Find similar entities
4. Then: Create/modify entity with confidence
```

### Before Adding Migrations

```
1. PostgreSQL MCP → Verify current database state
2. Check __EFMigrationsHistory → Last migration
3. Docker MCP → Ensure database container is running
4. Then: Add migration with accurate Up/Down methods
```

### Before Implementing Services

```
1. Filesystem MCP → Find similar service patterns
2. PostgreSQL MCP → Understand data relationships
3. Git MCP → Review related recent implementations
4. Then: Implement service following established patterns
```

### Before Debugging Issues

```
1. Docker MCP → Check service health
2. PostgreSQL MCP → Verify data integrity
3. Git MCP → Check recent changes that might have caused issues
4. Filesystem MCP → Locate related configuration files
5. Then: Debug with full context
```

### Before Writing API Endpoints

```
1. Filesystem MCP → Find existing endpoint patterns
2. PostgreSQL MCP → Understand data structure
3. Git MCP → Review API conventions used in recent commits
4. Then: Implement endpoint following project standards
```

### When Learning New Technology

```
1. Brave Search → Find official docs
2. Fetch → Read the documentation
3. Memory → Store key patterns
4. Then: Implement following best practices
```

### For Complex Decisions

```
1. Sequential Thinking → Break down problem
2. Memory → Check previous decisions
3. Git → Review similar implementations
4. Then: Implement with documented reasoning
```

---

## 📋 MCP Best Practices

### ✅ DO

- **Query database schema** before creating entities or migrations
- **Check Docker status** before debugging connection issues
- **Use Git history** to understand code evolution
- **Search filesystem** before assuming files don't exist
- **Combine multiple MCPs** for comprehensive context (PostgreSQL + Git + Docker)
- **Research with Brave Search + Fetch** for unknown libraries
- **Use Sequential Thinking** for complex decisions
- **Store decisions in Memory** for consistency

### ❌ DON'T

- Assume database schema matches entity definitions
- Create duplicate files without checking filesystem
- Debug connection issues without checking Docker
- Make breaking changes without reviewing Git history
- Ignore migration history in database
- Use outdated docs (use Brave Search + Fetch)
- Rush complex decisions without sequential thinking
- Forget to store decisions in Memory

---

## 🚀 Quick MCP Commands Reference

| Task                | MCP Tool            | Command/Query                                                                               |
| ------------------- | ------------------- | ------------------------------------------------------------------------------------------- |
| Check Users table   | PostgreSQL          | `SELECT tablename FROM pg_tables WHERE tablename = 'Users';`                                |
| Services running    | Docker              | `docker ps --format "table {{.Names}}\t{{.Status}}"`                                        |
| Find controllers    | Filesystem          | `**/*Controller.cs`                                                                         |
| Last 5 commits      | Git                 | `git log --oneline -5`                                                                      |
| Verify foreign keys | PostgreSQL          | `SELECT * FROM information_schema.table_constraints WHERE constraint_type = 'FOREIGN KEY';` |
| Container logs      | Docker              | `docker logs <container> --tail 50`                                                         |
| Find appsettings    | Filesystem          | `**/appsettings*.json`                                                                      |
| Uncommitted changes | Git                 | `git status --short`                                                                        |
| Find .NET docs      | Brave Search        | "ASP.NET Core 9 new features"                                                               |
| Read docs           | Fetch               | Fetch URL from search                                                                       |
| Design architecture | Sequential Thinking | "Microservices vs monolith"                                                                 |
| Store decision      | Memory              | "CQRS for Catalog service"                                                                  |

---

## 🔍 Before Every Task - MCP Checklist

- [ ] 🐘 **PostgreSQL MCP**: Query schema if touching database
- [ ] 🐳 **Docker MCP**: Check service health if debugging
- [ ] 🔀 **Git MCP**: Review recent changes if refactoring
- [ ] 📁 **Filesystem MCP**: Search for patterns before creating
- [ ] 🌐 **Brave Search + Fetch**: Research unknown libraries/errors
- [ ] 🧠 **Sequential Thinking**: Plan complex architectural changes
- [ ] 💾 **Memory**: Store/retrieve important decisions
