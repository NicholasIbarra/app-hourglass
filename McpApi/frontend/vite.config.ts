import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Proxy chat API calls to the MCP service
      '/api/chat': {
        target: process.env.MCPSANDBOX_MCP_HTTPS || process.env.MCPSANDBOX_MCP_HTTP,
        changeOrigin: true,
        secure: false
      },
      // Proxy other API calls to the app service
      '/api': {
        target: process.env.SERVER_HTTPS || process.env.SERVER_HTTP,
        changeOrigin: true
      }
    }
  }
})
