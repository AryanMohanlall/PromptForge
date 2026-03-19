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
            // Base theme colors (black / green / purple / orange)
            colorPrimary: '#22c55e', // vivid green
            colorSuccess: '#2dd4a8',
            colorWarning: '#facc15', // yellow

            colorBgLayout: '#050b13',
            colorBgContainer: '#0f1a27',
            colorBgElevated: '#15202f',

            colorText: '#e8f3ea',
            colorTextSecondary: '#a7cfc1',
            colorTextTertiary: '#7bb4a0',
            colorTextQuaternary: '#5f9b86',

            colorBorder: 'rgba(40,225,140,0.25)',

            fontFamily: "'Outfit', -apple-system, sans-serif",
            fontSize: 14,
            borderRadius: 10,
          },
          components: {
            Input: {
              colorBgContainer: 'rgba(18,30,34,0.72)',
              colorBorder: 'rgba(40,225,140,0.18)',
              hoverBorderColor: 'rgba(109,40,217,0.6)',
              activeBorderColor: 'rgba(250,204,21,0.55)',
              activeShadow: '0 0 0 2px rgba(109,40,217,0.25)',
              colorTextPlaceholder: 'rgba(200,230,210,0.65)',
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
              colorSplit: 'rgba(40,225,140,0.2)',
              colorText: 'rgba(200,230,210,0.75)',
            },
          },
        }}
      >
        {children}
      </ConfigProvider>
    </StyleProvider>
  );
}
