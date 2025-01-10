import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react-swc";

/*// Custom Vite plugin to remove comments from JSON files
function jsonCommentPlugin() {
    return {
        name: 'json-comment-plugin',
        transform(src: string, id: string) {
            if (id.endsWith('.json')) {
                // Remove single-line comments (//)
                const cleanedSrc = src.replace(/\/\/.*$/gm, '').trim();
                return {
                    code: cleanedSrc,
                    map: null, // Provide source map if necessary
                };
            }
            return null;
        },
    };
}
*/

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
