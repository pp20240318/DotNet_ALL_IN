<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<template>
  <div class="page">
    <div class="content">
      <div class="code">404</div>
      <h1>页面不存在</h1>
      <p>您访问的地址不存在或已被移除</p>
      <el-button type="primary" @click="router.replace('/dashboard')">返回首页</el-button>
    </div>
  </div>
</template>

<style scoped>
.page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  background: #f6f7fb;
}
.content {
  text-align: center;
  padding: 24px;
}
.code {
  font-size: 72px;
  font-weight: 800;
  color: #dcdfe6;
  line-height: 1;
}
h1 {
  margin: 12px 0 8px;
  font-size: 22px;
  color: #1a1a2e;
}
p {
  margin: 0 0 20px;
  color: #888;
}
</style>
