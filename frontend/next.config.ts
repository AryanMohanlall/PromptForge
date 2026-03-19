import type { NextConfig } from "next";
import path from "path";
import { loadEnvConfig } from "@next/env";

loadEnvConfig(path.resolve(__dirname, ".."));

const nextConfig: NextConfig = {
  // Ensure Turbopack uses the correct project root (avoids filesystem mismatch panics)
  turbopack: {
    root: path.resolve(__dirname),
  },

  reactCompiler: true,
};

export default nextConfig;
