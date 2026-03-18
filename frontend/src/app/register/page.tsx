import { Suspense } from 'react';
import SignUp from '@/components/auth/SignUp';

export const metadata = {
  title: 'Sign Up — PromptForge',
  description: 'Create your PromptForge account',
};

export default function RegisterPage() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <SignUp />
    </Suspense>
  );
}