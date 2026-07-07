<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { api } from '../utils/api'
import { useAuthStore } from '../stores/auth'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { Category, PagedResult } from '../types/api'

const auth = useAuthStore()
const canWrite = () => auth.hasPermission('product:write')

const loading = ref(false)
const items = ref<Category[]>([])
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)

const dialogVisible = ref(false)
const editing = ref<Category | null>(null)
const form = reactive({
  name: '',
  parentId: undefined as number | undefined,
  sortOrder: 0,
  isEnabled: true,
  version: 0,
})

async function load() {
  loading.value = true
  try {
    const res = await api.get<PagedResult<Category>>('/categories', { page: page.value, pageSize: pageSize.value })
    items.value = res.items ?? []
    total.value = res.total ?? 0
  } catch (e: any) {
    ElMessage.error(e?.message ?? '加载失败')
  } finally {
    loading.value = false
  }
}

function resetForm() {
  form.name = ''
  form.parentId = undefined
  form.sortOrder = 0
  form.isEnabled = true
  form.version = 0
}

function openCreate() {
  editing.value = null
  resetForm()
  dialogVisible.value = true
}

function openEdit(row: Category) {
  editing.value = row
  form.name = row.name
  form.parentId = row.parentId
  form.sortOrder = row.sortOrder
  form.isEnabled = row.isEnabled
  form.version = row.version
  dialogVisible.value = true
}

async function save() {
  if (!form.name.trim()) {
    ElMessage.warning('请填写分类名称')
    return
  }
  try {
    const body = {
      name: form.name.trim(),
      parentId: form.parentId ?? null,
      sortOrder: form.sortOrder,
      isEnabled: form.isEnabled,
    }
    if (editing.value) {
      await api.put(`/categories/${editing.value.id}`, body)
      ElMessage.success('更新成功')
    } else {
      await api.post('/categories', body)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await load()
  } catch (e: any) {
    ElMessage.error(e?.message ?? '保存失败')
  }
}

async function remove(row: Category) {
  try {
    await ElMessageBox.confirm(`确定删除分类「${row.name}」？`, '确认', { type: 'warning' })
    await api.delete(`/categories/${row.id}`)
    ElMessage.success('删除成功')
    await load()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.message ?? '删除失败')
  }
}

watch([page, pageSize], load, { immediate: true })
</script>

<template>
  <el-card>
    <template #header>
      <div class="toolbar">
        <span>分类管理</span>
        <el-button v-if="canWrite()" type="primary" size="small" @click="openCreate">新建分类</el-button>
      </div>
    </template>

    <el-table :data="items" v-loading="loading">
      <el-table-column prop="id" label="ID" width="100" />
      <el-table-column prop="name" label="名称" min-width="160" />
      <el-table-column prop="parentId" label="父分类 ID" width="120" />
      <el-table-column prop="sortOrder" label="排序" width="80" />
      <el-table-column label="启用" width="80">
        <template #default="{ row }">
          <el-tag :type="row.isEnabled ? 'success' : 'info'" size="small">{{ row.isEnabled ? '是' : '否' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column v-if="canWrite()" label="操作" width="140" fixed="right">
        <template #default="{ row }">
          <el-button size="small" link type="primary" @click="openEdit(row as Category)">编辑</el-button>
          <el-button size="small" link type="danger" @click="remove(row as Category)">删除</el-button>
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

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑分类' : '新建分类'" width="480px">
      <el-form label-position="top">
        <el-form-item label="名称" required>
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="父分类 ID">
          <el-input-number v-model="form.parentId" :min="1" controls-position="right" style="width: 100%" placeholder="留空为顶级" />
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
.pager {
  display: flex;
  justify-content: flex-end;
  margin-top: 12px;
}
</style>
