<script setup lang="ts">
import { ref, watch } from 'vue'
import { api } from '../utils/api'
import { useNotificationStore } from '../stores/notifications'
import { ElMessage } from 'element-plus'
import type { Notification, PagedResult } from '../types/api'
import { formatDateTime } from '../utils/format'

const notifications = useNotificationStore()

const loading = ref(false)
const items = ref<Notification[]>([])
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const unreadOnly = ref(false)

async function load() {
  loading.value = true
  try {
    const params: Record<string, unknown> = { page: page.value, pageSize: pageSize.value }
    if (unreadOnly.value) params.unreadOnly = true
    const res = await api.get<PagedResult<Notification>>('/notifications', params)
    items.value = res.items ?? []
    total.value = res.total ?? 0
    await notifications.refreshUnreadCount()
  } catch (e: any) {
    ElMessage.error(e?.message ?? '加载失败')
  } finally {
    loading.value = false
  }
}

async function markRead(row: Notification) {
  if (row.isRead) return
  try {
    await api.post(`/notifications/${row.id}/read`)
    row.isRead = true
    notifications.markReadLocally()
    ElMessage.success('已标记已读')
  } catch (e: any) {
    ElMessage.error(e?.message ?? '操作失败')
  }
}

async function markAllRead() {
  try {
    await api.post('/notifications/read-all')
    notifications.clearUnread()
    ElMessage.success('全部已读')
    await load()
  } catch (e: any) {
    ElMessage.error(e?.message ?? '操作失败')
  }
}

function onFilterChange() {
  page.value = 1
  load()
}

watch([page, pageSize], load, { immediate: true })
</script>

<template>
  <el-card>
    <template #header>
      <div class="toolbar">
        <span>通知中心</span>
        <div class="actions">
          <el-switch v-model="unreadOnly" active-text="仅未读" @change="onFilterChange" />
          <el-button size="small" @click="markAllRead">全部已读</el-button>
          <el-button size="small" :loading="loading" @click="load">刷新</el-button>
        </div>
      </div>
    </template>

    <el-table :data="items" v-loading="loading" style="width: 100%">
      <el-table-column prop="title" label="标题" width="180" />
      <el-table-column prop="content" label="内容" min-width="240" show-overflow-tooltip />
      <el-table-column prop="category" label="分类" width="100" />
      <el-table-column label="已读" width="80">
        <template #default="{ row }">
          <el-tag :type="row.isRead ? 'info' : 'warning'" size="small">{{ row.isRead ? '是' : '否' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="时间" width="180">
        <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="100" fixed="right">
        <template #default="{ row }">
          <el-button v-if="!row.isRead" size="small" link type="primary" @click="markRead(row as Notification)">标记已读</el-button>
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
  gap: 12px;
  align-items: center;
}
.pager {
  display: flex;
  justify-content: flex-end;
  margin-top: 12px;
}
</style>
