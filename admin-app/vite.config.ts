import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiTarget = env.VITE_API_PROXY_TARGET || 'http://localhost:5234'

  const proxy = {
    '/__api': {
      target: apiTarget,
      changeOrigin: true,
      secure: false,
      rewrite: (path: string) => path.replace(/^\/__api/, ''),
    },
  }

  return {
    plugins: [
      vue(),
      AutoImport({
        imports: ['vue', 'vue-router', 'pinia'],
        dts: 'src/auto-imports.d.ts',
        resolvers: [ElementPlusResolver()],
      }),
      Components({
        dts: 'src/components.d.ts',
        resolvers: [ElementPlusResolver()],
      }),
    ],
    server: {
      port: 5173,
      proxy,
    },
    preview: {
      port: 4173,
      proxy,
    },
  }
})
