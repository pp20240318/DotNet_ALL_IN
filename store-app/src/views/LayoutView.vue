<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useCartStore } from '../stores/cart'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const cart = useCartStore()

const pageTitle = computed(() => (route.meta?.title as string) ?? 'WatchShop')

onMounted(async () => {
  if (auth.isLoggedIn) {
    try {
      await cart.refresh()
    } catch {
      cart.clearLocal()
    }
  }
})

async function logout() {
  auth.logout()
  cart.clearLocal()
  await router.push('/login')
}
</script>

<template>
  <el-container class="layout">
    <el-header class="header">
      <div class="brand" @click="router.push('/')">WatchShop</div>
      <el-menu mode="horizontal" :ellipsis="false" router :default-active="route.path">
        <el-menu-item index="/">商品</el-menu-item>
        <el-menu-item v-if="auth.isLoggedIn" index="/cart">
          购物车
          <el-badge v-if="cart.count" :value="cart.count" :max="99" style="margin-left: 6px" />
        </el-menu-item>
        <el-menu-item v-if="auth.isLoggedIn" index="/orders">我的订单</el-menu-item>
      </el-menu>
      <div class="actions">
        <template v-if="auth.isLoggedIn">
          <span class="user">{{ auth.displayName }}</span>
          <el-button size="small" @click="logout">退出</el-button>
        </template>
        <el-button v-else size="small" type="primary" @click="router.push('/login')">登录</el-button>
      </div>
    </el-header>
    <el-main>
      <h2 class="title">{{ pageTitle }}</h2>
      <RouterView />
    </el-main>
  </el-container>
</template>

<style scoped>
.layout {
  min-height: 100vh;
}
.header {
  display: flex;
  align-items: center;
  gap: 16px;
  background: #fff;
  border-bottom: 1px solid #eee;
  padding: 0 20px;
}
.brand {
  font-weight: 800;
  font-size: 18px;
  cursor: pointer;
  flex-shrink: 0;
}
.actions {
  margin-left: auto;
  display: flex;
  align-items: center;
  gap: 10px;
}
.user {
  font-size: 14px;
  color: #666;
}
.title {
  margin: 0 0 16px;
  font-size: 20px;
}
</style>
