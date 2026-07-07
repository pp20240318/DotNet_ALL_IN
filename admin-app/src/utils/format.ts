import { OrderStatus, ProductStatus } from '../types/api'

type TagType = 'primary' | 'success' | 'warning' | 'info' | 'danger' | undefined

const productStatusLabels: Record<ProductStatus, string> = {
  [ProductStatus.Draft]: '草稿',
  [ProductStatus.OnSale]: '在售',
  [ProductStatus.OffSale]: '下架',
}

const orderStatusLabels: Record<OrderStatus, string> = {
  [OrderStatus.PendingPayment]: '待付款',
  [OrderStatus.Paid]: '已付款',
  [OrderStatus.Shipped]: '已发货',
  [OrderStatus.Completed]: '已完成',
  [OrderStatus.Cancelled]: '已取消',
}

const productStatusTypes: Record<ProductStatus, TagType> = {
  [ProductStatus.Draft]: 'info',
  [ProductStatus.OnSale]: 'success',
  [ProductStatus.OffSale]: 'warning',
}

const orderStatusTypes: Record<OrderStatus, TagType> = {
  [OrderStatus.PendingPayment]: 'warning',
  [OrderStatus.Paid]: 'info',
  [OrderStatus.Shipped]: 'primary',
  [OrderStatus.Completed]: 'success',
  [OrderStatus.Cancelled]: 'danger',
}

export function productStatusLabel(status: ProductStatus) {
  return productStatusLabels[status] ?? String(status)
}

export function orderStatusLabel(status: OrderStatus) {
  return orderStatusLabels[status] ?? String(status)
}

export function productStatusType(status: ProductStatus) {
  return productStatusTypes[status] ?? 'info'
}

export function orderStatusType(status: OrderStatus) {
  return orderStatusTypes[status] ?? 'info'
}

export function formatMoney(value: number) {
  return `¥${value.toFixed(2)}`
}

export function formatDateTime(value?: string | null) {
  if (!value) return '-'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return value
  return d.toLocaleString('zh-CN', { hour12: false })
}

export function downloadBlob(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}
