export function formatMoney(value: number | undefined | null): string {
  if (value == null) return '-'
  return `¥${value.toFixed(2)}`
}

export function formatDateTime(value: string | undefined | null): string {
  if (!value) return '-'
  const d = new Date(value)
  return Number.isNaN(d.getTime()) ? value : d.toLocaleString('zh-CN')
}

const orderStatusLabels: Record<number, string> = {
  0: '待付款',
  1: '已支付',
  2: '已发货',
  3: '已完成',
  4: '已取消',
}

export function orderStatusLabel(status: number): string {
  return orderStatusLabels[status] ?? String(status)
}

export function orderStatusType(status: number): 'warning' | 'success' | 'info' | 'danger' | '' {
  switch (status) {
    case 0:
      return 'warning'
    case 1:
      return 'success'
    case 2:
      return ''
    case 3:
      return 'info'
    case 4:
      return 'danger'
    default:
      return 'info'
  }
}
