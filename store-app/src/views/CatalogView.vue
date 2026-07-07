<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { api, getApiErrorMessage } from '../utils/api'
import { ElMessage } from 'element-plus'
import type { CatalogProduct } from '../types/api'
import { formatMoney } from '../utils/format'

const router = useRouter()
const loading = ref(false)
const products = ref<CatalogProduct[]>([])

async function load() {
  loading.value = true
  try {
    products.value = await api.get<CatalogProduct[]>('/catalog/products')
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加载失败'))
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <el-row :gutter="16" v-loading="loading">
    <el-col v-for="p in products" :key="p.id" :xs="24" :sm="12" :md="8" :lg="6">
      <el-card shadow="hover" class="card" @click="router.push(`/products/${p.id}`)">
        <el-image v-if="p.coverImage" :src="p.coverImage" fit="cover" class="cover" />
        <div v-else class="cover placeholder">暂无图片</div>
        <div class="name">{{ p.name }}</div>
        <div class="meta">{{ p.brandName }} · {{ p.categoryName }}</div>
        <div class="price">{{ formatMoney(p.price) }}</div>
      </el-card>
    </el-col>
    <el-empty v-if="!loading && !products.length" description="暂无上架商品" />
  </el-row>
</template>

<style scoped>
.card {
  margin-bottom: 16px;
  cursor: pointer;
}
.cover {
  width: 100%;
  height: 160px;
  border-radius: 6px;
  margin-bottom: 8px;
}
.placeholder {
  display: grid;
  place-items: center;
  background: #f0f0f0;
  color: #999;
  font-size: 13px;
}
.name {
  font-weight: 600;
  margin-bottom: 4px;
}
.meta {
  font-size: 12px;
  color: #888;
  margin-bottom: 6px;
}
.price {
  color: #e6a23c;
  font-weight: 700;
}
</style>
