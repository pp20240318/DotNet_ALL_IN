<script setup lang="ts">
import { ref, watch } from 'vue'
import { api, getApiErrorMessage } from '../utils/api'
import { ElMessage } from 'element-plus'
import type { OperationLog, PagedResult } from '../types/api'
import { formatDateTime } from '../utils/format'

const loading = ref(false)
const items = ref<OperationLog[]>([])
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const moduleFilter = ref('')
const dateRange = ref<[string, string] | null>(null)

async function load() {
  loading.value = true
  try {
    const params: Record<string, unknown> = { page: page.value, pageSize: pageSize.value }
    if (moduleFilter.value.trim()) params.module = moduleFilter.value.trim()
    if (dateRange.value?.[0]) params.from = dateRange.value[0]
    if (dateRange.value?.[1]) params.to = dateRange.value[1]
    const res = await api.get<PagedResult<OperationLog>>('/operation-logs', params)
    items.value = res.items ?? []
    total.value = res.total ?? 0
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加载失败'))
  } finally {
    loading.value = false
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
        <span>操作日志</span>
        <div class="filters">
          <el-input v-model="moduleFilter" placeholder="模块筛选" clearable style="width: 160px" @keyup.enter="onSearch" />
          <el-date-picker
            v-model="dateRange"
            type="daterange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            value-format="YYYY-MM-DD"
            style="width: 260px"
            @change="onSearch"
          />
          <el-button type="primary" size="small" @click="onSearch">查询</el-button>
        </div>
      </div>
    </template>

    <el-table :data="items" v-loading="loading" size="small">
      <el-table-column prop="adminName" label="操作人" width="120" />
      <el-table-column prop="module" label="模块" width="120" />
      <el-table-column prop="action" label="动作" width="120" />
      <el-table-column label="请求" min-width="200">
        <template #default="{ row }">
          <span v-if="row.requestMethod">{{ row.requestMethod }} </span>
          <span>{{ row.requestPath || '-' }}</span>
        </template>
      </el-table-column>
      <el-table-column label="结果" width="80">
        <template #default="{ row }">
          <el-tag :type="row.isSuccess ? 'success' : 'danger'" size="small">{{ row.isSuccess ? '成功' : '失败' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="时间" width="180">
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
  flex-wrap: wrap;
}
.pager {
  display: flex;
  justify-content: flex-end;
  margin-top: 12px;
}
</style>
