import { defineStore } from 'pinia'
import { api } from '../utils/api'
import type { CustomerProfile, LoginResponse } from '../types/api'

type AuthState = {
  token: string | null
  expiresAt: string | null
  profile: CustomerProfile | null
}

const STORAGE_KEY = 'watchshop_store_auth_v1'

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    token: null,
    expiresAt: null,
    profile: null,
  }),
  getters: {
    isLoggedIn: (s) => !!s.token,
    displayName: (s) => s.profile?.nickname || s.profile?.username || '',
  },
  actions: {
    hydrate() {
      const raw = localStorage.getItem(STORAGE_KEY)
      if (!raw) return
      try {
        Object.assign(this, JSON.parse(raw) as AuthState)
      } catch {
        localStorage.removeItem(STORAGE_KEY)
      }
    },
    persist() {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this.$state))
    },
    applyLogin(res: LoginResponse) {
      this.token = res.token
      this.expiresAt = res.expiresAt
      this.profile = res.profile
    },
    async login(username: string, password: string) {
      const res = await api.post<LoginResponse>('/store/auth/login', { username, password })
      this.applyLogin(res)
      this.persist()
    },
    async fetchMe() {
      const me = await api.get<CustomerProfile>('/store/auth/me')
      this.profile = me
      this.persist()
    },
    logout() {
      this.$reset()
      localStorage.removeItem(STORAGE_KEY)
    },
  },
})
