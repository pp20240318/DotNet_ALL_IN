<script setup lang="ts">
import { ref, watch } from 'vue'
import { api, getApiErrorMessage, http } from '../utils/api'
import { useAuthStore } from '../stores/auth'
import { useRouteQueryParam } from '../composables/useRouteQuery'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { OrderDetail, OrderListItem, PagedResult } from '../types/api'
import { OrderStatus } from '../types/api'
import { downloadBlob, formatDateTime, formatMoney, orderStatusLabel, orderStatusType } from '../utils/format'

const auth = useAuthStore()
const canWrite = () => auth.hasPermission('order:write')

const loading = ref(false)
const items = ref<OrderListItem[]>([])
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)
const orderNo = ref('')
const statusFilter = ref<OrderStatus | ''>('')

const detailVisible = ref(false)
const detailLoading = ref(false)
const detail = ref<OrderDetail | null>(null)

const statusOptions = [
  { value: OrderStatus.PendingPayment, label: orderStatusLabel(OrderStatus.PendingPayment) },
  { value: OrderStatus.Paid, label: orderStatusLabel(OrderStatus.Paid) },
  { value: OrderStatus.Shipped, label: orderStatusLabel(OrderStatus.Shipped) },
  { value: OrderStatus.Completed, label: orderStatusLabel(OrderStatus.Completed) },
  { value: OrderStatus.Cancelled, label: orderStatusLabel(OrderStatus.Cancelled) },
]

async function load() {
  loading.value = true
  try {
    const params: Record<string, unknown> = { page: page.value, pageSize: pageSize.value }
    if (orderNo.value.trim()) params.orderNo = orderNo.value.trim()
    if (statusFilter.value !== '') params.status = statusFilter.value
    const res = await api.get<PagedResult<OrderListItem>>('/orders', params)
    items.value = res.items ?? []
    total.value = res.total ?? 0
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加载失败'))
  } finally {
    loading.value = false
  }
}

async function exportCsv() {
  try {
    const resp = await http.get('/orders/export', { responseType: 'blob' })
    downloadBlob(new Blob([resp.data], { type: 'text/csv;charset=utf-8' }), `orders_${Date.now()}.csv`)
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '导出失败'))
  }
}

async function openDetail(row: OrderListItem) {
  detailVisible.value = true
  detailLoading.value = true
  detail.value = null
  try {
    detail.value = await api.get<OrderDetail>(`/orders/${row.id}`)
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加载详情失败'))
    detailVisible.value = false
  } finally {
    detailLoading.value = false
  }
}

async function ship(row: OrderListItem) {
  try {
    await ElMessageBox.confirm(`确认发货订单 ${row.orderNo}？`, '确认', { type: 'warning' })
    await api.post(`/orders/${row.id}/ship`)
    ElMessage.success('发货成功')
    await load()
    if (detail.value?.id === row.id) await openDetail(row)
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(getApiErrorMessage(e, '发货失败'))
  }
}

async function cancel(row: OrderListItem) {
  try {
    await ElMessageBox.confirm(`确认取消订单 ${row.orderNo}？`, '确认', { type: 'warning' })
    await api.post(`/orders/${row.id}/cancel`)
    ElMessage.success('取消成功')
    await load()
    if (detail.value?.id === row.id) await openDetail(row)
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(getApiErrorMessage(e, '取消失败'))
  }
}

async function createDemo() {
  try {
    await api.post('/orders/demo')
    ElMessage.success('演示订单已创建')
    await load()
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '创建失败'))
  }
}

function onSearch() {
  page.value = 1
  load()
}

const { hasQuery } = useRouteQueryParam('orderNo', (v) => {
  orderNo.value = v
  page.value = 1
  load()
})

watch([page, pageSize], load, { immediate: !hasQuery() })
</script>

<template>
  <el-card>
    <template #header>
      <div class="toolbar">
        <span>订单管理</span>
        <div class="actions">
          <el-button v-if="canWrite()" size="small" @click="createDemo">创建演示订单</el-button>
          <el-button size="small" @click="exportCsv">导出 CSV</el-button>
        </div>
      </div>
    </template>

    <div class="filters">
      <el-input v-model="orderNo" placeholder="订单号" clearable style="width: 200px" @keyup.enter="onSearch" />
      <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 140px" @change="onSearch">
        <el-option v-for="opt in statusOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-button type="primary" @click="onSearch">查询</el-button>
    </div>

    <el-table :data="items" v-loading="loading" style="width: 100%; margin-top: 12px">
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
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="{ row }">
          <el-button size="small" link type="primary" @click="openDetail(row as OrderListItem)">详情</el-button>
          <el-button
            v-if="canWrite() && row.status === OrderStatus.Paid"
            size="small"
            link
            type="success"
            @click="ship(row as OrderListItem)"
          >
            发货
          </el-button>
          <el-button
            v-if="canWrite() && (row.status === OrderStatus.PendingPayment || row.status === OrderStatus.Paid)"
            size="small"
            link
            type="danger"
            @click="cancel(row as OrderListItem)"
          >
            取消
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <div class="pager">
      <el-pagination
        v-model:current-page="page"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[10, 20, 50]"
        layout="total, prev, pager, next, sizes"
      />
    </div>

    <el-drawer v-model="detailVisible" title="订单详情" size="480px">
      <div v-loading="detailLoading">
        <template v-if="detail">
          <el-descriptions :column="1" border size="small">
            <el-descriptions-item label="订单号">{{ detail.orderNo }}</el-descriptions-item>
            <el-descriptions-item label="状态">
              <el-tag :type="orderStatusType(detail.status)" size="small">{{ orderStatusLabel(detail.status) }}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="金额">{{ formatMoney(detail.totalAmount) }}</el-descriptions-item>
            <el-descriptions-item label="收货人">{{ detail.receiverName || '-' }}</el-descriptions-item>
            <el-descriptions-item label="电话">{{ detail.receiverPhone || '-' }}</el-descriptions-item>
            <el-descriptions-item label="地址">{{ detail.receiverAddress || '-' }}</el-descriptions-item>
            <el-descriptions-item label="创建时间">{{ formatDateTime(detail.createdAt) }}</el-descriptions-item>
            <el-descriptions-item label="付款时间">{{ formatDateTime(detail.paidAt) }}</el-descriptions-item>
            <el-descriptions-item label="发货时间">{{ formatDateTime(detail.shippedAt) }}</el-descriptions-item>
          </el-descriptions>

          <div class="items-title">商品明细</div>
          <el-table :data="detail.items" size="small">
            <el-table-column prop="productName" label="商品" />
            <el-table-column prop="skuCode" label="SKU" width="100" />
            <el-table-column label="单价" width="90">
              <template #default="{ row }">{{ formatMoney(row.price) }}</template>
            </el-table-column>
            <el-table-column prop="quantity" label="数量" width="60" />
          </el-table>
        </template>
      </div>
    </el-drawer>
  </el-card>
</template>

<style scoped>
.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.actions {
  display: flex;
  gap: 8px;
}
.filters {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}
.pager {
  display: flex;
  justify-content: flex-end;
  margin-top: 12px;
}
.items-title {
  margin: 16px 0 8px;
  font-weight: 600;
}
</style>
