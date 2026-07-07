<script setup lang="ts">
import { ref, watch } from 'vue'
import { api, http } from '../utils/api'
import { ElMessage } from 'element-plus'

const loading = ref(false)
const items = ref<any[]>([])
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)

async function load() {
  loading.value = true
  try {
    const res = await api.get<any>('/orders', { page: page.value, pageSize: pageSize.value })
    items.value = res.items ?? []
    total.value = res.total ?? 0
  } catch (e: any) {
    ElMessage.error(e?.message ?? '加载失败')
  } finally {
    loading.value = false
  }
}

async function exportCsv() {
  try {
    const resp = await http.get('/orders/export', { responseType: 'blob' })
    const blob = new Blob([resp.data], { type: 'text/csv;charset=utf-8' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `orders_${Date.now()}.csv`
    a.click()
    URL.revokeObjectURL(url)
  } catch (e: any) {
    ElMessage.error(e?.message ?? '导出失败')
  }
}

watch([page, pageSize], load, { immediate: true })
</script>

<template>
  <el-card>
    <template #header>
      <div style="display: flex; justify-content: space-between; align-items: center">
        <span>订单</span>
        <el-button size="small" @click="exportCsv">导出 CSV</el-button>
      </div>
    </template>
    <el-table :data="items" v-loading="loading" style="width: 100%">
      <el-table-column prop="id" label="ID" width="170" />
      <el-table-column prop="orderNo" label="订单号" />
      <el-table-column prop="status" label="状态" width="120" />
      <el-table-column prop="totalAmount" label="金额" width="120" />
      <el-table-column prop="createdAt" label="创建时间" />
    </el-table>
    <div style="display: flex; justify-content: flex-end; margin-top: 12px">
      <el-pagination
        v-model:current-page="page"
        v-model:page-size="pageSize"
        :total="total"
        layout="total, prev, pager, next, sizes"
      />
    </div>
  </el-card>
</template>

