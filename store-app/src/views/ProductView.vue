<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { api, getApiErrorMessage } from '../utils/api'
import { useAuthStore } from '../stores/auth'
import { useCartStore } from '../stores/cart'
import { ElMessage } from 'element-plus'
import type { CatalogProductDetail } from '../types/api'
import { formatMoney } from '../utils/format'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const cart = useCartStore()

const loading = ref(false)
const product = ref<CatalogProductDetail | null>(null)
const selectedSkuId = ref<number | undefined>(undefined)
const quantity = ref(1)

const selectedSku = computed(() => product.value?.skus?.find((s) => s.id === selectedSkuId.value) ?? null)

async function load() {
  const id = Number(route.params.id)
  if (!id) return
  loading.value = true
  try {
    product.value = await api.get<CatalogProductDetail>(`/catalog/products/${id}`)
    selectedSkuId.value = product.value.skus?.[0]?.id
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加载失败'))
  } finally {
    loading.value = false
  }
}

async function addToCart() {
  if (!auth.isLoggedIn) {
    await router.push({ path: '/login', query: { redirect: route.fullPath } })
    return
  }
  if (!selectedSku.value) {
    ElMessage.warning('请选择 SKU')
    return
  }
  try {
    await api.post('/store/cart', { skuId: selectedSku.value.id, quantity: quantity.value })
    await cart.refresh()
    ElMessage.success('已加入购物车')
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加入购物车失败'))
  }
}

onMounted(load)
</script>

<template>
  <el-card v-loading="loading">
    <template v-if="product">
      <h3>{{ product.name }}</h3>
      <p class="desc">{{ product.description || '暂无描述' }}</p>
      <p class="price">起价 {{ formatMoney(product.price) }}</p>

      <div class="sku-block">
        <div class="label">选择规格</div>
        <el-radio-group v-model="selectedSkuId">
          <el-radio v-for="sku in product.skus" :key="sku.id" :value="sku.id">
            {{ sku.skuCode }} ({{ formatMoney(sku.price) }}, 库存 {{ sku.stock }})
          </el-radio>
        </el-radio-group>
      </div>

      <div class="qty">
        <span>数量</span>
        <el-input-number v-model="quantity" :min="1" :max="selectedSku?.stock || 99" />
      </div>

      <el-button type="primary" @click="addToCart">加入购物车</el-button>
    </template>
  </el-card>
</template>

<style scoped>
.desc {
  color: #666;
}
.price {
  color: #e6a23c;
  font-weight: 700;
}
.sku-block {
  margin: 16px 0;
}
.label {
  margin-bottom: 8px;
  font-weight: 600;
}
.qty {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}
</style>
