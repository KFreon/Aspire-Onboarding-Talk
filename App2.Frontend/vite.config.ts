import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: parseInt(process.env.PORT || "5174"),
    proxy: {
      "/api": {
        target:
          process.env.services__app2__https__0 ||
          process.env.services__app2__http__0 ||
          "http://localhost:5085",
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
