"use client";

import { Suspense, useEffect } from "react";
import { useRouter } from "next/navigation";
import { setAuthToken } from "@/utils/axiosInstance";

const AUTH_USER_KEY = "auth_user";

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

    const [accessToken, userId, expireInSeconds] = raw.split("|");

    if (!accessToken || !userId) {
      router.replace("/login?error=missing_token");
      return;
    }

    setAuthToken(accessToken);

    sessionStorage.setItem(
      AUTH_USER_KEY,
      JSON.stringify({
        userId: Number(userId),
        accessToken,
        expireInSeconds: Number(expireInSeconds ?? 86400),
      }),
    );

    router.replace("/dashboard");
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
