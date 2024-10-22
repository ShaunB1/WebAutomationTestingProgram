import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      "/api": {
        target: "http://localhost:5223",
        changeOrigin: true,
        secure: false,
      }
    }
  },
  build: {
    outDir: "../wwwroot",
    emptyOutDir: true,
  }
})
