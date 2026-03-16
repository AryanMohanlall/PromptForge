'use client';

import { ConfigProvider, theme } from 'antd';
import { StyleProvider } from 'antd-style';

export default function AntdProvider({ children }: { readonly children: React.ReactNode }) {
  return (
    <StyleProvider>
      <ConfigProvider
        theme={{
          algorithm: theme.darkAlgorithm,
          token: {
            colorPrimary: '#2dd4a8',
            colorBgLayout: '#0a0f14',
            colorBgContainer: '#101820',
            colorBgElevated: '#141e28',
            colorText: '#e8edf2',
            colorTextSecondary: '#6b7a8a',
            colorTextTertiary: '#4a5a6a',
            colorTextQuaternary: '#3a4a5a',
            colorBorder: 'rgba(255,255,255,0.08)',
            fontFamily: "'Outfit', -apple-system, sans-serif",
            fontSize: 14,
            borderRadius: 10,
          },
          components: {
            Input: {
              colorBgContainer: 'rgba(16,24,34,0.6)',
              colorBorder: 'rgba(255,255,255,0.06)',
              hoverBorderColor: 'rgba(45,212,168,0.2)',
              activeBorderColor: 'rgba(45,212,168,0.3)',
              activeShadow: '0 0 0 2px rgba(45,212,168,0.1)',
              colorTextPlaceholder: '#3a4a5a',
              controlHeight: 48,
              borderRadius: 10,
              paddingInline: 16,
            },
            Button: {
              controlHeight: 48,
              borderRadius: 10,
              fontWeight: 700,
              fontSize: 15,
            },
            Divider: {
              colorSplit: 'rgba(255,255,255,0.06)',
              colorText: '#3a4a5a',
            },
          },
        }}
      >
        {children}
      </ConfigProvider>
    </StyleProvider>
  );
}
