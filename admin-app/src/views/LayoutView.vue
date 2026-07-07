<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useNotificationStore } from '../stores/notifications'
import { router } from '../router'
import { api } from '../utils/api'
import { connectNotifications } from '../utils/signalr'
import { ElMessage } from 'element-plus'
import {
  Bell,
  Expand,
  Odometer,
  Goods,
  Collection,
  Menu as MenuIcon,
  List,
  Message,
  Document,
  User,
  Setting,
} from '@element-plus/icons-vue'
import type { SearchResult } from '../types/api'
import type { Component } from 'vue'

const auth = useAuthStore()
const notifications = useNotificationStore()

if (auth.isLoggedIn && auth.token) {
  connectNotifications(auth.token)
  notifications.refreshUnreadCount()
}

const active = computed(() => router.currentRoute.value.path)
const pageTitle = computed(() => (router.currentRoute.value.meta?.title as string) ?? '管理后台')

const drawerVisible = ref(false)
const isMobile = ref(false)

function checkMobile() {
  isMobile.value = window.innerWidth < 768
  if (!isMobile.value) drawerVisible.value = false
}

function can(p?: string) {
  return !p || auth.hasPermission(p)
}

const menu = computed(() => [
  { path: '/dashboard', label: '仪表盘', permission: 'dashboard:read', icon: Odometer },
  { path: '/products', label: '商品', permission: 'product:read', icon: Goods },
  { path: '/brands', label: '品牌', permission: 'product:read', icon: Collection },
  { path: '/categories', label: '分类', permission: 'product:read', icon: MenuIcon },
  { path: '/orders', label: '订单', permission: 'order:read', icon: List },
  { path: '/customers', label: '客户', permission: 'customer:read', icon: User },
  { path: '/notifications', label: '通知', permission: 'dashboard:read', icon: Message },
  { path: '/operation-logs', label: '操作日志', permission: 'system:admin', icon: Document },
  { path: '/rbac', label: 'RBAC', permission: 'system:admin', icon: Setting },
])

const visibleMenu = computed(() => menu.value.filter((m) => can(m.permission)))

const searchKeyword = ref('')

const searchTypeLabels: Record<string, string> = {
  product: '商品',
  brand: '品牌',
  order: '订单',
}

async function onSearch(query: string, cb: (items: { value: string; item: SearchResult }[]) => void) {
  if (!query.trim()) {
    cb([])
    return
  }
  try {
    const res = await api.get<SearchResult[]>('/search', { q: query.trim(), limit: 12 })
    cb(
      (res ?? []).map((item) => ({
        value: `[${searchTypeLabels[item.type] ?? item.type}] ${item.title}`,
        item,
      })),
    )
  } catch {
    cb([])
  }
}

function onSearchSelect(row: Record<string, any>) {
  const item = row.item as SearchResult
  const { type, title } = item
  if (type === 'product') router.push({ path: '/products', query: { keyword: title } })
  else if (type === 'brand') router.push({ path: '/brands', query: { keyword: title } })
  else if (type === 'order') router.push({ path: '/orders', query: { orderNo: title } })
  else router.push('/dashboard')
  searchKeyword.value = ''
}

function onMenuSelect(path: string) {
  drawerVisible.value = false
  router.push(path)
}

async function logout() {
  auth.logout()
  notifications.clearUnread()
  ElMessage.success('已退出')
  await router.replace('/login')
}

onMounted(() => {
  checkMobile()
  window.addEventListener('resize', checkMobile)
  if (auth.isLoggedIn) {
    auth.fetchMe().catch(() => {})
    notifications.refreshUnreadCount()
  }
})

onUnmounted(() => {
  window.removeEventListener('resize', checkMobile)
})

function menuIcon(icon: Component) {
  return icon
}
</script>

<template>
  <el-container class="layout">
    <el-aside v-if="!isMobile" width="220px" class="aside">
      <div class="brand">WatchShop</div>
      <el-menu :default-active="active" router>
        <el-menu-item v-for="item in visibleMenu" :key="item.path" :index="item.path">
          <el-icon><component :is="menuIcon(item.icon)" /></el-icon>
          <span>{{ item.label }}</span>
        </el-menu-item>
      </el-menu>
    </el-aside>

    <el-drawer v-model="drawerVisible" direction="ltr" size="240px" :with-header="false" class="mobile-drawer">
      <div class="brand drawer-brand">WatchShop</div>
      <el-menu :default-active="active" @select="onMenuSelect">
        <el-menu-item v-for="item in visibleMenu" :key="item.path" :index="item.path">
          <el-icon><component :is="menuIcon(item.icon)" /></el-icon>
          <span>{{ item.label }}</span>
        </el-menu-item>
      </el-menu>
    </el-drawer>

    <el-container>
      <el-header class="header">
        <div class="header-left">
          <el-button v-if="isMobile" :icon="Expand" circle @click="drawerVisible = true" />
          <el-autocomplete
            v-if="can('dashboard:read')"
            v-model="searchKeyword"
            :fetch-suggestions="onSearch"
            placeholder="搜索..."
            clearable
            class="search-input"
            @select="onSearchSelect"
          />
        </div>
        <div class="right">
          <el-badge v-if="can('dashboard:read')" :value="notifications.unreadCount" :hidden="!notifications.unreadCount" :max="99">
            <el-button :icon="Bell" circle @click="router.push('/notifications')" />
          </el-badge>
          <el-dropdown trigger="click">
            <span class="user-trigger">
              <el-avatar :size="28" :icon="User" />
              <span class="user-name">{{ auth.displayName || auth.username }}</span>
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item disabled>{{ auth.username }}</el-dropdown-item>
                <el-dropdown-item v-for="r in auth.roles" :key="r" disabled>{{ r }}</el-dropdown-item>
                <el-dropdown-item divided @click="logout">退出登录</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>
      <el-main class="main">
        <div class="page-head">
          <el-breadcrumb separator="/">
            <el-breadcrumb-item :to="{ path: '/dashboard' }">首页</el-breadcrumb-item>
            <el-breadcrumb-item>{{ pageTitle }}</el-breadcrumb-item>
          </el-breadcrumb>
          <h2 class="page-title">{{ pageTitle }}</h2>
        </div>
        <RouterView />
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.layout {
  min-height: 100vh;
}
.aside {
  border-right: 1px solid #eee;
  background: #fff;
}
.brand {
  height: 56px;
  display: flex;
  align-items: center;
  padding: 0 16px;
  font-weight: 800;
  font-size: 18px;
  color: #1a1a2e;
}
.drawer-brand {
  border-bottom: 1px solid #eee;
}
.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid #eee;
  background: #fff;
  gap: 12px;
  padding: 0 16px;
}
.header-left {
  display: flex;
  align-items: center;
  gap: 10px;
  flex: 1;
  min-width: 0;
}
.search-input {
  width: 320px;
  max-width: 100%;
}
.right {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-shrink: 0;
}
.user-trigger {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  outline: none;
}
.user-name {
  color: #333;
  font-size: 14px;
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.main {
  background: #f6f7fb;
}
.page-head {
  margin-bottom: 16px;
}
.page-title {
  margin: 8px 0 0;
  font-size: 20px;
  font-weight: 600;
  color: #1a1a2e;
}
@media (max-width: 768px) {
  .search-input {
    width: 100%;
  }
  .user-name {
    display: none;
  }
}
</style>
