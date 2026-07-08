<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useNotificationStore } from '../stores/notifications'
import { api, getApiErrorMessage } from '../utils/api'
import { ElMessage } from 'element-plus'
import type { DashboardStats, OrderListItem, PagedResult } from '../types/api'
import { OrderStatus } from '../types/api'
import { formatDateTime, formatMoney, orderStatusLabel, orderStatusType } from '../utils/format'
import { Goods, List, User, Wallet, TrendCharts, Document } from '@element-plus/icons-vue'

const router = useRouter()
const auth = useAuthStore()
const notifications = useNotificationStore()

const loading = ref(false)
const stats = ref<DashboardStats | null>(null)
const recentOrders = ref<OrderListItem[]>([])
const orderSamples = ref<OrderListItem[]>([])

const statCards = computed(() => [
  { label: '商品总数', value: stats.value?.productCount, icon: Goods, color: '#409eff' },
  { label: '订单总数', value: stats.value?.orderCount, icon: List, color: '#67c23a' },
  { label: '客户总数', value: stats.value?.customerCount, icon: User, color: '#909399' },
  { label: '待付款', value: stats.value?.pendingPaymentCount, icon: Wallet, color: '#e6a23c', highlight: true },
  {
    label: '今日成交额',
    value: stats.value ? formatMoney(stats.value.todayOrderAmount) : undefined,
    icon: TrendCharts,
    color: '#626aef',
  },
])

const quickLinks = computed(() => {
  const links = [
    { label: '商品管理', path: '/products', permission: 'product:read' },
    { label: '订单管理', path: '/orders', permission: 'order:read' },
    { label: '客户管理', path: '/customers', permission: 'customer:read' },
    { label: '通知中心', path: '/notifications', permission: 'dashboard:read', badge: notifications.unreadCount },
    { label: '操作日志', path: '/operation-logs', permission: 'system:admin' },
  ]
  return links.filter((l) => auth.hasPermission(l.permission))
})

const statusDistribution = computed(() => {
  const counts: Record<number, number> = {}
  for (const o of orderSamples.value) {
    counts[o.status] = (counts[o.status] ?? 0) + 1
  }
  const total = orderSamples.value.length || 1
  return [
    OrderStatus.PendingPayment,
    OrderStatus.Paid,
    OrderStatus.Shipped,
    OrderStatus.Completed,
    OrderStatus.Cancelled,
    OrderStatus.Refunded,
  ].map((status) => ({
    status,
    label: orderStatusLabel(status),
    count: counts[status] ?? 0,
    percent: Math.round(((counts[status] ?? 0) / total) * 100),
    type: orderStatusType(status),
  }))
})

