import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react-swc'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const host = env.VITE_HOST || 'localhost:5223'
  
  return {
    plugins: [react()],
    server: {
      proxy: {
        "/api": {
          target: `http://${host}`,
          changeOrigin: true,
          secure: false,
        },
      },
    },
    build: {
      outDir: "../wwwroot",
      emptyOutDir: true,
    }
  }
})
