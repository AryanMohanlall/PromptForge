import type { Metadata } from 'next';
import AntdProvider from '@/components/providers/AntdProvider';
import { AppProviders } from '@/providers';
import './globals.css';

export const metadata: Metadata = {
  title: 'PromptForge',
  description: 'Build AI-powered apps with PromptForge',
  icons: {
    icon: '/logo.svg',
  },
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body>
        <AppProviders>
          <AntdProvider>{children}</AntdProvider>
        </AppProviders>
      </body>
    </html>
  );
}
