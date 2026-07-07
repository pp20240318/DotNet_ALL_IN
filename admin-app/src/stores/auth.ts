import { defineStore } from 'pinia'
import { api } from '../utils/api'
import type { LoginResponse } from '../types/auth'
import { connectNotifications, disconnectNotifications } from '../utils/signalr'

type AuthState = {
  token: string | null
  refreshToken: string | null
  expiresAt: string | null
  refreshExpiresAt: string | null
  username: string | null
  displayName: string | null
  roles: string[]
  permissions: string[]
}

const STORAGE_KEY = 'watchshop_admin_auth_v1'

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    token: null,
    refreshToken: null,
    expiresAt: null,
    refreshExpiresAt: null,
    username: null,
    displayName: null,
    roles: [],
    permissions: [],
  }),
  getters: {
    isLoggedIn: (s) => !!s.token,
  },
  actions: {
    hydrate() {
      const raw = localStorage.getItem(STORAGE_KEY)
      if (!raw) return
      try {
        const parsed = JSON.parse(raw) as AuthState
        Object.assign(this, parsed)
      } catch {
        localStorage.removeItem(STORAGE_KEY)
      }
    },
    persist() {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this.$state))
    },
    hasPermission(permission: string) {
      return this.roles.includes('SuperAdmin') || this.permissions.includes(permission) || this.permissions.includes('system:admin')
    },
    async login(username: string, password: string) {
      const res = await api.post<LoginResponse>('/auth/login', { username, password })
      this.applyLogin(res)
      this.persist()
      connectNotifications(this.token!)
    },
    async refresh() {
      if (!this.refreshToken) throw new Error('missing refreshToken')
      const res = await api.post<LoginResponse>('/auth/refresh', { refreshToken: this.refreshToken })
      this.applyLogin(res)
      this.persist()
      connectNotifications(this.token!)
    },
    async fetchMe() {
      const me = await api.get<{ id: number; username: string; displayName: string; roles: string[]; permissions: string[] }>('/auth/me')
      this.username = me.username
      this.displayName = me.displayName
      this.roles = me.roles ?? []
      this.permissions = me.permissions ?? []
      this.persist()
    },
    logout() {
      this.$reset()
      localStorage.removeItem(STORAGE_KEY)
      disconnectNotifications()
    },
    applyLogin(res: LoginResponse) {
      this.token = res.token
      this.expiresAt = res.expiresAt
      this.refreshToken = res.refreshToken
      this.refreshExpiresAt = res.refreshExpiresAt
      this.username = res.username
      this.displayName = res.displayName
      this.roles = res.roles ?? []
      this.permissions = res.permissions ?? []
    },
  },
})

