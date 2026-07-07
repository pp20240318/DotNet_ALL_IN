<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { api, getApiErrorMessage } from '../utils/api'
import { useCartStore } from '../stores/cart'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { CartItem } from '../types/api'
import { formatMoney } from '../utils/format'

const router = useRouter()
const cart = useCartStore()

const loading = ref(false)
const items = ref<CartItem[]>([])

const checkoutVisible = ref(false)
const checkoutForm = reactive({
  receiverName: '',
  receiverPhone: '',
  receiverAddress: '',
})

async function load() {
  loading.value = true
  try {
    await cart.refresh()
    items.value = cart.items
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '加载购物车失败'))
  } finally {
    loading.value = false
  }
}

async function updateQty(row: CartItem, quantity: number) {
  if (quantity < 1) return
  try {
    await api.put(`/store/cart/${row.skuId}`, { quantity })
    await load()
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '更新失败'))
  }
}

async function remove(row: CartItem) {
  try {
    await ElMessageBox.confirm(`移除「${row.productName}」？`, '确认', { type: 'warning' })
    await api.delete(`/store/cart/${row.skuId}`)
    await load()
    ElMessage.success('已移除')
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(getApiErrorMessage(e, '移除失败'))
  }
}

function openCheckout() {
  if (!items.value.length) {
    ElMessage.warning('购物车为空')
    return
  }
  checkoutVisible.value = true
}

async function checkout() {
  if (!checkoutForm.receiverName.trim() || !checkoutForm.receiverPhone.trim() || !checkoutForm.receiverAddress.trim()) {
    ElMessage.warning('请填写收货信息')
    return
  }
  try {
    const res = await api.post<{ id: number }>('/store/cart/checkout', {
      receiverName: checkoutForm.receiverName.trim(),
      receiverPhone: checkoutForm.receiverPhone.trim(),
      receiverAddress: checkoutForm.receiverAddress.trim(),
    })
    checkoutVisible.value = false
    ElMessage.success('下单成功')
    await load()
    await router.push('/orders')
    if (res?.id) {
      // optional: could open order detail later
    }
  } catch (e) {
    ElMessage.error(getApiErrorMessage(e, '结算失败'))
  }
}

onMounted(load)
</script>

<template>
  <el-card v-loading="loading">
    <el-table :data="items" style="width: 100%">
      <el-table-column prop="productName" label="商品" min-width="160" />
      <el-table-column prop="skuCode" label="SKU" width="120" />
      <el-table-column label="单价" width="100">
        <template #default="{ row }">{{ formatMoney(row.price) }}</template>
      </el-table-column>
      <el-table-column label="数量" width="140">
        <template #default="{ row }">
          <el-input-number
            :model-value="row.quantity"
            :min="1"
            size="small"
            @change="(v: number | undefined) => updateQty(row as CartItem, v ?? 1)"
          />
        </template>
      </el-table-column>
      <el-table-column label="小计" width="100">
        <template #default="{ row }">{{ formatMoney(row.price * row.quantity) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="90" fixed="right">
        <template #default="{ row }">
          <el-button link type="danger" @click="remove(row as CartItem)">移除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-empty v-if="!loading && !items.length" description="购物车为空">
      <el-button type="primary" @click="router.push('/')">去逛逛</el-button>
    </el-empty>

    <div v-if="items.length" class="footer">
      <div class="total">合计：{{ formatMoney(cart.total) }}</div>
      <el-button type="primary" @click="openCheckout">结算</el-button>
    </div>

    <el-dialog v-model="checkoutVisible" title="填写收货信息" width="440px">
      <el-form label-position="top">
        <el-form-item label="收货人">
          <el-input v-model="checkoutForm.receiverName" />
        </el-form-item>
        <el-form-item label="电话">
          <el-input v-model="checkoutForm.receiverPhone" />
        </el-form-item>
        <el-form-item label="地址">
          <el-input v-model="checkoutForm.receiverAddress" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="checkoutVisible = false">取消</el-button>
        <el-button type="primary" @click="checkout">确认下单</el-button>
      </template>
    </el-dialog>
  </el-card>
</template>

<style scoped>
.footer {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 16px;
  margin-top: 16px;
}
.total {
  font-size: 18px;
  font-weight: 700;
  color: #e6a23c;
}
</style>
