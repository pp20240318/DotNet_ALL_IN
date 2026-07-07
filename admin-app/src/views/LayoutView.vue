<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '../stores/auth'
import { router } from '../router'
import { ElMessage } from 'element-plus'

const auth = useAuthStore()
auth.hydrate()

const active = computed(() => router.currentRoute.value.path)

function can(p?: string) {
  return !p || auth.hasPermission(p)
}

const menu = computed(() => [
  { path: '/dashboard', label: '仪表盘', permission: 'dashboard:read' },
  { path: '/products', label: '商品', permission: 'product:read' },
  { path: '/brands', label: '品牌', permission: 'product:read' },
  { path: '/orders', label: '订单', permission: 'order:read' },
  { path: '/notifications', label: '通知', permission: 'dashboard:read' },
  { path: '/rbac', label: 'RBAC', permission: 'system:admin' },
])

async function logout() {
  auth.logout()
  ElMessage.success('已退出')
  await router.replace('/login')
}
</script>

<template>
  <el-container style="min-height: 100vh">
    <el-aside width="220px" class="aside">
      <div class="brand">WatchShop</div>
      <el-menu :default-active="active" router>
        <template v-for="item in menu" :key="item.path">
          <el-menu-item v-if="can(item.permission)" :index="item.path">{{ item.label }}</el-menu-item>
        </template>
      </el-menu>
    </el-aside>
    <el-container>
      <el-header class="header">
        <div class="right">
          <span class="user">{{ auth.displayName || auth.username }}</span>
          <el-button size="small" @click="logout">退出</el-button>
        </div>
      </el-header>
      <el-main class="main">
        <RouterView />
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.aside {
  border-right: 1px solid #eee;
}
.brand {
  height: 56px;
  display: flex;
  align-items: center;
  padding: 0 12px;
  font-weight: 800;
}
.header {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  border-bottom: 1px solid #eee;
}
.right {
  display: flex;
  gap: 10px;
  align-items: center;
}
.user {
  color: #333;
}
.main {
  background: #f6f7fb;
}
</style>

