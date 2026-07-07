<script setup lang="ts">
import { reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { useAuthStore } from '../stores/auth'
import { router } from '../router'

const auth = useAuthStore()
auth.hydrate()

const form = reactive({
  username: 'admin',
  password: 'Admin@123',
})
const loading = ref(false)

async function onSubmit() {
  loading.value = true
  try {
    await auth.login(form.username, form.password)
    await auth.fetchMe().catch(() => {})
    const redirect = (router.currentRoute.value.query.redirect as string | undefined) ?? '/dashboard'
    await router.replace(redirect)
  } catch (e: any) {
    ElMessage.error(e?.message ?? '登录失败')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="page">
    <div class="card">
      <div class="title">WatchShop Admin</div>
      <el-form @submit.prevent="onSubmit" label-position="top">
        <el-form-item label="用户名">
          <el-input v-model="form.username" autocomplete="username" />
        </el-form-item>
        <el-form-item label="密码">
          <el-input v-model="form.password" type="password" autocomplete="current-password" show-password />
        </el-form-item>
        <el-button type="primary" :loading="loading" style="width: 100%" @click="onSubmit">登录</el-button>
      </el-form>
    </div>
  </div>
</template>

<style scoped>
.page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  background: linear-gradient(135deg, #0b1020, #121a33);
}
.card {
  width: 360px;
  padding: 20px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.98);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.35);
}
.title {
  font-weight: 700;
  font-size: 20px;
  margin-bottom: 12px;
}
</style>

