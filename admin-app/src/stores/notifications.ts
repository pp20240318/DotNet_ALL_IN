import { defineStore } from 'pinia'
import { api } from '../utils/api'

type NotificationState = {
  unreadCount: number
}

export const useNotificationStore = defineStore('notifications', {
  state: (): NotificationState => ({
    unreadCount: 0,
  }),
  actions: {
    async refreshUnreadCount() {
      try {
        const res = await api.get<{ count: number }>('/notifications/unread-count')
        this.unreadCount = res.count ?? 0
      } catch {
        // ignore when offline or unauthorized
      }
    },
    bumpUnread() {
      this.unreadCount += 1
    },
    markReadLocally() {
      if (this.unreadCount > 0) this.unreadCount -= 1
    },
    clearUnread() {
      this.unreadCount = 0
    },
  },
})
