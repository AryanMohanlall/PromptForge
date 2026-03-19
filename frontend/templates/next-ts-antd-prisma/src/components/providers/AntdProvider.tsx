"use client";

import { ConfigProvider, theme as antdTheme } from "antd";
import { StyleProvider } from "antd-style";
import { THEME_TOKENS } from "@/styles/theme";

export function AntdProvider({ children }: { children: React.ReactNode }) {
  return (
    <StyleProvider>
      <ConfigProvider
        theme={{
          algorithm: antdTheme.darkAlgorithm,
          token: THEME_TOKENS,
        }}
      >
        {children}
      </ConfigProvider>
    </StyleProvider>
  );
}
