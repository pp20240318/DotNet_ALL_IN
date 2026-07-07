import axios, { AxiosError } from 'axios'
import type { ApiEnvelope } from '../types/common'
import { useAuthStore } from '../stores/auth'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || '/__api'

export const http = axios.create({
  baseURL: apiBaseUrl,
  timeout: 15000,
})

http.interceptors.request.use((config) => {
  const auth = useAuthStore()
  if (auth.token) {
    config.headers = config.headers ?? {}
    config.headers.Authorization = `Bearer ${auth.token}`
  }
  config.headers = config.headers ?? {}
  config.headers['X-Api-Version'] = '1.0'
  return config
})

let refreshing: Promise<void> | null = null

http.interceptors.response.use(
  (resp) => resp,
  async (error: AxiosError) => {
    const auth = useAuthStore()
    const status = error.response?.status
    const original = error.config as any

    if (status === 401 && auth.refreshToken && !original?._retried) {
      original._retried = true
      try {
        if (!refreshing) {
          refreshing = auth.refresh().finally(() => {
            refreshing = null
          })
        }
        await refreshing
        return http(original)
      } catch {
        auth.logout()
      }
    }
    throw error
  },
)

export const api = {
  async get<T>(url: string, params?: any): Promise<T> {
    const resp = await http.get<ApiEnvelope<T>>(url, { params })
    return unwrap(resp.data)
  },
  async post<T>(url: string, body?: any): Promise<T> {
    const resp = await http.post<ApiEnvelope<T>>(url, body)
    return unwrap(resp.data)
  },
  async put<T>(url: string, body?: any): Promise<T> {
    const resp = await http.put<ApiEnvelope<T>>(url, body)
    return unwrap(resp.data)
  },
}

function unwrap<T>(env: ApiEnvelope<T>): T {
  if (env.code !== 0) {
    throw new Error(env.message || `API error code=${env.code}`)
  }
  return env.data as T
}

