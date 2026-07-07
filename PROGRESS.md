# WatchShop 生产化进度

最后更新：2026-07-07

## 已完成

### P0 — admin-app 功能补齐
- [x] RBAC 管理员编辑（显示名、启用/禁用）
- [x] 403 无权访问页 + 路由守卫
- [x] 统一 `getApiErrorMessage` 错误处理
- [x] SKU 入库/出库走 `api.post` 封装
- [x] 删除 HelloWorld 脚手架

### P1 — 工程化
- [x] GitHub Actions CI（dotnet test + admin-app/store-app build）
- [x] 根目录 README（从零启动指南）
- [x] `admin-app/.env.production.example`

### P2 — Docker
- [x] `Dockerfile.admin-web` + `deploy/nginx/admin.conf`
- [x] `Dockerfile.store-web` + `deploy/nginx/store.conf`
- [x] docker-compose 增加 admin-web (:8082) / store-web (:8083)

### P3 — 测试
- [x] 删除 `UnitTest1` 占位
- [x] 新增管理员更新集成测试

### P4 — store-app MVP
- [x] 商品目录、详情、加购
- [x] 登录、购物车、结算、我的订单（模拟支付/取消）

## 验收状态

| 项 | 状态 |
|----|------|
| `dotnet test WatchShop.slnx` | 待每次提交前验证 |
| `admin-app pnpm build` | 通过 |
| `store-app pnpm build` | 通过 |
| docker-compose 文档 | README 已写 |

## 待办 / 可选深化

- [ ] 生产密钥管理（User Secrets / 环境变量注入，勿提交真实值）
- [ ] admin-app / store-app E2E 测试（Playwright）
- [ ] ESLint + Prettier
- [ ] 订单退款 API + UI
- [ ] 客户管理后台页
- [ ] Elasticsearch/Redis 全开环境下的集成验证

## 阻塞项

无（本地 dotnet test 若失败，先停止正在运行的 Admin.Api 进程再 rebuild）
