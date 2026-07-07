<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { api } from '../utils/api'
import { ElMessage } from 'element-plus'

import type { AdminUser, RoleInfo } from '../types/api'

const roles = ref<RoleInfo[]>([])
const admins = ref<AdminUser[]>([])
const loading = ref(false)

const assignDialog = ref(false)
const selectedAdmin = ref<any>(null)
const selectedRoles = ref<string[]>([])

async function load() {
  loading.value = true
  try {
    roles.value = await api.get<RoleInfo[]>('/roles')
    admins.value = await api.get<AdminUser[]>('/roles/admins')
  } catch (e: any) {
    ElMessage.error(e?.message ?? '加载失败')
  } finally {
    loading.value = false
  }
}

function openAssign(row: any) {
  selectedAdmin.value = row
  selectedRoles.value = [...(row.roles ?? [])]
  assignDialog.value = true
}

async function saveAssign() {
  try {
    await api.put(`/roles/admins/${selectedAdmin.value.id}/roles`, { roles: selectedRoles.value })
    ElMessage.success('保存成功')
    assignDialog.value = false
    await load()
  } catch (e: any) {
    ElMessage.error(e?.message ?? '保存失败')
  }
}

const createDialog = ref(false)
const createForm = reactive({
  username: '',
  password: '',
  displayName: '',
  roles: [] as string[],
})

async function createAdmin() {
  try {
    await api.post('/roles/admins', createForm)
    ElMessage.success('创建成功')
    createDialog.value = false
    createForm.username = ''
    createForm.password = ''
    createForm.displayName = ''
    createForm.roles = []
    await load()
  } catch (e: any) {
    ElMessage.error(e?.message ?? '创建失败')
  }
}

onMounted(load)
</script>

<template>
  <el-card>
    <template #header>
      <div style="display: flex; justify-content: space-between; align-items: center">
        <span>RBAC</span>
        <el-button size="small" type="primary" @click="createDialog = true">创建管理员</el-button>
      </div>
    </template>

    <el-row :gutter="12">
      <el-col :span="10">
        <el-card shadow="never">
          <template #header>角色</template>
          <el-table :data="roles" v-loading="loading" size="small">
            <el-table-column prop="code" label="Code" width="140" />
            <el-table-column prop="permissions" label="Permissions">
              <template #default="{ row }">
                <el-tag v-for="p in row.permissions" :key="p" style="margin-right: 6px" size="small">{{ p }}</el-tag>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
      <el-col :span="14">
        <el-card shadow="never">
          <template #header>管理员</template>
          <el-table :data="admins" v-loading="loading" size="small">
            <el-table-column prop="id" label="ID" width="170" />
            <el-table-column prop="username" label="用户名" width="120" />
            <el-table-column prop="displayName" label="显示名" width="120" />
            <el-table-column label="启用" width="70">
              <template #default="{ row }">
                <el-tag :type="row.isEnabled ? 'success' : 'info'" size="small">{{ row.isEnabled ? '是' : '否' }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="roles" label="角色">
              <template #default="{ row }">
                <el-tag v-for="r in row.roles" :key="r" size="small" style="margin-right: 4px">{{ r }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="110">
              <template #default="{ row }">
                <el-button size="small" @click="openAssign(row)">分配</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>
    </el-row>

    <el-dialog v-model="assignDialog" title="分配角色" width="420px">
      <div style="margin-bottom: 8px">管理员：{{ selectedAdmin?.username }}</div>
      <el-select v-model="selectedRoles" multiple style="width: 100%">
        <el-option v-for="r in roles" :key="r.code" :label="r.code" :value="r.code" />
      </el-select>
      <template #footer>
        <el-button @click="assignDialog = false">取消</el-button>
        <el-button type="primary" @click="saveAssign">保存</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="createDialog" title="创建管理员" width="480px">
      <el-form label-position="top">
        <el-form-item label="用户名">
          <el-input v-model="createForm.username" />
        </el-form-item>
        <el-form-item label="密码">
          <el-input v-model="createForm.password" type="password" show-password />
        </el-form-item>
        <el-form-item label="显示名">
          <el-input v-model="createForm.displayName" />
        </el-form-item>
        <el-form-item label="角色">
          <el-select v-model="createForm.roles" multiple style="width: 100%">
            <el-option v-for="r in roles" :key="r.code" :label="r.code" :value="r.code" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createDialog = false">取消</el-button>
        <el-button type="primary" @click="createAdmin">创建</el-button>
      </template>
    </el-dialog>
  </el-card>
</template>

