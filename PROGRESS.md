# WatchShop 生产化进度

最后更新：2026-07-08（P6 第三轮）

## 已完成

### P0 — admin-app 功能补齐
- [x] RBAC 管理员编辑（显示名、启用/禁用）
- [x] 403 无权访问页 + 路由守卫
- [x] 统一 `getApiErrorMessage` 错误处理
- [x] SKU 入库/出库走 `api.post` 封装
- [x] 删除 HelloWorld 脚手架

### P1 — 工程化
- [x] GitHub Actions CI（dotnet test + 双前端 build + lint）
- [x] 根目录 README（从零启动指南）
- [x] `admin-app/.env.production.example`
- [x] `deploy/PRODUCTION.md` 生产密钥说明

### P2 — Docker
- [x] nginx 托管 admin-app / store-app
- [x] docker-compose 全栈（:8082 / :8083）

### P3 — 测试
- [x] 删除 UnitTest1 占位
- [x] 管理员更新、客户列表、Store 登录/购物车集成测试

### P4 — store-app MVP
- [x] 商品目录、详情、加购、结算、订单
- [x] 用户注册页

### P5 — 客户与 ESLint
- [x] 客户管理 API（`GET /customers` + `customer:read` 权限）
- [x] admin-app 客户管理页 + 菜单/仪表盘入口
- [x] store-app 注册页
- [x] ESLint（admin-app + store-app）

### P6 — 本轮新增
- [x] 订单退款 API（`POST /orders/{id}/refund`）+ 取消已支付订单时恢复库存
- [x] admin-app 退款按钮 + 已退款状态展示
- [x] 客户启用/禁用写 API（`PUT /customers/{id}` + `customer:write`）
- [x] admin-app 客户开关编辑
- [x] 操作日志日期范围筛选（`from` / `to`）
- [x] 根目录 Prettier 配置（`.prettierrc`）
- [x] store-app Element Plus 按需加载（减小 bundle）
- [x] 集成测试：客户更新、订单退款

## 验收状态

| 项 | 状态 |
|----|------|
| `dotnet test WatchShop.slnx` | 26/26 通过 |
| `admin-app pnpm build` | 通过 |
| `admin-app pnpm lint` | 通过 |
| `store-app pnpm build` | 通过 |
| `store-app pnpm lint` | 通过 |

## 待办 / 可选深化

- [ ] Playwright E2E
- [ ] Prettier format 脚本接入 CI
- [ ] Elasticsearch/Redis 全开环境联调

## 阻塞项

无
