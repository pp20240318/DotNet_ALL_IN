# WatchShop Store App

C 端商城前端（Vue 3 + Vite + Element Plus）。

## 开发

```bash
pnpm install
pnpm dev
# http://localhost:5175
```

需先启动 Store API：

```bash
dotnet run --project ../src/WatchShop.Store.Api --launch-profile http
```

## 演示账号

- `demo` / `Demo@123`

## 生产构建

复制 `.env.production.example` 为 `.env.production`，Docker 部署见根目录 README。
