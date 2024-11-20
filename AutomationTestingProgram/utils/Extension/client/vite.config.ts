import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: "./dist",
    emptyOutDir: true,
    rollupOptions: {
      input: {
        sidepanel: "src/Extension/sidepanel.html",
        content: "src/Extension/content.ts",
        background: "src/Extension/background.ts",
      },
      output: {
        entryFileNames: "[name].js"
      }
    },
  }
})
