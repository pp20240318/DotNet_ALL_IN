export type PagedResult<T> = {
  page: number
  pageSize: number
  total: number
  items: T[]
}

export const ProductStatus = {
  Draft: 0,
  OnSale: 1,
  OffSale: 2,
} as const
export type ProductStatus = (typeof ProductStatus)[keyof typeof ProductStatus]

export const OrderStatus = {
  PendingPayment: 0,
  Paid: 1,
  Shipped: 2,
  Completed: 3,
  Cancelled: 4,
} as const
export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus]

export type DashboardStats = {
  productCount: number
  orderCount: number
  customerCount: number
  pendingPaymentCount: number
  todayOrderAmount: number
}

export type Product = {
  id: number
  name: string
  brandId: number
  brandName?: string
  categoryId: number
  categoryName?: string
  description?: string
  price: number
  status: ProductStatus
  coverImage?: string
  version: number
}

export type Brand = {
  id: number
  name: string
  logoUrl?: string
  description?: string
  sortOrder: number
  isEnabled: boolean
  version: number
}

export type Category = {
  id: number
  name: string
  parentId?: number
  sortOrder: number
  isEnabled: boolean
  version: number
}

export type OrderListItem = {
  id: number
  orderNo: string
  status: OrderStatus
  totalAmount: number
  createdAt: string
}

export type OrderDetail = OrderListItem & {
  receiverName?: string
  receiverPhone?: string
  receiverAddress?: string
  paidAt?: string
  shippedAt?: string
  items: OrderItem[]
}

export type OrderItem = {
  productId: number
  skuId: number
  productName: string
  skuCode: string
  price: number
  quantity: number
}

export type Notification = {
  id: number
  title: string
  content: string
  category: string
  relatedId?: number
  isRead: boolean
  createdAt: string
}

export type OperationLog = {
  id: number
  adminName: string
  module: string
  action: string
  requestPath?: string
  requestMethod?: string
  isSuccess: boolean
  createdAt: string
}

export type SearchResult = {
  type: string
  id: number
  title: string
  subtitle?: string
}

export type ProductSku = {
  id: number
  productId: number
  skuCode: string
  specJson?: string
  price: number
  stock: number
  isEnabled: boolean
  version: number
}

export type RoleInfo = {
  code: string
  permissions: string[]
}

export type AdminUser = {
  id: number
  username: string
  displayName: string
  isEnabled: boolean
  roles: string[]
}

export type Customer = {
  id: number
  username: string
  nickname?: string
  phone?: string
  email?: string
  isEnabled: boolean
  createdAt: string
}
