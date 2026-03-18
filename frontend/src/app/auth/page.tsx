import { redirect } from 'next/navigation';

export const metadata = {
  title: 'Sign In — PromptForge',
  description: 'Sign in to your PromptForge account',
};

export default function AuthPage() {
  redirect('/login');
}
