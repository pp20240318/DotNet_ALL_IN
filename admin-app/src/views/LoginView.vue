<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { useAuthStore } from '../stores/auth'
import { router } from '../router'
import { getApiErrorMessage } from '../utils/api'

const auth = useAuthStore()
auth.hydrate()

const remember = ref(!!auth.getRememberedUsername())
const form = reactive({
  username: auth.getRememberedUsername(),
  password: '',
})
const loading = ref(false)

onMounted(() => {
  if (auth.isLoggedIn) {
    router.replace('/dashboard')
  }
})

async function onSubmit() {
  if (!form.username.trim() || !form.password) {
    ElMessage.warning('请输入用户名和密码')
    return
  }
  loading.value = true
  try {
    await auth.login(form.username.trim(), form.password)
    if (remember.value) auth.rememberUsername(form.username.trim())
    else auth.forgetUsername()
    await auth.fetchMe().catch(() => {})
    const redirect = (router.currentRoute.value.query.redirect as string | undefined) ?? '/dashboard'
    await router.replace(redirect)
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '登录失败'))
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="page">
    <div class="card">
      <div class="title">WatchShop Admin</div>
      <p class="subtitle">手表商城管理后台</p>
      <el-form @submit.prevent="onSubmit" label-position="top">
        <el-form-item label="用户名">
          <el-input v-model="form.username" autocomplete="username" placeholder="请输入用户名" />
        </el-form-item>
        <el-form-item label="密码">
          <el-input
            v-model="form.password"
            type="password"
            autocomplete="current-password"
            show-password
            placeholder="请输入密码"
            @keyup.enter="onSubmit"
          />
        </el-form-item>
        <div class="options">
          <el-checkbox v-model="remember">记住用户名</el-checkbox>
        </div>
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
  width: 380px;
  padding: 28px 24px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.98);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.35);
}
.title {
  font-weight: 700;
  font-size: 22px;
  margin-bottom: 4px;
  color: #1a1a2e;
}
.subtitle {
  margin: 0 0 20px;
  color: #888;
  font-size: 13px;
}
.options {
  margin-bottom: 16px;
}
</style>
