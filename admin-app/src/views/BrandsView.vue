<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { api } from '../utils/api'
import { ElMessage } from 'element-plus'

const loading = ref(false)
const items = ref<any[]>([])

async function load() {
  loading.value = true
  try {
    const res = await api.get<any>('/brands', { page: 1, pageSize: 50 })
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
    <template #header>品牌</template>
    <el-table :data="items" v-loading="loading">
      <el-table-column prop="id" label="ID" width="170" />
      <el-table-column prop="name" label="名称" />
      <el-table-column prop="description" label="描述" />
    </el-table>
  </el-card>
</template>

