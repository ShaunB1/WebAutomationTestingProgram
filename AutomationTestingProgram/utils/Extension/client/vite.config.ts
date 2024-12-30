import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import { viteStaticCopy } from "vite-plugin-static-copy"

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
        app: "src/Components/App.tsx"
      },
      output: {
        entryFileNames: "[name].js",
        assetFileNames: "assets/[name].[ext]"
      }
    },
  },
  server: {
    open: "sidepanel.html",
  }
})
