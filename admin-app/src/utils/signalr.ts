import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'

let connection: HubConnection | null = null

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || '/__api'

export function connectNotifications(token: string) {
  if (connection) return
  connection = new HubConnectionBuilder()
    .withUrl(`${apiBaseUrl}/hubs/notifications?access_token=${encodeURIComponent(token)}`)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  connection.on('ReceiveNotification', (msg) => {
    // keep it simple: log + browser notification later
    console.log('[notification]', msg)
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

