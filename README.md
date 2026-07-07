# WatchShop — 手表商城全栈示例

.NET 8 后端（Admin + Store 双 API）+ Vue 3 管理后台 + Vue 3 C 端商城，用于熟悉分层架构、CQRS、JWT、Docker 等实践。

## 仓库结构

```
src/
  WatchShop.Domain/          领域实体
  WatchShop.Application/     应用层（MediatR、DTO、校验）
  WatchShop.Infrastructure/  基础设施（SqlSugar、Redis、MQ…）
  WatchShop.Admin.Api/       管理后台 API  → :5234 / Docker :8080
  WatchShop.Store.Api/       C 端商城 API   → :5240 / Docker :8081
admin-app/                   管理后台前端   → :5173 / Docker :8082
store-app/                   C 端商城前端   → :5175 / Docker :8083
tests/WatchShop.Tests/       xUnit 单元与集成测试
doc/                         分步学习文档（00–52）
deploy/nginx/                前端静态资源 + API 反代配置
```

## 环境要求

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) + [pnpm](https://pnpm.io/)
- MySQL 8（本地或 Docker）
- 可选：Redis、RabbitMQ、MinIO、Elasticsearch（见 `docker-compose.yml`）

## 快速启动（本地开发）

### 1. 启动依赖（推荐 Docker）

```bash
docker compose up -d mysql redis
```

MySQL 默认：`dotnet_all_in1` / 用户 `dotnet_all_in2` / 密码 `dotnet_all_in3`（与 `appsettings.json` 一致）。

### 2. 启动后端

```bash
# 管理 API
dotnet run --project src/WatchShop.Admin.Api --launch-profile http

# C 端 API（另开终端）
dotnet run --project src/WatchShop.Store.Api --launch-profile http
```

- Admin Swagger: http://localhost:5234/swagger  
- Store Swagger: http://localhost:5240/swagger  

### 3. 启动前端

```bash
cd admin-app
pnpm install
pnpm dev
# → http://localhost:5173

cd store-app
pnpm install
pnpm dev
# → http://localhost:5175
```

前端通过 Vite 代理访问 `/__api`，无需额外配置 CORS。

### 4. 默认账号

| 端 | 用户名 | 密码 |
|----|--------|------|
| 管理后台 | `admin` | `Admin@123` |
| 管理后台（只读） | `viewer` | `Viewer@123` |
| C 端商城 | `demo` | `Demo@123` |

## Docker 全栈部署

```bash
# 构建并启动全部服务（MySQL、Redis、RabbitMQ、MinIO、ES、双 API、双前端）
docker compose up -d --build
```

| 服务 | 地址 |
|------|------|
| 管理后台 Web | http://localhost:8082 |
| 管理 API | http://localhost:8080 |
| C 端商城 Web | http://localhost:8083 |
| C 端 API | http://localhost:8081 |
| RabbitMQ 管理台 | http://localhost:15672 (guest/guest) |
| MinIO Console | http://localhost:9001 (minioadmin/minioadmin) |

首次启动会自动 Code First 建表并种子数据。生产环境配置见 [deploy/PRODUCTION.md](deploy/PRODUCTION.md)。

## 测试

```bash
dotnet test WatchShop.slnx
cd admin-app && pnpm build
cd store-app && pnpm build
```

CI 见 [.github/workflows/ci.yml](.github/workflows/ci.yml)。

## 生产配置提示

- 复制 `admin-app/.env.production.example` → `.env.production`，设置 `VITE_API_BASE_URL`
- 管理 API / Store API 使用 `ASPNETCORE_ENVIRONMENT=Production`
- 不要将真实密钥提交到 Git；使用环境变量或密钥管理服务
- Hangfire 面板：`/hangfire`（需 SuperAdmin 登录）

## 学习路线

详见 [doc/00-Roadmap.txt](doc/00-Roadmap.txt)。后端 52 步已完成；前端与工程化持续完善见 [PROGRESS.md](PROGRESS.md)。
