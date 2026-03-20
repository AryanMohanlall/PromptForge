"use client";

import { Suspense, useEffect } from "react";
import { useRouter } from "next/navigation";
import { setAuthToken } from "@/utils/axiosInstance";

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

  /*   useEffect(() => {
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

    router.replace("/dashboard");
  }, [router]); */

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const accessToken = params.get("token");
    const userId = params.get("userId");
    const expireInSeconds = params.get("expireInSeconds");

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
