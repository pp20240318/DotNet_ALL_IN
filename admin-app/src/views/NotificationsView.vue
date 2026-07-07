<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { api } from '../utils/api'
import { ElMessage } from 'element-plus'

const loading = ref(false)
const items = ref<any[]>([])

async function load() {
  loading.value = true
  try {
    const res = await api.get<any>('/notifications', { page: 1, pageSize: 50 })
    items.value = res.items ?? []
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
    <template #header>通知</template>
    <el-table :data="items" v-loading="loading" style="width: 100%">
      <el-table-column prop="id" label="ID" width="170" />
      <el-table-column prop="title" label="标题" width="160" />
      <el-table-column prop="content" label="内容" />
      <el-table-column prop="category" label="分类" width="120" />
      <el-table-column prop="isRead" label="已读" width="80" />
      <el-table-column prop="createdAt" label="时间" width="220" />
    </el-table>
  </el-card>
</template>

