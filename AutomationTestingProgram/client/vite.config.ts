import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react-swc";
import path from 'path';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  let host = env.NODE_ENV === env.LOCAL_HOST || "localhost:5223";
  console.log(host);

  return {
    plugins: [react()],
    server: {
      proxy: {
        "/api": {
          target: `http://${host}`,
          changeOrigin: true,
        },
        "/testHub": {
          target: `http://${host}`,
          changeOrigin: true,
          ws: true
        },
      },
    },
    build: {
      outDir: "../wwwroot",
      emptyOutDir: true,
    },
    define: {
      "process.env": env,
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
  };
});
