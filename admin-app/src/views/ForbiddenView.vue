<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const fromPath = computed(() => {
  const from = route.query.from
  return typeof from === 'string' ? from : ''
})
</script>

<template>
  <div class="page">
    <div class="content">
      <div class="code">403</div>
      <h1>无权访问</h1>
      <p>您没有访问该页面的权限，请联系管理员分配角色。</p>
      <p v-if="fromPath" class="from">请求路径：{{ fromPath }}</p>
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
  margin: 0 0 12px;
  color: #888;
}
.from {
  font-size: 13px;
  color: #aaa;
  margin-bottom: 20px !important;
}
</style>
