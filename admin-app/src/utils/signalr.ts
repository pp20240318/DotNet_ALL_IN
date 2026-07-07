import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'
import { ElNotification } from 'element-plus'
import { useNotificationStore } from '../stores/notifications'

let connection: HubConnection | null = null

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || '/__api'

export function connectNotifications(token: string) {
  if (connection) return
  connection = new HubConnectionBuilder()
    .withUrl(`${apiBaseUrl}/hubs/notifications?access_token=${encodeURIComponent(token)}`)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  connection.on('ReceiveNotification', (msg: { title?: string; content?: string }) => {
    const notifications = useNotificationStore()
    notifications.bumpUnread()
    ElNotification({
      title: msg?.title ?? '新通知',
      message: msg?.content ?? '',
      type: 'info',
      duration: 5000,
    })
  })

  connection.start().catch((err) => {
    console.warn('signalr start failed', err)
  })
}

export function disconnectNotifications() {
  if (!connection) return
  const c = connection
  connection = null
  c.stop().catch(() => {})
}
