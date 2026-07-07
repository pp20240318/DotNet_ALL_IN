# WatchShop Admin App

Vue 3 + Vite + TypeScript 管理后台前端。

## 路径问题（重要）

当前仓库在 `E:\99Test\c#\DotNet_ALL_IN`，路径中的 **`#` 会导致 Vite dev/build 在 Windows 上失败**（无法加载 `/src/main.ts`）。

### 推荐方案（一劳永逸）

把目录 **`c#` 重命名为 `csharp`**，例如：

```
E:\99Test\csharp\DotNet_ALL_IN
```

然后在 `admin-app` 下正常执行：

```bash
pnpm install
pnpm dev
```

### 临时方案（不重命名）

在 `admin-app` 目录执行，会复制到无 `#` 路径再启动 dev：

```bash
pnpm dev:copy
```

开发目录：`E:\99Test\watchshop-admin-dev`（修改源码后需重新运行 `pnpm dev:copy` 同步）

### 预览方案（无需 dev）

```bash
pnpm build
pnpm preview
```

访问 `http://localhost:4173`（API 走 `/__api` 代理）。

## 启动后端

```bash
dotnet run --project ../src/WatchShop.Admin.Api --launch-profile http
```

## 默认账号

- `admin` / `Admin@123`
