import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react-swc";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  let host = env.NODE_ENV === "production" ? env.PROD_HOST : env.LOCAL_HOST || "localhost:5223";
  console.log(host);

  return {
    plugins: [react()],
    server: {
      proxy: {
        "/api": {
          target: `https://${host}`,
          changeOrigin: true,
          secure: false,
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
  };
});
