import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiTarget = env.VITE_API_PROXY_TARGET || 'http://localhost:5240'

  return {
    plugins: [vue()],
    server: {
      port: 5175,
      proxy: {
        '/__api': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
          rewrite: (path: string) => path.replace(/^\/__api/, ''),
        },
      },
    },
    preview: {
      port: 4175,
      proxy: {
        '/__api': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
          rewrite: (path: string) => path.replace(/^\/__api/, ''),
        },
      },
    },
  }
})
