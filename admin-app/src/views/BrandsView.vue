<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { api, getApiErrorMessage } from '../utils/api'
import { useAuthStore } from '../stores/auth'
import { useRouteQueryParam } from '../composables/useRouteQuery'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { Brand, PagedResult } from '../types/api'

const auth = useAuthStore()
const canWrite = () => auth.hasPermission('brand:write')

const loading = ref(false)
const items = ref<Brand[]>([])
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)
const keyword = ref('')

const dialogVisible = ref(false)
const editing = ref<Brand | null>(null)
const form = reactive({
  name: '',
  logoUrl: '',
  description: '',
  sortOrder: 0,
  isEnabled: true,
  version: 0,
})

async function load() {
  loading.value = true
  try {
    const searching = !!keyword.value.trim()
    const res = await api.get<PagedResult<Brand>>('/brands', {
      page: searching ? 1 : page.value,
      pageSize: searching ? 200 : pageSize.value,
    })
    let list = res.items ?? []
    if (searching) {
      const k = keyword.value.trim().toLowerCase()
      list = list.filter((b) => b.name.toLowerCase().includes(k))
      total.value = list.length
      const start = (page.value - 1) * pageSize.value
      items.value = list.slice(start, start + pageSize.value)
    } else {
      items.value = list
      total.value = res.total ?? 0
    }
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加载失败'))
  } finally {
    loading.value = false
  }
}

function resetForm() {
  form.name = ''
  form.logoUrl = ''
  form.description = ''
  form.sortOrder = 0
  form.isEnabled = true
  form.version = 0
}

function openCreate() {
  editing.value = null
  resetForm()
  dialogVisible.value = true
}

function openEdit(row: Brand) {
  editing.value = row
  form.name = row.name
  form.logoUrl = row.logoUrl ?? ''
  form.description = row.description ?? ''
  form.sortOrder = row.sortOrder
  form.isEnabled = row.isEnabled
  form.version = row.version
  dialogVisible.value = true
}

async function save() {
  if (!form.name.trim()) {
    ElMessage.warning('请填写品牌名称')
    return
  }
  try {
    const body = {
      name: form.name.trim(),
      logoUrl: form.logoUrl || null,
      description: form.description || null,
      sortOrder: form.sortOrder,
      isEnabled: form.isEnabled,
    }
    if (editing.value) {
      await api.put(`/brands/${editing.value.id}`, body)
      ElMessage.success('更新成功')
    } else {
      await api.post('/brands', body)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await load()
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '保存失败'))
  }
}

async function remove(row: Brand) {
  try {
    await ElMessageBox.confirm(`确定删除品牌「${row.name}」？`, '确认', { type: 'warning' })
    await api.delete(`/brands/${row.id}`)
    ElMessage.success('删除成功')
    await load()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(getApiErrorMessage(e, '删除失败'))
  }
}

function onSearch() {
  page.value = 1
  load()
}

const { hasQuery } = useRouteQueryParam('keyword', (v) => {
  keyword.value = v
  page.value = 1
  load()
})

watch([page, pageSize], load, { immediate: !hasQuery() })
</script>

<template>
  <el-card>
    <template #header>
      <div class="toolbar">
        <span>品牌管理</span>
        <el-button v-if="canWrite()" type="primary" size="small" @click="openCreate">新建品牌</el-button>
      </div>
    </template>

    <div class="filters">
      <el-input v-model="keyword" placeholder="搜索品牌名称" clearable style="width: 220px" @keyup.enter="onSearch" />
      <el-button type="primary" @click="onSearch">查询</el-button>
    </div>

    <el-table :data="items" v-loading="loading" style="margin-top: 12px">
      <el-table-column prop="id" label="ID" width="100" />
      <el-table-column prop="name" label="名称" min-width="140" />
      <el-table-column label="Logo" width="72">
        <template #default="{ row }">
          <el-image
            v-if="row.logoUrl"
            :src="row.logoUrl"
            fit="contain"
            style="width: 40px; height: 40px"
          />
          <span v-else class="muted">-</span>
        </template>
      </el-table-column>
      <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
      <el-table-column prop="sortOrder" label="排序" width="80" />
      <el-table-column label="启用" width="80">
        <template #default="{ row }">
          <el-tag :type="row.isEnabled ? 'success' : 'info'" size="small">{{ row.isEnabled ? '是' : '否' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column v-if="canWrite()" label="操作" width="140" fixed="right">
        <template #default="{ row }">
          <el-button size="small" link type="primary" @click="openEdit(row as Brand)">编辑</el-button>
          <el-button size="small" link type="danger" @click="remove(row as Brand)">删除</el-button>
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

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑品牌' : '新建品牌'" width="480px">
      <el-form label-position="top">
        <el-form-item label="名称" required>
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="Logo URL">
          <el-input v-model="form.logoUrl" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" :rows="3" />
        </el-form-item>
        <el-row :gutter="12">
          <el-col :span="12">
            <el-form-item label="排序">
              <el-input-number v-model="form.sortOrder" :min="0" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="启用">
              <el-switch v-model="form.isEnabled" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </el-card>
</template>

<style scoped>
.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.filters {
  display: flex;
  gap: 10px;
}
.pager {
  display: flex;
  justify-content: flex-end;
  margin-top: 12px;
}
.muted {
  color: #ccc;
}
</style>
