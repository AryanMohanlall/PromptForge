"use client";

import { Suspense, useEffect } from "react";
import { useRouter } from "next/navigation";

const AUTH_USER_KEY = "auth_user";
const GITHUB_CONNECTED_KEY = "github_connected";

function getCookie(name: string): string | null {
  const match = document.cookie.match(new RegExp(`(?:^|; )${name}=([^;]*)`));
  return match ? decodeURIComponent(match[1]) : null;
}

function deleteCookie(name: string) {
  document.cookie = `${name}=; Max-Age=0; path=/`;
}

function GitHubCallback() {
  const router = useRouter();

  useEffect(() => {
    const raw = getCookie("github_auth_result");

    if (!raw) {
      router.replace("/login?error=missing_token");
      return;
    }

    deleteCookie("github_auth_result");

    const currentUser = sessionStorage.getItem(AUTH_USER_KEY);
    if (!currentUser) {
      router.replace("/login?error=github_requires_login");
      return;
    }

    sessionStorage.setItem(GITHUB_CONNECTED_KEY, "true");

    // Return the user to project creation after linking GitHub.
    window.location.replace("/create-project");
  }, [router]);

  return (
    <div
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        height: "100vh",
      }}
    >
      Signing you in…
    </div>
  );
}

export default function GitHubCallbackPage() {
  return (
    <Suspense>
      <GitHubCallback />
    </Suspense>
  );
}
