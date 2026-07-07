<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { api, getApiErrorMessage } from '../utils/api'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { OrderSummary, PagedResult } from '../types/api'
import { formatDateTime, formatMoney, orderStatusLabel, orderStatusType } from '../utils/format'
import { OrderStatus } from '../types/api'

const loading = ref(false)
const items = ref<OrderSummary[]>([])

async function load() {
  loading.value = true
  try {
    const res = await api.get<PagedResult<OrderSummary>>('/store/orders', { page: 1, pageSize: 50 })
    items.value = res.items ?? []
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加载订单失败'))
  } finally {
    loading.value = false
  }
}

async function pay(row: OrderSummary) {
  try {
    await ElMessageBox.confirm(`模拟支付订单 ${row.orderNo}？`, '确认', { type: 'info' })
    await api.post(`/store/orders/${row.id}/pay`)
    ElMessage.success('支付成功')
    await load()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(getApiErrorMessage(e, '支付失败'))
  }
}

async function cancel(row: OrderSummary) {
  try {
    await ElMessageBox.confirm(`取消订单 ${row.orderNo}？`, '确认', { type: 'warning' })
    await api.post(`/store/orders/${row.id}/cancel`)
    ElMessage.success('已取消')
    await load()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(getApiErrorMessage(e, '取消失败'))
  }
}

onMounted(load)
</script>

<template>
  <el-card v-loading="loading">
    <el-table :data="items" style="width: 100%">
      <el-table-column prop="orderNo" label="订单号" min-width="180" />
      <el-table-column label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="orderStatusType(row.status)" size="small">{{ orderStatusLabel(row.status) }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="金额" width="120">
        <template #default="{ row }">{{ formatMoney(row.totalAmount) }}</template>
      </el-table-column>
      <el-table-column label="创建时间" width="180">
        <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="160" fixed="right">
        <template #default="{ row }">
          <el-button
            v-if="row.status === OrderStatus.PendingPayment"
            size="small"
            link
            type="primary"
            @click="pay(row as OrderSummary)"
          >
            支付
          </el-button>
          <el-button
            v-if="row.status === OrderStatus.PendingPayment"
            size="small"
            link
            type="danger"
            @click="cancel(row as OrderSummary)"
          >
            取消
          </el-button>
        </template>
      </el-table-column>
    </el-table>
    <el-empty v-if="!loading && !items.length" description="暂无订单" />
  </el-card>
</template>
