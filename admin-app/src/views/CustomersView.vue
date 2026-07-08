<script setup lang="ts">
import { ref, watch } from 'vue'
import { api, getApiErrorMessage } from '../utils/api'
import { useAuthStore } from '../stores/auth'
import { ElMessage } from 'element-plus'
import type { Customer, PagedResult } from '../types/api'
import { formatDateTime } from '../utils/format'

const auth = useAuthStore()
const canWrite = () => auth.hasPermission('customer:write')

const loading = ref(false)
const items = ref<Customer[]>([])
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const keyword = ref('')

async function load() {
  loading.value = true
  try {
    const params: Record<string, unknown> = { page: page.value, pageSize: pageSize.value }
    if (keyword.value.trim()) params.keyword = keyword.value.trim()
    const res = await api.get<PagedResult<Customer>>('/customers', params)
    items.value = res.items ?? []
    total.value = res.total ?? 0
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加载失败'))
  } finally {
    loading.value = false
  }
}

async function toggleEnable(row: Customer, enabled: boolean) {
  if (!canWrite()) return
  try {
    await api.put(`/customers/${row.id}`, { isEnabled: enabled })
    row.isEnabled = enabled
    ElMessage.success('已更新')
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '更新失败'))
    await load()
  }
}

function onSearch() {
  page.value = 1
  load()
}

watch([page, pageSize], load, { immediate: true })
</script>

<template>
  <el-card>
    <template #header>
      <div class="toolbar">
        <span>客户管理</span>
        <div class="filters">
          <el-input
            v-model="keyword"
            placeholder="用户名 / 昵称 / 手机 / 邮箱"
            clearable
            style="width: 260px"
            @keyup.enter="onSearch"
          />
          <el-button type="primary" size="small" @click="onSearch">查询</el-button>
        </div>
      </div>
    </template>

    <el-table :data="items" v-loading="loading" size="small">
      <el-table-column prop="id" label="ID" width="170" />
      <el-table-column prop="username" label="用户名" width="120" />
      <el-table-column prop="nickname" label="昵称" width="120" />
      <el-table-column prop="phone" label="手机" width="130" />
      <el-table-column prop="email" label="邮箱" min-width="160" show-overflow-tooltip />
      <el-table-column label="启用" width="100">
        <template #default="{ row }">
          <el-switch
            v-if="canWrite()"
            :model-value="row.isEnabled"
            @change="(v) => toggleEnable(row as Customer, v === true)"
          />
          <el-tag v-else :type="row.isEnabled ? 'success' : 'info'" size="small">{{ row.isEnabled ? '是' : '否' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="注册时间" width="180">
        <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
      </el-table-column>
    </el-table>

    <div class="pager">
      <el-pagination
        v-model:current-page="page"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[20, 50, 100]"
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
  flex-wrap: wrap;
  gap: 12px;
}
.filters {
  display: flex;
  gap: 8px;
}
.pager {
  display: flex;
  justify-content: flex-end;
  margin-top: 12px;
}
</style>
