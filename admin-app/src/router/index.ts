import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { connectNotifications } from '../utils/signalr'

const routes: RouteRecordRaw[] = [
  { path: '/login', name: 'login', component: () => import('../views/LoginView.vue'), meta: { title: '登录' } },
  {
    path: '/',
    component: () => import('../views/LayoutView.vue'),
    children: [
      { path: '', redirect: '/dashboard' },
      {
        path: 'dashboard',
        name: 'dashboard',
        component: () => import('../views/DashboardView.vue'),
        meta: { permission: 'dashboard:read', title: '仪表盘' },
      },
      {
        path: 'products',
        name: 'products',
        component: () => import('../views/ProductsView.vue'),
        meta: { permission: 'product:read', title: '商品管理' },
      },
      {
        path: 'brands',
        name: 'brands',
        component: () => import('../views/BrandsView.vue'),
        meta: { permission: 'product:read', title: '品牌管理' },
      },
      {
        path: 'categories',
        name: 'categories',
        component: () => import('../views/CategoriesView.vue'),
        meta: { permission: 'product:read', title: '分类管理' },
      },
      {
        path: 'orders',
        name: 'orders',
        component: () => import('../views/OrdersView.vue'),
        meta: { permission: 'order:read', title: '订单管理' },
      },
      {
        path: 'notifications',
        name: 'notifications',
        component: () => import('../views/NotificationsView.vue'),
        meta: { permission: 'dashboard:read', title: '通知中心' },
      },
      {
        path: 'operation-logs',
        name: 'operation-logs',
        component: () => import('../views/OperationLogsView.vue'),
        meta: { permission: 'system:admin', title: '操作日志' },
      },
      {
        path: 'rbac',
        name: 'rbac',
        component: () => import('../views/RbacView.vue'),
        meta: { permission: 'system:admin', title: 'RBAC' },
      },
    ],
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('../views/NotFoundView.vue'),
    meta: { title: '页面不存在' },
  },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()
  auth.hydrate()

  if (to.name === 'not-found') return true

  if (to.path === '/login') {
    if (auth.isLoggedIn) return { path: '/dashboard' }
    return true
  }

  if (!auth.isLoggedIn) {
    return { path: '/login', query: { redirect: to.fullPath } }
  }

  try {
    await auth.ensureSession()
    if (auth.token) connectNotifications(auth.token)
  } catch {
    auth.logout()
    return { path: '/login', query: { redirect: to.fullPath } }
  }

  const required = (to.meta?.permission as string | undefined) ?? undefined
  if (!required) return true
  if (auth.hasPermission(required)) return true

  return { path: '/dashboard' }
})
