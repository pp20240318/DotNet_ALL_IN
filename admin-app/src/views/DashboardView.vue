<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { api } from '../utils/api'
import { ElMessage } from 'element-plus'

const loading = ref(false)
const stats = ref<any>(null)

async function load() {
  loading.value = true
  try {
    stats.value = await api.get('/dashboard/stats')
  } catch (e: any) {
    ElMessage.error(e?.message ?? '加载失败')
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <el-card>
    <template #header>
      <div style="display: flex; justify-content: space-between; align-items: center">
        <span>仪表盘</span>
        <el-button size="small" :loading="loading" @click="load">刷新</el-button>
      </div>
    </template>
    <pre v-if="stats" style="margin: 0">{{ stats }}</pre>
    <el-empty v-else description="暂无数据" />
  </el-card>
</template>

