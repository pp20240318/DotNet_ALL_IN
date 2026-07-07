import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const routes: RouteRecordRaw[] = [
  { path: '/login', name: 'login', component: () => import('../views/LoginView.vue'), meta: { title: '登录' } },
  {
    path: '/',
    component: () => import('../views/LayoutView.vue'),
    children: [
      { path: '', name: 'catalog', component: () => import('../views/CatalogView.vue'), meta: { title: '商品' } },
      { path: 'products/:id', name: 'product', component: () => import('../views/ProductView.vue'), meta: { title: '商品详情' } },
      { path: 'cart', name: 'cart', component: () => import('../views/CartView.vue'), meta: { title: '购物车', auth: true } },
      { path: 'orders', name: 'orders', component: () => import('../views/OrdersView.vue'), meta: { title: '我的订单', auth: true } },
    ],
  },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()
  auth.hydrate()

  if (to.path === '/login') {
    if (auth.isLoggedIn) return { path: '/' }
    return true
  }

  if (to.meta.auth && !auth.isLoggedIn) {
    return { path: '/login', query: { redirect: to.fullPath } }
  }

  return true
})
