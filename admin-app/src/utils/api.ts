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
  async post<T>(url: string, body?: any, params?: Record<string, unknown>): Promise<T> {
    const resp = await http.post<ApiEnvelope<T>>(url, body, { params })
    return unwrap(resp.data)
  },
  async put<T>(url: string, body?: any): Promise<T> {
    const resp = await http.put<ApiEnvelope<T>>(url, body)
    return unwrap(resp.data)
  },
  async delete<T>(url: string): Promise<T> {
    const resp = await http.delete<ApiEnvelope<T>>(url)
    return unwrap(resp.data)
  },
  async upload<T>(url: string, file: File, fieldName = 'file'): Promise<T> {
    const form = new FormData()
    form.append(fieldName, file)
    const resp = await http.post<ApiEnvelope<T>>(url, form)
    return unwrap(resp.data)
  },
}

export function getApiErrorMessage(error: unknown, fallback = '请求失败'): string {
  if (error instanceof AxiosError) {
    const data = error.response?.data as ApiEnvelope<unknown> | undefined
    if (data?.message) return data.message
    if (error.message) return error.message
  }
  if (error instanceof Error) return error.message
  return fallback
}

function unwrap<T>(env: ApiEnvelope<T>): T {
  if (env.code !== 0) {
    throw new Error(env.message || `API error code=${env.code}`)
  }
  return env.data as T
}

