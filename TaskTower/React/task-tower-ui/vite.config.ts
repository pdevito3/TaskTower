import react from "@vitejs/plugin-react-swc";
import { defineConfig } from "vite";
import { viteStaticCopy } from "vite-plugin-static-copy";

// https://vitejs.dev/config/
export default defineConfig({
  build: {
    outDir: "../../WebApp",
  },
  plugins: [
    react(),
    viteStaticCopy({
      targets: [
        {
          src: "public/**/*",
          dest: "assets",
        },
      ],
    }),
  ],
});
