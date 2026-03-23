import type { NextConfig } from "next";
import path from "path";
import { loadEnvConfig } from "@next/env";

loadEnvConfig(path.resolve(__dirname, ".."));

const nextConfig: NextConfig = {
  reactCompiler: true,
  turbopack: {
    root: path.resolve(__dirname),
  },
  images: {
    remotePatterns: [
      {
        protocol: "https",
        hostname: "placehold.co",
      },
    ],
    dangerouslyAllowSVG: true,
  },
  webpack: (config) => {
    // Allow pdfjs-dist worker to be loaded as a static asset
    config.resolve.alias["pdfjs-dist"] = path.resolve(
      __dirname,
      "node_modules/pdfjs-dist"
    );
    return config;
  },
};

export default nextConfig;