async function load() {
  loading.value = true
  try {
    const tasks: Promise<void>[] = [
      api.get<DashboardStats>('/dashboard/stats').then((s) => {
        stats.value = s
      }),
      notifications.refreshUnreadCount().then(() => {}),
    ]
    if (auth.hasPermission('order:read')) {
      tasks.push(
        api.get<PagedResult<OrderListItem>>('/orders', { page: 1, pageSize: 5 }).then((res) => {
          recentOrders.value = res.items ?? []
        }),
        api.get<PagedResult<OrderListItem>>('/orders', { page: 1, pageSize: 100 }).then((res) => {
          orderSamples.value = res.items ?? []
        }),
      )
    }
    await Promise.all(tasks)
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加载失败'))
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div v-loading="loading">
    <el-row :gutter="16">
      <el-col v-for="card in statCards" :key="card.label" :xs="24" :sm="12" :md="8" :lg="4">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-head">
            <el-icon :size="20" :style="{ color: card.color }"><component :is="card.icon" /></el-icon>
            <span class="stat-label">{{ card.label }}</span>
          </div>
          <div class="stat-value" :class="{ highlight: card.highlight }">
            {{ card.value ?? '-' }}
          </div>
        </el-card>
      </el-col>
      <el-col :xs="24" :sm="12" :md="8" :lg="4">
        <el-card shadow="hover" class="stat-card action">
          <el-button :loading="loading" @click="load">刷新数据</el-button>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16" style="margin-top: 8px">
      <el-col :xs="24" :md="12">
        <el-card>
          <template #header>快捷入口</template>
          <div class="quick-grid">
            <el-button v-for="link in quickLinks" :key="link.path" class="quick-btn" @click="router.push(link.path)">
              {{ link.label }}
              <el-badge v-if="link.badge" :value="link.badge" :max="99" class="quick-badge" />
            </el-button>
          </div>
        </el-card>
      </el-col>
      <el-col :xs="24" :md="12">
        <el-card>
          <template #header>运营概览</template>
          <el-descriptions :column="1" border size="small">
            <el-descriptions-item label="待处理订单">
              <el-tag v-if="stats" :type="stats.pendingPaymentCount > 0 ? 'warning' : 'success'" size="small">
                {{ stats.pendingPaymentCount }} 笔待付款
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="未读通知">
              <el-tag :type="notifications.unreadCount > 0 ? 'danger' : 'info'" size="small">
                {{ notifications.unreadCount }} 条未读
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="当前角色">
              <el-tag v-for="r in auth.roles" :key="r" size="small" style="margin-right: 4px">{{ r }}</el-tag>
            </el-descriptions-item>
          </el-descriptions>
          <div class="hint">
            <el-icon><Document /></el-icon>
            使用顶部搜索框可快速定位商品、品牌与订单
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row v-if="auth.hasPermission('order:read')" :gutter="16" style="margin-top: 8px">
      <el-col :xs="24" :md="14">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>最近订单</span>
              <el-button link type="primary" @click="router.push('/orders')">查看全部</el-button>
            </div>
          </template>
          <el-table v-if="recentOrders.length" :data="recentOrders" size="small">
            <el-table-column prop="orderNo" label="订单号" min-width="160" />
            <el-table-column label="状态" width="90">
              <template #default="{ row }">
                <el-tag :type="orderStatusType(row.status)" size="small">{{ orderStatusLabel(row.status) }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="金额" width="100">
              <template #default="{ row }">{{ formatMoney(row.totalAmount) }}</template>
            </el-table-column>
            <el-table-column label="时间" width="160">
              <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
            </el-table-column>
          </el-table>
          <el-empty v-else description="暂无订单" :image-size="64" />
        </el-card>
      </el-col>
      <el-col :xs="24" :md="10">
        <el-card>
          <template #header>订单状态分布（近 100 笔）</template>
          <div v-if="orderSamples.length" class="status-bars">
            <div v-for="item in statusDistribution" :key="item.status" class="status-row">
              <span class="status-label">{{ item.label }}</span>
              <el-progress :percentage="item.percent" :stroke-width="10" :color="undefined" />
              <span class="status-count">{{ item.count }}</span>
            </div>
          </div>
          <el-empty v-else description="暂无数据" :image-size="64" />
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped>
.stat-card {
  margin-bottom: 16px;
  min-height: 96px;
}
.stat-card.action {
  display: flex;
  align-items: center;
  justify-content: center;
}
.stat-head {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 8px;
}
.stat-label {
  color: #888;
  font-size: 13px;
}
.stat-value {
  font-size: 26px;
  font-weight: 700;
  color: #1a1a2e;
}
.stat-value.highlight {
  color: #e6a23c;
}
.quick-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}
.quick-btn {
  position: relative;
}
.quick-badge {
  position: absolute;
  top: -6px;
  right: -6px;
}
.hint {
  margin-top: 12px;
  display: flex;
  align-items: center;
  gap: 6px;
  color: #888;
  font-size: 13px;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.status-bars {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.status-row {
  display: grid;
  grid-template-columns: 64px 1fr 32px;
  align-items: center;
  gap: 10px;
}
.status-label {
  font-size: 13px;
  color: #666;
}
.status-count {
  font-size: 13px;
  color: #999;
  text-align: right;
}
</style>
