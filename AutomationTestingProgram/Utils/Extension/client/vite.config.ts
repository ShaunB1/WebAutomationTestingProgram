import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import { viteStaticCopy } from "vite-plugin-static-copy"
import path from 'path';

export default defineConfig({
  plugins: [
    react(),
    viteStaticCopy({
      targets: [
        {
          src: "manifest.json",
          dest: ""
        }
      ]
    }),
  ],
  build: {
    outDir: "./dist",
    rollupOptions: {
      input: {
        sidepanel: "sidepanel.html",
        content: "src/Extension/content.ts",
        background: "src/Extension/background.ts",
        app: "src/App.tsx"
      },
      output: {
        entryFileNames: "[name].js",
        assetFileNames: "assets/[name].[ext]"
      }
    },
  },
  server: {
    open: "sidepanel.html",
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, 'src'),
      '@modules': path.resolve(__dirname, 'src/modules'),
      '@assets': path.resolve(__dirname, 'src/assets'),
      '@auth': path.resolve(__dirname, 'src/auth'),
      '@interfaces': path.resolve(__dirname, 'src/interfaces'),
    },
  },
})
