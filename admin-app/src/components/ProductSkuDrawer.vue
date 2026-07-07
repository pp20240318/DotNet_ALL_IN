<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { api, getApiErrorMessage, http } from '../utils/api'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { Product, ProductSku } from '../types/api'
import { formatMoney } from '../utils/format'

const props = defineProps<{
  product: Product | null
}>()

const visible = defineModel<boolean>({ default: false })

const loading = ref(false)
const items = ref<ProductSku[]>([])

const formVisible = ref(false)
const editing = ref<ProductSku | null>(null)
const form = reactive({
  skuCode: '',
  specJson: '',
  price: 0,
  stock: 0,
  isEnabled: true,
  version: 0,
})

async function load() {
  if (!props.product) return
  loading.value = true
  try {
    items.value = await api.get<ProductSku[]>(`/skus/by-product/${props.product.id}`)
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加载 SKU 失败'))
  } finally {
    loading.value = false
  }
}

function resetForm() {
  form.skuCode = ''
  form.specJson = ''
  form.price = props.product?.price ?? 0
  form.stock = 0
  form.isEnabled = true
  form.version = 0
}

function openCreate() {
  editing.value = null
  resetForm()
  formVisible.value = true
}

function openEdit(row: ProductSku) {
  editing.value = row
  form.skuCode = row.skuCode
  form.specJson = row.specJson ?? ''
  form.price = row.price
  form.stock = row.stock
  form.isEnabled = row.isEnabled
  form.version = row.version
  formVisible.value = true
}

async function save() {
  if (!props.product || !form.skuCode.trim()) {
    ElMessage.warning('请填写 SKU 编码')
    return
  }
  try {
    const body = {
      productId: props.product.id,
      skuCode: form.skuCode.trim(),
      specJson: form.specJson || null,
      price: form.price,
      stock: form.stock,
      isEnabled: form.isEnabled,
    }
    if (editing.value) {
      await api.put(`/skus/${editing.value.id}`, { ...body, version: form.version })
      ElMessage.success('更新成功')
    } else {
      await api.post('/skus', body)
      ElMessage.success('创建成功')
    }
    formVisible.value = false
    await load()
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '保存失败'))
  }
}

async function remove(row: ProductSku) {
  try {
    await ElMessageBox.confirm(`确定删除 SKU「${row.skuCode}」？`, '确认', { type: 'warning' })
    await api.delete(`/skus/${row.id}`)
    ElMessage.success('删除成功')
    await load()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(getApiErrorMessage(e, '删除失败'))
  }
}

async function stockIn(row: ProductSku) {
  try {
    const { value } = await ElMessageBox.prompt('入库数量', '入库', {
      inputPattern: /^[1-9]\d*$/,
      inputErrorMessage: '请输入正整数',
    })
    await http.post(`/skus/${row.id}/stock-in?quantity=${value}`)
    ElMessage.success('入库成功')
    await load()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(getApiErrorMessage(e, '入库失败'))
  }
}

async function stockOut(row: ProductSku) {
  try {
    const { value } = await ElMessageBox.prompt('出库数量', '出库', {
      inputPattern: /^[1-9]\d*$/,
      inputErrorMessage: '请输入正整数',
    })
    await http.post(`/skus/${row.id}/stock-out?quantity=${value}`)
    ElMessage.success('出库成功')
    await load()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(getApiErrorMessage(e, '出库失败'))
  }
}

watch(visible, (v) => {
  if (v) load()
})
</script>

<template>
  <el-drawer v-model="visible" :title="`SKU 管理 - ${product?.name ?? ''}`" size="640px">
    <div class="toolbar">
      <el-button type="primary" size="small" @click="openCreate">新建 SKU</el-button>
      <el-button size="small" :loading="loading" @click="load">刷新</el-button>
    </div>

    <el-table :data="items" v-loading="loading" size="small" style="margin-top: 12px">
      <el-table-column prop="skuCode" label="编码" width="120" />
      <el-table-column prop="specJson" label="规格" min-width="120" show-overflow-tooltip />
      <el-table-column label="价格" width="100">
        <template #default="{ row }">{{ formatMoney(row.price) }}</template>
      </el-table-column>
      <el-table-column prop="stock" label="库存" width="70" />
      <el-table-column label="启用" width="70">
        <template #default="{ row }">
          <el-tag :type="row.isEnabled ? 'success' : 'info'" size="small">{{ row.isEnabled ? '是' : '否' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="{ row }">
          <el-button size="small" link type="primary" @click="openEdit(row as ProductSku)">编辑</el-button>
          <el-button size="small" link type="success" @click="stockIn(row as ProductSku)">入库</el-button>
          <el-button size="small" link type="warning" @click="stockOut(row as ProductSku)">出库</el-button>
          <el-button size="small" link type="danger" @click="remove(row as ProductSku)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="formVisible" :title="editing ? '编辑 SKU' : '新建 SKU'" width="440px" append-to-body>
      <el-form label-position="top">
        <el-form-item label="SKU 编码" required>
          <el-input v-model="form.skuCode" />
        </el-form-item>
        <el-form-item label="规格 JSON">
          <el-input v-model="form.specJson" placeholder='如 {"color":"黑"}' />
        </el-form-item>
        <el-row :gutter="12">
          <el-col :span="12">
            <el-form-item label="价格">
              <el-input-number v-model="form.price" :min="0" :precision="2" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="库存">
              <el-input-number v-model="form.stock" :min="0" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="启用">
          <el-switch v-model="form.isEnabled" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="formVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>
  </el-drawer>
</template>

<style scoped>
.toolbar {
  display: flex;
  gap: 8px;
}
</style>
