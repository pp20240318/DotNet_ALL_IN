<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { api, getApiErrorMessage, http } from '../utils/api'
import { useAuthStore } from '../stores/auth'
import { useRouteQueryParam } from '../composables/useRouteQuery'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { Brand, Category, PagedResult, Product } from '../types/api'
import { ProductStatus } from '../types/api'
import { downloadBlob, formatMoney, productStatusLabel, productStatusType } from '../utils/format'
import ProductSkuDrawer from '../components/ProductSkuDrawer.vue'
import type { UploadRequestOptions } from 'element-plus'

const auth = useAuthStore()
const canWrite = () => auth.hasPermission('product:write')

const loading = ref(false)
const items = ref<Product[]>([])
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)
const keyword = ref('')
const statusFilter = ref<ProductStatus | ''>('')

const brands = ref<Brand[]>([])
const categories = ref<Category[]>([])

const dialogVisible = ref(false)
const editing = ref<Product | null>(null)
const coverPreview = ref('')
const form = reactive({
  name: '',
  brandId: undefined as number | undefined,
  categoryId: undefined as number | undefined,
  description: '',
  price: 0,
  status: ProductStatus.Draft as ProductStatus,
  version: 0,
})

const skuDrawerVisible = ref(false)
const skuProduct = ref<Product | null>(null)

const statusOptions = [
  { value: ProductStatus.Draft, label: productStatusLabel(ProductStatus.Draft) },
  { value: ProductStatus.OnSale, label: productStatusLabel(ProductStatus.OnSale) },
  { value: ProductStatus.OffSale, label: productStatusLabel(ProductStatus.OffSale) },
]

async function load() {
  loading.value = true
  try {
    const params: Record<string, unknown> = { page: page.value, pageSize: pageSize.value }
    if (keyword.value.trim()) params.keyword = keyword.value.trim()
    if (statusFilter.value !== '') params.status = statusFilter.value
    const res = await api.get<PagedResult<Product>>('/products', params)
    items.value = res.items ?? []
    total.value = res.total ?? 0
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加载失败'))
  } finally {
    loading.value = false
  }
}

async function loadOptions() {
  const [brandRes, categoryRes] = await Promise.all([
    api.get<PagedResult<Brand>>('/brands', { page: 1, pageSize: 200 }),
    api.get<PagedResult<Category>>('/categories', { page: 1, pageSize: 200 }),
  ])
  brands.value = brandRes.items ?? []
  categories.value = categoryRes.items ?? []
}

async function exportCsv() {
  try {
    const resp = await http.get('/products/export', { responseType: 'blob' })
    downloadBlob(new Blob([resp.data], { type: 'text/csv;charset=utf-8' }), `products_${Date.now()}.csv`)
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '导出失败'))
  }
}

function resetForm() {
  form.name = ''
  form.brandId = undefined
  form.categoryId = undefined
  form.description = ''
  form.price = 0
  form.status = ProductStatus.Draft
  form.version = 0
  coverPreview.value = ''
}

async function openCreate() {
  await loadOptions()
  editing.value = null
  resetForm()
  dialogVisible.value = true
}

async function openEdit(row: Product) {
  await loadOptions()
  editing.value = row
  form.name = row.name
  form.brandId = row.brandId
  form.categoryId = row.categoryId
  form.description = row.description ?? ''
  form.price = row.price
  form.status = row.status
  form.version = row.version
  coverPreview.value = row.coverImage ?? ''
  dialogVisible.value = true
}

function openSku(row: Product) {
  skuProduct.value = row
  skuDrawerVisible.value = true
}

async function save() {
  if (!form.name.trim() || !form.brandId || !form.categoryId) {
    ElMessage.warning('请填写名称、品牌和分类')
    return
  }
  try {
    const body = {
      name: form.name.trim(),
      brandId: form.brandId,
      categoryId: form.categoryId,
      description: form.description || null,
      price: form.price,
      status: form.status,
    }
    if (editing.value) {
      await api.put(`/products/${editing.value.id}`, { ...body, version: form.version })
      ElMessage.success('更新成功')
    } else {
      await api.post('/products', body)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await load()
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '保存失败'))
  }
}

async function uploadCover(options: UploadRequestOptions) {
  if (!editing.value) {
    ElMessage.warning('请先保存商品后再上传封面')
    options.onError?.(new Error('no product') as any)
    return
  }
  try {
    const res = await api.upload<{ url: string }>(`/products/${editing.value.id}/cover`, options.file as File)
    coverPreview.value = res.url
    editing.value.coverImage = res.url
    ElMessage.success('封面上传成功')
    options.onSuccess?.(res as any)
    await load()
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '上传失败'))
    options.onError?.(e as any)
  }
}

