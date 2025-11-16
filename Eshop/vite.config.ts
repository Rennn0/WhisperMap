import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    port: 18000,
    strictPort: true,
    host: true,
    proxy: {
      "/cl": {
        target: "http://localhost:18070",
        changeOrigin: true,
      }
    }
  }
})
