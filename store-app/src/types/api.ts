export const OrderStatus = {
  PendingPayment: 0,
  Paid: 1,
  Shipped: 2,
  Completed: 3,
  Cancelled: 4,
} as const
export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus]

export type CatalogProduct = {
  id: number
  name: string
  brandName?: string
  categoryName?: string
  price: number
  coverImage?: string
}

export type CatalogSku = {
  id: number
  skuCode: string
  specJson?: string
  price: number
  stock: number
}

export type CatalogProductDetail = CatalogProduct & {
  description?: string
  skus: CatalogSku[]
}

export type CartItem = {
  skuId: number
  skuCode: string
  productName: string
  price: number
  quantity: number
}

export type CustomerProfile = {
  id: number
  username: string
  nickname?: string
  phone?: string
}

export type LoginResponse = {
  token: string
  expiresAt: string
  profile: CustomerProfile
}

export type OrderSummary = {
  id: number
  orderNo: string
  status: OrderStatus
  totalAmount: number
  createdAt: string
}

export type PagedResult<T> = {
  page: number
  pageSize: number
  total: number
  items: T[]
}