async function changeStatus(row: Product, status: ProductStatus) {
  try {
    const statusName = Object.entries(ProductStatus).find(([, v]) => v === status)?.[0]
    await http.put(`/products/${row.id}/status?status=${statusName}`)
    ElMessage.success('状态已更新')
    await load()
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '更新失败'))
  }
}

async function remove(row: Product) {
  try {
    await ElMessageBox.confirm(`确定删除商品「${row.name}」？`, '确认', { type: 'warning' })
    await api.delete(`/products/${row.id}`)
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
        <span>商品管理</span>
        <div class="actions">
          <el-button v-if="canWrite()" type="primary" size="small" @click="openCreate">新建商品</el-button>
          <el-button size="small" @click="exportCsv">导出 CSV</el-button>
        </div>
      </div>
    </template>

    <div class="filters">
      <el-input v-model="keyword" placeholder="搜索名称" clearable style="width: 220px" @keyup.enter="onSearch" />
      <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 140px" @change="onSearch">
        <el-option v-for="opt in statusOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-button type="primary" @click="onSearch">查询</el-button>
    </div>

    <el-table :data="items" v-loading="loading" style="width: 100%; margin-top: 12px">
      <el-table-column label="封面" width="72">
        <template #default="{ row }">
          <el-image
            v-if="row.coverImage"
            :src="row.coverImage"
            fit="cover"
            style="width: 48px; height: 48px; border-radius: 6px"
            :preview-src-list="[row.coverImage]"
            preview-teleported
          />
          <span v-else class="no-cover">-</span>
        </template>
      </el-table-column>
      <el-table-column prop="id" label="ID" width="90" />
      <el-table-column prop="name" label="名称" min-width="160" />
      <el-table-column prop="brandName" label="品牌" width="110" />
      <el-table-column prop="categoryName" label="分类" width="110" />
      <el-table-column label="价格" width="110">
        <template #default="{ row }">{{ formatMoney(row.price) }}</template>
      </el-table-column>
      <el-table-column label="状态" width="90">
        <template #default="{ row }">
          <el-tag :type="productStatusType(row.status)" size="small">{{ productStatusLabel(row.status) }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" :width="canWrite() ? 260 : 80" fixed="right">
        <template #default="{ row }">
          <el-button size="small" link type="primary" @click="openSku(row as Product)">SKU</el-button>
          <template v-if="canWrite()">
            <el-button size="small" link type="primary" @click="openEdit(row as Product)">编辑</el-button>
            <el-button
              v-if="row.status !== ProductStatus.OnSale"
              size="small"
              link
              type="success"
              @click="changeStatus(row as Product, ProductStatus.OnSale)"
            >
              上架
            </el-button>
            <el-button v-else size="small" link type="warning" @click="changeStatus(row as Product, ProductStatus.OffSale)">
              下架
            </el-button>
            <el-button size="small" link type="danger" @click="remove(row as Product)">删除</el-button>
          </template>
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

    <el-dialog v-model="dialogVisible" :title="editing ? '编辑商品' : '新建商品'" width="540px">
      <el-form label-position="top">
        <el-form-item v-if="editing" label="封面">
          <div class="cover-row">
            <el-image
              v-if="coverPreview"
              :src="coverPreview"
              fit="cover"
              style="width: 80px; height: 80px; border-radius: 8px"
            />
            <el-upload :show-file-list="false" accept="image/*" :http-request="uploadCover">
              <el-button size="small">{{ coverPreview ? '更换封面' : '上传封面' }}</el-button>
            </el-upload>
          </div>
        </el-form-item>
        <el-form-item label="名称" required>
          <el-input v-model="form.name" />
        </el-form-item>
        <el-row :gutter="12">
          <el-col :span="12">
            <el-form-item label="品牌" required>
              <el-select v-model="form.brandId" filterable style="width: 100%">
                <el-option v-for="b in brands" :key="b.id" :label="b.name" :value="b.id" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="分类" required>
              <el-select v-model="form.categoryId" filterable style="width: 100%">
                <el-option v-for="c in categories" :key="c.id" :label="c.name" :value="c.id" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="12">
          <el-col :span="12">
            <el-form-item label="价格">
              <el-input-number v-model="form.price" :min="0" :precision="2" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="状态">
              <el-select v-model="form.status" style="width: 100%">
                <el-option v-for="opt in statusOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" :rows="3" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="save">保存</el-button>
      </template>
    </el-dialog>

    <ProductSkuDrawer v-model="skuDrawerVisible" :product="skuProduct" />
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
.no-cover {
  color: #ccc;
}
.cover-row {
  display: flex;
  align-items: center;
  gap: 12px;
}
</style>
