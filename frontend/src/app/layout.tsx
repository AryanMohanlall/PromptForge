import type { Metadata } from 'next';
import AntdProvider from '@/components/providers/AntdProvider';
import { AuthProvider } from '@/providers/auth-provider';
import './globals.css';

export const metadata: Metadata = {
  title: 'PromptForge',
  description: 'Build AI-powered apps with PromptForge',
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body>
        <AuthProvider>
          <AntdProvider>{children}</AntdProvider>
        </AuthProvider>
      </body>
    </html>
  );
}
