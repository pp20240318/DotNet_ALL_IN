import { onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'

/** 从路由 query 读取参数并触发回调，返回是否命中 query */
export function useRouteQueryParam(param: string, apply: (value: string) => void) {
  const route = useRoute()

  function read(): boolean {
    const q = route.query[param]
    if (typeof q === 'string' && q.trim()) {
      apply(q.trim())
      return true
    }
    return false
  }

  onMounted(read)
  watch(() => route.query[param], read)

  return { hasQuery: read }
}
