import { defineStore } from 'pinia'
import { api } from '../utils/api'
import type { CartItem } from '../types/api'

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: [] as CartItem[],
    loaded: false,
  }),
  getters: {
    count: (s) => s.items.reduce((sum, i) => sum + i.quantity, 0),
    total: (s) => s.items.reduce((sum, i) => sum + i.price * i.quantity, 0),
  },
  actions: {
    async refresh() {
      const items = await api.get<CartItem[]>('/store/cart')
      this.items = items ?? []
      this.loaded = true
    },
    clearLocal() {
      this.items = []
      this.loaded = false
    },
  },
})
