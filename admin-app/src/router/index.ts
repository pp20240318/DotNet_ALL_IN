import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const routes: RouteRecordRaw[] = [
  { path: '/login', name: 'login', component: () => import('../views/LoginView.vue') },
  {
    path: '/',
    component: () => import('../views/LayoutView.vue'),
    children: [
      { path: '', redirect: '/dashboard' },
      {
        path: 'dashboard',
        name: 'dashboard',
        component: () => import('../views/DashboardView.vue'),
        meta: { permission: 'dashboard:read' },
      },
      {
        path: 'products',
        name: 'products',
        component: () => import('../views/ProductsView.vue'),
        meta: { permission: 'product:read' },
      },
      {
        path: 'brands',
        name: 'brands',
        component: () => import('../views/BrandsView.vue'),
        meta: { permission: 'product:read' },
      },
      {
        path: 'orders',
        name: 'orders',
        component: () => import('../views/OrdersView.vue'),
        meta: { permission: 'order:read' },
      },
      {
        path: 'notifications',
        name: 'notifications',
        component: () => import('../views/NotificationsView.vue'),
        meta: { permission: 'dashboard:read' },
      },
      {
        path: 'rbac',
        name: 'rbac',
        component: () => import('../views/RbacView.vue'),
        meta: { permission: 'system:admin' },
      },
    ],
  },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()

  if (to.path === '/login') return true
  if (!auth.isLoggedIn) return { path: '/login', query: { redirect: to.fullPath } }

  const required = (to.meta?.permission as string | undefined) ?? undefined
  if (!required) return true
  if (auth.hasPermission(required)) return true

  return { path: '/dashboard' }
})

